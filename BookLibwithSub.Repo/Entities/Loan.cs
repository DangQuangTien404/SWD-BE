using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookLibwithSub.Repo.Entities;
public class Loan
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Open";

    public Member? Member { get; set; }
    public ICollection<LoanItem> Items { get; set; } = new List<LoanItem>();
}