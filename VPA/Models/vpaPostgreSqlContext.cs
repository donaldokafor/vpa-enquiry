using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VPA.Models
{
    // >dotnet ef migration add testMigration in AspNet5MultipleProject
    public class vpaPostgreSqlContext : DbContext
    {
        public vpaPostgreSqlContext(DbContextOptions<vpaPostgreSqlContext> options) : base(options)
        {
        }

        public DbSet<VPAEnquiryRequest> VPAEnquiryRequest { get; set; }

        //public DbSet<SourceInfo> SourceInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<VPAEnquiryRequest>().HasKey(m => m.requestId);
            //builder.Entity<SourceInfo>().HasKey(m => m.SourceInfoId);

            // shadow properties
            builder.Entity<VPAEnquiryRequest>().Property<DateTime>("updatedOn");
            //builder.Entity<SourceInfo>().Property<DateTime>("UpdatedTimestamp");

            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            //updateUpdatedProperty<SourceInfo>();
            updateUpdatedProperty<VPAEnquiryRequest>();

            return base.SaveChanges();
        }

        private void updateUpdatedProperty<T>() where T : class
        {
            var modifiedSourceInfo =
                ChangeTracker.Entries<T>()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in modifiedSourceInfo)
            {
                entry.Property("updatedOn").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
