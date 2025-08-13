using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookLibwithSub.Repo.Entities;
public class LoanItem
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public int BookId { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal FineAmount { get; set; }
    public bool IsLost { get; set; }

    public Loan? Loan { get; set; }
    public Book? Book { get; set; }
}