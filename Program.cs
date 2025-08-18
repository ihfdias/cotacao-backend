using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter 'Bearer ' followed by your JWT",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var corsPolicyName = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName,
                      policy =>
                      {
                          policy.WithOrigins("https://cotacao-frontend.vercel.app", "http://localhost:5173")
                                .WithMethods("GET", "POST", "OPTIONS")
                                .AllowAnyHeader();
                      });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", (UserLogin userLogin, IConfiguration config) =>
{
    if (userLogin.Username == "admin" && userLogin.Password == "12345")
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];
        var claims = new[] { new Claim("username", userLogin.Username) };
        
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var stringToken = tokenHandler.WriteToken(token);

        return Results.Ok(new { token = stringToken });
    }
    return Results.Unauthorized();
});

app.MapGet("/", async (HttpClient client, IMemoryCache cache) => {
    
    TimeZoneInfo brazilTimeZone;
    try
    {
        brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    }
    catch (TimeZoneNotFoundException)
    {
        brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
    }
    DateTime brasiliaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);
    var todayDate = brasiliaTime.ToString("MM-dd-yyyy");

    var cacheKey = $"quote_{todayDate}";
    
    if (cache.TryGetValue(cacheKey, out string cachedQuote))
    {
        return Results.Content(cachedQuote, "application/json");
    }

    var url = $"https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoDolarDia(dataCotacao=@dataCotacao)?@dataCotacao='{todayDate}'&$format=json";
    var centralBankResponse = await client.GetStringAsync(url);
    
    var cacheOptions = new MemoryCacheEntryOptions();
    if (centralBankResponse.Contains("\"value\":[]")) 
    {
        cacheOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
    }
    else
    {
        cacheOptions.SetAbsoluteExpiration(TimeSpan.FromHours(1));
    }

    cache.Set(cacheKey, centralBankResponse, cacheOptions);
    
    return Results.Content(centralBankResponse, "application/json");

}).RequireAuthorization();

app.Run();

public record UserLogin(string Username, string Password);
