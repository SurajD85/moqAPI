using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Moq.DB.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Candidate> Candidates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure nullable properties
            modelBuilder.Entity<Candidate>()
                .Property(c => c.PhoneNumber)
                .IsRequired(false);

            modelBuilder.Entity<Candidate>()
                .Property(c => c.CallTimeInterval)
                .IsRequired(false);

            modelBuilder.Entity<Candidate>()
                .Property(c => c.LinkedInUrl)
                .IsRequired(false);

            modelBuilder.Entity<Candidate>()
                .Property(c => c.GitHubUrl)
                .IsRequired(false);
        }

    }
    // Candidate Entity
    public class Candidate
    {
        [Key]
        public string Email { get; set; } // Unique identifier

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public string CallTimeInterval { get; set; }
        public string LinkedInUrl { get; set; }
        public string GitHubUrl { get; set; }

        [Required]
        public string Comment { get; set; }
    }
}
