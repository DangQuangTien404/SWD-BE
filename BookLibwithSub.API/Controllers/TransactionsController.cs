using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using BookLibwithSub.Service.Constants;
using BookLibwithSub.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
            var transactions = await _transactionService.GetUserTransactionsAsync(userId);
            return Ok(transactions);
        }

        [HttpGet("all")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            return Ok(transactions);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetById(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }
    }
}

