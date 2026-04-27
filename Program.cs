using History_DataMoex.Options;
using History_DataMoex.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<MoexHttpIssClient>();
builder.Services.AddHttpClient<MoexHttpAlgClient>();
builder.Services.Configure<MoexIssOptions>(
    builder.Configuration.GetSection("MoexIss"));
builder.Services.Configure<MoexAlgOptions>(
    builder.Configuration.GetSection("MoexAlg"));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
