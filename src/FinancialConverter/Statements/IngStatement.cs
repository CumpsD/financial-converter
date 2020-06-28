namespace FinancialConverter.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using CodaParser.Statements;
    using CsvHelper.Configuration.Attributes;
    using Microsoft.Extensions.Logging;

    public class IngStatement
    {
        public string Datum { get; set; }

        [Name("Naam / Omschrijving")]
        public string Naam { get; set; }

        public string Rekening { get; set; }

        public string Tegenrekening { get; set; }

        public string Code { get; set; }

        [Name("Af Bij")]
        public string AfBij { get; set; }

        [Name("Bedrag (EUR)")]
        public string Bedrag { get; set; }

        public string MutatieSoort { get; set; }

        public string Mededelingen { get; set; }
    }

    public static class IngStatementExtensions
    {
        private static readonly CultureInfo ExportCulture = CultureInfo.CreateSpecificCulture("nl-BE");
        private static readonly CultureInfo LogCulture = CultureInfo.CreateSpecificCulture("nl-BE");
        private static readonly Regex SpaceReplace = new Regex("[ ]{2,}", RegexOptions.None);

        public static IEnumerable<IngStatement> ToIng(
            this IEnumerable<Statement> statements,
            ILogger logger)
        {
            var ingStatementList = new List<IngStatement>();

            foreach (var statement in statements)
            {
                logger.LogInformation(
                    "Parsing statement from {Date} for {AccountName}, {AccountNumber}.",
                    statement.Date.ToString("yyyyMMdd"),
                    statement.Account.Name.Trim(),
                    statement.Account.Number.Trim());

                foreach (var transaction in statement.Transactions)
                {
                    var ingStatement = new IngStatement
                    {
                        Datum = transaction.TransactionDate.ToString("yyyyMMdd"),
                        Naam = transaction.Account.Name.Trim(),
                        Rekening = statement.Account.Number.Trim(),
                        Code = "DV",
                        AfBij = transaction.Amount >= decimal.Zero
                            ? "Bij"
                            : "Af",
                        Bedrag = Math.Abs(transaction.Amount).ToString(ExportCulture),
                        MutatieSoort = "Diversen",
                        Mededelingen = !string.IsNullOrWhiteSpace(transaction.Message)
                            ? SpaceReplace.Replace(transaction.Message, " ")
                            : SpaceReplace.Replace(transaction.StructuredMessage, " ")
                    };

                    ingStatementList.Add(ingStatement);

                    logger.LogInformation(
                        "Parsing transaction from {Date} for {Payee} ({Information}): {Amount}",
                        transaction.TransactionDate.ToString("yyyy-MM-dd"),
                        string.IsNullOrWhiteSpace(ingStatement.Naam) ? "N/A" : ingStatement.Naam,
                        ingStatement.Mededelingen,
                        transaction.Amount.ToString("C", LogCulture));
                }
            }

            return ingStatementList;
        }
    }
}
