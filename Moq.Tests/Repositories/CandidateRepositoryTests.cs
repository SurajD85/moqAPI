using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq.DB.Context;
using Moq.DB.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moq.Tests.Repositories
{
    public class CandidateRepositoryTests
    {
        private readonly Mock<AppDbContext> _mockContext;
        private readonly Mock<DbSet<Candidate>> _mockDbSet;
        private readonly CandidateRepository _candidateRepository;

        public CandidateRepositoryTests()
        {
            _mockContext = new Mock<AppDbContext>();
            _mockDbSet = new Mock<DbSet<Candidate>>();
            _mockContext.Setup(c => c.Candidates).Returns(_mockDbSet.Object);
            _candidateRepository = new CandidateRepository(_mockContext.Object);
        }

        [Fact]
        public async Task AddCandidateAsync_AddsCandidateToDatabase()
        {
            // Arrange
            var candidate = new Candidate { Email = "test@example.com", FirstName = "John", LastName = "Doe", Comment = "Test comment" };

            // Act
            await _candidateRepository.AddCandidateAsync(candidate);

            // Assert
            _mockDbSet.Verify(dbSet => dbSet.Add(candidate), Times.Once);
            _mockContext.Verify(context => context.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task GetCandidateByEmailAsync_ReturnsCandidate_WhenCandidateExists()
        {
            // Arrange
            var candidate = new Candidate { Email = "test@example.com", FirstName = "John", LastName = "Doe", Comment = "Test comment" };
            _mockDbSet.Setup(dbSet => dbSet.FindAsync(candidate.Email)).ReturnsAsync(candidate);

            // Act
            var result = await _candidateRepository.GetCandidateByEmailAsync(candidate.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(candidate.Email, result.Email);
        }

        [Fact]
        public async Task UpdateCandidateAsync_UpdatesCandidateInDatabase()
        {
            // Arrange
            var candidate = new Candidate { Email = "test@example.com", FirstName = "John", LastName = "Doe", Comment = "Test comment" };

            // Act
            await _candidateRepository.UpdateCandidateAsync(candidate);

            // Assert
            _mockContext.Verify(context => context.Entry(candidate).State == EntityState.Modified, Times.Once);
            _mockContext.Verify(context => context.SaveChangesAsync(default), Times.Once);
        }
    }

}
