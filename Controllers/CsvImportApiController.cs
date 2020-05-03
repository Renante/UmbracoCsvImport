using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;
using System.Net;
using UmbracoCsvImport.Models;

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
                var defaultVariant = model.Page.Variants.FirstOrDefault(variant => variant.Language.IsDefault);
                var content = new Content(
                        defaultVariant?.Language.Value,
                        model.ParentId,
                        cts.Get(model.ContentTypeAlias));

                foreach (var variant in model.Page.Variants)
                {
                    if (model.Page.AllowVaryingByCulture)
                        content.SetCultureName(variant.Language.Value, variant.Language.CultureInfo);

                    if (variant.PropertyTypes != null)
                        foreach (var prop in variant.PropertyTypes)
                            if (prop.AllowVaryingByCulture)
                                content.SetValue(prop.Alias, prop.Value, culture: variant.Language.CultureInfo);
                            else
                                content.SetValue(prop.Alias, prop.Value);
                }

                if (model.Page.AllowVaryingByCulture)
                    cs.SaveAndPublish(content, defaultVariant.Language.CultureInfo);
                else
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
            var page = new Page();
            var contentType = Services.ContentTypeService.Get(contentTypeId);
            var properties = contentType.CompositionPropertyTypes;

            page.Variants = new List<Variant>();
            page.AllowVaryingByCulture = contentType.Variations.Equals(ContentVariation.Culture);

            List<ILanguage> languages;
            if (page.AllowVaryingByCulture)
                languages = Services.LocalizationService.GetAllLanguages().ToList();
            else
                languages = Services.LocalizationService.GetAllLanguages().Where(lang => lang.IsDefault).ToList();
            
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

                foreach (var prop in properties)
                {
                    var propAllowVaryingByCulture = prop.Variations.Equals(ContentVariation.Culture);

                    if (!lang.IsDefault && !propAllowVaryingByCulture)
                    { }
                    else
                    {
                        var propType = new Models.PropertyType();
                        propType.Alias = prop.Alias;
                        propType.Name = prop.Name;
                        propType.AllowVaryingByCulture = propAllowVaryingByCulture;
                        variant.PropertyTypes.Add(propType);
                    }
                }

                page.Variants.Add(variant);
            }

            return Request.CreateResponse(HttpStatusCode.OK, page);
        }
    }
}