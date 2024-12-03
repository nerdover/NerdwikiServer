using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using NerdwikiServer;
using NerdwikiServer.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterCoreServices(builder.Configuration);
builder.Services.AddOpenApi();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapAuthApi();
app.MapCategoriesApi();
app.MapTopicsApi();
app.MapTagsApi();
app.MapLessonsApi();
app.MapUploadApi();

app.Run();