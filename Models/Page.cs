using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UmbracoCsvImport.Models
{
    public class Page
    {
        public bool AllowVaryingByCulture { get; set; }
        public List<Variant> Variants { get; set; }
    }
}