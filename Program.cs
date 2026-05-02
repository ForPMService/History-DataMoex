using History_DataMoex.Options;
using History_DataMoex.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
builder.Services.AddHttpClient<MoexHttpIssClient>();
builder.Services.AddHttpClient<MoexHttpAlgClient>();
builder.Services.Configure<MoexIssOptions>(
    builder.Configuration.GetSection("MoexIss"));
builder.Services.Configure<MoexAlgOptions>(
    builder.Configuration.GetSection("MoexAlg"));


builder.Services.AddOpenApi();

var app = builder.Build();
app.MapGet("/GetStockMarkets", async (MoexHttpIssClient moexHttpIssClient) => {
    string url = "https://iss.moex.com/iss/engines/stock/markets/shares/boards/tqbr/securities.json";
    string response = await moexHttpIssClient.GetInfoTradedStockAssets(url);
    return Results.Content(response, "application/json");
});

app.UseHttpsRedirection();

app.Run();
