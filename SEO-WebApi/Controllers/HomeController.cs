using System.Threading.Tasks;
using Microsoft.Web.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using ASI.Services.Monitoring;
using ASI.Services.Statistics.Data;

namespace WebApi.Controllers
{
    [EnableCors("*", "*", "*")]
    [ApiVersion("1.0")]
    [RoutePrefix("api")]
    public class HomeController : ApiController
    {
        // GET: api/home
        // GET: api/v1/home
        [Route("home")]
        [Route("v{version:apiVersion}/home")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Get()
        {
            await Task.Delay(1000);
            var result = new[] { "value1", "value2" };
            return Ok(result);
        }

        // GET: api/home/5
        // GET: api/v1/home/5
        [Route("home/{id}")]
        [Route("v{version:apiVersion}/home/{id}")]
        [AllowAnonymous]
        public IHttpActionResult Get(int id)
        {
            var result = "value";
            return Ok(result);
        }

        // POST: api/home
        // POST: api/v1/home
        [Route("home")]
        [Route("v{version:apiVersion}/home")]
        [Authorize(Roles = "Administrators")]
        public IHttpActionResult Post([FromBody]string value)
        {
            return Ok();
        }

        // PUT: api/home/5
        // PUT: api/v1/home/5
        [Route("home/{id}")]
        [Route("v{version:apiVersion}/home/{id}")]
        [Authorize(Roles = "Administrators")]
        public IHttpActionResult Put(int id, [FromBody] string value)
        {
            return Ok();
        }

        // DELETE: api/home/5
        // DELETE: api/v1/home/5
        [Route("home/{id}")]
        [Route("v{version:apiVersion}/home/{id}")]
        [Authorize]
        public IHttpActionResult Delete(int id)
        {
            return Ok();
        }
    }
}
