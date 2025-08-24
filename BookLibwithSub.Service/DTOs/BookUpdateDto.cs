using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibwithSub.Service.DTOs
{
    public class BookUpdateDto 
    {
        public string? Title { get; set; }
        public string? AuthorName { get; set; }
        public string? ISBN { get; set; }
        public string? Publisher { get; set; }
        public int PublishedYear { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string? CoverImageUrl { get; set; }
    }
}
