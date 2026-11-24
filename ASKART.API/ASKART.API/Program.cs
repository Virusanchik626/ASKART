using ASKART.API.Data;
using ASKART.API.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=db;Port=3306;Database=askart;User=root;Password=1234;SslMode=None;";

builder.Services.AddDbContext<AskartContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.Parse("8.0.46-mysql"),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    ));

builder.Services.AddScoped<ASKART.API.Services.UserAuthenticationService>();
builder.Services.AddScoped<ASKART.API.Services.AdminQueryService>();
builder.Services.AddScoped<ASKART.API.Services.AdminStatisticsService>();
builder.Services.AddScoped<ASKART.API.Services.AdminModerationService>();
builder.Services.AddScoped<ASKART.API.Services.AdminChatNamesService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var app = builder.Build();

// Ждём, пока база проснётся, и создаём таблицы, если их нет
using (var scope = app.Services.CreateScope())
{
    AskartContext db = scope.ServiceProvider.GetRequiredService<AskartContext>();

    int attempt = 0;
    bool ready = false;

    while (ready == false && attempt < 10)
    {
        try
        {
            db.Database.EnsureCreated();
            ready = true;
            Console.WriteLine("База готова, таблицы на месте");
        }
        catch (Exception ex)
        {
            attempt++;
            Console.WriteLine("База не готова (попытка " + attempt + "): " + ex.Message);
            Thread.Sleep(3000);
        }
    }
}

// Все ошибки показываем нормальным текстом (должно стоять ПЕРВЫМ)
app.UseExceptionHandler("/error");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS-редирект убрал: клиент ходит по http://localhost:8080
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();