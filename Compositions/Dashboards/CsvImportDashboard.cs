using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Dashboards;

namespace UmbracoFormToFirebase.Compositions.Dashboards
{
    public class CsvImportDashboard : IDashboard
    {
        public string[] Sections => new[]
        {
            Umbraco.Core.Constants.Applications.Settings
        };

        public IAccessRule[] AccessRules
        {
            get
            {
                var rules = new IAccessRule[]
                {
                    new AccessRule {Type = AccessRuleType.Grant, Value = Umbraco.Core.Constants.Security.AdminGroupAlias}
                };
                return rules;
            }
        }

        public string Alias => "csvImportDashboard";

        public string View => "/App_Plugins/CsvImport/csvimport.html";
    }
}