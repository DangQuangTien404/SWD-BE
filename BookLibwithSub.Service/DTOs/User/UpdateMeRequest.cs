using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibwithSub.Service.Models.User
{
    public class UpdateMeRequest
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}

