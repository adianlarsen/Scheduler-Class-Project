using GroupSchedulerApp.Data;
using GroupSchedulerApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Models contain circular relationships, such as
        // Group.Members -> User.Groups. Skip back-references in API responses.
        // JsonDataStore separately preserves relationships in the saved file.
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// All repositories share one store and one file-access lock.
builder.Services.AddSingleton<JsonDataStore>(_ =>
    new JsonDataStore(Path.Combine(
        builder.Environment.ContentRootPath,
        "App_Data",
        "scheduler.json")));

builder.Services.AddScoped<GroupRepository>();
builder.Services.AddScoped<MeetingRepository>();
builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<ScheduleRepository>();

var app = builder.Build();

// The JSON file is created on the first successful storage update.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.Run();