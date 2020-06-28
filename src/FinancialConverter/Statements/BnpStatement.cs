namespace FinancialConverter.Statements
{
    using CodaParser.Statements;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static class BnpStatementExtensions
    {
        public static IEnumerable<Statement> FromBnp(
            this string bnpFile,
            ILogger logger)
        {
            logger.LogInformation(
                "Reading {FileType} file '{InFile}'.",
                "BNP",
                bnpFile);

            // TODO: Implement
            // BNP has a quirk where you have to check the previous file
            // to check which date was exported last.

            return new List<Statement>();
        }
    }
}
