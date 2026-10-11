using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using ResumeManagement.Api.Entities;
namespace ResumeManagement.Api
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Applicant> Applicants => Set<Applicant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
