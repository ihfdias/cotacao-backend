var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("baseToken", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "baseToken",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Description = "Token de autorização para acessar a API."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "baseToken"
                }
            },
            new string[] {}
        }
    });
});

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