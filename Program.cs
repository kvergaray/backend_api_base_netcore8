using backend_api_base_netcore8.Application.DTOs;
using backend_api_base_netcore8.Application.Interfaces;
using backend_api_base_netcore8.Application.Services;
using backend_api_base_netcore8.Application.Validators;
using backend_api_base_netcore8.Infrastructure.Data;
using backend_api_base_netcore8.Infrastructure.Repositories;
using backend_api_base_netcore8.Infrastructure.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "API para autenticacion mediante JWT."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Introduce el token JWT con el esquema Bearer."
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddLogging();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
    }

    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    throw new InvalidOperationException("JWT configuration is missing the signing key.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) || string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    throw new InvalidOperationException("JWT configuration requires both Issuer and Audience.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", async (
        LoginRequest request,
        IAuthService authService,
        IValidator<LoginRequest> validator,
        CancellationToken cancellationToken) =>
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var loginResponse = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (loginResponse is null)
        {
            return Results.Json(
                new { error = "Invalid credentials" },
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Results.Ok(loginResponse);
    })
    .WithName("Login")
    .WithTags("Auth")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Autentica un usuario y emite un JWT.";
        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["email"] = new OpenApiString("admin@local"),
                        ["password"] = new OpenApiString("P@ssw0rd!")
                    }
                }
            }
        };

        operation.Responses["200"] = new OpenApiResponse
        {
            Description = "Login exitoso",
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["token"] = new OpenApiString("<jwt>"),
                        ["expiresIn"] = new OpenApiInteger(3600),
                        ["user"] = new OpenApiObject
                        {
                            ["id"] = new OpenApiInteger(1),
                            ["roleId"] = new OpenApiInteger(2),
                            ["name"] = new OpenApiString("Doe"),
                            ["firstName"] = new OpenApiString("John"),
                            ["email"] = new OpenApiString("john@acme.com"),
                            ["degreeId"] = new OpenApiInteger(3),
                            ["phone"] = new OpenApiLong(9999999999),
                            ["cip"] = new OpenApiLong(12345678)
                        }
                    }
                }
            }
        };

        operation.Responses["401"] = new OpenApiResponse
        {
            Description = "Credenciales invalidas",
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = new OpenApiObject
                    {
                        ["error"] = new OpenApiString("Invalid credentials")
                    }
                }
            }
        };

        return operation;
    })
    .Produces<LoginResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized)
    .ProducesValidationProblem();

app.Run();
