using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using api.Controllers;

namespace api.tests
{
    public class CountControllerTests
    {
        [Fact]
        public async Task WordCount_ReturnsCorrectWordCount()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockBody = new MemoryStream(Encoding.UTF8.GetBytes("Banan Äpple Katt Hund Banan Hund Katt Hund"));
            mockRequest.Setup(r => r.Body).Returns(mockBody);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

            var controller = new CountController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Equal(3, wordCount["Hund"]);
            Assert.Equal(2, wordCount["Banan"]);
            Assert.Equal(2, wordCount["Katt"]);
            Assert.Equal(1, wordCount["Äpple"]);
        }
    }
}