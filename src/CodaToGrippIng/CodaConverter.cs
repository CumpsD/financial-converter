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
            foreach (var file in Directory.GetFiles(mapping.InPath, mapping.InExtension))
            {
                var statements = mapping.InType switch
                {
                    StatementType.Coda => ParseCoda(file)
                };

                switch (mapping.OutType)
                {
                    case StatementType.ING:
                        WriteCsvLines(
                            statements.ToIng(_logger),
                            mapping.OutPath,
                            file);
                        break;

                    case StatementType.YNAB:
                        WriteCsvLines(
                            statements.ToYnab(_logger),
                            mapping.OutPath,
                            file);
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

        private void WriteCsvLines<T>(
            IEnumerable<T> csvLines,
            string outPath,
            string codaFile)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = (_, __) => true,
                Delimiter = ","
            };

            configuration.AutoMap<T>();

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var targetFile = Path.Combine(
                outPath,
                Path.GetFileName(Path.ChangeExtension(codaFile, "csv")));

            using (var text = File.CreateText(targetFile))
                using (var csvWriter = new CsvWriter(text, configuration))
                    csvWriter.WriteRecords<T>(csvLines);

            _logger.LogInformation(
                "Wrote {OutFile}",
                targetFile);
        }
    }
}
