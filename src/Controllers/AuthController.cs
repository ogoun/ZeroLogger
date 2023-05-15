using APILogger.Models;
using APILogger.Models.Requests;
using APILogger.Services;
using Microsoft.AspNetCore.Mvc;

namespace APILogger.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("/api/auth")]
        public ActionResult<string> Auth([FromBody] AuthRequest request)
        {
            try
            {
                var token = AuthProvider.Auth(request.Username, request.Password, request.Source);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/api/user")]
        public ActionResult CreateUser(UserRequest user) 
        {
            user.ValidateModel();
            AuthProvider.CreateUseer(user);
            return Ok();
        }

        [HttpGet("/api/user")]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            if (HttpContext.Items["IsAdmin"]?.Equals(true) ?? false)
            {
                return Ok(AuthProvider.GetUsers());
            }
            return Unauthorized();
        }

        [HttpDelete("/api/user/{id}")]
        public ActionResult BlockUser([FromRoute]long id)
        {
            if (HttpContext.Items["IsAdmin"]?.Equals(true) ?? false)
            {
                AuthProvider.BlockUser(id);
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPut("/api/user/{id}")]
        public ActionResult UnblockUser([FromRoute] long id)
        {
            if (HttpContext.Items["IsAdmin"]?.Equals(true) ?? false)
            {
                AuthProvider.UnblockUser(id);
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPut("/api/user/source")]
        public ActionResult AllowSource([FromBody] SourceRequest request)
        {
            if (HttpContext.Items["IsAdmin"]?.Equals(true) ?? false)
            {
                AuthProvider.AllowSource(request.Id, request.Source);
                return Ok();
            }
            return Unauthorized();
        }

        [HttpDelete("/api/user/source")]
        public ActionResult DenySource([FromBody] SourceRequest request)
        {
            if (HttpContext.Items["IsAdmin"]?.Equals(true) ?? false)
            {
                AuthProvider.DenySource(request.Id, request.Source);
                return Ok();
            }
            return Unauthorized();
        }
    }
}
