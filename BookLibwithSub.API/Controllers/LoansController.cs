using System.Collections.Generic;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using BookLibwithSub.Service.Constants;
using BookLibwithSub.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        public class BorrowRequest
        {
            public int SubscriptionId { get; set; }
            public List<int> BookIds { get; set; } = new();
        }

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> Borrow([FromBody] BorrowRequest request)
        {
            await _loanService.BorrowAsync(request.SubscriptionId, request.BookIds);
            return Ok();
        }

        public class AddItemsRequest
        {
            public List<int> BookIds { get; set; } = new();
        }

        [HttpPost("{loanId}/items")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> AddItems(int loanId, [FromBody] AddItemsRequest request)
        {
            await _loanService.AddItemsAsync(loanId, request.BookIds);
            return Ok();
        }

        [HttpPost("items/{loanItemId}/return")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> ReturnItem(int loanItemId)
        {
            await _loanService.ReturnAsync(loanItemId);
            return Ok();
        }

        [HttpGet("history")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> GetHistory()
        {
            var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
            var loans = await _loanService.GetLoanHistoryAsync(userId);
            return Ok(loans);
        }

        [HttpGet("active")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> GetActive()
        {
            var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
            var loans = await _loanService.GetActiveLoansAsync(userId);
            return Ok(loans);
        }
    }
}
