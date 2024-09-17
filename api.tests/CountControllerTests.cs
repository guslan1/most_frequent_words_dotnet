using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using api.Controllers;

namespace api.tests
{
    public class CountControllerTests
    {
        private CountController CreateControllerWithMockedContext(string requestBody, string contentType = "text/plain")
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockBody = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
            mockRequest.Setup(r => r.Body).Returns(mockBody);
            mockRequest.Setup(r => r.ContentType).Returns(contentType);
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

        [Fact]
        public async Task WordCount_ReturnsBadRequest_ForEmptyOrWhitespaceInput()
        {
            // Arrange
            var controllerEmpty = CreateControllerWithMockedContext("");
            var controllerWhitespace = CreateControllerWithMockedContext("   ");

            // Act
            var resultEmpty = await controllerEmpty.WordCount() as BadRequestObjectResult;
            var resultWhitespace = await controllerWhitespace.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(resultEmpty);
            Assert.Equal("Input text cannot be empty or consist only of whitespace.", resultEmpty.Value);

            Assert.NotNull(resultWhitespace);
            Assert.Equal("Input text cannot be empty or consist only of whitespace.", resultWhitespace.Value);
        }

        [Fact]
        public async Task WordCount_IgnoresPunctuationAndSpecialCharacters()
        {
            // Arrange
            var text = "banan, äpple! katt? hund. banan; hund: katt hund @#%^&*()_+={}[]|\\:\"'<>,.?/~`";
            var controller = CreateControllerWithMockedContext(text);

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
        public async Task WordCount_ReturnsBadRequest_WhenInputExceedsMaxLength()
        {
            // Arrange
            var longText = new string('a', 10001); // Text that exceeds the max length
            var controller = CreateControllerWithMockedContext(longText);

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Input text cannot exceed 10000 characters.", result.Value);
        }

        [Fact]
        public async Task WordCount_ReturnsBadRequest_ForInvalidContentType()
        {
            // Arrange
            var controller = CreateControllerWithMockedContext("Banan Äpple Katt Hund Banan Hund Katt Hund", "application/json");

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid content type. Only 'text/plain' is supported.", result.Value);
        }

        [Fact]
        public async Task WordCount_HandlesMultilineInput()
        {
            // Arrange
            var text = "banan äpple\nkatt hund\nbanan hund\nkatt hund";
            var controller = CreateControllerWithMockedContext(text);

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
        public async Task WordCount_HandlesDifferentEncodings()
        {
            // Arrange
            var textUtf8 = "banan äpple katt hund banan hund katt hund";
            var textAscii = "banana apple cat dog banana dog cat dog";

            // Test with UTF-8 encoding
            var utf8Encoding = Encoding.UTF8;
            var utf8MockHttpContext = new Mock<HttpContext>();
            var utf8MockRequest = new Mock<HttpRequest>();
            var utf8MockBody = new MemoryStream(utf8Encoding.GetBytes(textUtf8));
            utf8MockRequest.Setup(r => r.Body).Returns(utf8MockBody);
            utf8MockRequest.Setup(r => r.ContentType).Returns("text/plain");
            utf8MockHttpContext.Setup(c => c.Request).Returns(utf8MockRequest.Object);

            var utf8Controller = new CountController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = utf8MockHttpContext.Object
                }
            };

            // Act
            var utf8Result = await utf8Controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(utf8Result);
            var utf8WordCount = utf8Result.Value as Dictionary<string, int>;
            Assert.NotNull(utf8WordCount);
            Assert.Equal(3, utf8WordCount["hund"]);
            Assert.Equal(2, utf8WordCount["banan"]);
            Assert.Equal(2, utf8WordCount["katt"]);
            Assert.Equal(1, utf8WordCount["äpple"]);

            // Test with ASCII encoding
            var asciiEncoding = Encoding.ASCII;
            var asciiMockHttpContext = new Mock<HttpContext>();
            var asciiMockRequest = new Mock<HttpRequest>();
            var asciiMockBody = new MemoryStream(asciiEncoding.GetBytes(textAscii));
            asciiMockRequest.Setup(r => r.Body).Returns(asciiMockBody);
            asciiMockRequest.Setup(r => r.ContentType).Returns("text/plain");
            asciiMockHttpContext.Setup(c => c.Request).Returns(asciiMockRequest.Object);

            var asciiController = new CountController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = asciiMockHttpContext.Object
                }
            };

            // Act
            var asciiResult = await asciiController.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(asciiResult);
            var asciiWordCount = asciiResult.Value as Dictionary<string, int>;
            Assert.NotNull(asciiWordCount);
            Assert.Equal(3, asciiWordCount["dog"]);
            Assert.Equal(2, asciiWordCount["banana"]);
            Assert.Equal(2, asciiWordCount["cat"]);
            Assert.Equal(1, asciiWordCount["apple"]);
        }

        [Fact]
        public async Task WordCount_IgnoresOnlySpecialCharacters()
        {
            // Arrange
            var controller = CreateControllerWithMockedContext("@#$%^&12345");

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.Empty(wordCount);  // Should return an empty dictionary
        }


        [Fact]
        public async Task WordCount_SortsAlphabetically_WhenAllWordsHaveSameFrequency()
        {
            // Arrange
            var text = "äpple banan citron durian";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.Equal(new[] { "banan", "citron", "durian", "äpple" }, wordCount.Keys.ToArray());
        }


        [Fact]
        public async Task WordCount_HandlesVeryLargeInput()
        {
            // Arrange
            var veryLargeText = new string('a', 100001);
            var controller = CreateControllerWithMockedContext(veryLargeText);

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"Input text cannot exceed 10000 characters.", result.Value);
        }

        [Fact]
        public async Task WordCount_IgnoresInputWithOnlyNumbers()
        {
            // Arrange
            var controller = CreateControllerWithMockedContext("12345 67890");

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.Empty(wordCount);  // Should return an empty dictionary
        }





    }
}