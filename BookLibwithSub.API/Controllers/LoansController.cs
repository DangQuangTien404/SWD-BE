using System;
using System.Collections.Generic;
using System.Linq;
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

        public record LoanItemResponse(int LoanItemID, int BookID, DateTime DueDate, DateTime? ReturnedDate, string Status);
        public record LoanResponse(int LoanID, int SubscriptionID, DateTime LoanDate, DateTime? ReturnDate, string Status, IEnumerable<LoanItemResponse> Items);

        [HttpGet("{loanId}")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> GetLoan(int loanId)
        {
            var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
            var loan = await _loanService.GetLoanAsync(loanId, userId);
            if (loan == null) return NotFound();
            var response = new LoanResponse(
                loan.LoanID,
                loan.SubscriptionID,
                loan.LoanDate,
                loan.ReturnDate,
                loan.Status,
                loan.LoanItems.Select(li => new LoanItemResponse(li.LoanItemID, li.BookID, li.DueDate, li.ReturnedDate, li.Status))
            );
            return Ok(response);
        }

        public class ExtendLoanRequest
        {
            public DateTime? NewDueDate { get; set; }
            public int? DaysToExtend { get; set; }
        }

        [HttpPut("{loanId}/extend")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> ExtendLoan(int loanId, [FromBody] ExtendLoanRequest request)
        {
            var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
            await _loanService.ExtendLoanAsync(loanId, userId, request.NewDueDate, request.DaysToExtend);
            return Ok();
        }
    }
}
