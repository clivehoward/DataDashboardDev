using System.Data.Entity;

namespace DataDashboardPromotionsLib
{
    class PromotionContext : DbContext
    {
        public DbSet<Submission> Submissions { get; set; }

        public PromotionContext(string connString)
        {
            this.Database.Connection.ConnectionString = connString;
        }
    }
}
