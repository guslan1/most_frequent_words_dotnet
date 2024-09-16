using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace most_frequent_words_dotnet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CountController : ControllerBase
    {

        [HttpPost("count")]
        [Consumes("text/plain")]
        public async Task<IActionResult> WordCount()
        {
            var body = await Request.BodyReader.ReadAsync();
            var text = Encoding.UTF8.GetString(body.Buffer.First.Span);
            var words = Regex.Split(text, @"\W+");
            var wordCount = new Dictionary<string, int>();
            foreach (var word in words)
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
            var sortedWordCount = wordCount.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            return Ok(sortedWordCount);
        }
    }

}