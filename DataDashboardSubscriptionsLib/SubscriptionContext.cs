using System.Data.Entity;

namespace DataDashboardSubscriptionsLib
{
    class SubscriptionContext : DbContext
    {
        public DbSet<Subscription> Subscriptions { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder) => modelBuilder.Entity<Subscription>()
        //        .Property(s => s.StartDate)
        //        .HasDefaultValueSql("getdate()");

        public SubscriptionContext(string connString)
        {
            this.Database.Connection.ConnectionString = connString;
        }
    }
}
