namespace FinancialConverter.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using CodaParser.Statements;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.Configuration.Attributes;
    using Microsoft.Extensions.Logging;
    using Account = CodaParser.Statements.Account;

    // Volgnummer;Uitvoeringsdatum;Valutadatum;Bedrag;Valuta rekening;TEGENPARTIJ VAN DE VERRICHTING;Details;Rekeningnummer
    public class BnpStatement
    {
        public string Volgnummer { get; set; }

        public DateTime Uitvoeringsdatum { get; set; }

        public DateTime Valutadatum { get; set; }

        public decimal Bedrag { get; set; }

        [Name("Valuta rekening")]
        public string ValutaRekening { get; set; }

        [Name("TEGENPARTIJ VAN DE VERRICHTING")]
        public string Tegenpartij { get; set; }

        public string Details { get; set; }

        public string Rekeningnummer { get; set; }
    }

    public static class BnpStatementExtensions
    {
        private static readonly CultureInfo ImportCulture = CultureInfo.CreateSpecificCulture("nl-BE");

        public static IEnumerable<Statement> FromBnp(
            this string bnpFile,
            ILogger logger,
            string[] inFiles)
        {
            // BNP has a quirk where you have to check the previous file
            // to check which date was exported last.
            var previousFile = GetPreviousFile(inFiles, bnpFile);
            var latestSequence = GetLatestSequence(previousFile);

            logger.LogInformation(
                "Reading {FileType} file '{InFile}' from '{Date}'.",
                "BNP",
                bnpFile,
                latestSequence);

            return ReadFile(bnpFile, latestSequence);
        }

        private static string? GetPreviousFile(string[] inFiles, string file)
        {
            var previousIndex = Array.FindIndex(inFiles, 0, inFiles.Length, s => s.Equals(file));
            return previousIndex > 0 ? inFiles[previousIndex - 1] : null;
        }

        private static string GetLatestSequence(string? file)
        {
            if (file == null)
                return "2000-0001";

            var configuration = new CsvConfiguration(ImportCulture)
            {
                ShouldQuote = _ => true,
                Delimiter = ";",
            };

            //configuration.AutoMap<BnpStatement>();

            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, configuration))
            {
                var records = csv.GetRecords<BnpStatement>();

                return records
                    .Select(x => x.Volgnummer)
                    .Max();
            }
        }

        private static IEnumerable<Statement> ReadFile(string file, string from)
        {
            var configuration = new CsvConfiguration(ImportCulture)
            {
                ShouldQuote = _ => true,
                Delimiter = ";",
            };

            //configuration.AutoMap<BnpStatement>();

            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, configuration))
            {
                var records = csv.GetRecords<BnpStatement>();
                var validRecords = records
                    .Where(x => string.Compare(x.Volgnummer, from, StringComparison.Ordinal) > 0)
                    .ToList();

                var transactions = new List<Transaction>();
                foreach (var record in validRecords)
                {
                    var transaction = new Transaction(
                        new AccountOtherParty(
                            record.Tegenpartij,
                            string.Empty,
                            record.Tegenpartij,
                            record.ValutaRekening),
                        1,
                        1,
                        record.Uitvoeringsdatum,
                        record.Valutadatum,
                        record.Bedrag,
                        record.Details,
                        record.Details,
                        null);

                    transactions.Add(transaction);
                }

                var number = file.Substring(0, file.IndexOf("-", StringComparison.Ordinal));
                var account = new Account(
                    "BNP File",
                    number,
                    number,
                    number,
                    "EUR",
                    "BE");

                var statements = new List<Statement>();
                if (transactions.Count > 0)
                    statements.Add(
                        new Statement(
                            transactions.Max(x => x.TransactionDate),
                            account,
                            0,
                            0,
                            string.Empty,
                            transactions));

                return statements;
            }
        }
    }
}
