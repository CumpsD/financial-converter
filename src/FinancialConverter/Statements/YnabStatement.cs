namespace FinancialConverter.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using CodaParser.Statements;
    using Microsoft.Extensions.Logging;

    public class YnabStatement
    {
        public string Date { get; set; }

        public string Payee { get; set; }

        public string Memo { get; set; }

        public string Outflow { get; set; }

        public string Inflow { get; set; }
    }

    public static class YnabStatementExtensions
    {
        private static readonly CultureInfo ExportCulture = CultureInfo.CreateSpecificCulture("en-US");
        private static readonly CultureInfo LogCulture = CultureInfo.CreateSpecificCulture("nl-BE");
        private static readonly Regex SpaceReplace = new Regex("[ ]{2,}", RegexOptions.None);

        public static IEnumerable<YnabStatement> ToYnab(
            this IEnumerable<Statement> statements,
            ILogger logger)
        {
            var ynabStatementList = new List<YnabStatement>();

            foreach (var statement in statements)
            {
                logger.LogInformation(
                    "Parsing statement from {Date} for {AccountName}, {AccountNumber}.",
                    statement.Date.ToString("yyyyMMdd"),
                    statement.Account.Name.Trim(),
                    statement.Account.Number.Trim());

                foreach (var transaction in statement.Transactions)
                {
                    var ynabStatement = new YnabStatement
                    {
                        Date = transaction.TransactionDate.ToString("MM/dd/yyyy"),
                        Payee = transaction.Account.Name.Trim(),
                        Outflow = transaction.Amount >= 0
                            ? string.Empty
                            : Math.Abs(transaction.Amount).ToString(ExportCulture),
                        Inflow = transaction.Amount >= 0
                            ? Math.Abs(transaction.Amount).ToString(ExportCulture)
                            : string.Empty,
                        Memo = !string.IsNullOrWhiteSpace(transaction.Message)
                            ? SpaceReplace.Replace(transaction.Message, " ")
                            : SpaceReplace.Replace(transaction.StructuredMessage, " ")
                    };

                    ynabStatementList.Add(ynabStatement);

                    logger.LogInformation(
                        "Parsing transaction from {Date} for {Payee} ({Information}): {Amount}",
                        transaction.TransactionDate.ToString("yyyy-MM-dd"),
                        string.IsNullOrWhiteSpace(ynabStatement.Payee) ? "N/A" : ynabStatement.Payee,
                        ynabStatement.Memo,
                        transaction.Amount.ToString("C", LogCulture));
                }
            }

            return ynabStatementList;
        }
    }
}
