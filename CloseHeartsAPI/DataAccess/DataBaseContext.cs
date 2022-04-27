using CloseHeartsAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloseHeartsAPI.DataAccess
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
    }
}
