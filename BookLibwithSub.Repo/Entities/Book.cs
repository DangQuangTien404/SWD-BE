using System.Collections.Generic;
using System.Text.Json.Serialization;

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

        public string? CoverImageUrl { get; set; }

        [JsonIgnore]
        public ICollection<LoanItem> LoanItems { get; set; } = new List<LoanItem>();
    }
}
