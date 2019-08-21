using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using ExceptionHandler = ASI.Services.Http.Exceptions.ExceptionHandler;

namespace WebApiTemplate
{
    public static class ExceptionsConfig
    {
        public static void Register(HttpConfiguration config)
        {
            bool.TryParse(ConfigurationManager.AppSettings["Errors:IncludeStackTrace"], out bool includeStackTrace);

            bool.TryParse(ConfigurationManager.AppSettings["Errors:CaptureBaseException"], out bool captureBaseException);

            var logger = new ASI.Services.WebApi.ExceptionLogger();

            var handler = new ExceptionHandler(logger, includeStackTrace, captureBaseException);

            handler
                .Register<ApplicationException>(HttpStatusCode.BadRequest)
                .Register<FileNotFoundException>(HttpStatusCode.NotFound)

                .Register<ASI.Sugar.Exceptions.ConfigurationException>(HttpStatusCode.InternalServerError)
                .Register<ASI.Sugar.Exceptions.BadRequestException>(HttpStatusCode.BadRequest)
                .Register<ASI.Sugar.Exceptions.DuplicateEntityException>(HttpStatusCode.Conflict)
                .Register<ASI.Sugar.Exceptions.EntityNotFoundException>(HttpStatusCode.NotFound)
                .Register<ASI.Sugar.Exceptions.EntityValidationException>(HttpStatusCode.BadRequest)
                ;

            //var handler = new ExceptionHandler(includeStackTrace);
            config.Services.Replace(typeof(IExceptionHandler), handler);
            //config.Filters.Add(new UnhandledExceptionFilterAttribute(handler));
        }
    }
}