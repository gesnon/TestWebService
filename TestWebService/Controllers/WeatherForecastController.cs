using Microsoft.AspNetCore.Mvc;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace TestWebService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private const string PipeName = "LoodsmanPipe";
        //LoodsmanApiService _loodsmanApiService;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            //_loodsmanApiService = loodsmanApiService;
        }

        [HttpGet("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("GetPuctutes")]
        public async Task<IActionResult> GetPuctutes(string name)
        {

            var response = await SendToConsoleAsync(new PipeRequest { Text = name });

            if (!response.Ok)
            {
                if (response.Error== "404")
                {
                    return NotFound();
                }
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(response.ImageBase64);
            }
            catch (FormatException)
            {
                return Problem("Console app returned invalid image data.");
            }

            return File(bytes, "image/png");


            //string path = _loodsmanApiService.GetPath(name);

            //if (string.IsNullOrEmpty(path))
            //{
            //    return NotFound();
            //}
            //if (string.IsNullOrEmpty(path) || path == "Занято")
            //{
            //    return StatusCode(StatusCodes.Status503ServiceUnavailable);
            //}

            //byte[] bData = Service.GetBytes(path);

            //return File(bData, "image/jpeg");
        }

        private static async Task<PipeResponse> SendToConsoleAsync(PipeRequest request)
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
            await client.ConnectAsync(1000);

            using var reader = new StreamReader(client, Encoding.UTF8);
            using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };

            var json = JsonSerializer.Serialize(request);
            await writer.WriteLineAsync(json);

            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            return JsonSerializer.Deserialize<PipeResponse>(line);
        }

        public class PipeResponse
        {
            [JsonPropertyName("ok")]
            public bool Ok { get; set; }

            [JsonPropertyName("imageBase64")]
            public string ImageBase64 { get; set; }

            [JsonPropertyName("error")]
            public string Error { get; set; }
        }
        public class PipeRequest
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
        }
    }
}
