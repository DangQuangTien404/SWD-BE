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
    public class Subscription
    {
        public int SubscriptionID { get; set; }
        public int UserID { get; set; }
        public int SubscriptionPlanID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }

        [JsonIgnore] public User? User { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }

        [JsonIgnore] public ICollection<Loan> Loans { get; set; } = new List<Loan>();
        [JsonIgnore] public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
