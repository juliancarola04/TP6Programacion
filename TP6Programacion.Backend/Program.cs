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
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddScoped<ICategoriaRepository, CategoriaRepositoryPsqlEF>();
builder.Services.AddScoped<IClienteRepository, ClienteRepositoryPsqlEF>();
builder.Services.AddScoped<IImagenRepository, ImagenRepositoryPsqlEF>();
builder.Services.AddScoped<IIngresoRepository, IngresoRepositoryPsqlEF>();
builder.Services.AddScoped<IProductoRepository, ProductoRepositoryPsqlEF>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepositoryPsqlEF>();
builder.Services.AddScoped<IRegisterRepository, RegisterRepositoryPsqlEF>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepositoryPsqlEF>();
builder.Services.AddScoped<IVentaRepository, VentaRepositoryPsqlEF>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ImagenService>();
builder.Services.AddScoped<IngresoService>();
builder.Services.AddScoped<LoginService>();
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
app.UseAuthorization();

app.MapControllers();

app.Run();

