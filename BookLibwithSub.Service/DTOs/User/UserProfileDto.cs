using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookLibwithSub.Service.Models.User
{
    public class CurrentSubscriptionDto
    {
        public int SubscriptionId { get; set; }
        public string PlanName { get; set; } = "Unknown";
        public string Status { get; set; } = "Inactive";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int MaxPerDay { get; set; }
        public int MaxPerMonth { get; set; }
    }

    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = default!;
        public string Role { get; set; } = default!;

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }

        public CurrentSubscriptionDto? CurrentSubscription { get; set; }
    }
}
