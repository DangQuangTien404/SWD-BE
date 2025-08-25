using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookLibwithSub.Repo.Entities
{
    public class LoanItem
    {
        public int LoanItemID { get; set; }
        public int LoanID { get; set; }
        public int BookID { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string Status { get; set; }

        [JsonIgnore] public Loan? Loan { get; set; }
        public Book? Book { get; set; }
    }
}
