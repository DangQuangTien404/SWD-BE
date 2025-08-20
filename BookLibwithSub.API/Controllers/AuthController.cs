using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models;
using BookLibwithSub.Service.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("register")]
        [AllowAnonymous] 
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _authService.RegisterAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous] 
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var token = await _authService.LoginAsync(request);
                if (token == null) return Unauthorized();
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPost("logout")]
        //[AllowAnonymous] 
        //public async Task<IActionResult> Logout()
        //{


        //    try
        //    {
        //        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        //        if (int.TryParse(userIdClaim, out var userId))
        //        {
        //            await _authService.LogoutAsync(userId);
        //        }

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}


        [HttpPut("users/{id:int}")]
        [AllowAnonymous] 
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _authService.UpdateAccountAsync(id, request);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {

                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("users/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                await _authService.DeleteAccountAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
