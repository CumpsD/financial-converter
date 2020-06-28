namespace FinancialConverter.Statements
{
    using CodaParser;
    using CodaParser.Statements;
    using FinancialConverter.Statements;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.Configuration.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

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
        private static readonly CultureInfo _exportCulture = CultureInfo.CreateSpecificCulture("en-US");
        private static readonly CultureInfo _logCulture = CultureInfo.CreateSpecificCulture("nl-BE");
        private static readonly Regex _spaceReplace = new Regex("[ ]{2,}", RegexOptions.None);

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
                            : Math.Abs(transaction.Amount).ToString(_exportCulture),
                        Inflow = transaction.Amount >= 0
                            ? Math.Abs(transaction.Amount).ToString(_exportCulture)
                            : string.Empty,
                        Memo = !string.IsNullOrWhiteSpace(transaction.Message)
                            ? _spaceReplace.Replace(transaction.Message, " ")
                            : _spaceReplace.Replace(transaction.StructuredMessage, " ")
                    };

                    ynabStatementList.Add(ynabStatement);

                    logger.LogInformation(
                        "Parsing transaction from {Date} for {Payee} ({Information}): {Amount}",
                        transaction.TransactionDate.ToString("yyyy-MM-dd"),
                        string.IsNullOrWhiteSpace(ynabStatement.Payee) ? "N/A" : ynabStatement.Payee,
                        ynabStatement.Memo,
                        transaction.Amount.ToString("C", _logCulture));
                }
            }

            return ynabStatementList;
        }
    }
}