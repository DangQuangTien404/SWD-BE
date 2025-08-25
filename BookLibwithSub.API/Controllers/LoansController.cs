using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Service.Constants;
using BookLibwithSub.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.User)]
    [Produces("application/json")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        public LoansController(ILoanService loanService) => _loanService = loanService;

        
        public sealed class BorrowRequest
        {
            public int SubscriptionId { get; set; }
            public List<int> BookIds { get; set; } = new();
        }
        public sealed class AddItemsRequest
        {
            public List<int> BookIds { get; set; } = new();
        }
        public sealed class ExtendLoanRequest
        {
            public DateTime? NewDueDate { get; set; }
            public int? DaysToExtend { get; set; }
        }
        public sealed record LoanItemResponse(int LoanItemID, int BookID, DateTime DueDate, DateTime? ReturnedDate, string Status);
        public sealed record LoanResponse(int LoanID, int SubscriptionID, DateTime LoanDate, DateTime? ReturnDate, string Status, IEnumerable<LoanItemResponse> Items);

        private bool TryGetUserId(out int userId)
        {
            var id =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return int.TryParse(id, out userId);
        }

        private static LoanResponse MapLoan(Loan loan) =>
            new(
                loan.LoanID,
                loan.SubscriptionID,
                loan.LoanDate,
                loan.ReturnDate,
                loan.Status,
                loan.LoanItems.Select(li =>
                    new LoanItemResponse(li.LoanItemID, li.BookID, li.DueDate, li.ReturnedDate, li.Status))
            );

        
        [HttpPost]
        public async Task<IActionResult> Borrow([FromBody] BorrowRequest request)
        {
            if (!TryGetUserId(out _)) return Unauthorized();
            var loan = await _loanService.BorrowAsync(request.SubscriptionId, request.BookIds);
            return CreatedAtAction(nameof(GetLoan), new { loanId = loan.LoanID }, MapLoan(loan));
        }

        
        [HttpPost("{loanId:int}/items")]
        public async Task<IActionResult> AddItems([FromRoute] int loanId, [FromBody] AddItemsRequest request)
        {
            if (!TryGetUserId(out _)) return Unauthorized();
            var loan = await _loanService.AddItemsAsync(loanId, request.BookIds);
            return Ok(MapLoan(loan));
        }

        
        [HttpPost("items/{loanItemId:int}/return")]
        public async Task<IActionResult> ReturnItem([FromRoute] int loanItemId)
        {
            if (!TryGetUserId(out _)) return Unauthorized();
            var item = await _loanService.ReturnAsync(loanItemId);
            var response = new LoanItemResponse(item.LoanItemID, item.BookID, item.DueDate, item.ReturnedDate, item.Status);
            return Ok(response);
        }

        
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var loans = await _loanService.GetLoanHistoryAsync(userId);
            return Ok(loans.Select(MapLoan));
        }

        
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var loans = await _loanService.GetActiveLoansAsync(userId);
            return Ok(loans.Select(MapLoan));
        }

        
        [HttpGet("{loanId:int}")]
        public async Task<IActionResult> GetLoan([FromRoute] int loanId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var loan = await _loanService.GetLoanAsync(loanId, userId);
            if (loan is null) return NotFound();
            return Ok(MapLoan(loan));
        }

        
        [HttpPut("{loanId:int}/extend")]
        public async Task<IActionResult> ExtendLoan([FromRoute] int loanId, [FromBody] ExtendLoanRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            var loan = await _loanService.ExtendLoanAsync(loanId, userId, request.NewDueDate, request.DaysToExtend);
            return Ok(MapLoan(loan));
        }
    }
}
