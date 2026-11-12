using System.Text.Json;
using System.Text.Json.Serialization;

namespace GroupSchedulerApp.Data;

// Shared JSON storage for the scheduler.
//
// BACKEND INTEGRATION:
// Register ONE instance as a singleton in Program.cs and inject it
// into repositories. All repositories must share that instance.
//
// Use Read for queries and Update for changes.
// Do not call Read or Update from inside another Read/Update callback.
//
// This implementation supports one running application process.
// Multiple servers must not write to the same JSON file.
public class JsonDataStore
{
    private readonly string _filePath;
    private readonly object _fileLock = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,

        // Our models contain circular relationships:
        // Group.Members -> User.Groups -> Group.
        //
        // Preserve saves $id and $ref metadata and reconstructs shared
        // references when loading. These markers are NOT model IDs.
        ReferenceHandler = ReferenceHandler.Preserve,

        // Store task statuses as readable names such as "ToDo".
        Converters = { new JsonStringEnumConverter() }
    };

    // Pass a path such as:
    // <project folder>/App_Data/scheduler.json
    //
    // Keep this file outside wwwroot because it contains user data.
    public JsonDataStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "A JSON storage path is required.",
                nameof(filePath));
        }

        _filePath = Path.GetFullPath(filePath);

        var directory = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(directory);
    }

    // Loads a fresh snapshot and returns the query result.
    //
    // Returned objects are detached snapshots. Editing them later
    // does not save changes. Use Update to persist modifications.
    public TResult Read<TResult>(Func<AppData, TResult> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        lock (_fileLock)
        {
            return query(Load());
        }
    }

    // Loads, modifies, and saves data as one locked operation.
    // This prevents overlapping requests from losing each other's edits.
    //
    // If the callback throws, the modified snapshot is not saved.
    // Keep callbacks synchronous and limited to working with the data;
    // make Canvas API requests BEFORE entering this method.
    public TResult Update<TResult>(Func<AppData, TResult> change)
    {
        ArgumentNullException.ThrowIfNull(change);

        lock (_fileLock)
        {
            var data = Load();
            var result = change(data);

            Save(data);

            return result;
        }
    }

    // Convenience overload for changes that do not return a result.
    public void Update(Action<AppData> change)
    {
        ArgumentNullException.ThrowIfNull(change);

        Update<bool>(data =>
        {
            change(data);
            return true;
        });
    }

    // Only call while holding _fileLock.
    private AppData Load()
    {
        // A new project starts with empty collections.
        // The JSON file is created on the first successful Update.
        if (!File.Exists(_filePath))
        {
            return new AppData();
        }

        var json = File.ReadAllText(_filePath);

        try
        {
            return JsonSerializer.Deserialize<AppData>(
                json,
                _jsonOptions)
                ?? throw new InvalidDataException(
                    "The scheduler JSON file contains null instead of data.");
        }
        catch (JsonException exception)
        {
            // Never silently replace unreadable data with empty data.
            // Leave the original file intact so it can be investigated.
            throw new InvalidDataException(
                "The scheduler JSON file is invalid. " +
                "Existing data has not been replaced.",
                exception);
        }
    }

    // Only call while holding _fileLock.
    private void Save(AppData data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);

        // Write a complete temporary file in the same directory before
        // replacing the saved file. This reduces the risk of leaving
        // a partially written scheduler.json if writing fails.
        var temporaryPath =
            _filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}