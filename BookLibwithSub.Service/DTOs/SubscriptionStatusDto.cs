using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibwithSub.Service.Models
{
    public sealed class SubscriptionStatusDto
    {
        
        public int? SubscriptionId { get; set; }
        public string? PlanName { get; set; }
        public int? DurationDays { get; set; }
        public decimal? Price { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Inactive"; 

        
        public int? MaxPerDay { get; set; }
        public int? MaxPerMonth { get; set; }

        
        public int BorrowedToday { get; set; }
        public int BorrowedThisMonth { get; set; }

        
        public int RemainingToday { get; set; }
        public int RemainingThisMonth { get; set; }
    }

}
