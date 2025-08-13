using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookLibwithSub.Repo.Entities;
public class Member
{
    public int Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime? MembershipStartDate { get; set; }
    public DateTime? MembershipEndDate { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

