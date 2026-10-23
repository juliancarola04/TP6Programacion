using TP6Programacion.Backend.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TP6Programacion.Backend.Data;
using TP6Programacion.Backend.Implementacion;
using TP6Programacion.Backend.Options;
using TP6Programacion.Backend.Repositories;
using TP6Programacion.Backend.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CookieCsrfFilter>();
});

// Atención a esto, Sol. Esto es útil para evitar este problema https://www.youtube.com/watch?v=80S8h5hEwTY. 
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddCors(options =>
{
    if (builder.Environment.IsProduction())
    {
        options.AddPolicy("Web", policy =>
        {
            policy.WithOrigins("https://tpprogramacion.runasp.net")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    }
    else if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("Web", policy =>
        {
            policy.WithOrigins("https://localhost:7248")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    }
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration
            .GetRequiredSection("JwtSettings")
            .Get<JwtSettings>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            NameClaimType = ClaimTypes.Email,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                /* Atención a esto, Sol. Como este backend va a ser utilizado tanto para mobile como web vamos a tener que hacer ciertas consideraciones.
                Cuando nos logeemos en web vamos a almacenar el JWT en una cookie HttpOnly y, sumado a eso, se va a guardar un token CSRF. Ambos serán
                utilizados para validar que la persona que se intenta registrar es efectivamente la persona que es.
                Cuando nos logeemos en mobile vamos a almacenar el JWT en SecureStorage y será utilizado como autenticación a la hora de contactar a los endpoints,
                pero no va a tener una cookie como lo teníamos en web.*/
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Token = context.Request.Cookies["access_token"];
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var id = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(id, out var usuarioId))
                {
                    context.Fail("El identificador del usuario no es válido.");
                    return;
                }

                var repository = context.HttpContext.RequestServices
                    .GetRequiredService<IUsuarioRepository>();

                var usuario = await repository.BuscarPorId(usuarioId);

                if (usuario is null || usuario.Eliminado)
                {
                    context.Fail("La cuenta no está habilitada.");
                }
            }
        };
    });

builder.Services.AddScoped<ICategoriaRepository, CategoriaRepositoryPsqlEF>();
builder.Services.AddScoped<IClienteRepository, ClienteRepositoryPsqlEF>();
builder.Services.AddScoped<IImagenRepository, ImagenRepositoryPsqlEF>();
builder.Services.AddScoped<IIngresoRepository, IngresoRepositoryPsqlEF>();
builder.Services.AddScoped<IProductoRepository, ProductoRepositoryPsqlEF>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepositoryPsqlEF>();
builder.Services.AddScoped<IRegisterRepository, RegisterRepositoryPsqlEF>();
builder.Services.AddScoped<ISesionUsuarioRepository, SesionUsuarioRepositoryPsqlEF>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepositoryPsqlEF>();
builder.Services.AddScoped<IVentaRepository, VentaRepositoryPsqlEF>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ImagenService>();
builder.Services.AddScoped<IngresoService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<SesionService>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<ProveedorService>();
builder.Services.AddScoped<RegisterService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<VentaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Web");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

