using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq.Business.Service;
using Moq.DB.Context;
using Moq.DB.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moq.Tests.Services
{

    public class CandidateServiceTests
    {
        private readonly Mock<ICandidateRepository> _mockRepository;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<CandidateService>> _mockLogger;
        private readonly CandidateService _candidateService;

        public CandidateServiceTests()
        {
            _mockRepository = new Mock<ICandidateRepository>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<CandidateService>>();
            _candidateService = new CandidateService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object);
        }


        [Fact]
        public async Task AddOrUpdateCandidateAsync_AddsNewCandidate_WhenCandidateDoesNotExist()
        {
            // Arrange
            var candidate = new Candidate { Email = "test@example.com", FirstName = "John", LastName = "Doe", Comment = "Test comment" };
          
            // Simulate that the candidate does not exist in the database
            _mockRepository.Setup(r => r.GetCandidateByEmailAsync(candidate.Email))
                           .ReturnsAsync((Candidate)null);

            // Setup the repository to just return a completed Task when adding the candidate
            _mockRepository.Setup(r => r.AddCandidateAsync(candidate)).Returns(Task.CompletedTask);


            // Act
            await _candidateService.AddOrUpdateCandidateAsync(candidate);

            // Assert
            _mockRepository.Verify(r => r.AddCandidateAsync(candidate), Times.Once);
            _mockRepository.Verify(r => r.UpdateCandidateAsync(It.IsAny<Candidate>()), Times.Never);
        }

        [Fact]
        public async Task AddOrUpdateCandidateAsync_UpdatesExistingCandidate_WhenCandidateExists()
        {
            // Arrange
            var existingCandidate = new Candidate { Email = "test@example.com", FirstName = "John", LastName = "Doe", Comment = "Test comment" };
            var updatedCandidate = new Candidate { Email = "test@example.com", FirstName = "Jane", LastName = "Smith", Comment = "Updated comment" };

            // Setup the repository to return the existing candidate
            _mockRepository.Setup(r => r.GetCandidateByEmailAsync(existingCandidate.Email))
                           .ReturnsAsync(existingCandidate);

            // Setup the repository to return a task with the updated candidate (to match the return type)
            _mockRepository.Setup(r => r.UpdateCandidateAsync(updatedCandidate))
                           .ReturnsAsync(updatedCandidate); // Return updatedCandidate to match expected return type


            // Act
            await _candidateService.AddOrUpdateCandidateAsync(updatedCandidate);

            // Assert
            _mockRepository.Verify(r => r.UpdateCandidateAsync(It.Is<Candidate>(c => c.FirstName == "Jane" && c.LastName == "Smith" && c.Comment == "Updated comment")), Times.Once);
            _mockRepository.Verify(r => r.AddCandidateAsync(It.IsAny<Candidate>()), Times.Never);
        }

        [Fact]
        public async Task GetCandidateByEmailCacheAsync_ReturnsCandidateFromCache_WhenCacheHit()
        {
            // Arrange
            var candidate = new Candidate { Email = "test@example.com", FirstName = "John", LastName = "Doe", Comment = "Test comment" };
            _mockCache.Setup(c => c.TryGetValue(candidate.Email, out candidate)).Returns(true);

            // Act
            var result = await _candidateService.GetCandidateByEmailCacheAsync(candidate.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(candidate.Email, result.Email);
            _mockRepository.Verify(r => r.GetCandidateByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetCandidateByEmailCacheAsync_ReturnsCandidateFromRepository_WhenCacheMiss()
        {
            // Arrange
            var candidate = new Candidate { Email = "test@example.com", FirstName = "John", LastName = "Doe", Comment = "Test comment" };

            // Setup the cache mock to simulate a cache miss
            _mockCache.Setup(c => c.TryGetValue(candidate.Email, out candidate)).Returns(false);

            // Setup the repository mock to return the candidate when called
            _mockRepository.Setup(r => r.GetCandidateByEmailAsync(candidate.Email)).ReturnsAsync(candidate);

            // Act
            var result = await _candidateService.GetCandidateByEmailCacheAsync(candidate.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(candidate.Email, result.Email);

            // Verify that the repository method was called once
            _mockRepository.Verify(r => r.GetCandidateByEmailAsync(candidate.Email), Times.Once);

            // Optionally verify that the cache is set with the candidate
            _mockCache.Verify(c => c.Set(candidate.Email, candidate, It.IsAny<TimeSpan>()), Times.Once);
        }

    }
}
