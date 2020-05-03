namespace UmbracoCsvImport.Models
{
    public class Language
    {
        public string CultureInfo { get; set; }
        public string CultureName { get; set; }
        public string CsvHeader { get; set; }
        public bool IsDefault { get; set; }
        public string Value { get; set; }
    }
}