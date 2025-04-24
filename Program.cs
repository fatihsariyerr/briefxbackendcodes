using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add HttpClient service
builder.Services.AddHttpClient(); // HttpClient'ı burada ekleyin
builder.Services.AddHostedService<RssBackgroundService>();
builder.Services.AddScoped<NewsService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Swagger UI başlığını değiştirme
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BriefX Api",
        Version = "v1",
        Description = "BriefX API Documentation", // İsteğe bağlı açıklama
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
