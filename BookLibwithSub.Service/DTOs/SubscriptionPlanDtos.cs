using System.ComponentModel.DataAnnotations;

namespace BookLibwithSub.Service.Models
{
    public class CreateSubscriptionPlanRequest
    {
        [Required, StringLength(100)]
        public string PlanName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "DurationDays must be > 0")]
        public int DurationDays { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxPerDay { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxPerMonth { get; set; }

        [Range(typeof(decimal), "0.00", "999999999999.99")]
        public decimal Price { get; set; }
    }

    public class UpdateSubscriptionPlanRequest : CreateSubscriptionPlanRequest { }

    public record SubscriptionPlanResponse(
        int SubscriptionPlanID,
        string PlanName,
        int DurationDays,
        int MaxPerDay,
        int MaxPerMonth,
        decimal Price
    );
}
