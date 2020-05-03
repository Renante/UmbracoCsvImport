using Umbraco.Core.Models;

namespace UmbracoCsvImport.Models
{
    public class PropertyType
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string CsvHeader { get; set; }
        public string Value { get; set; }
        public bool AllowVaryingByCulture { get; set; }
    }
}