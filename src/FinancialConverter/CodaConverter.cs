namespace FinancialConverter
{
    using CodaParser;
    using CodaParser.Statements;
    using FinancialConverter.Statements;
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
            {
                if (!Directory.Exists(mapping.InPath))
                {
                    _logger.LogError("Directory {InPath} does not exist.", mapping.InPath);
                    continue;
                }

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
                                "ING",
                                mapping.OutPath,
                                file);
                            break;

                        case StatementType.YNAB:
                            WriteCsvLines(
                                statements.ToYnab(_logger),
                                "YNAB",
                                mapping.OutPath,
                                file);
                            break;
                    }
                }
            }
        }

        private IEnumerable<Statement> ParseCoda(string codaFile)
        {
            _logger.LogInformation(
                "Reading {FileType} file '{InFile}'.",
                "CODA",
                codaFile);

            return _codaParser.ParseFile(codaFile);
        }

        private void WriteCsvLines<T>(
            IEnumerable<T> csvLines,
            string fileType,
            string outPath,
            string originalFile)
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
                Path.GetFileName(Path.ChangeExtension(originalFile, "csv")));

            using (var text = File.CreateText(targetFile))
                using (var csvWriter = new CsvWriter(text, configuration))
                    csvWriter.WriteRecords<T>(csvLines);

            _logger.LogInformation(
                "Wrote {FileType} file '{OutFile}'.",
                fileType,
                targetFile);
        }
    }
}
