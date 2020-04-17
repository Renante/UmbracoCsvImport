using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

namespace UmbracoCsvImport.Controllers
{
    public class ToolsController : SurfaceController
    {
        private readonly IContentService contentService;
        private readonly IContentTypeService contentTypeService;

        public ToolsController(IContentService contentService, IContentTypeService contentTypeService)
        {
            this.contentService = contentService;
            this.contentTypeService = contentTypeService;
        }

        public void Cleanup()
        {
            contentService.DeleteOfType(1100);
        }
    }
}