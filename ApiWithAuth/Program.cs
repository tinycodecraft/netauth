using System.Text;
using ApiWithAuth;
using ApiWithAuth.Abstraction;
using ApiWithAuth.Models;
using ApiWithAuth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NSwag.Generation.Processors.Security;

var builder = WebApplication.CreateBuilder(args);
var authsetting = builder.Configuration.GetSection(Constants.Setting.AuthSetting);
var encryptionService = new StringEncrypService();
authsetting[nameof(AuthSetting.Secret)] = encryptionService.EncryptString(authsetting[nameof(AuthSetting.SecretKey)] ?? "");

builder.Services.Configure<AuthSetting>(authsetting);

// Add services to the container.
builder.Services.AddDbContext<UsersContext>();
builder.Services.AddScoped<TokenService, TokenService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(option =>
{
    option.Version = "v1";
    option.Title = authsetting[nameof(AuthSetting.Issuer)];
    option.Description = authsetting[nameof(AuthSetting.Issuer)];
    option.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Type into the textbox:Bearer {your JWT token}"
    });
    option.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));


});

builder.Services
    .AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        
    })    
    .AddJwtBearer(options =>
    {
        
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authsetting[nameof(AuthSetting.Issuer)],
            ValidAudience = authsetting[nameof(AuthSetting.Audience)],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(authsetting[nameof(AuthSetting.Secret)] ?? "")
            ),
        };
    });

builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<UsersContext>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseOpenApi();
    app.UseSwaggerUi3();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();