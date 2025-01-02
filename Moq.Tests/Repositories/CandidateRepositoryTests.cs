using Microsoft.EntityFrameworkCore;
using Moq.DB.Context;
using Moq.DB.Repository;

namespace Moq.Tests.Repositories
{
    public class CandidateRepositoryTests
    {
        private readonly CandidateRepository _repository;
        private readonly AppDbContext _context;


        public CandidateRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "CandidateDb") // In-memory database
                .Options;

            _context = new AppDbContext(options);
            _repository = new CandidateRepository(_context);
        }

        [Fact]
        public async Task AddCandidateAsync_ShouldAddCandidateToDb()
        {
            // Arrange
            var candidate = new Candidate
            {
                Email = "test@example.com",
                FirstName = "Suraj",
                LastName = "Deuja",
                Comment = "Test comment"
            };

            // Clear the database before each test to ensure no duplicates
            _context.Candidates.RemoveRange(_context.Candidates);
            await _context.SaveChangesAsync();

            // Act
            await _repository.AddCandidateAsync(candidate);
            var addedCandidate = await _context.Candidates.FindAsync(candidate.Email);

            // Assert
            Assert.NotNull(addedCandidate);
            Assert.Equal(candidate.Email, addedCandidate.Email);
            Assert.Equal(candidate.FirstName, addedCandidate.FirstName);
            Assert.Equal(candidate.LastName, addedCandidate.LastName);
            Assert.Equal(candidate.Comment, addedCandidate.Comment);
        }

        [Fact]
        public async Task GetCandidateByEmailAsync_ShouldReturnCandidate_WhenExists()
        {
            // Arrange
            var candidate = new Candidate
            {
                Email = "test@example.com",
                FirstName = "Suraj",
                LastName = "Deuja",
                Comment = "Test comment"
            };

            // Ensure no candidates already exist in the DB
            _context.Candidates.RemoveRange(_context.Candidates);
            await _context.SaveChangesAsync();  // Commit any changes

            await _repository.AddCandidateAsync(candidate); // Add candidate to DB

            // Act
            var foundCandidate = await _repository.GetCandidateByEmailAsync(candidate.Email);

            // Assert
            Assert.NotNull(foundCandidate);
            Assert.Equal(candidate.Email, foundCandidate.Email);
            Assert.Equal(candidate.FirstName, foundCandidate.FirstName);
            Assert.Equal(candidate.LastName, foundCandidate.LastName);
            Assert.Equal(candidate.Comment, foundCandidate.Comment);
        }

        [Fact]
        public async Task GetCandidateByEmailAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var candidate = await _repository.GetCandidateByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(candidate); // Candidate should not exist
        }

        [Fact]
        public async Task UpdateCandidateAsync_ShouldUpdateExistingCandidate()
        {
            // Arrange
            var candidate = new Candidate
            {
                Email = "test@example.com",
                FirstName = "Suraj",
                LastName = "Deuja",
                PhoneNumber = "123-456-7890",
                CallTimeInterval = "Morning",
                LinkedInUrl = "https://www.linkedin.com/in/suraj-deuja-131340154/",
                GitHubUrl = "https://github.com/SurajDeuja",
                Comment = "Test comment"
            };
            await _repository.AddCandidateAsync(candidate); // Add candidate to DB

            // Update candidate information
            candidate.FirstName = "UpdatedSuraj";


            // Act
            var updatedCandidate = await _repository.UpdateCandidateAsync(candidate);
            // Assert
            Assert.Equal("UpdatedSuraj", updatedCandidate.FirstName);  // Check if the first name was updated
            Assert.Equal("test@example.com", updatedCandidate.Email);  // Ensure email remains unchanged
            Assert.Equal("Deuja", updatedCandidate.LastName);  // Ensure last name remains unchanged
            Assert.Equal("123-456-7890", updatedCandidate.PhoneNumber);  // Ensure phone number remains unchanged
            Assert.Equal("Morning", updatedCandidate.CallTimeInterval);  // Ensure call time interval remains unchanged
            Assert.Equal("https://www.linkedin.com/in/suraj-deuja-131340154/", updatedCandidate.LinkedInUrl);  // Ensure LinkedIn URL remains unchanged
            Assert.Equal("https://github.com/SurajDeuja", updatedCandidate.GitHubUrl);  // Ensure GitHub URL remains unchanged
            Assert.Equal("Test comment", updatedCandidate.Comment);  // Ensure comment remains unchanged

        }
    }

}
