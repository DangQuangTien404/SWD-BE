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
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Role { get; set; }
        public string? CurrentToken { get; set; }

        [JsonIgnore] public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        [JsonIgnore] public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}

