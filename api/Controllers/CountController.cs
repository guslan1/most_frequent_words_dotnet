using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

//    if (Request.Body == null)
//     {
//         return BadRequest("Request body cannot be null.");
//     }


namespace api.Controllers
{
    public class CountController : ControllerBase
    {

        [HttpPost("count")]
        [Consumes("text/plain")]
        public async Task<IActionResult> WordCount()
        {
            // Läs in texten från Request Body
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var text = await reader.ReadToEndAsync();

            // Kontrollera om texten är tom eller bara innehåller vita tecken
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("Input text cannot be empty or consist only of whitespace.");
            }

            text = text.ToLower();

            // Dela upp texten i ord med hjälp av Regex
            var words = Regex.Split(text, @"\W+");

            // Vi använder dictionary för att enkelt och effektivt kunna söka och räkna antalet förekomster av varje ord
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

            // Sortera orden efter deras förekomst och ta de tio största
            var topTenWords = wordCount
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .Take(10)
                .ToDictionary(x => x.Key, x => x.Value);

            // Returnera resultatet
            return Ok(topTenWords);
        }
    }

}