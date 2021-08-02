namespace FinancialConverter
{
    public enum InputStatementType
    {
        CODA,
        BNP,
        Curve,
        Argenta
    }
    public enum OutputStatementType
    {
        ING,
        YNAB,
        SWIFT,
    }

    public class CodaConverterConfiguration
    {
        public const string ConfigurationPath = "CodaConverter";

        public CodaConverterConfigurationMapping[] Mappings { get; set; }
    }

    public class CodaConverterConfigurationMapping
    {
        public InputStatementType? InType { get; set; }
        public string? InPath { get; set; }
        public string? InExtension { get; set; }

        public OutputStatementType? OutType { get; set; }
        public string? OutPath { get; set; }
    }
}
