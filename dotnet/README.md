# Registration API - .NET 9

A comprehensive user registration API built with .NET 9, implementing best practices for validation, security, and testing.

## Features

- User registration with comprehensive validation
- Password hashing using BCrypt
- Email, username, and phone uniqueness validation
- Date of birth validation (minimum age 13)
- Idempotency support
- Structured logging
- PostgreSQL database support
- Unit tests with Moq
- Integration tests with in-memory database

## Project Structure

```
api/
├── Controllers/         # API endpoints
├── Services/           # Business logic
├── Repositories/       # Data access layer
├── Models/             # DTOs and entities
└── Program.cs          # Application configuration

tests/
├── Unit/              # Unit tests
└── Integration/       # Integration tests
```

## Technology Stack

- .NET 9 with C#
- ASP.NET Core Web API
- Entity Framework Core 9
- PostgreSQL 18
- BCrypt for password hashing
- xUnit for testing
- Moq for mocking
- In-memory database for integration testing

## Prerequisites

- .NET 9 SDK
- PostgreSQL 18 (for production)

## Getting Started

### 1. Clone the repository

```bash
cd /path/to/dotnet
```

### 2. Update database connection string

Edit `api/appsettings.json` or `api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=registration_db;Username=your_username;Password=your_password"
  }
}
```

### 3. Create PostgreSQL database

```sql
CREATE DATABASE registration_db;

CREATE TABLE users (
    id VARCHAR PRIMARY KEY,
    full_name VARCHAR NOT NULL,
    username VARCHAR UNIQUE NOT NULL,
    email VARCHAR UNIQUE NOT NULL,
    phone VARCHAR UNIQUE NOT NULL,
    password_hash VARCHAR NOT NULL,
    dob DATE NOT NULL,
    status VARCHAR NOT NULL DEFAULT 'pending_verification',
    accept_terms BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    verified_at TIMESTAMP WITH TIME ZONE
);
```

### 4. Run the application

```bash
cd api
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

Swagger UI
* http://localhost:5023/swagger/index.html

### 5. Run tests

```bash
dotnet test
```

### Run tests by folder

```bash
# Run all tests
dotnet test

dotnet test --filter FullyQualifiedName~Unit

dotnet test --filter FullyQualifiedName~Integration

dotnet test --filter FullyQualifiedName~Register_WithValidRequest_ShouldReturnOk
dotnet test --filter FullyQualifiedName~Register_WithDuplicateUsername_ShouldReturnBadRequest
```

### Run test with code coverage
```
dotnet test --collect:"XPlat Code Coverage"

# Generate report
dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator -reports:"Path\To\TestProject\TestResults\{guid}\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

Open report
* coveragereport/index.html



## API Documentation

### Register Endpoint

**Endpoint:** `POST /api/v1/register`

**Headers:**
- `Content-Type: application/json`
- `Idempotency-Key: <uuid>` (required)

**Request Body:**

```json
{
  "fullName": "Somkiat Pui",
  "username": "somkiat.p",
  "email": "somkiat.p@example.com",
  "phone": "+66812345678",
  "password": "Pa$$w0rd2025!",
  "confirmPassword": "Pa$$w0rd2025!",
  "dob": "1995-05-10",
  "acceptTerms": true
}
```

**Success Response (200 OK):**

```json
{
  "userId": "usr_a5323ce4a2054c458d8efedb76db871c",
  "status": "pending_verification",
  "verification": {
    "channel": "email",
    "sentAt": "2025-10-28T04:35:00Z"
  }
}
```

**Validation Error Response (400 Bad Request):**

```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "fields": {
      "email": "Enter a valid email address",
      "password": "Password must be 8–64 chars incl. upper/lower/digit/special"
    }
  }
}
```

## Validation Rules

### Full Name
- Required
- 2-100 characters

### Username
- Required
- 3-50 characters
- Only letters, numbers, dots, and underscores
- Must be unique

### Email
- Required
- Valid email format
- Must be unique

### Phone
- Required
- Valid international phone format (E.164)
- Must be unique

### Password
- Required
- 8-64 characters
- Must contain:
  - At least one uppercase letter
  - At least one lowercase letter
  - At least one digit
  - At least one special character (@$!%*?&#)

### Date of Birth
- Required
- Valid date in YYYY-MM-DD format
- User must be at least 13 years old
- Cannot be in the future

### Accept Terms
- Required
- Must be `true`

## Testing

The project includes comprehensive test coverage:

### Unit Tests
- Password hashing validation
- Uniqueness validation (username, email, phone)
- Date of birth validation
- Age restriction validation
- Service layer business logic

### Integration Tests
- End-to-end API testing
- Request/response validation
- Error handling
- Database integration

**Run specific test categories:**

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "FullyQualifiedName~Unit"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## HTTP File Examples

Test the API using the provided `register.http` file in VS Code with the REST Client extension, or use curl:

```bash
curl -X POST http://localhost:5000/api/v1/register \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: $(uuidgen)" \
  -d '{
    "fullName": "Somkiat Pui",
    "username": "somkiat.p",
    "email": "somkiat.p@example.com",
    "phone": "+66812345678",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "Pa$$w0rd2025!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```

## Security Features

1. **Password Hashing**: Passwords are hashed using BCrypt before storage
2. **Idempotency**: Prevents duplicate submissions with idempotency keys
3. **Input Validation**: Comprehensive server-side validation
4. **SQL Injection Protection**: Entity Framework Core parameterized queries
5. **Secure Defaults**: HTTPS redirection enabled

## Future Enhancements

- Email verification implementation
- SMS OTP verification
- Rate limiting
- CAPTCHA integration
- OAuth2/OpenID Connect support
- Account activation workflow
- Password strength meter
- Audit logging to database

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
