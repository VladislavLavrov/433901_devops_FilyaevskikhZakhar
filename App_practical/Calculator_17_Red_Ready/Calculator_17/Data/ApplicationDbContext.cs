
using Calculator_17.Models;
using Microsoft.EntityFrameworkCore;

namespace Calculator_17.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CalculationHistory> CalculationHistories { get; set; }
    }
}
