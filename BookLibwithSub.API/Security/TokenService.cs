using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookLibwithSub.Repo.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookLibwithSub.API.Security;

public interface ITokenService
{
    (string token, DateTime expiresAtUtc) CreateToken(SystemAccount account);
}

public sealed class TokenService : ITokenService
{
    private readonly string _issuer;
    private readonly SymmetricSecurityKey _signingKey;
    private const int ExpiryHours = 2;

    public TokenService(IConfiguration config, SymmetricSecurityKey signingKey)
    {
        _issuer = config["Jwt:Issuer"] ?? "BookLibIssuer";
        _signingKey = signingKey;
    }

    public (string token, DateTime expiresAtUtc) CreateToken(SystemAccount account)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddHours(ExpiryHours);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, account.Username),
            new(ClaimTypes.Role, account.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("acc_status", account.Status ?? "active")
        };

        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: null,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, expires);
    }
}
