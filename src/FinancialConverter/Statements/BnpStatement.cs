namespace FinancialConverter.Statements
{
    using CodaParser.Statements;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using System;

    public static class BnpStatementExtensions
    {
        public static IEnumerable<Statement> FromBnp(
            this string bnpFile,
            ILogger logger,
            string[] inFiles)
        {
            // BNP has a quirk where you have to check the previous file
            // to check which date was exported last.
            var previousFile = GetPreviousFile(inFiles, bnpFile);
            var latestDate = GetLatestDate(previousFile);

            logger.LogInformation(
                "Reading {FileType} file '{InFile}' from {Date}.",
                "BNP",
                bnpFile,
                latestDate.ToString("yyyy-MM-dd"));

            return ReadFile(bnpFile, latestDate);
        }

        private static string? GetPreviousFile(string[] inFiles, string file)
        {
            var previousIndex = Array.FindIndex(inFiles, 0, inFiles.Length, s => s.Equals(file));
            return previousIndex > 0 ? inFiles[previousIndex - 1] : null;
        }

        private static DateTime GetLatestDate(string? file)
        {
            if (file == null)
                return DateTime.MinValue;

            // TODO: Read csv and parse highest date

            return DateTime.UtcNow;
        }

        private static List<Statement> ReadFile(string file, DateTime from)
        {
            // TODO: Read csv and filter out anything below from

            return new List<Statement>();
        }
    }
}
