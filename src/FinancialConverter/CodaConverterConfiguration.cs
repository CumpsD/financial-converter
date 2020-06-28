namespace FinancialConverter
{
    public enum StatementType
    {
        Coda,
        ING,
        YNAB,
    }

    public class CodaConverterConfiguration
    {
        public const string ConfigurationPath = "CodaConverter";

        public CodaConverterConfigurationMapping[] Mappings { get; set; }
    }

    public class CodaConverterConfigurationMapping
    {
        public StatementType? InType { get; set; }
        public string? InPath { get; set; }
        public string? InExtension { get; set; }

        public StatementType? OutType { get; set; }
        public string? OutPath { get; set; }
    }
}
