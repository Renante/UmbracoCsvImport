using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Umbraco.Web.WebApi;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace UmbracoCsvImport.Controllers
{

    [PluginController("Csv")]
    public class CsvApiController : UmbracoAuthorizedJsonController
    {

        [HttpPost]
        public HttpResponseMessage GetHeaders()
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;

            if (file == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "File is null");

            using (var reader = new StreamReader(file.InputStream))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    return Request.CreateResponse(HttpStatusCode.OK, csv.Context.HeaderRecord);
                }
            }
        }
    }
}