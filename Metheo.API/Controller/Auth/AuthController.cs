using Metheo.BL;
using Metheo.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ApiDotnetMetheoOrm.Controller.Auth;

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
    public async Task<IActionResult> Login([FromBody] LoginRequest? loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) ||
            string.IsNullOrEmpty(loginRequest.Password)) return BadRequest();

        var token = await _authBusinessLogic.LoginAsync(loginRequest);

        return Ok(new { Token = token });
    }
}