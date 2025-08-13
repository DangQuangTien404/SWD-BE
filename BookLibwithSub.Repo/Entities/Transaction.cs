using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookLibwithSub.Repo.Entities;

public class Transaction
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int? SubscriptionId { get; set; }
    public int? LoanItemId { get; set; }
    public string Type { get; set; } = "Payment";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = "Posted";
    public DateTime CreatedAt { get; set; }
    public string? Note { get; set; }

    public Member? Member { get; set; }
    public Subscription? Subscription { get; set; }
    public LoanItem? LoanItem { get; set; }
}
