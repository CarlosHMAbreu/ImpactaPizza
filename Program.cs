using Microsoft.EntityFrameworkCore;
using Pizzaria.Data;

var builder = WebApplication.CreateBuilder(args);

// Add serviços ao container.
builder.Services.AddControllersWithViews();

// Configuração do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 26))));

// Adicionar suporte a sessões
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Adicione suporte a sessões
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configura o pipeline de requisições HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Middleware para log de operações de banco de dados
IApplicationBuilder applicationBuilder = app.Use(async (context, next) =>
{
    // Intercepta requests para detectar problemas
    if (context.Request.Path.StartsWithSegments("/Home/FazerPedido"))
    {
        Console.WriteLine($"Request para FazerPedido: {context.Request.Method}");
    }
    
    await next();
});

// Usar sessões
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseMiddleware<Pizzaria.Middleware.AdminAuthorizationMiddleware>();

var controllerActionEndpointConventionBuilder = app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();