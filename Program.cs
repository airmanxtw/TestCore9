using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

// * https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/using-openapi-documents?view=aspnetcore-9.0#use-scalar-for-interactive-api-documentation

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
var pv = Path.Combine(env.ContentRootPath, "Keys", "Private.pem");
var pb = Path.Combine(env.ContentRootPath, "Keys", "Public.pem");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// * https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "測試.NET9 API",
            Version = "v1",
            Description = "API for processing checkouts from cart."
        };
        return Task.CompletedTask;
    });
});


var publicKey = RSA.Create();
var PKeyText = File.ReadAllText(pb);
var PVKeyText = File.ReadAllText(pv);
publicKey.ImportFromPem(PKeyText);
var rkey = new RsaSecurityKey(publicKey);

//配置認證服務
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        //是否驗證發行人
        ValidateIssuer = true,
        ValidIssuer = "STUST",//發行人
                              //是否驗證受眾人
        ValidateAudience = true,
        ValidAudience = "STUST",//受眾人
                                //是否驗證金鑰
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = rkey, //new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretCredentials)),
        ValidateLifetime = true, //驗證生命週期
        RequireExpirationTime = true, //過期時間
        ClockSkew = TimeSpan.Zero
    };

});

builder.Services.AddScoped<TestCore9.Api.Interface.IClimsApi>(x => new TestCore9.Api.ClimsApi(PVKeyText));


builder.Services.AddControllers();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference(options =>
   {
       //options.WithEndpointPrefix("/api-reference/{documentName}");
       options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

       options.WithTheme(ScalarTheme.BluePlanet).WithDarkMode(true);

   });

if (app.Environment.IsDevelopment())
{

}

app.UseRouting();

//1.先開啟認證
app.UseAuthentication();
//2.再開啟授權
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();




// * Bearer
// * https://www.answeroverflow.com/m/1306435010865401907
// * https://learn.microsoft.com/zh-tw/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0
internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            // Apply it as a requirement for all operations
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                if (operation.Value.Responses.Any(v => v.Key == "401"))
                {
                    operation.Value.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                    });
                }
            }
        }
    }
}


