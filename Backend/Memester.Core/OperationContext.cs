using System;
using Memester.Models;

namespace Memester.Core
{
    public class OperationContext
    {
        public Guid CorrelationId { get; } = Guid.NewGuid();
        public Session Session { get; set; } = null!;
        public string SessionId { get; set; } = null!;
        public string ClientIp { get; set; } = null!;
    }
}