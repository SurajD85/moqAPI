using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq.Business;
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
        private readonly Mock<ICacheService> _mockCacheWrapper;

        private readonly Mock<ILogger<CandidateService>> _mockLogger;
        private readonly CandidateService _candidateService;

        public CandidateServiceTests()
        {
            _mockRepository = new Mock<ICandidateRepository>();
            _mockCacheWrapper = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<CandidateService>>();
            _candidateService = new CandidateService(_mockRepository.Object, _mockCacheWrapper.Object, _mockLogger.Object);
        }


        [Fact]
        public async Task AddOrUpdateCandidateAsync_ShouldUpdateExistingCandidate_WhenCandidateExists()
        {
            // Arrange
            var candidate = new Candidate
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Comment = "Test comment"
            };

            var existingCandidate = new Candidate
            {
                Email = "test@example.com",
                FirstName = "Jane",
                LastName = "Doe",
                Comment = "Old comment"
            };

            // Mock the ICacheService to simulate TryGetValue behavior
            _mockCacheWrapper.Setup(x => x.TryGetValue(candidate.Email, out existingCandidate))
                             .Returns(true);  // Simulate that the candidate exists in the cache

            // Mock the UpdateCandidateAsync method in the repository
            _mockRepository.Setup(x => x.UpdateCandidateAsync(existingCandidate))
                                     .Returns(Task.FromResult(existingCandidate));

            // Mock cache update to set the updated candidate in the cache
            _mockCacheWrapper.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<Candidate>(), It.IsAny<TimeSpan>()));

            // Act
            await _candidateService.AddOrUpdateCandidateAsync(candidate);

            // Assert that the repository update method was called once
            _mockRepository.Verify(x => x.UpdateCandidateAsync(It.IsAny<Candidate>()), Times.Once);

            // Assert that the cache update method was called once
            _mockCacheWrapper.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<Candidate>(), It.IsAny<TimeSpan>()), Times.Once);
        }


        [Fact]
        public async Task AddOrUpdateCandidateAsync_ShouldAddNewCandidate_WhenCandidateDoesNotExist()
        {
            // Arrange
            var candidate = new Candidate
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Comment = "Test comment"
            };

            // Mock the ICacheService to simulate that the candidate does not exist in the cache
            _mockCacheWrapper.Setup(x => x.TryGetValue(candidate.Email, out It.Ref<Candidate>.IsAny))
                             .Returns(false);

            // Mock the AddCandidateAsync method in the repository
            _mockRepository.Setup(x => x.AddCandidateAsync(candidate))
                           .Returns(Task.CompletedTask);

            // Mock cache update to set the new candidate in the cache
            _mockCacheWrapper.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<Candidate>(), It.IsAny<TimeSpan>()));

            // Act
            await _candidateService.AddOrUpdateCandidateAsync(candidate);

            // Assert that the repository add method was called once
            _mockRepository.Verify(x => x.AddCandidateAsync(It.IsAny<Candidate>()), Times.Once);

            // Assert that the cache update method was called once
            _mockCacheWrapper.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<Candidate>(), It.IsAny<TimeSpan>()), Times.Once);
        }
        [Fact]
        public async Task GetCandidateByEmailCacheAsync_ShouldReturnCandidate_WhenCacheMissAndFoundInRepository()
        {
            // Arrange
            var email = "test@example.com";
            var candidateFromRepository = new Candidate
            {
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Comment = "Test comment"
            };

            // Simulate cache miss (return false for TryGetValue)
            _mockCacheWrapper.Setup(x => x.TryGetValue(email, out It.Ref<Candidate>.IsAny))
                             .Returns(false);

            // Mock the repository call to get the candidate by email
            _mockRepository.Setup(x => x.GetCandidateByEmailAsync(email))
                           .ReturnsAsync(candidateFromRepository);

            // Mock cache set to simulate updating the cache with the candidate
            _mockCacheWrapper.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<Candidate>(), It.IsAny<TimeSpan>()));

            // Act
            var result = await _candidateService.GetCandidateByEmailCacheAsync(email);

            // Assert: Ensure the candidate is returned and cache was updated
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            _mockCacheWrapper.Verify(x => x.Set(email, candidateFromRepository, It.IsAny<TimeSpan>()), Times.Once);
        }
    }
}
