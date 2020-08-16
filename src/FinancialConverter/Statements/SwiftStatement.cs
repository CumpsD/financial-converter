namespace FinancialConverter.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using CodaParser.Statements;
    using Microsoft.Extensions.Logging;

    public class SwiftStatement
    {
        public string Account { get; set; }
        public string Date { get; set; }
        public List<SwiftStatementLine> Lines { get; set; } = new List<SwiftStatementLine>();
    }

    public class SwiftStatementLine
    {
        public string Name { get; set; }
        public string Reference { get; set; }
        public bool IsCredit { get; set; }
        public decimal Amount { get; set;}
    }

    public static class SwiftStatementExtensions
    {
        private static readonly CultureInfo ExportCulture = CultureInfo.CreateSpecificCulture("en-US");
        private static readonly CultureInfo LogCulture = CultureInfo.CreateSpecificCulture("nl-BE");
        private static readonly Regex SpaceReplace = new Regex("[ ]{2,}", RegexOptions.None);

        public static IEnumerable<SwiftStatement> ToSwift(
            this IEnumerable<Statement> statements,
            ILogger logger)
        {
            var swiftStatementList = new Dictionary<string, SwiftStatement>();

            foreach (var statement in statements)
            {
                logger.LogInformation(
                    "Parsing statement from {Date} for {AccountName}, {AccountNumber}.",
                    statement.Date.ToString("yyyyMMdd"),
                    statement.Account.Name.Trim(),
                    statement.Account.Number.Trim());

                foreach (var transaction in statement.Transactions)
                {
                    var date = transaction.TransactionDate.ToString("yyyyMMdd");

                    if (!swiftStatementList.ContainsKey(date))
                        swiftStatementList.Add(date, new SwiftStatement
                        {
                            Date = date,
                            Account = statement.Account.Number.Trim(),
                        });

                    var day = swiftStatementList[date];

                    var line = new SwiftStatementLine
                    {
                        Name = transaction.Account.Name.Trim(),
                        Amount = Math.Abs(transaction.Amount),
                        IsCredit = transaction.Amount >= decimal.Zero,
                        Reference = !string.IsNullOrWhiteSpace(transaction.Message)
                            ? SpaceReplace.Replace(transaction.Message, " ")
                            : SpaceReplace.Replace(transaction.StructuredMessage, " ")
                    };

                    day.Lines.Add(line);

                    logger.LogInformation(
                        "Parsing transaction from {Date} for {Payee} ({Information}): {Amount}",
                        transaction.TransactionDate.ToString("yyyy-MM-dd"),
                        string.IsNullOrWhiteSpace(line.Name) ? "N/A" : line.Name,
                        line.Reference,
                        transaction.Amount.ToString("C", LogCulture));
                }
            }

            return swiftStatementList.Values;
        }
    }
}
