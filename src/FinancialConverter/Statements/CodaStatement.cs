namespace FinancialConverter.Statements
{
    using System.Collections.Generic;
    using CodaParser;
    using CodaParser.Statements;
    using Microsoft.Extensions.Logging;

    public static class CodaStatementExtensions
    {
        private static readonly Parser CodaParser = new Parser();

        public static IEnumerable<Statement> FromCoda(
            this string codaFile,
            ILogger logger)
        {
            logger.LogInformation(
                "Reading {FileType} file '{InFile}'.",
                "CODA",
                codaFile);

            return CodaParser.ParseFile(codaFile);
        }
    }
}
