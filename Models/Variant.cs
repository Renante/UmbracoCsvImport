using System.Collections.Generic;

namespace UmbracoCsvImport.Models
{
    public class Variant
    {
        public Language Language { get; set; }
        public List<PropertyType> PropertyTypes { get; set; }
    }
}