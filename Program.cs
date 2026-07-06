using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.Mappings;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllers();
builder.Services.AddOpenApi();

//EF
var connectionString = builder.Configuration.GetConnectionString("SQLiteDefault");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

//Build
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {

        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
