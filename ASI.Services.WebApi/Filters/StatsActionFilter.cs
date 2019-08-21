using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ASI.Services.Http;
using ASI.Services.Monitoring;
using ASI.Services.Security;
using ASI.Services.Statistics.Data;
using ASI.Services.WebApi.Controllers.Diagnostics;

namespace ASI.Services.WebApi.Filters
{
    public class StatsActionFilter : ActionFilterAttribute
    {
        private const string RecordKey = "__asi_statsrecord";

        private static readonly Lazy<string> ServerIp = new Lazy<string>(() => Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString(), true);

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.ActionContext.ControllerContext.Controller is HealthController || actionExecutedContext.ActionContext.ControllerContext.Controller is MetricsController) return;
            var record = (actionExecutedContext.Request.Properties.ContainsKey(RecordKey) ? actionExecutedContext.Request.Properties[RecordKey] : null) as ExecutionTimeRecord;
            if (record != null)
            {
                record.ResponseCode = ((int)actionExecutedContext.Response.StatusCode).ToString();
                record.IsSuccessfulResponse = actionExecutedContext.Response.IsSuccessStatusCode;
                record.Stop();
            }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ControllerContext.Controller is HealthController || actionContext.ControllerContext.Controller is MetricsController) return;

            var collector = Collector.Current;
            if (collector == null) return;
            var record = collector.Append<ExecutionTimeRecord>();
            record.Url = actionContext.Request.RequestUri.ToString();
            record.Method = actionContext.Request.Method.ToString();
            record.Machine = Environment.MachineName;
            record.ClientIp = actionContext.Request.GetClientIpAddress();
            record.ServerIp = ServerIp.Value;
            if (ClaimsPrincipal.Current.Identity is AuthenticatedUser user)
            {
                record.UserId = user.UserId.ToString();
                record.UserName = user.Name;
                if (user.Token != null)
                {
                    record.UserSessionId = user.Token.Value;
                }
            }

            record.OperationId = GetParentOperationId(actionContext.Request);
            record.UserAgent = actionContext.Request.Headers.UserAgent.ToString();
            
            var applicationName = ConfigurationManager.AppSettings["ASI:ApplicationName"];
            if (string.IsNullOrEmpty(applicationName))
                throw new Exception("Site not configured, must set ASI:ApplicationName in web.config");
            record.Application = applicationName;
            record.Component = HostingEnvironment.SiteName;

            record.Start();
            actionContext.Request.Properties.Add(RecordKey, record);
        }

        private static string GetParentOperationId(HttpRequestMessage request)
        {
            if (TryExtractHeader(request, "traceparent", out var operationId))
            {
                return operationId;
            }

            if (TryExtractHeader(request, "Request-Id", out operationId))
            {
                return operationId;
            }

            if (TryExtractHeader(request, "x-ms-request-id", out operationId))
            {
                return operationId;
            }

            if (TryExtractHeader(request, "x-ms-request-root-id", out operationId))
            {
                return operationId;
            }

            return null;
        }

        private static bool TryExtractHeader(HttpRequestMessage request, string header, out string value)
        {
            value = null;
            if (request.Headers.TryGetValues("Request-Id", out var values) && values != null)
            {
                var temp = values.FirstOrDefault();
                if (temp != null)
                {
                    value = temp;
                    return true;
                }
            }
            return false;
        }
    }
}