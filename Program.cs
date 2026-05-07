using History_DataMoex.Options;
using History_DataMoex.Clients;
using History_DataMoex.Parsing;
using History_DataMoex.DataTransfers;
using History_DataMoex.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы в контейнер.

builder.Services.AddHttpClient<MoexHttpIssClient>();
builder.Services.AddHttpClient<MoexHttpAlgClient>();
builder.Services.AddHttpClient<MoexHttpCalendarClient>();
builder.Services.Configure<MoexIssOptions>(
    builder.Configuration.GetSection("MoexIss"));
builder.Services.Configure<MoexAlgOptions>(
    builder.Configuration.GetSection("MoexAlg"));


builder.Services.AddOpenApi();

var app = builder.Build();

// Экспонируемые эндпоинты исходных данных: возвращают DTO MOEX напрямую.
// Это не финальное production API. В будущем /v1 эндпойнты будут возвращать канонические модели.
app.MapReferenceEndpoints();

// ALGOPACK-эндпоинты исходных данных: возвращают DTO MOEX напрямую.
// Жёстко заданные инструменты и диапазоны дат сохранены в рамках этой задачи.
app.MapAlgopackEndpoints();

// Календарные эндпоинты исходных данных: возвращают DTO MOEX напрямую.
// В будущем календарный /v1 API будет использовать нормализованные модели календаря.
app.MapCalendarEndpoints();

app.UseHttpsRedirection();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
