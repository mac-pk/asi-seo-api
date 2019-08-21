using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace ASI.Services.WebApi
{
    public class CentralizedPrefixProvider : DefaultDirectRouteProvider
    {
        private readonly string _centralizedPrefix;

        public CentralizedPrefixProvider(string centralizedPrefix)
        {
            _centralizedPrefix = (centralizedPrefix ?? "").Trim('/');
        }

        protected override string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor)
        {
            var existingPrefix = base.GetRoutePrefix(controllerDescriptor);

            if (string.IsNullOrEmpty(_centralizedPrefix)) return existingPrefix;

            if (existingPrefix == null) return _centralizedPrefix;
            
            var prefixParts = existingPrefix.Split('/');
            if (prefixParts.Contains(_centralizedPrefix, StringComparer.OrdinalIgnoreCase))
            {
                return existingPrefix;
            }

            return $"{_centralizedPrefix}/{existingPrefix}";
        }
    }
}
