using Memester.Core;
using Serilog.Core;
using Serilog.Events;

namespace Memester.Middleware
{
    public class CommonEventEnricher : ILogEventEnricher
    {
        private readonly OperationContext _operationContext;

        public CommonEventEnricher(OperationContext operationContext)
        {
            _operationContext = operationContext;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(nameof(OperationContext.SessionId), _operationContext.SessionId));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(nameof(OperationContext.Session.UserId), _operationContext.Session));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(nameof(OperationContext.CorrelationId), _operationContext.CorrelationId.ToString()));
        }
    }
}