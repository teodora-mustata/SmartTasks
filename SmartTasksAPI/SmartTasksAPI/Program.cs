
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.Models.Data;
using SmartTasksAPI.Repositories;
using SmartTasksAPI.Services;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Linq;

namespace SmartTasksAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IBoardRepository, BoardRepository>();
            builder.Services.AddScoped<IListRepository, ListRepository>();
            builder.Services.AddScoped<ICardRepository, CardRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IBoardService, BoardService>();
            builder.Services.AddScoped<IListService, ListService>();
            builder.Services.AddScoped<ICardService, CardService>();
            builder.Services.AddScoped<ICommentService, CommentService>();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Ensure database is created and migrations are applied on startup.
            // This will run once per environment (the first time the DB is created in the Docker volume).
            if (!app.Environment.IsEnvironment("Testing"))
            {
                using (var scope = app.Services.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    const int maxRetries = 15;
                    var delay = TimeSpan.FromSeconds(5);
                    for (int attempt = 0; attempt < maxRetries; attempt++)
                    {
                        try
                        {
                            var pending = db.Database.GetPendingMigrations();
                            if (pending.Any())
                            {
                                db.Database.Migrate();
                                logger.LogInformation("Applied {Count} pending EF migrations.", pending.Count());
                            }
                            else
                            {
                                // No migrations found - ensure database is created to support dev scenarios without migrations
                                db.Database.EnsureCreated();
                                logger.LogInformation("No EF migrations found; ensured database is created.");
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Database unavailable, retrying in {Delay}s (attempt {Attempt}/{Max})", delay.TotalSeconds, attempt + 1, maxRetries);
                            if (attempt == maxRetries - 1)
                            {
                                logger.LogError(ex, "Failed to apply database migrations after {Max} attempts.", maxRetries);
                                throw;
                            }
                            Thread.Sleep(delay);
                        }
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsEnvironment("Testing"))
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
