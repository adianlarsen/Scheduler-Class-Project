using GroupSchedulerApp.Data;
using GroupSchedulerApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Layer 2: controllers
builder.Services.AddControllers();

// Layer 4: JSON storage
// One shared instance keeps file updates synchronized.
// Store data outside wwwroot so it is not publicly downloadable.
builder.Services.AddSingleton<JsonDataStore>(_ =>
{
    var filePath = Path.Combine(
        builder.Environment.ContentRootPath,
        "App_Data",
        "scheduler.json");

    return new JsonDataStore(filePath);
});

// Layer 3: repositories
//
// TODO (Jace): Finish connecting the backend to JSON storage.
// - Convert GroupRepository and MeetingRepository from AppDbContext
//   to JsonDataStore, using its Read and Update methods.
// - Create ScheduleRepository and TaskRepository with the methods
//   their controllers need, then register them below.
// - Check controllers for references to those missing classes.
//
// IMPORTANT: JSON integration is not finished. The existing group
// and meeting repositories still require AppDbContext, which is no
// longer registered. Their endpoints will fail until converted.
builder.Services.AddScoped<GroupRepository>();
builder.Services.AddScoped<MeetingRepository>();

// Uncomment after these classes are implemented:
// builder.Services.AddScoped<ScheduleRepository>();
// builder.Services.AddScoped<TaskRepository>();

var app = builder.Build();

// JsonDataStore creates scheduler.json on the first successful update.
// SQLite database initialization is no longer used.

// Layer 1: serve the HTML/CSS/JS in wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.Run();