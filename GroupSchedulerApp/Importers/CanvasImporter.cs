using System.Net.Http;
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Importers;

// Imports course names and identifiers from Canvas.
//
// INTENDED FLOW:
// 1. The backend calls GetCoursesAsync after Canvas access is configured.
// 2. The frontend displays the returned courses.
// 3. The user selects courses and enters class meeting days/times.
// 4. The backend converts those entries into schedule busy blocks.
// 5. The backend saves the schedule using JsonDataStore.
//
// RESPONSIBILITIES:
// This importer retrieves course information only.
// It does not determine class times, create busy blocks, or save data.
//
// STATUS:
// Starter code only. The Canvas request and response parsing are not
// implemented yet. Token configuration will be added separately.
public class CanvasImporter
{
    private readonly HttpClient _httpClient;

    // The backend supplies HttpClient when registering this importer.
    // Reuse this client rather than creating a new one per request.
    public CanvasImporter(HttpClient httpClient)
    {
        _httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
    }

    // Returns courses available to the connected Canvas user.
    //
    // The frontend should allow the user to choose which courses belong
    // in their schedule. Importing a course does not imply attendance
    // at any particular time.
    //
    // Cancellation lets the backend stop a request that is no longer needed.
    public Task<List<CanvasCourse>> GetCoursesAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TODO: Obtain the Canvas site URL and access token securely.
        // Never hardcode tokens or save them in scheduler.json.

        // TODO: Use _httpClient to request the user's Canvas courses.
        // Send authorization with each request for the correct user.

        // TODO: Follow Canvas pagination so all courses are retrieved.
        // Only send credentials to the configured, trusted Canvas site.

        // TODO: Handle authentication errors and unsuccessful responses.
        // A failed import must not be treated as an empty course list.

        // TODO: Map Canvas course IDs, names, and course codes into
        // CanvasCourse objects. Do not infer class meeting times.

        // Throw until implemented so callers know this is unfinished.
        throw new NotImplementedException(
            "Canvas course importing has not been implemented yet.");
    }
}