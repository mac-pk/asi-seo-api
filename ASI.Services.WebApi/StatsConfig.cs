using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ASI.Services.Monitoring;
using ASI.Services.Statistics;
using ASI.Services.Statistics.Data;
using ASI.Services.Statistics.Http;
using ASI.Services.WebApi.Filters;
using WebApi.StructureMap;

namespace ASI.Services.WebApi
{
    public static class StatsConfig
    {
        public static void Register(HttpConfiguration config, Func<IQuery, IEnumerable<IHealthCheck>> healthChecksFactory = null)
        {
            var store = config.DependencyResolver.GetService<IQuery>();
            var collector = config.DependencyResolver.GetService<ICollector>();

            // Enable stats collection
            var mapper = new StatsCollectorMapper(collector);
            config.MessageHandlers.Add(collector != null ? new StatsHandler(mapper, collector) : new StatsHandler(mapper));
            config.Filters.Add(new StatsActionFilter());

            // Enable health checks
            IEnumerable<IHealthCheck> healthChecks = null;
            if (healthChecksFactory != null)
            {
                healthChecks = healthChecksFactory(store);
            }

            HealthCheckConfig.Register(config, store, healthChecks);
        }

        private class StatsCollectorMapper : ICollectorMapper
        {
            private readonly ICollector _executionTimeRecordCollector;

            public StatsCollectorMapper(ICollector executionTimeRecordCollector)
            {
                _executionTimeRecordCollector = executionTimeRecordCollector;
            }

            public IDictionary<ICollector, ICollection<IRecord>> Map(IEnumerable<ICollector> collectors, ICollection<IRecord> records)
            {
                return collectors.ToDictionary(c => c, c => Map(c, records));
            }

            private ICollection<IRecord> Map(ICollector collector, IEnumerable<IRecord> records)
            {
                return _executionTimeRecordCollector != null && _executionTimeRecordCollector == collector ? records.OfType<ExecutionTimeRecord>().Cast<IRecord>().ToArray() : records.Where(r => !(r is ExecutionTimeRecord)).ToArray();
            }
        }
    }
}