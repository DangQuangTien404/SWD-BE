using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Interfaces;            // <-- added
using BookLibwithSub.Service.Constants;
using BookLibwithSub.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionPlanService _planService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IZaloPayService _zaloPayService;

        // NEW: use repo to flip status & dates
        private readonly ISubscriptionRepository _subscriptionRepo;

        public SubscriptionsController(
            ISubscriptionPlanService planService,
            ISubscriptionService subscriptionService,
            IZaloPayService zaloPayService,
            ISubscriptionRepository subscriptionRepo)   // <-- added
        {
            _planService = planService;
            _subscriptionService = subscriptionService;
            _zaloPayService = zaloPayService;
            _subscriptionRepo = subscriptionRepo;       // <-- added
        }

        public class PurchaseRequest { public int PlanId { get; set; } }

        [HttpPost("purchase")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> Purchase([FromBody] PurchaseRequest request)
        {
            var userIdOpt = GetUserId(User);
            if (userIdOpt == null)
                return Unauthorized(new { message = "Invalid token. Please login again." });

            try
            {
                // 1) create the transaction (your current behavior)
                var transaction = await _subscriptionService.PurchaseAsync(userIdOpt.Value, request.PlanId);

                // 2) create ZaloPay order (your current behavior)
                var order = await _zaloPayService.CreateOrderAsync(transaction.TransactionID, userIdOpt.Value);

                // 3) IMMEDIATELY ACTIVATE the most recent subscription for this user
                //    (we fetch it with plan to compute the end date correctly)
                var latest = await _subscriptionRepo.GetLatestByUserAsync(userIdOpt.Value);
                if (latest != null)
                {
                    var subWithPlan = await _subscriptionRepo.GetByIdWithPlanAsync(latest.SubscriptionID);
                    if (subWithPlan?.SubscriptionPlan != null)
                    {
                        var nowUtc = DateTime.UtcNow;
                        var endUtc = nowUtc.AddDays(subWithPlan.SubscriptionPlan.DurationDays);
                        await _subscriptionRepo.ActivateAsync(subWithPlan.SubscriptionID, nowUtc, endUtc);
                    }
                }

                // 4) keep returning your original payload
                return Ok(new { transactionId = transaction.TransactionID, order });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("status")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> MyStatus()
        {
            var sub =
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(sub, out var userId) || userId <= 0)
                return Unauthorized(new { message = "Invalid token." });

            var dto = await _subscriptionService.GetMyStatusAsync(userId);
            return Ok(dto);
        }

        [HttpPost("renew")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> Renew()
        {
            var userIdOpt = GetUserId(User);
            if (userIdOpt == null)
                return Unauthorized(new { message = "Invalid token. Please login again." });

            try
            {
                // 1) create renewal transaction (your current behavior)
                var transaction = await _subscriptionService.RenewAsync(userIdOpt.Value);

                // 2) create ZaloPay order (your current behavior)
                var order = await _zaloPayService.CreateOrderAsync(transaction.TransactionID, userIdOpt.Value);

                // 3) IMMEDIATELY ACTIVATE (or re-activate) with plan duration
                var latest = await _subscriptionRepo.GetLatestByUserAsync(userIdOpt.Value);
                if (latest != null)
                {
                    var subWithPlan = await _subscriptionRepo.GetByIdWithPlanAsync(latest.SubscriptionID);
                    if (subWithPlan?.SubscriptionPlan != null)
                    {
                        var nowUtc = DateTime.UtcNow;
                        var endUtc = nowUtc.AddDays(subWithPlan.SubscriptionPlan.DurationDays);
                        await _subscriptionRepo.ActivateAsync(subWithPlan.SubscriptionID, nowUtc, endUtc);
                    }
                }

                return Ok(new { transactionId = transaction.TransactionID, order });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static int? GetUserId(ClaimsPrincipal user)
        {
            var sub =
                user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(sub, out var id) ? id : (int?)null;
        }
    }
}
