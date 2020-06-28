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
    using Microsoft.Extensions.Logging;

    public class BnpStatement
    {
        public DateTime Something { get; set; }
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
            var latestDate = GetLatestDate(previousFile);

            logger.LogInformation(
                "Reading {FileType} file '{InFile}' from {Date}.",
                "BNP",
                bnpFile,
                latestDate.ToString("yyyy-MM-dd"));

            return ReadFile(bnpFile, latestDate);
        }

        private static string? GetPreviousFile(string[] inFiles, string file)
        {
            var previousIndex = Array.FindIndex(inFiles, 0, inFiles.Length, s => s.Equals(file));
            return previousIndex > 0 ? inFiles[previousIndex - 1] : null;
        }

        private static DateTime GetLatestDate(string? file)
        {
            if (file == null)
                return DateTime.MinValue;

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = (_, __) => true,
                Delimiter = ","
            };

            configuration.AutoMap<BnpStatement>();

            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, ImportCulture))
            {
                var records = csv.GetRecords<BnpStatement>();

                return records
                    .Select(x => x.Something)
                    .Max();
            }
        }

        private static List<Statement> ReadFile(string file, DateTime from)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = (_, __) => true,
                Delimiter = ","
            };

            configuration.AutoMap<BnpStatement>();

            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, ImportCulture))
            {
                var records = csv.GetRecords<BnpStatement>();
                var validRecords = records
                    .Where(x => x.Something > from);

                var transactions = new List<Transaction>();
                foreach (var record in validRecords)
                {
                    // TODO: Create a Transaction to return
                    Transaction transaction = null; // TODO: Create transaciton

                    transactions.Add(transaction);
                }

                Account account = null; // TODO: Create Account

                return new List<Statement>
                {
                    new Statement(
                        transactions.Max(x => x.TransactionDate),
                        null,
                        0,
                        0,
                        string.Empty,
                        transactions)
                };
            }
        }
    }
}
