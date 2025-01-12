using Metheo.BL;
using Metheo.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Metheo.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthBusinessLogic _authBusinessLogic;

        public AuthController(IAuthBusinessLogic authBusinessLogic)
        {
            _authBusinessLogic = authBusinessLogic;
        }
        

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var token = await _authBusinessLogic.LoginAsync(loginRequest);

            if (token == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(new { Token = token });
        }
    }
}