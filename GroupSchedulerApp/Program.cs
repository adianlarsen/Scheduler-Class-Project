using GroupSchedulerApp.Data;
using GroupSchedulerApp.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Layer 2: controllers
builder.Services.AddControllers();

// Layer 4: database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Layer 3: repositories
builder.Services.AddScoped<GroupRepository>();
builder.Services.AddScoped<ScheduleRepository>();
builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<MeetingRepository>();

var app = builder.Build();

// Create the database file and tables on first run
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

// Layer 1: serve the HTML/CSS/JS in wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.Run();