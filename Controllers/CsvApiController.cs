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
using Umbraco.Core.Services;
using Umbraco.Core.Logging;
using Umbraco.Core;

namespace UmbracoCsvImport.Controllers
{

    [PluginController("Csv")]
    public class CsvApiController : UmbracoAuthorizedJsonController
    {

        const string csvPath = "~/App_Data/csvimport/";

        private readonly IContentTypeService contentTypeService;
        private readonly IDataTypeService dataTypeService;
        private readonly IContentService contentService;
        private readonly ILogger logger;

        public CsvApiController(IContentTypeService contentTypeService,
            IContentService contentService,
            IDataTypeService dataTypeService,
            ILogger logger)
        {
            this.contentTypeService = contentTypeService;
            this.contentService = contentService;
            this.dataTypeService = dataTypeService;
            this.logger = logger;
        }

        [HttpPost]
        public HttpResponseMessage GetHeaders()
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;

            if (file == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "File is null");

            var csvAbsolutePath = HttpContext.Current.Server.MapPath(csvPath);

            if (!Directory.Exists(csvAbsolutePath))
                Directory.CreateDirectory(csvAbsolutePath);
            
            var csvFileName = $"{csvAbsolutePath}/file.csv";

            if (File.Exists(csvFileName))
                File.Delete(csvFileName);
            
            file.SaveAs(csvFileName);

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

        [HttpPost]
        public HttpResponseMessage Process(Data data)
        {
            int totalItems = 0;
            var contentType = contentTypeService.Get(data.ContentTypeId);
            
            try
            {
                using (var reader = new StreamReader($"{HttpContext.Current.Server.MapPath(csvPath)}/file.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var name = csv.GetField(data.Fields.FirstOrDefault(field => field.PropertyTypeAlias.Equals("__name")).Header);
                        var content = contentService.Create(!string.IsNullOrEmpty(name) ? name.Truncate(250,"") : "Unnamed", data.ParentId, contentType.Alias);
                        foreach (var field in data.Fields.Where(field => !field.PropertyTypeAlias.Equals("__name") && field.Header != null))
                        {
                            var fieldValue = csv.GetField(field.Header);
                            if (!string.IsNullOrEmpty(fieldValue))
                            {
                                switch (field.Editor)
                                {
                                    case "Umbraco.TextBox":
                                        fieldValue = fieldValue.Truncate(250, "");
                                        break;
                                }

                                content.SetValue(field.PropertyTypeAlias, fieldValue);
                            }
                        }

                        contentService.SaveAndPublish(content);
                        totalItems++;
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, totalItems);
            }
            
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }


    public class Data
    {
        public int ParentId { get; set; }
        public int ContentTypeId { get; set; }
        public Field[] Fields { get; set; }
    }

    public class Field
    {
        public string PropertyTypeAlias { get; set; }
        public string Editor { get; set; }
        public string Header { get; set; }
    }


}