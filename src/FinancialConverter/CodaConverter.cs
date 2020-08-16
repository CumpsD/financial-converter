namespace FinancialConverter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Statements;

    public class CodaConverter
    {
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

                var inFiles = Directory.GetFiles(mapping.InPath, mapping.InExtension);
                Array.Sort(inFiles);

                foreach (var file in inFiles)
                {
                    var statements = mapping.InType switch
                    {
                        InputStatementType.CODA => file.FromCoda(_logger),
                        InputStatementType.BNP => file.FromBnp(_logger, inFiles),
                        InputStatementType.Curve => file.FromCurve(_logger)
                    };

                    switch (mapping.OutType)
                    {
                        case OutputStatementType.ING:
                            WriteCsvLines(
                                statements.ToIng(_logger),
                                "ING",
                                mapping.OutPath,
                                file);
                            break;

                        case OutputStatementType.YNAB:
                            WriteCsvLines(
                                statements.ToYnab(_logger),
                                "YNAB",
                                mapping.OutPath,
                                file);
                            break;

                        case OutputStatementType.SWIFT:
                            WriteSwiftLines(
                                statements.ToSwift(_logger),
                                "SWIFT",
                                mapping.OutPath,
                                file);
                            break;
                    }
                }
            }
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

            if (File.Exists(targetFile))
            {
                _logger.LogInformation(
                    "File already exists. Skipping {FileType} file '{OutFile}'.",
                    fileType,
                    targetFile);

                return;
            }

            using (var text = File.CreateText(targetFile))
                using (var csvWriter = new CsvWriter(text, configuration))
                    csvWriter.WriteRecords(csvLines);

            _logger.LogInformation(
                "Wrote {FileType} file '{OutFile}'.",
                fileType,
                targetFile);
        }

        private void WriteSwiftLines(
            IEnumerable<SwiftStatement> swiftLines,
            string fileType,
            string outPath,
            string originalFile)
        {
            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var targetFile = Path.Combine(
                outPath,
                Path.GetFileName(Path.ChangeExtension(originalFile, "940")));

            if (File.Exists(targetFile))
            {
                _logger.LogInformation(
                    "File already exists. Skipping {FileType} file '{OutFile}'.",
                    fileType,
                    targetFile);

                return;
            }

            using (var text = File.CreateText(targetFile))
            {
                var swiftWriter = new SwiftWriter(text);
                swiftWriter.WriteRecords(swiftLines);
            }

            _logger.LogInformation(
                "Wrote {FileType} file '{OutFile}'.",
                fileType,
                targetFile);
        }
    }
}
