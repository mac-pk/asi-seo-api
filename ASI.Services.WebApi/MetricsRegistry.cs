using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ASI.Services.Monitoring;
using ASI.Services.Statistics;
using ASI.Services.Statistics.Data;
using ASI.Sugar.Data.Queries;
using StructureMap;

namespace ASI.Services.WebApi
{
    public class MetricsRegistry : Registry
    {
        private static readonly Lazy<InMemoryRecordStore> InMemoryRecordStoreFactory = new Lazy<InMemoryRecordStore>(CreateInMemoryRecordStore, true);
        private static readonly Lazy<MultiplexingCollector> PersistentCollectorFactory = new Lazy<MultiplexingCollector>(CreatePersistentCollectorFactory, true);

        public MetricsRegistry(bool persistent = true)
        {
            if (persistent)
            {
                ForSingletonOf<ICollector>().Use(context => PersistentCollectorFactory.Value);
                ForSingletonOf<IQuery>().Use(context => InMemoryRecordStoreFactory.Value);
            }
            else
            {
                ForSingletonOf<ICollector>().Use(context => InMemoryRecordStoreFactory.Value);
                ForSingletonOf<IQuery>().Use(context => InMemoryRecordStoreFactory.Value);
            }
        }

        private static InMemoryRecordStore CreateInMemoryRecordStore()
        {
            if (!uint.TryParse(ConfigurationManager.AppSettings["Metrics:AbsoluteExpiration"], out var expiration) || expiration < 600000)
            {
                expiration = 3600000;
            }
            return new InMemoryRecordStore(expiration);
        }

        private static MultiplexingCollector CreatePersistentCollectorFactory()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["Metrics:RetentionPolicy"], out var minutes) || minutes < 1)
            {
                minutes = 1440;
            }

            int.TryParse(ConfigurationManager.AppSettings["Metrics:BufferSize"], out var bufferSize);

            if (bufferSize < 10 || bufferSize > 1000)
            {
                bufferSize = 100;
            }

            return new MultiplexingCollector(new BufferingCollector(new PerformanceMetricsCollector
            {
                InstrumentationKey = ConfigurationManager.AppSettings["Metrics:ApplicationInsights:InstrumentationKey"],
                RetentionMinutes = minutes
            }, bufferSize), InMemoryRecordStoreFactory.Value);
        }

        private class EmptyQuery : IQuery
        {
            public void Dispose()
            {

            }

            public Task<IEnumerable<T>> Read<T>(string query) where T : class, IRecord, new()
            {
                return Task.FromResult(Enumerable.Empty<T>());
            }

            public Task<IEnumerable<T>> Filter<T>(string measurement, Expression<Func<T, bool>> criteria, IQueryConstraints<T> constraints = null) where T : class, IRecord, new()
            {
                return Task.FromResult(Enumerable.Empty<T>());
            }

            public Task<long> Count<T>(string measurement, Expression<Func<T, bool>> criteria, IQueryConstraints<T> constraints = null) where T : class, IRecord, new()
            {
                return Task.FromResult(0L);
            }

            public Task<IEnumerable<TAggregate>> Group<T, TAggregate>(string measurement, Expression<Func<T, bool>> criteria, IQueryConstraints<T> constraints) where T : class, IRecord, new() where TAggregate : class, IRecord, new()
            {
                return Task.FromResult(Enumerable.Empty<TAggregate>());
            }
        }

    }
}
