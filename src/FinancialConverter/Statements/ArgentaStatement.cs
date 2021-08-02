namespace FinancialConverter.Statements
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CodaParser.Statements;
    using Microsoft.Extensions.Logging;
    using Npoi.Mapper;
    using Npoi.Mapper.Attributes;
    using NPOI.SS.UserModel;

    public class ArgentaStatement
    {
        [Column("Valutadatum")]
        public DateTime Valutadatum { get; set; }

        [Column("Verrichtingsdatum")]
        public DateTime Verrichtingsdatum { get; set; }

        [Column("Bedrag")]
        public decimal Bedrag { get; set; }

        [Column("Rekening tegenpartij")]
        public string RekeningTegenpartij { get; set; }

        [Column("Naam tegenpartij")]
        public string NaamTegenpartij { get; set; }

        [Column("Munt")]
        public string Munt { get; set; }

        [Column("Mededeling")]
        public string Mededeling { get; set; }
    }

    public static class ArgentaStatementExtensions
    {
        public static IEnumerable<Statement> FromArgenta(
            this string argentaFile,
            ILogger logger)
        {
            logger.LogInformation(
                "Reading {FileType} file '{InFile}'.",
                "Argenta",
                argentaFile);

            return ReadFile(argentaFile);
        }

        private static IEnumerable<Statement> ReadFile(string file)
        {
            IWorkbook workbook;
            using (var excel = new FileStream(file, FileMode.Open, FileAccess.Read))
                workbook = WorkbookFactory.Create(excel);

            var importer = new Mapper(workbook);
            var items = importer.Take<ArgentaStatement>();

            var transactions = new List<Transaction>();
            foreach (var item in items)
            {
                var row = item.Value;

                var transaction = new Transaction(
                    new AccountOtherParty(
                        row.NaamTegenpartij,
                        string.Empty,
                        row.RekeningTegenpartij,
                        row.Munt),
                    1,
                    1,
                    row.Verrichtingsdatum,
                    row.Valutadatum,
                    row.Bedrag,
                    row.Mededeling,
                    row.Mededeling,
                    null);

                transactions.Add(transaction);
            }

            var number = file.Split('_')[1];
            var account = new Account(
                "Argenta File",
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
