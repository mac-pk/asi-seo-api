using ASI.Services.Messaging;
using ASI.Services.Monitoring;
using ASI.Services.Statistics.Data;
using ASI.Services.WebApi;
using log4net.Config;
using StructureMap;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebApi.StructureMap;

namespace WebApiTemplate
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Initialize log4net 
            InitializeLogging();

            // Initialize DI
            
            // Using implicit container creation
            //GlobalConfiguration.Configuration.UseStructureMap(ServiceConfig.RegisterServices);
            
            // Using explicit container creation
            var container = CreateContainer();
            GlobalConfiguration.Configuration.UseStructureMap(container);

            // Web API configuration
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Enable (unhandled) exception handling
            GlobalConfiguration.Configure(ExceptionsConfig.Register);

            // Enable stats and health checks
            GlobalConfiguration.Configure(config => StatsConfig.Register(config, GetApplicationHealthChecks));
        }

        protected void Application_End()
        {
            // Cleanup ESB connections
            PonyEsb.CloseAll();
        }

        protected void Application_Disposed()
        {
            // Cleanup ESB connections
            PonyEsb.CloseAll();
        }

        private static void InitializeLogging()
        {
            var log4NetConfig = System.Web.Hosting.HostingEnvironment.MapPath("~/log4net.config");
            if (log4NetConfig != null && File.Exists(log4NetConfig))
            {
                XmlConfigurator.Configure(new FileInfo(log4NetConfig));
            }
        }

        private static IEnumerable<IHealthCheck> GetApplicationHealthChecks(IQuery metrics)
        {
            return Enumerable.Empty<IHealthCheck>();
        }

        private static IContainer CreateContainer()
        {
            var container = new Container(config => { config.AddRegistry(new MetricsRegistry()); });
            return container;
        }
    }
}
