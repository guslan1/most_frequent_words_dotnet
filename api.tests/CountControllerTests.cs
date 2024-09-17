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
            Assert.Equal(200, result.StatusCode);

            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);

            // Verify the word counts
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
            Assert.Equal(200, result.StatusCode);

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
            Assert.Equal(200, result.StatusCode);

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
            Assert.Equal(200, result.StatusCode);

            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);

            // Verify word counts
            Assert.Equal(2, wordCount.Count);
            Assert.Equal(4, wordCount["banan"]);
            Assert.Equal(4, wordCount["björn"]);

            // Verify that words are sorted alphabetically when frequencies are equal
            var sortedKeys = new List<string>(wordCount.Keys);
            sortedKeys.Sort();
            Assert.Equal("banan", sortedKeys[0]);
            Assert.Equal("björn", sortedKeys[1]);
        }

        [Fact]
        public async Task WordCount_ReturnsBadRequest_ForEmptyInput()
        {
            // Arrange
            var controller = CreateControllerWithMockedContext("");

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Input text cannot be empty or consist only of whitespace.", result.Value);
        }

        [Fact]
        public async Task WordCount_ReturnsBadRequest_ForWhitespaceInput()
        {
            // Arrange
            var controller = CreateControllerWithMockedContext("   ");

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Input text cannot be empty or consist only of whitespace.", result.Value);
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
            Assert.Equal(200, result.StatusCode);

            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);

            // Verify word counts
            Assert.Equal(3, wordCount["hund"]);
            Assert.Equal(2, wordCount["banan"]);
            Assert.Equal(2, wordCount["katt"]);
            Assert.Equal(1, wordCount["äpple"]);
        }

        [Fact]
        public async Task WordCount_ReturnsBadRequest_WhenInputExceedsMaxLength()
        {
            // Arrange
            const int maxLength = 10000;
            var textExceedingMaxLength = new string('a', maxLength + 1);
            var controller = CreateControllerWithMockedContext(textExceedingMaxLength);

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal($"Input text cannot exceed {maxLength} characters.", result.Value);
        }

        [Fact]
        public async Task WordCount_ReturnsBadRequest_ForInvalidContentType()
        {
            // Arrange
            const string invalidContentType = "application/json";
            const string expectedErrorMessage = "Invalid content type. Only 'text/plain' is supported.";
            var controller = CreateControllerWithMockedContext("Banan Äpple Katt Hund Banan Hund Katt Hund", invalidContentType);

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedErrorMessage, result.Value);
        }

        [Fact]
        public async Task WordCount_HandlesMultilineInput()
        {
            // Arrange
            const string text = "banan äpple\nkatt hund\nbanan hund\nkatt hund";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);

            // Verify word counts
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
        public async Task WordCount_ReturnsBadRequest_ForOnlySpecialCharacters()
        {
            // Arrange
            const string input = "@#$%^&12345";
            const string expectedErrorMessage = "Input text does not contain any valid words.";
            var controller = CreateControllerWithMockedContext(input);

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedErrorMessage, result.Value);
        }

        [Fact]
        public async Task WordCount_SortsAlphabetically_WhenAllWordsHaveSameFrequency()
        {
            // Arrange
            const string text = "äpple banan citron päron";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode); // Verify that the status code is 200 OK

            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Equal(new[] { "banan", "citron", "päron", "äpple" }, wordCount.Keys.ToArray());
        }


        [Fact]
        public async Task WordCount_HandlesVeryLargeInput()
        {
            // Arrange
            const int maxLength = 10000;
            var veryLargeText = new string('a', maxLength + 1);
            var controller = CreateControllerWithMockedContext(veryLargeText);

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal($"Input text cannot exceed {maxLength} characters.", result.Value);
        }

        [Fact]
        public async Task WordCount_ReturnsBadRequest_ForInputWithOnlyNumbers()
        {
            // Arrange
            const string input = "12345 67890";
            const string expectedErrorMessage = "Input text does not contain any valid words.";
            var controller = CreateControllerWithMockedContext(input);

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedErrorMessage, result.Value);
        }

        [Fact]
        public async Task WordCount_HandlesVariousWhitespaceCharacters()
        {
            // Arrange
            const string text = "word1 \t word2  word3\nword4\rword5";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Equal(1, wordCount["word1"]);
            Assert.Equal(1, wordCount["word2"]);
            Assert.Equal(1, wordCount["word3"]);
            Assert.Equal(1, wordCount["word4"]);
            Assert.Equal(1, wordCount["word5"]);
        }

        [Fact]
        public async Task WordCount_HandlesRepeatedWordsWithPunctuation()
        {
            // Arrange
            const string text = "cat, cat! cat?";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Equal(3, wordCount["cat"]);
        }

        [Fact]
        public async Task WordCount_IgnoresEmptyWordsAfterFiltering()
        {
            // Arrange
            const string text = "... , ! ? ";
            const string expectedErrorMessage = "Input text does not contain any valid words.";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedErrorMessage, result.Value);
        }

        [Fact]
        public async Task WordCount_HandlesEdgeCaseWithFewValidWords()
        {
            // Arrange
            const string text = "!@#$%^&*()hund";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Equal(1, wordCount["hund"]);
        }

        [Fact]
        public async Task WordCount_HandlesInputAtMaxWordLengthLimit()
        {
            // Arrange
            var text = new string('a', 99); // Text that is exactly 99 characters long
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode); 
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Single(wordCount); // Ensure only one word is counted
            Assert.Equal(1, wordCount[text]); // The word should be the entire string of 'a's
        }

        [Fact]
        public async Task WordCount_IgnoresWordsLongerThanMaxWordLength()
        {
            // Arrange
            const int maxWordLength = 100;
            var longWord = new string('a', maxWordLength + 1); // A word that is 101 characters long
            const string validWord = "validword";
            var text = $"{validWord} {longWord}";
            var controller = CreateControllerWithMockedContext(text);

            // Act
            var result = await controller.WordCount() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var wordCount = result.Value as Dictionary<string, int>;
            Assert.NotNull(wordCount);
            Assert.Equal(1, wordCount[validWord]); // Verify that the word "validword" appears exactly once in the word count dictionary
            Assert.False(wordCount.ContainsKey(longWord));
        }

    }
}