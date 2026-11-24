# Group Scheduler
Adian Larsen, Jace Nielson, Ben Seamons
We are making a group scheduler that will allow users to enter their schedule information and coordinate available times.


## Tech Stack:
Frontend: HTMl, CSS, JavaScript
Backend and Database: C#


## Prerequisites
.NET SDK 10.0
Web Browser


## Step By Step Setup
1. Clone the repository:
   ```
   git clone https://github.com/adianlarsen/Scheduler-Class-Project.git
   cd Scheduler-Class-Project
   ```
2. Build the project (this also downloads the packages it needs):
   ```
   dotnet build GroupSchedulerApp
   ```
   
## Commands to Run
From the project's top-level folder:
```
dotnet run --project GroupSchedulerApp
```
The terminal prints the address the app is running on (for example, http://localhost:5000). Open that address in your browser. Press Ctrl+C in the terminal to stop the app.

To run it in Visual Studio instead, open `GroupSchedulerApp/GroupSchedulerApp.csproj` and press F5.

The app saves its data to `GroupSchedulerApp/App_Data/scheduler.json`, which is created the first time something is saved. To start over with empty data, stop the app and delete that file.


## Tests
From the project's top-level folder:
```
dotnet test tests/GroupSchedulerApp.Tests
```
In Visual Studio, use Test > Run All Tests.


## Login Credentials or Sample Data
There is no login yet and no sample data. The app starts with empty data.


## Known Issues
- The web page only shows a placeholder heading. The group, schedule, task, and work session screens are not built yet.
- Group endpoints (create, join, add and remove members) are implemented, but there is no way to create a user yet, so creating a group returns an error that the user does not exist.
- The schedule, task, and work session endpoints return 501 (Not implemented).
- Schedule file import and Canvas course import are not implemented yet.


