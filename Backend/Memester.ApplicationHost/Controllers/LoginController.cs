using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Memester.Filters;
using Memester.Services;
using Microsoft.AspNetCore.Mvc;

namespace Memester.Controllers
{
    [Route("api/authentication"), ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly IHttpSessionService _httpSessionService;

        public LoginController(AuthenticationService authenticationService, IHttpSessionService httpSessionService)
        {
            _authenticationService = authenticationService;
            _httpSessionService = httpSessionService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(
            [FromForm(Name = "email"), EmailAddress, RegularExpression("[a-z]{2,5}@netcompany\\.com$", ErrorMessage = "Email domain not supported")] string email, 
            [FromHeader(Name = "User-Agent")] string userAgent)
        { 
            await _authenticationService.SendLoginMail(email, userAgent);
            return Ok();
        }
        
        [HttpGet("login/{token}")]
        public async Task<ActionResult> PerformLogin([FromRoute, Required] string token, [FromHeader(Name = "User-Agent")] string userAgent)
        {
            var session = await _authenticationService.Login(token, userAgent);
            if (session == default) return BadRequest("Invalid credentials");
            _httpSessionService.AttachSessionToken(HttpContext.Response, token, session.Expiry);
            return Redirect("/");
        }
        
        [HttpPost("logout")]
        [SessionAuthentication]
        public async Task<ActionResult> Logout()
        {
            await _authenticationService.Logout();
            _httpSessionService.AttachSessionToken(HttpContext.Response, null);
            return Ok();
        }
        
        [HttpGet("verify")]
        [SessionAuthentication]
        public ActionResult Verify() => Ok();
    }
}