using ASI.Services.Http;
using ASI.Services.Http.Compression;
using ASI.Services.Http.Logging;
using ASI.Services.Http.Security;
using ASI.Services.Http.Throttling;
using Microsoft.Web.Http.Routing;
using System;
using System.Web.Http;
using System.Web.Http.Routing;
using ASI.Services.WebApi;
using Newtonsoft.Json.Serialization;

namespace WebApiTemplate
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //// Web API routes without versioning
            //config.MapHttpAttributeRoutes();

            // Web API routes with versioning
            var constraintResolver = new DefaultInlineConstraintResolver
            {
                ConstraintMap =
                {
                    ["apiVersion"] = typeof(ApiVersionRouteConstraint)
                }
            };
            config.MapHttpAttributeRoutes(constraintResolver, new CentralizedPrefixProvider("api"));
            config.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            // Remove XML formatter
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new DefaultContractResolver();
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Enable CORS
            config.EnableCors();

            // Enable compression
            config.MessageHandlers.Insert(0, new CompressionHandler());

            // Enable logging
            config.MessageHandlers.Add(new LoggingHandler(CreateLoggingPolicy()));
            
            // Enable throttling
            config.MessageHandlers.Add(new ThrottlingHandler(new InMemoryThrottleStore(), CreateThrottlePolicy()));
            
            // Expose correlation id to the client
            config.MessageHandlers.Add(new CorrelationIdHandler());
            
            // Enable OAuth2 token validation
            config.EnableJwt(anonymousRoutes: new[] { "/swagger/docs/v1", "/swagger", "/swagger/ui*" });
            //config.EnableImplicitGrant(anonymousRoutes: new[] { "/swagger/docs/v1", "/swagger", "/swagger/ui*" });
        }

        private static LoggingPolicy CreateLoggingPolicy()
        {
            return new LoggingPolicy
            {
                LogRequestContent = false,
                LogResponseContent = false,
                LogSingleEntry = true,
                EnableLogging = true,
                //EndPoints = new [] { "/api/v1/home", "/api/home"},
                //HttpMethods = new[] { HttpMethod.Get, HttpMethod.Post }
            };
        }
        
        //TODO: Define your throttling policy
        private static ThrottlePolicy CreateThrottlePolicy()
        {
            var policy = new ThrottlePolicy
            {
                //enables throttling by IP
                IpThrottling = true,
                //enables throttling by UserId/CompanyId if user is authenticated
                ClientThrottling = true,
                //enables throttling by URL endpoint
                EndPointThrottling = false
            };

            //overrides default rate limit for a given IP pattern
            policy.IpThrottlingRateLimits["10.0.*.*"] = new RateLimit { Period = TimeSpan.FromHours(1), Value = 5000 };

            //// override default rate limit for a given authenticated client
            //policy.ClientThrottlingRateLimits["<CompanyId>:<UserId>"] = new RateLimit { Period = TimeSpan.FromHours(1), Value = 5000 };

            //white list following IP addresses
            policy.IpWhiteList.Add("10.0.*.10");
            policy.IpWhiteList.Add("10.0.*.11");
            policy.IpWhiteList.Add("10.0.*.12");

            return policy;
        }
    }
}
