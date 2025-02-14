using Microsoft.EntityFrameworkCore;

namespace JwT_Implementation.Data
{
    public class JwTApiContext:DbContext
    {

        public JwTApiContext(DbContextOptions<JwTApiContext> options):base(options) 
        {            
        }
        public DbSet<UserDetails> UsersDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserDetails>().ToTable("UserDetails");
            modelBuilder.Entity<UserDetails>().HasKey(x => x.UserId);
            modelBuilder.Entity<UserDetails>().HasData(
                new UserDetails { UserId=1, username = "Tamil", email = "abc@ab.com", password = "user123", role = "user" },
                new UserDetails { UserId=2, username = "TamilAdmin", email = "abc@ab.com", password = "admin123", role = "admin" }
            );
        }
    }
}
