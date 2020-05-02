using System.Collections.Generic;

namespace UmbracoCsvImport.Models
{
    public class ImportData
    {
        public string ContentTypeAlias { get; set; }
        public int ParentId { get; set; }
        public IEnumerable<Variant> Variants { get; set; }
    }
}