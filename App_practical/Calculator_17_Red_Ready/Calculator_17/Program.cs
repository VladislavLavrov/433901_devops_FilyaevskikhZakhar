using Calculator_17.Data;
using Microsoft.EntityFrameworkCore;
using Calculator_17.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Не задана строка подключения DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
    connectionString,
    new MariaDbServerVersion(new Version(11, 4, 0))
));
builder.Services.AddControllersWithViews();

// Регистрация Kafka-сервисов
builder.Services.AddSingleton<CalculationResultStore>();
builder.Services.AddSingleton<KafkaProducerService>();

builder.Services.AddHttpClient();

builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

// Создаём базу и таблицы при первом запуске. В Docker Compose приложение
// стартует только после проверки готовности MariaDB.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");
app.Run();
