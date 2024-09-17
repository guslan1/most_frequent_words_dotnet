using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.RegularExpressions;


namespace api.Controllers
{
    public class CountController : ControllerBase
    {
        private const int MaxTextLength = 10000;

        [HttpPost("count")]
        [Consumes("text/plain")]
        public async Task<IActionResult> WordCount()
        {

            if (Request.ContentType != "text/plain")
            {
                return BadRequest("Invalid content type. Only 'text/plain' is supported.");
            }

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var text = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("Input text cannot be empty or consist only of whitespace.");
            }

            if (text.Length > MaxTextLength)
            {
                return BadRequest($"Input text cannot exceed {MaxTextLength} characters.");
            }

            text = text.ToLower();

            string[] words = Regex.Split(text, @"\W+");

            Dictionary<string, int> wordCount = CountWords(words);

            if (wordCount.Count == 0)
            {
                return BadRequest("Input text does not contain any valid words.");
            }


            Dictionary<string, int> topTenWords = GetTopTenWords(wordCount);

            return Ok(topTenWords);
        }

        private Dictionary<string, int> CountWords(string[] words)
        {
            var wordCount = new Dictionary<string, int>();

            foreach (var word in words)
            {
                // Kontrollera att ordet inte är tomt, inte bara består av vita tecken och innehåller minst en bokstav
                if (!string.IsNullOrWhiteSpace(word) && Regex.IsMatch(word, @"[a-zA-Z]"))
                {
                    if (wordCount.ContainsKey(word))
                    {
                        wordCount[word]++;
                    }
                    else
                    {
                        wordCount[word] = 1;
                    }
                }
            }

            return wordCount;
        }


        private Dictionary<string, int> GetTopTenWords(Dictionary<string, int> wordCount)
        {
            return wordCount
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .Take(10)
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }

}