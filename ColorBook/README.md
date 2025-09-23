# ColorBook Backend API

Azure Functions Isolated backend with .NET 9 for a coloring book application.

## Features

- **Azure Functions Isolated Worker** with .NET 9
- **JWT Authentication** middleware
- **MongoDB** integration with repository pattern
- **Structured logging** middleware
- **Health check** endpoint
- **Book management** endpoints

## Project Structure

```
ColorBook/
├── ColorBook.csproj          # Project file with dependencies
├── host.json                 # Azure Functions host configuration
├── local.settings.json       # Local development settings
├── Program.cs                # Application entry point with DI setup
├── Config/
│   └── MongoSettings.cs      # MongoDB configuration
├── Auth/
│   ├── JwtValidationOptions.cs # JWT configuration
│   └── JwtTokenValidator.cs    # JWT token validation service
├── Models/
│   ├── BookItem.cs           # Book entity model
│   └── PageDetails.cs        # Page status model
├── Data/
│   ├── MongoContext.cs       # MongoDB context
│   └── BookRepository.cs     # Book data access layer
├── Middleware/
│   ├── LoggingMiddleware.cs  # Request/response logging
│   └── JwtAuthMiddleware.cs  # JWT authentication
└── Functions/
    ├── HealthFunctions.cs    # Health check endpoints
    └── BooksFunctions.cs     # Book management endpoints
```

## Endpoints

### Public Endpoints
- `GET /api/health` - Health check (no authentication required)

### Authenticated Endpoints (require JWT Bearer token)
- `GET /api/books` - Get user's books with progress
- `POST /api/books` - Add new book from catalog

## Configuration

Configure the following settings in `local.settings.json`:

```json
{
  "Values": {
    "MongoSettings__ConnectionString": "mongodb://localhost:27017",
    "MongoSettings__DatabaseName": "ColorBookDb",
    "JwtSettings__Secret": "your-secret-key-here",
    "JwtSettings__Issuer": "ColorBookApp",
    "JwtSettings__Audience": "ColorBookUsers"
  }
}
```

## Models

### BookItem
- User's book with progress tracking
- Contains list of pages with individual status
- Tracks completion percentage and timestamps

### PageDetails  
- Individual page within a book
- Status: NotStarted, InProgress, Completed
- Completion timestamp

## Getting Started

1. Install .NET 9 SDK
2. Install MongoDB locally or configure connection string
3. Update `local.settings.json` with your configuration
4. Run `func start` or `dotnet run`

## Authentication

The API uses JWT Bearer tokens for authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

The JWT token should contain a `nameidentifier` claim with the user ID.
