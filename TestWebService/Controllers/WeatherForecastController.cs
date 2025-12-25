using LoodsmanApiProject;
using Microsoft.AspNetCore.Mvc;
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

        LoodsmanApiService _loodsmanApiService;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, LoodsmanApiService loodsmanApiService)
        {
            _logger = logger;
            _loodsmanApiService = loodsmanApiService;
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
        public IActionResult GetPuctutes(string name)
        {

            string path = _loodsmanApiService.GetPath(name);

            if (string.IsNullOrEmpty(path))
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(path) || path == "Занято")
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            byte[] bData = Service.GetBytes(path);

            return File(bData, "image/jpeg");
        }
    }
}
