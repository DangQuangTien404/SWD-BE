using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibwithSub.Service.Models
{
    public sealed class SubscriptionStatusDto
    {
        // plan / subscription basics
        public int? SubscriptionId { get; set; }
        public string? PlanName { get; set; }
        public int? DurationDays { get; set; }
        public decimal? Price { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Inactive"; // Active/Inactive/Pending

        // limits from plan
        public int? MaxPerDay { get; set; }
        public int? MaxPerMonth { get; set; }

        // usage
        public int BorrowedToday { get; set; }
        public int BorrowedThisMonth { get; set; }

        // remaining quota
        public int RemainingToday { get; set; }
        public int RemainingThisMonth { get; set; }
    }

}
