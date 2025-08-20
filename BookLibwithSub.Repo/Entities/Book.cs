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
    public class Book
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string ISBN { get; set; }
        public string Publisher { get; set; }
        public int PublishedYear { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public byte[]? CoverImage { get; set; }
        public string? CoverImageContentType { get; set; }
        [JsonIgnore] public ICollection<LoanItem> LoanItems { get; set; } = new List<LoanItem>();
    }
}
