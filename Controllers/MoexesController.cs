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
        private readonly MoexAlgOptions _moexOptions;
        private readonly MoexHttpIssClient _moexHttpIssClient;
        private readonly MoexHttpAlgClient _moexHttpAlgClient;

        public MoexesController(IConfiguration configuration, MoexHttpIssClient moexHttpIssClient, MoexHttpAlgClient moexHttpAlgClient, IOptions<MoexAlgOptions> moexOptions)
        {
            _configuration = configuration;
            _moexHttpIssClient = moexHttpIssClient;
            _moexHttpAlgClient = moexHttpAlgClient;
            _moexOptions = moexOptions.Value;
        }

        [HttpGet("GetSecuritiesChanges")]
        public async Task<IActionResult> GetSecuritiesChanges()
        {
            string url = "/calendars/stock/securities/changes.json";
            var response = await _moexHttpIssClient.GetRaws(url);
            System.IO.File.WriteAllText("stock,securities,changes.json", response);
            CompanyCardParsing parsing = new CompanyCardParsing();
            parsing.CreateTable(response);
            Console.WriteLine();
            return Content(response,"application/json");
            
            
            

        }
        [HttpGet("GetSecuritiesBoards")]
        public async Task<IActionResult> GetSecurities()
        {
            string url = "/calendars/stock/securities/boards.json";
            var response = await _moexHttpIssClient.GetRaws(url);
            System.IO.File.WriteAllText("stock,securities,boards.json", response);
            /*MoexTableParsing parsing = new MoexTableParsing();
            parsing.CreateTable(response);*/
            Console.WriteLine();
            return Content(response, "application/json");




        }

    }
}
