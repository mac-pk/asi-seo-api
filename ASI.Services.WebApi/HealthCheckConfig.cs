using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using ASI.Services.Monitoring;
using ASI.Services.Statistics.Data;
using HostingEnvironment = System.Web.Hosting.HostingEnvironment;

namespace ASI.Services.WebApi
{
    public static class HealthCheckConfig
    {
        public static void Register(HttpConfiguration config, IQuery store, IEnumerable<IHealthCheck> healthChecks = null)
        {
            HealthChecks.SetSiteName(HostingEnvironment.SiteName);
            HealthChecks.SetApplication(ConfigurationManager.AppSettings["ASI:ApplicationName"] ?? HostingEnvironment.ApplicationVirtualPath);
            var checks = healthChecks as IHealthCheck[] ?? healthChecks?.ToArray() ?? new IHealthCheck[] { };
            if (checks.OfType<PerformanceHealthCheck>().Any())
            {
                HealthChecks.RegisterHealthCheck(new PerformanceHealthCheck("Performance", store));
            }

            for (var i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
                HealthChecks.RegisterHealthCheck(new DatabaseConnectivityHealthCheck(ConfigurationManager.ConnectionStrings[i]));

            if (healthChecks != null)
            {
                foreach (var check in checks)
                {
                    HealthChecks.RegisterHealthCheck(check);
                }
            }

            var interval = GetPollingInterval();

            HealthChecks.Poll((int)TimeSpan.FromMinutes(interval).TotalMilliseconds);
        }

        private static int GetPollingInterval()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["HealthCheck:PollingInterval"], out var interval) || interval <= 0)
            {
                interval = 5;
            }
            return interval;
        }
    }
}