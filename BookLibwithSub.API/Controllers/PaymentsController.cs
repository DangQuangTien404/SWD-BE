using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IZaloPayService _zaloPayService;
        private readonly ITransactionService _transactionService;

        public PaymentsController(
            IPaymentService paymentService,
            IZaloPayService zaloPayService,
            ITransactionService transactionService)
        {
            _paymentService = paymentService;
            _zaloPayService = zaloPayService;
            _transactionService = transactionService;
        }

        public record CheckoutRequest(int SubscriptionId, decimal Amount);
        public record CheckoutResponse(string PayUrl, int TxId, string? ZpTransToken);

        [HttpPost("checkout")]
        [Authorize]
        public async Task<ActionResult<CheckoutResponse>> Checkout([FromBody] CheckoutRequest req)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var tx = await _paymentService.CreatePendingTransactionAsync(
                userId.Value, req.SubscriptionId, req.Amount);

            var zp = await _zaloPayService.CreateOrderAsync(tx.TransactionID, userId.Value);
            if (zp == null || zp.ReturnCode != 1)
                return BadRequest(new { message = "Create ZaloPay order failed", detail = zp });

            return Ok(new CheckoutResponse(zp.OrderUrl, tx.TransactionID, zp.ZpTransToken));
        }

        [HttpPost("zalo/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromForm] ZaloPayCallbackRequest request)
        {
            var result = await _zaloPayService.HandleCallbackAsync(request);
            if (!result.Success)
                return Ok(new { return_code = -1, return_message = result.Message });

            return Ok(new { return_code = 1, return_message = "success" });
        }

        [HttpGet("tx/{id:int}/status")]
        [Authorize]
        public async Task<IActionResult> GetTxStatus([FromRoute] int id)
        {
            var tx = await _transactionService.GetTransactionByIdAsync(id);
            if (tx == null) return NotFound();

            return Ok(new { id = tx.TransactionID, status = tx.Status });
        }

        private int? GetUserId()
        {
            var sub = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User?.FindFirst("sub")?.Value
                      ?? User?.FindFirst("nameid")?.Value;
            return int.TryParse(sub, out var id) ? id : (int?)null;
        }
    }
}
