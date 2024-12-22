using Moq.API.Controllers;
using Moq.Business.Service;
using Moq.DB.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Moq.Tests.Controller
{
    public class CandidateControllerTests
    {
        [Fact]
        public async Task AddOrUpdateCandidate_ReturnsOk_WhenModelStateIsValid()
        {
            // Arrange
            var mockService = new Mock<ICandidateService>();
            var candidate = new Candidate { Email = "test@example.com", FirstName = "John" };
            mockService.Setup(s => s.AddOrUpdateCandidateAsync(candidate)).Returns(Task.CompletedTask);
            var mockLogger = new Mock<ILogger<CandidatesController>>();

            var controller = new CandidatesController(mockService.Object, mockLogger.Object);

            // Act
            var result = await controller.AddOrUpdateCandidate(candidate);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task AddOrUpdateCandidate_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var mockService = new Mock<ICandidateService>();
            var mockLogger = new Mock<ILogger<CandidatesController>>();
            var candidate = new Candidate { Email = "test@example.com", FirstName = "John" };
            var controller = new CandidatesController(mockService.Object, mockLogger.Object );
            controller.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await controller.AddOrUpdateCandidate(candidate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }
    }
}
