using Microsoft.AspNetCore.Mvc;

namespace Memester.Filters
{
    public class SessionAuthenticationAttribute : TypeFilterAttribute
    {
        public SessionAuthenticationAttribute() : base(typeof(SessionAuthenticationFilter)) { }
    }
}