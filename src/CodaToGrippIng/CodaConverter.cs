namespace CodaToGrippIng
{
    using CodaParser;
    using CodaParser.Statements;
    using CodaToGrippIng.Statements;
    using CsvHelper;
    using CsvHelper.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class CodaConverter
    {
        private readonly Parser _codaParser = new Parser();
        private readonly CultureInfo _dutchCulture = CultureInfo.CreateSpecificCulture("nl-BE");
        private readonly Regex _spaceReplace = new Regex("[ ]{2,}", RegexOptions.None);

        private readonly ILogger<CodaConverter> _logger;
        private readonly CodaConverterConfiguration _configuration;

        public CodaConverter(
            ILogger<CodaConverter> logger,
            IOptions<CodaConverterConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
        }

        public void Start()
        {
            foreach (var mapping in _configuration.Mappings)
            {
                switch (mapping.InType)
                {
                    case StatementType.Coda:
                        foreach (var file in Directory.GetFiles(mapping.InPath, mapping.InExtension))
                        {
                            var statements = ParseCoda(file);
                            WriteCsvLines(
                                ParseCodaToIngStatements(statements),
                                mapping.OutPath,
                                file);
                        }
                        break;
                }
            }
        }

        private IEnumerable<Statement> ParseCoda(string codaFile)
        {
            _logger.LogInformation(
                "Reading {InFile}",
                codaFile);

            return _codaParser.ParseFile(codaFile);
        }

        private IEnumerable<IngStatement> ParseCodaToIngStatements(
            IEnumerable<Statement> statements)
        {
            var ingStatementList = new List<IngStatement>();

            foreach (var statement in statements)
            {
                _logger.LogInformation(
                    "Parsing statement from {Date} for {AccountName}, {AccountNumber}",
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
                        AfBij = transaction.Amount >= Decimal.Zero ? "Bij" : "Af",
                        Bedrag = Math.Abs(transaction.Amount).ToString(_dutchCulture),
                        MutatieSoort = "Diversen",
                        Mededelingen = !string.IsNullOrWhiteSpace(transaction.Message) ? _spaceReplace.Replace(transaction.Message, " ") : _spaceReplace.Replace(transaction.StructuredMessage, " ")
                    };

                    ingStatementList.Add(ingStatement);

                    _logger.LogInformation(
                        "Parsing transaction from {Date} for {Information}: {Amount}",
                        ingStatement.Datum,
                        ingStatement.Mededelingen,
                        transaction.Amount.ToString("C", _dutchCulture));
                }
            }

            return ingStatementList;
        }

        private void WriteCsvLines(
            IEnumerable<IngStatement> csvLines,
            string outPath,
            string codaFile)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = (_, __) => true,
                Delimiter = ","
            };

            configuration.AutoMap<IngStatement>();

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var targetFile = Path.Combine(
                outPath,
                Path.GetFileName(Path.ChangeExtension(codaFile, "csv")));

            using (var text = File.CreateText(targetFile))
                using (var csvWriter = new CsvWriter(text, configuration))
                    csvWriter.WriteRecords<IngStatement>(csvLines);

            _logger.LogInformation(
                "Wrote {OutFile}",
                targetFile);
        }
    }
}
