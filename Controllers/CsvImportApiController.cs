using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using System.Net;
using UmbracoCsvImport.Models;
using UmbracoModels = Umbraco.Core.Models;

namespace UmbracoCsvImport.Controllers
{
    public class CsvImportApiController : UmbracoAuthorizedApiController
    {
        [HttpPost]
        public HttpResponseMessage Publish(ImportData model)
        {
            var cs = Services.ContentService;
            var cts = Services.ContentTypeService;

            try
            {
                var content = new Content(
                        model.Variants.FirstOrDefault(v => v.Language.IsDefault)?.Language.Value,
                        model.ParentId,
                        cts.Get(model.ContentTypeAlias));

                foreach (var variant in model.Variants)
                {
                    content.SetCultureName(variant.Language.Value, variant.Language.CultureInfo);

                    if (variant.PropertyTypes != null)
                    {
                        foreach (var prop in variant.PropertyTypes)
                        {
                            if (prop.Variations.Equals(ContentVariation.Nothing))
                            {
                                content.SetValue(prop.Alias, prop.Value);
                            }
                            else
                            {
                                content.SetValue(prop.Alias, prop.Value, culture: variant.Language.CultureInfo);
                            }
                        }
                    }
                }

                cs.SaveAndPublish(content);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetModel(int contentTypeId)
        {
            var variants = new List<Variant>();
            var languages = Services.LocalizationService.GetAllLanguages();
            var contentType = Services.ContentTypeService.Get(contentTypeId);
            var properties = contentType.CompositionPropertyTypes;

            foreach (var lang in languages)
            {
                var language = new Models.Language()
                {
                    CultureName = lang.CultureName,
                    IsDefault = lang.IsDefault,
                    CultureInfo = lang.CultureInfo.ToString()
                };

                var variant = new Variant()
                {
                    Language = language,
                    PropertyTypes = new List<Models.PropertyType>()
                };

                foreach(var prop in properties)
                {
                    if(!(!lang.IsDefault && prop.Variations.Equals(ContentVariation.Nothing))) {
                        var propType = new Models.PropertyType();
                        propType.Alias = prop.Alias;
                        propType.Name = prop.Name;
                        propType.Variations = prop.Variations;
                        variant.PropertyTypes.Add(propType);
                    }
                }

                variants.Add(variant);
            }

            return Request.CreateResponse(HttpStatusCode.OK, variants);
        }
    }
}