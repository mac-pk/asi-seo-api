using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using ASI.Services.Monitoring;
using ASI.Services.Statistics.Data;

namespace ASI.Services.WebApi.Controllers.Diagnostics
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("diagnostics/metrics")]
    public class MetricsController : ApiController
    {
        private readonly IQuery _store;

        public MetricsController(IQuery store)
        {
            _store = store;
        }

        [Route(""), AllowAnonymous]
        public async Task<IHttpActionResult> GetAll()
        {
            var result = (await _store.Filter<ExecutionTimeRecord>(typeof(ExecutionTimeRecord).Name, r => r.Component == HostingEnvironment.SiteName)).OrderByDescending(r => r.Observation.TimeStamp);
            return Ok(result.ToArray());
        }
    }
}
