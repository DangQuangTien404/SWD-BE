using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookLibwithSub.Repo.Entities;
public class Inventory
{
    public int BookId { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int ReservedCopies { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Book? Book { get; set; }
}
