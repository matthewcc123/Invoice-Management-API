using InvoiceManagement.Api.Data;
using InvoiceManagement.Api.Enum;
using InvoiceManagement.Api.Mappings;
using InvoiceManagement.Api.Middleware;
using InvoiceManagement.Api.Models;
using InvoiceManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

//Controller
builder.Services.AddControllers().AddJsonOptions(options => 
{ 
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
        ); 
});

//Open Api
builder.Services.AddOpenApi(options =>
{
    //Scalar Transofmer
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

    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type.IsEnum)
        {
            schema.Type = "string";
            schema.Format = null;
            schema.Enum.Clear();

            foreach (var name in Enum.GetNames(context.JsonTypeInfo.Type))
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(name));
            }
        }

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

//Config
builder.Services.Configure<RouteOptions>( options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

//Logging
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
});

//Auth
builder.Services.AddAuthorization();

//Service
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<BarcodeService>();


//Build
var app = builder.Build();

//Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();

    // Apply migrations only once
    await context.Database.MigrateAsync();

    var seeder = services.GetRequiredService<SeedService>();
    await seeder.SeedDataAsync();
}

//Configure the HTTP request pipeline.
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

//Middleware
app.UseMiddleware<ExceptionMiddleware>();

//Auth
app.UseAuthentication();
app.UseAuthorization();

//wwwroot
app.UseStaticFiles();

app.MapControllers();

app.Run();
