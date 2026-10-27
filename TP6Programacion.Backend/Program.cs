using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Data.Common;
using System.Security.Claims;
using System.Text;
using TP6Programacion.Backend.Data;
using TP6Programacion.Backend.Implementacion;
using TP6Programacion.Backend.Options;
using TP6Programacion.Backend.PasarelaSimulada.Pagos;
using TP6Programacion.Backend.Repositories;
using TP6Programacion.Backend.Services;
using TP6Programacion.Backend.Utilidades;

var bootstrapLogger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Logger = bootstrapLogger;

try
{
    Log.Information("Iniciando aplicación TP6Programacion.Backend...");
    var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddControllers();

    string[] origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
        options.AddPolicy("Web", policy => policy
            .WithOrigins(origenes)
            .AllowAnyHeader()
            .AllowAnyMethod()));

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
        builder.Services.AddScoped<IPasarelaPago, PasarelaPagoSimulada>();
        builder.Services.AddScoped<IPagoRepository, PagoRepositoryPsqlEF>();



        builder.Services.AddScoped<CategoriaService>();
        builder.Services.AddScoped<ClienteService>();
        builder.Services.AddScoped<ImagenService>();
        builder.Services.AddScoped<IngresoService>();
        builder.Services.AddScoped<LoginService>();
        builder.Services.AddScoped<ProductoService>();
        builder.Services.AddScoped<ProveedorService>();
        builder.Services.AddScoped<PagoService>();
        builder.Services.AddScoped<RegisterService>();
        builder.Services.AddScoped<SesionService>();
        builder.Services.AddScoped<UsuarioService>();
        builder.Services.AddScoped<VentaService>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            await SeedDatos.CrearAdministrador(context);
        }

        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exceptionFeature =
                    context.Features.Get<IExceptionHandlerFeature>();

                Exception? exception = exceptionFeature?.Error;

                var logger = context.RequestServices
                    .GetRequiredService<ILogger<Program>>();

                context.Response.ContentType = "application/json";
                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;

                DbException? dbException =
                    BuscarDbExceptionEnCadena(exception);

                if (dbException is not null)
                {
                    logger.LogError(
                        exception,
                        "Error de base de datos al procesar {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);

                    await context.Response.WriteAsJsonAsync(
                        "Ocurrió un error interno en el servidor. Por favor, intentá nuevamente más tarde.");
                }
                else
                {
                    logger.LogError(
                        exception,
                        "Excepción no controlada al procesar {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);

                    await context.Response.WriteAsJsonAsync(
                        "Ocurrió un error interno en el servidor.");
                }
            });
        });
        static DbException? BuscarDbExceptionEnCadena(Exception? ex)
        {
            while (ex is not null)
            {
                if (ex is DbException dbEx)
                    return dbEx;

                ex = ex.InnerException;
            }

            return null;
        }

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseCors("Web");
        var imagenesPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "uploads");
        Directory.CreateDirectory(imagenesPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(imagenesPath),
            RequestPath = "/uploads"
        });
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        Log.Information("Aplicación iniciada correctamente.");

        app.Run();
}

catch (Exception ex)

{
    Log.Fatal(ex,"Fallo crítico no controlado durante el inicio de la aplicación.");
}
finally
{
    Log.CloseAndFlush();
}




