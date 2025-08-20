using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Service.Constants;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly ISubscriptionPlanService _service;

        public SubscriptionPlansController(ISubscriptionPlanService service)
        {
            _service = service;
        }

        private static SubscriptionPlanResponse ToResponse(SubscriptionPlan p) =>
            new(p.SubscriptionPlanID, p.PlanName, p.DurationDays, p.MaxPerDay, p.MaxPerMonth, p.Price);

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<SubscriptionPlanResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _service.GetAllAsync();
            return Ok(plans.Select(ToResponse));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(SubscriptionPlanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            var plan = await _service.GetByIdAsync(id);
            if (plan == null) return NotFound();
            return Ok(ToResponse(plan));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(typeof(SubscriptionPlanResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionPlanRequest req)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = new SubscriptionPlan
            {
                PlanName = req.PlanName,
                DurationDays = req.DurationDays,
                MaxPerDay = req.MaxPerDay,
                MaxPerMonth = req.MaxPerMonth,
                Price = req.Price
            };

            try
            {
                var created = await _service.AddAsync(entity);
                return CreatedAtAction(nameof(Get), new { id = created.SubscriptionPlanID }, ToResponse(created));
            }
            catch (Exception ex)
            {
                return Problem(title: "Failed to create plan", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSubscriptionPlanRequest req)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.PlanName = req.PlanName;
            existing.DurationDays = req.DurationDays;
            existing.MaxPerDay = req.MaxPerDay;
            existing.MaxPerMonth = req.MaxPerMonth;
            existing.Price = req.Price;

            try
            {
                await _service.UpdateAsync(id, existing);
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(title: "Failed to update plan", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
