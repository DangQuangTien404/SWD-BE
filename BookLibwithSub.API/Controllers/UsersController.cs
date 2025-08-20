using System.Security.Claims;
using System.Threading.Tasks;
using BookLibwithSub.Service.Constants;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) => _userService = userService;

        private bool TryGetUserId(out int userId)
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return int.TryParse(idStr, out userId);
        }

        [HttpGet("me")]
        [Authorize(Roles = Roles.User)]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMe()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut("me")]
        [Authorize(Roles = Roles.User)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateMeRequest req)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            await _userService.UpdateProfileAsync(userId, req);
            return NoContent();
        }
    }
}
