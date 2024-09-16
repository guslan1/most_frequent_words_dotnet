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
        private CountController CreateControllerWithMockedContext(string requestBody)
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockBody = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
            mockRequest.Setup(r => r.Body).Returns(mockBody);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

            return new CountController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
        }

        [Fact]
        public async Task WordCount_ReturnsCorrectWordCount()
        {
            // Arrange
            var controller = CreateControllerWithMockedContext("Banan Äpple Katt Hund Banan Hund Katt Hund");

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Equal(3, wordCount["hund"]);
            Assert.Equal(2, wordCount["banan"]);
            Assert.Equal(2, wordCount["katt"]);
            Assert.Equal(1, wordCount["äpple"]);
        }

        [Fact]
        public async Task WordCount_ConvertsUppercaseToLowercase()
        {
            // Arrange
            var controller = CreateControllerWithMockedContext("Hund HUND hund");

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Equal(3, wordCount["hund"]);
        }

        [Fact]
        public async Task WordCount_ReturnsMaxTenWords()
        {
            // Arrange
            var text = "ord1 ord2 ord3 ord4 ord5 ord6 ord7 ord8 ord9 ord10 ord11 ord12 ord13 ord14 ord15";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.True(wordCount.Count <= 10, "The result should contain at most 10 words.");
        }

        [Fact]
        public async Task WordCount_SortsWordsAlphabeticallyWhenFrequenciesAreEqual()
        {
            // Arrange
            var text = "banan björn banan björn banan björn banan björn";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Equal(2, wordCount.Count);
            Assert.Equal(4, wordCount["banan"]);
            Assert.Equal(4, wordCount["björn"]);

            // Kontrollera att orden är sorterade alfabetiskt när frekvenserna är lika
            var sortedKeys = new List<string>(wordCount.Keys);
            Assert.Equal("banan", sortedKeys[0]);
            Assert.Equal("björn", sortedKeys[1]);
        }
    }
}