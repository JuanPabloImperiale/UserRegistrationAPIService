using Microsoft.EntityFrameworkCore;

namespace dotnet_registration_api.Data
{
    public class UserDataContext : DbContext
    {
        public UserDataContext(DbContextOptions<UserDataContext> options) : base(options)
        {
        }
        public DbSet<Entities.User> Users { get; set; }
    }
}
