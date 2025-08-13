using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookLibwithSub.Repo.Entities;

public class SystemAccount
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; 
    public string Role { get; set; } = "Librarian"; 
    public string Status { get; set; } = "Active";
}

