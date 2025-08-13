using BookLibwithSub.API.Contracts;
using BookLibwithSub.API.Security;
using BookLibwithSub.Repo;
using BookLibwithSub.Repo.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibwithSub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;

    public AuthController(AppDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    // -------------------------
    // POST /api/auth/login
    // -------------------------
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password are required.");

        var acc = await _db.SystemAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username == req.Username && a.Password == req.Password);

        if (acc is null || !string.Equals(acc.Status, "Active", StringComparison.OrdinalIgnoreCase))
            return Unauthorized("Invalid credentials or inactive account.");

        var (token, exp) = _tokens.CreateToken(acc);

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = exp,
            Username = acc.Username,
            Role = acc.Role
        });
    }

    // -------------------------
    // POST /api/auth/register
    // -------------------------
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password are required.");

        if (!string.Equals(req.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(req.Role, "Librarian", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Role must be 'Admin' or 'Librarian'.");
        }

        var exists = await _db.SystemAccounts.AnyAsync(a => a.Username == req.Username);
        if (exists) return Conflict("Username already exists.");

        var account = new SystemAccount
        {
            Username = req.Username,
            Password = req.Password,
            Role = req.Role,
            Status = "Active"
        };

        _db.SystemAccounts.Add(account);
        await _db.SaveChangesAsync();

        var (token, exp) = _tokens.CreateToken(account);

        var response = new RegisterResponse
        {
            Id = account.Id,
            Username = account.Username,
            Role = account.Role,
            Status = account.Status
        };

        return CreatedAtAction(nameof(Me), new { }, new
        {
            user = response,
            login = new { token, expiresAtUtc = exp }
        });
    }

    // -------------------------
    // GET /api/auth/me
    // -------------------------
    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        var username = User.Identity?.Name ?? User.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
        var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
        return Ok(new { username, role });
    }
}
