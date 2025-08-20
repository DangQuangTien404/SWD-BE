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

        public PaymentsController(IPaymentService paymentService, IZaloPayService zaloPayService)
        {
            _paymentService = paymentService;
            _zaloPayService = zaloPayService;
        }

        public class WebhookRequest
        {
            public int TransactionId { get; set; }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] WebhookRequest request, [FromHeader] string signature)
        {
            if (!ValidateSignature(request, signature)) return Unauthorized();
            await _paymentService.MarkTransactionPaidAsync(request.TransactionId);
            return Ok();
        }
        private bool ValidateSignature(object request, string signature)
        {
             return signature == "my-secret";
        }


        [HttpPost("zalo/create-order/{transactionId}")]
        [Authorize]
        public async Task<IActionResult> CreateZaloOrder([FromRoute] int transactionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var result = await _zaloPayService.CreateOrderAsync(transactionId, userId.Value);
            return Ok(result);
        }

        [HttpPost("zalo/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> ZaloCallback([FromForm] ZaloPayCallbackRequest request)
        {
            var result = await _zaloPayService.HandleCallbackAsync(request);
            if (!result.Success) return Ok(new { return_code = -1, return_message = result.Message });
            return Ok(new { return_code = 1, return_message = "success" });
        }

        private int? GetUserId()
        {
            var sub = User?.FindFirst("sub")?.Value ?? User?.FindFirst("nameid")?.Value;
            return int.TryParse(sub, out var id) ? id : (int?)null;
        }
    }
}
