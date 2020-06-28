namespace FinancialConverter.Statements
{
    using CodaParser.Statements;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using CodaParser;

    public static class CodaStatementExtensions
    {
        private static readonly Parser _codaParser = new Parser();

        public static IEnumerable<Statement> FromCoda(
            this string codaFile,
            ILogger logger)
        {
            logger.LogInformation(
                "Reading {FileType} file '{InFile}'.",
                "CODA",
                codaFile);

            return _codaParser.ParseFile(codaFile);
        }
    }
}
