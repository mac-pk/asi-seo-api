using System;
using log4net;

namespace ASI.Services.WebApi
{
    public class ExceptionLogger : ASI.Services.Http.Exceptions.IExceptionLogger
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ExceptionLogger));

        public void Capture(Exception exception)
        {
            if (exception == null) return;
            _logger.Error(exception.GetBaseException());
        }

        public string Translate(Exception exception)
        {
            return exception?.GetBaseException().Message;
        }
    }
}