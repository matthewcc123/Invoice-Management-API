using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.Enum;
using InvoiceManagement.Api.Mappings;
using InvoiceManagement.Api.Models;
using InvoiceManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllers().AddJsonOptions(options => 
{ 
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
        ); 
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new();

        document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
        {
            ["Bearer"] = new()
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            }
        };

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

//EF
var connectionString = builder.Configuration.GetConnectionString("SQLiteDefault");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

//JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Value,
        ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value!))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtService>();

//Build
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    //Scalar
    app.MapScalarApiReference(options =>
    {

        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

        options
        .AddPreferredSecuritySchemes("Bearer")
        .AddHttpAuthentication(
               "Bearer",
               auth =>
               {
                   auth.Token = "";
               }
           ).EnablePersistentAuthentication();


    });
}

app.UseHttpsRedirection();

//Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
