using System;
using System.Configuration;
using ASI.Services.Statistics.Data;
using ASI.Services.WebApi;
using StructureMap;

namespace WebApiTemplate
{
    public static class ServiceConfig
    {
        public static void RegisterServices(ConfigurationExpression config)
        {
            config.AddRegistry(new MetricsRegistry());
        }
    }
}