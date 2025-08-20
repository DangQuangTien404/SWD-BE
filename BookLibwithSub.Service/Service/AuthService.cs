using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Repo.Interfaces;
using BookLibwithSub.Service.Constants;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models;
using BookLibwithSub.Service.Models.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookLibwithSub.Service.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtOptions _jwtOptions;

        public AuthService(IUserRepository userRepository, IOptions<JwtOptions> jwtOptions)
        {
            _userRepository = userRepository;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var existing = await _userRepository.GetByUsernameAsync(request.Username);
            if (existing != null) throw new InvalidOperationException("Username already exists");

            var emailAttr = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
            if (!emailAttr.IsValid(request.Email)) throw new InvalidOperationException("Invalid email format");

            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null) throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                Username = request.Username.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName?.Trim() ?? string.Empty,
                Email = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim(),
                Role = request.Role?.ToLower() == Roles.Admin ? Roles.Admin : Roles.User,
                CreatedDate = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
        }
        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;
            if (!string.IsNullOrEmpty(user.CurrentToken))
            {
                await _userRepository.UpdateTokenAsync(user.UserID, null);
            }

            var key = _jwtOptions.Key ?? throw new InvalidOperationException("JWT key not configured");
            var issuer = _jwtOptions.Issuer;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            await _userRepository.UpdateTokenAsync(user.UserID, tokenString);
            return tokenString;
        }

        public async Task LogoutAsync(int userId)
        {
            await _userRepository.UpdateTokenAsync(userId, null);
        }
        public async Task UpdateAccountAsync(int userId, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                       ?? throw new InvalidOperationException("User not found");

            if (!string.IsNullOrWhiteSpace(request.Username) &&
                !string.Equals(request.Username, user.Username, StringComparison.Ordinal))
            {
                var dup = await _userRepository.GetByUsernameAsync(request.Username);
                if (dup != null && dup.UserID != userId)
                    throw new InvalidOperationException("Username already exists");
                user.Username = request.Username.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Email) &&
                !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailAttr = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
                if (!emailAttr.IsValid(request.Email))
                    throw new InvalidOperationException("Invalid email format");

                var dupEmail = await _userRepository.GetByEmailAsync(request.Email);
                if (dupEmail != null && dupEmail.UserID != userId)
                    throw new InvalidOperationException("Email already exists");
                user.Email = request.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber.Trim();

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                user.CurrentToken = null; 
            }

            await _userRepository.UpdateAsync(user);
        }
        public async Task DeleteAccountAsync(int userId)
        {
            try
            {
                await _userRepository.DeleteAsync(userId);
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }
    }
}
