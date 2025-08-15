using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

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
        
        ValidateLifetime = false 
    };
});

builder.Services.AddAuthorization();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          
                          policy.WithOrigins("https://cotacao-frontend.vercel.app") 
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});



var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors(MyAllowSpecificOrigins);
app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) => {
   
    var tokenSecretoDoArquivo = app.Configuration["SecretToken"]; 

    if (!context.Request.Headers.TryGetValue("baseToken", out var token))
    {
        context.Response.StatusCode = 401; 
        await context.Response.WriteAsync("Token de autorização não fornecido no header.");
        return;
    }

    
    if (token != tokenSecretoDoArquivo) 
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Token de autorização inválido.");
        return;
    }

    await next(context);
});

app.MapGet("/", async () => {    
    var client = new HttpClient();
    var dataDeHoje = DateTime.Now.ToString("MM-dd-yyyy");
    var url = $"https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoDolarDia(dataCotacao=@dataCotacao)?@dataCotacao='{dataDeHoje}'&$format=json";
    var respostaDoBancoCentral = await client.GetStringAsync(url);
    return respostaDoBancoCentral;
});

app.Run();