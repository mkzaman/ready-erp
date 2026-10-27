using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Dtos;
using ReadyErp.Api.Infrastructure;
using ReadyErp.Api.Services;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;

    public AuthController(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password" });

        var token = _tokens.Create(user);
        return Ok(new LoginResponse(token, user.Username, user.Role));
    }
}
