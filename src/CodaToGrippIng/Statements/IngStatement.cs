namespace CodaToGrippIng.Statements
{
    using CsvHelper.Configuration.Attributes;

    public class IngStatement
    {
        public string Datum { get; set; }

        [Name("Naam / Omschrijving")]
        public string Naam { get; set; }

        public string Rekening { get; set; }

        public string Tegenrekening { get; set; }

        public string Code { get; set; }

        [Name("Af Bij")]
        public string AfBij { get; set; }

        [Name("Bedrag (EUR)")]
        public string Bedrag { get; set; }

        public string MutatieSoort { get; set; }

        public string Mededelingen { get; set; }
    }
}
