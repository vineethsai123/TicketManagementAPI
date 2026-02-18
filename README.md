# Ticket Management API

A simple ASP.NET Core Web API for managing support tickets with Excel file integration.

## Features

- ✅ Load tickets from an Excel file (`tickets.xlsx`) on startup
- ✅ In-memory CRUD operations (no database required)
- ✅ RESTful API endpoints
- ✅ Dependency injection pattern
- ✅ Swagger/OpenAPI documentation
- ✅ Proper HTTP status codes

## Project Structure

```
TicketManagementApi/
├── Models/
│   └── Ticket.cs                 # Ticket model class
├── Services/
│   └── TicketService.cs          # Service for business logic and data storage
├── Controllers/
│   └── TicketController.cs       # API endpoints controller
├── Properties/
│   └── launchSettings.json       # Launch configuration
├── Program.cs                     # Application entry point & DI setup
├── appsettings.json              # Application settings
├── appsettings.Development.json  # Development settings
├── TicketManagementApi.csproj    # Project file with NuGet dependencies
└── GenerateSampleData.cs         # Helper script to create sample Excel file
```

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension

## Setup & Installation

### 1. Restore NuGet Packages
```bash
cd TicketManagementApi
dotnet restore
```

### 2. Create Sample Data (Optional)
You have two options:

**Option A: Use the helper script**
```bash
dotnet run GenerateSampleData.cs
```

**Option B: Create manually in the bin/Debug/net8.0 directory**
Create a file named `tickets.xlsx` using Excel or a similar tool with the following columns:
- TicketId (numeric)
- Email
- Type
- Description
- Status
- Resolution

Sample data structure:
| TicketId | Email | Type | Description | Status | Resolution |
|----------|-------|------|-------------|--------|------------|
| 1 | user@example.com | Bug | Issue description | Open | - |
| 2 | user2@example.com | Feature | Feature request | In Progress | - |

### 3. Run the Application
```bash
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:44300`
- Swagger UI: `https://localhost:44300/swagger`

## API Endpoints

### Get All Tickets
```http
GET /api/tickets
```
**Response:** `200 OK`
```json
[
  {
    "ticketId": 1,
    "email": "user@example.com",
    "type": "Bug",
    "description": "Login button not working",
    "status": "Open",
    "resolution": ""
  }
]
```

### Get Ticket by ID
```http
GET /api/tickets/{id}
```
**Response:** `200 OK` or `404 Not Found`

### Create New Ticket
```http
POST /api/tickets
Content-Type: application/json

{
  "ticketId": 6,
  "email": "newuser@example.com",
  "type": "Support",
  "description": "Need help with API",
  "status": "Open",
  "resolution": ""
}
```
**Response:** `201 Created`

### Update Ticket
```http
PUT /api/tickets/{id}
Content-Type: application/json

{
  "ticketId": 1,
  "email": "user@example.com",
  "type": "Bug",
  "description": "Login button not working",
  "status": "Closed",
  "resolution": "Button CSS fixed"
}
```
**Response:** `200 OK` or `404 Not Found`

### Delete Ticket
```http
DELETE /api/tickets/{id}
```
**Response:** `200 OK` or `404 Not Found`

## Testing with Swagger

After running the application, navigate to `https://localhost:44300/swagger` to test all endpoints interactively.

## NuGet Dependencies

- **ClosedXML** (v0.102.1) - For reading Excel files
- **Microsoft.AspNetCore.OpenApi** (v8.0.22) - For OpenAPI support
- **Swashbuckle.AspNetCore** (v6.6.2) - For Swagger/OpenAPI documentation

## Architecture Notes

- **ITicketService Interface**: Defines the contract for ticket operations
- **TicketService Class**: Singleton service that maintains the in-memory ticket list
- **TicketController**: RESTful controller with dependency injection
- **Excel Loading**: Automatic on startup, handles missing files gracefully

## Error Handling

- Returns `400 Bad Request` for invalid input
- Returns `404 Not Found` for non-existent tickets
- Returns `201 Created` for successfully created resources
- Console logs errors during Excel loading without crashing

## Future Enhancements

- Add pagination for large datasets
- Add filtering and sorting
- Add validation attributes (DataAnnotations)
- Add logging framework (Serilog)
- Add unit tests
- Add database persistence option
