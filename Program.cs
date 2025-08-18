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
        Description = "Por favor, insira 'Bearer ' (com espaço) seguido do seu token JWT",
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

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
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
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/login", (UserLogin userLogin, IConfiguration config) =>
{
    if (userLogin.username == "admin" && userLogin.password == "12345")
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];
        var claims = new[] { new Claim("username", userLogin.username) };
        
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
    
    DateTime horaDeBrasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);
    var dataDeHoje = horaDeBrasilia.ToString("MM-dd-yyyy");
   
    var cacheKey = $"cotacao_{dataDeHoje}";
    
    if (cache.TryGetValue(cacheKey, out string cotacaoEmCache))
    {
        return Results.Content(cotacaoEmCache, "application/json");
    }

    var url = $"https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoDolarDia(dataCotacao=@dataCotacao)?@dataCotacao='{dataDeHoje}'&$format=json";
    var respostaDoBancoCentral = await client.GetStringAsync(url);
    
    var opcoesDeCache = new MemoryCacheEntryOptions();
    if (respostaDoBancoCentral.Contains("\"value\":[]")) 
    {
        opcoesDeCache.SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); 
    }
    else
    {
        opcoesDeCache.SetAbsoluteExpiration(TimeSpan.FromHours(1));
    }

    cache.Set(cacheKey, respostaDoBancoCentral, opcoesDeCache);
    
    return Results.Content(respostaDoBancoCentral, "application/json");

}).RequireAuthorization();



app.Run();

public record UserLogin(string username, string password);