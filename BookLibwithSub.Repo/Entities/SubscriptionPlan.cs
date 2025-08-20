using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BookLibwithSub.Repo.Entities
{
    public class SubscriptionPlan
    {
        public int SubscriptionPlanID { get; set; }
        public string PlanName { get; set; }
        public int DurationDays { get; set; }
        public int MaxPerDay { get; set; }
        public int MaxPerMonth { get; set; }
        public decimal Price { get; set; }

        [JsonIgnore] public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}

