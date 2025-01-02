using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq.API.Controllers;
using Moq.Business.Service;
using Moq.DB.Context;

namespace Moq.Tests.Controller
{
    public class CandidateControllerTests
    {
        private readonly Mock<ICandidateService> _mockCandidateService;
        private readonly Mock<ILogger<CandidatesController>> _mockLogger;
        private readonly CandidatesController _controller;

        public CandidateControllerTests()
        {
            _mockCandidateService = new Mock<ICandidateService>();
            _mockLogger = new Mock<ILogger<CandidatesController>>();
            _controller = new CandidatesController(_mockCandidateService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task AddOrUpdateCandidate_ShouldReturnOk_WhenCandidateIsValid()
        {
            // Arrange
            var candidate = new Candidate
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Comment = "Test comment"
            };

            _mockCandidateService
                .Setup(service => service.AddOrUpdateCandidateAsync(It.IsAny<Candidate>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddOrUpdateCandidate(candidate);

            // Assert
            var okResult = result as OkResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200); // HTTP 200 OK
        }

        [Fact]
        public async Task AddOrUpdateCandidate_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var candidate = new Candidate
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Comment = null // Comment is required, so this will make the model invalid
            };

            _controller.ModelState.AddModelError("Comment", "The Comment field is required.");

            // Act
            var result = await _controller.AddOrUpdateCandidate(candidate);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400); // HTTP 400 Bad Request
        }

        [Fact]
        public async Task AddOrUpdateCandidate_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var candidate = new Candidate
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Comment = "Test comment"
            };

            _mockCandidateService
                .Setup(service => service.AddOrUpdateCandidateAsync(It.IsAny<Candidate>()))
                .ThrowsAsync(new ServiceException("An error occurred while adding or updating the candidate.", new Exception("Inner exception message")));

            // Act
            var result = await _controller.AddOrUpdateCandidate(candidate);

            // Assert
            var internalServerErrorResult = result as ObjectResult;
            internalServerErrorResult.Should().NotBeNull();
            internalServerErrorResult?.StatusCode.Should().Be(500); // HTTP 500 Internal Server Error
        }
    }
}
