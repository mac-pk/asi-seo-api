using System.Web.Http;
using System.Web.Http.Cors;
using ASI.Services.Monitoring;

namespace ASI.Services.WebApi.Controllers.Diagnostics
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("diagnostics/health")]
    public class HealthController : ApiController
    {
        [Route(""), AllowAnonymous]
        public IHttpActionResult Get()
        {
            return Ok(HealthChecks.GetStatus());
        }
    }
}
