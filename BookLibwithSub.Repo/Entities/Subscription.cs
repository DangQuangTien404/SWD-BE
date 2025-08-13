using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookLibwithSub.Repo.Entities;

public class Subscription
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public bool AutoRenew { get; set; }

    public Member? Member { get; set; }
    public SubscriptionPlan? Plan { get; set; }
}