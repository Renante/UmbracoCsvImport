using System.Collections.Generic;

namespace UmbracoCsvImport.Models
{
    public class ImportData
    {
        public string ContentTypeAlias { get; set; }
        public int ParentId { get; set; }
        public Page Page { get; set; }
    }
}