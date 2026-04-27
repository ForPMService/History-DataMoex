using History_DataMoex.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using History_DataMoex.Parsing;
using History_DataMoex.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace History_DataMoex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoexesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly MoexOptions _moexOptions;
        private readonly MoexHttpClient _moexHttpClient;

        public MoexesController(IConfiguration configuration, MoexHttpClient moexHttpClient, IOptions<MoexOptions> moexOptions)
        {
            _configuration = configuration;
            _moexHttpClient = moexHttpClient;
            _moexOptions = moexOptions.Value;
        }

        [HttpGet("GetSecuritiesChanges")]
        public async Task<IActionResult> GetSecuritiesChanges()
        {
            string url = "/calendars/stock/securities/changes.json";
            var response = await _moexHttpClient.GetRaws(url);
            System.IO.File.WriteAllText("stock,securities,changes.json", response);
            MoexTableParsing parsing = new MoexTableParsing();
            parsing.CreateTable(response);
            Console.WriteLine();
            return Content(response,"application/json");
            
            
            

        }
        [HttpGet("GetSecuritiesBoards")]
        public async Task<IActionResult> GetSecurities()
        {
            string url = "/calendars/stock/securities/boards.json";
            var response = await _moexHttpClient.GetRaws(url);
            System.IO.File.WriteAllText("stock,securities,boards.json", response);
            /*MoexTableParsing parsing = new MoexTableParsing();
            parsing.CreateTable(response);*/
            Console.WriteLine();
            return Content(response, "application/json");




        }

    }
}
