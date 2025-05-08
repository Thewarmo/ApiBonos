using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BonosEsteticaApi.Services;
using BonosEsteticaApi.Data;
using BonosEsteticaApi.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar servicios de base de datos
builder.Services.AddSingleton<DatabaseConnection>();
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<ProcedimientoRepository>();
builder.Services.AddScoped<BonoRepository>();
builder.Services.AddScoped<HistorialBonoRepository>();
// Registrar otros repositorios según sea necesario

// Registrar el servicio de tokens
builder.Services.AddScoped<TokenService>();

// Agregar servicios de autenticación
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Agregar servicios de autorización
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Agregar middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
