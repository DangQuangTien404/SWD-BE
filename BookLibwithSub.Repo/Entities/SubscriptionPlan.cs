using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookLibwithSub.Repo.Entities;

public class SubscriptionPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MonthlyUniqueTitles { get; set; }
    public int DailyMaxLoans { get; set; }
    public bool AllowRepeatSameTitleInMonth { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
