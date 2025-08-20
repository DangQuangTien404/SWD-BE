using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookLibwithSub.Repo
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=metro.proxy.rlwy.net;Port=15446;Database=railway;Username=postgres;Password=AzHSRxHXHuqkqEyGbQbggstbMUNgWvql;")
                .Options;

            return new AppDbContext(options);
        }
    }
}
