using API.Configuration;
using API.Data;
using API.Exceptions;
using API.Interfaces;
using API.Middleware;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors();

var configuredJwtOptions = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.SectionName).Bind(configuredJwtOptions);
if (string.IsNullOrWhiteSpace(configuredJwtOptions.Key))
{
    configuredJwtOptions.Key = builder.Configuration["TokenKey"];
}

if (string.IsNullOrWhiteSpace(configuredJwtOptions.Key))
{
    throw new InvalidOperationException("JWT key is missing. Configure Jwt:Key or TokenKey in appsettings.");
}

builder.Services.Configure<JwtOptions>(options =>
{
    options.Key = configuredJwtOptions.Key;
    options.Issuer = configuredJwtOptions.Issuer;
    options.Audience = configuredJwtOptions.Audience;
    options.ExpiryDays = configuredJwtOptions.ExpiryDays;
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuredJwtOptions.Key)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(configuredJwtOptions.Issuer),
            ValidIssuer = configuredJwtOptions.Issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(configuredJwtOptions.Audience),
            ValidAudience = configuredJwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200", "https://localhost:4200"));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
