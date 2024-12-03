using NerdwikiServer;
using NerdwikiServer.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterCoreServices(builder.Configuration);
builder.Services.AddOpenApi();

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