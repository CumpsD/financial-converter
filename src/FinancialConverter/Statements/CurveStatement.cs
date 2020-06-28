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

    // Export Format, Date (YYYY-MM-DD as UTC), Time (HH:MM:SS), Merchant, Txn Amount (Funding Card), Txn Currency (Funding Card), Txn Amount (Foreign Spend), Txn Currency (Foreign Spend), Card Name, Card Last 4 Digits, Type, Category, Notes
    public class CurveStatement
    {
        [Name("Export Format")]
        public string ExportFormat { get; set; }

        [Name("Date (YYYY-MM-DD as UTC)")]
        public DateTime Date { get; set; }

        [Name("Time (HH:MM:SS)")]
        public string Time{ get; set; }

        public string Merchant { get; set; }

        [Name("Txn Amount (Funding Card)")]
        public decimal Amount { get; set; }

        [Name("Txn Currency (Funding Card)")]
        public string Currency { get; set; }

        [Name("Txn Amount (Foreign Spend)")]
        public decimal AmountForeign { get; set; }

        [Name("Txn Currency (Foreign Spend)")]
        public string CurrencyForeign { get; set; }

        [Name("Card Name")]
        public string CardName { get; set; }

        [Name("Card Last 4 Digits")]
        public string LastDigits { get; set; }

        public string Type { get; set; }

        public string Category { get; set; }

        public string Notes { get; set; }
    }

    public static class CurveStatementExtensions
    {
        private static readonly CultureInfo ImportCulture = CultureInfo.CreateSpecificCulture("en-US");

        public static IEnumerable<Statement> FromCurve(
            this string curveFile,
            ILogger logger)
        {
            logger.LogInformation(
                "Reading {FileType} file '{InFile}'.",
                "Curve",
                curveFile);

            return ReadFile(curveFile);
        }

        private static IEnumerable<Statement> ReadFile(string file)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = (_, __) => true,
                Delimiter = ", ",
                CultureInfo = ImportCulture,
                HeaderValidated = (isValid, headerNames, headerNameIndex, context) => { }
            };

            configuration.AutoMap<CurveStatement>();

            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, configuration))
            {
                var records = csv.GetRecords<CurveStatement>();

                var transactions = new List<Transaction>();
                foreach (var record in records)
                {
                    var transaction = new Transaction(
                        new AccountOtherParty(
                            record.Merchant,
                            string.Empty,
                            record.Merchant,
                            record.Currency),
                        1,
                        1,
                        record.Date.ToLocalTime(),
                        record.Date.ToLocalTime(),
                        record.Amount * -1,
                        record.Notes,
                        record.Notes,
                        null);

                    transactions.Add(transaction);
                }

                var number = file.Substring(0, file.IndexOf("-", StringComparison.Ordinal));
                var account = new Account(
                    "Curve File",
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
