# Register API

A Spring Boot REST API for user registration with validation, password encryption, and idempotency support.

## Technology Stack

- Java 21
- Spring Boot 3.5.7
- Spring Data JPA
- Spring Validation
- PostgreSQL 18 (production)
- H2 Database (testing)
- BCrypt (password encryption)
- JUnit 5 (unit testing)
- MockMvc (component testing)

## Project Structure

```
src/
├── main/
│   ├── java/com/example/register/
│   │   ├── controller/         # REST controllers
│   │   ├── dto/                # Data Transfer Objects
│   │   ├── entity/             # JPA entities
│   │   ├── exception/          # Custom exceptions
│   │   ├── repository/         # Data repositories
│   │   └── service/            # Business logic
│   └── resources/
│       └── application.properties
└── test/
    ├── java/com/example/register/
    │   ├── controller/         # Integration tests
    │   └── service/            # Unit tests
    └── resources/
        └── application.properties
```

## API Endpoints

### Register User

**Endpoint:** `POST /api/v1/register`

**Headers:**
- `Content-Type: application/json`
- `Idempotency-Key: <uuid>` (optional - auto-generated if not provided)

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

**Success Response (201 Created):**
```json
{
  "userId": "usr_12345",
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

### Field Validations

| Field | Rules |
|-------|-------|
| fullName | Required, 2-100 characters |
| username | Required, 3-50 characters, alphanumeric with dots/underscores/hyphens |
| email | Required, valid email format |
| phone | Required, valid E.164 format (e.g., +66812345678) |
| password | Required, 8-64 characters with upper/lower/digit/special character |
| confirmPassword | Required, must match password |
| dob | Required, must be in the past |
| acceptTerms | Must be true |

### Business Validations

- Username must be unique
- Email must be unique
- Phone number must be unique
- Password and confirmPassword must match

## Features

### 1. Idempotency Support
- Client can provide `Idempotency-Key` header to prevent duplicate registrations
- If key is not provided, one is auto-generated
- Same idempotency key returns the same response without creating duplicate users

### 2. Password Encryption
- Passwords are encrypted using BCrypt before storage
- Plain text passwords are never stored in the database

### 3. User Status Management
- New users start in `PENDING_VERIFICATION` state
- Verification email is sent upon registration
- Users can be in states: `PENDING_VERIFICATION`, `ACTIVE`, or `SUSPENDED`

### 4. Comprehensive Error Handling
- Validation errors return detailed field-level error messages
- Unique constraint violations are caught and reported clearly
- Generic exceptions are handled gracefully

## Setup Instructions

### Prerequisites
- Java 21 or higher
- PostgreSQL 18
- Maven 3.9+

### Database Setup

1. Create PostgreSQL database:
```sql
CREATE DATABASE registerdb;
```

2. Update database credentials in `src/main/resources/application.properties`:
```properties
spring.datasource.url=jdbc:postgresql://localhost:5432/registerdb
spring.datasource.username=your_username
spring.datasource.password=your_password
```

### Build and Run

1. Build the project:
```bash
./mvnw clean install
```

2. Run the application:
```bash
./mvnw spring-boot:run
```

The API will be available at `http://localhost:8080`

* API Document
  * http://localhost:8080/swagger-ui/index.html

### Run Tests

```bash
# Run all tests
./mvnw test

# Run only unit tests
./mvnw test -Dtest=""*ServiceTest"

# Run only integration tests
./mvnw test -Dtest="*IntegrationTest"
```

## Testing

### Unit Tests
- Located in `src/test/java/com/example/register/service/`
- Tests service layer business logic with mocked dependencies
- Coverage includes:
  - Successful registration
  - Idempotency checks
  - Password mismatch
  - Duplicate username/email/phone
  - Multiple validation errors

### Component Tests
- Located in `src/test/java/com/example/register/controller/`
- Full integration tests with Spring context and H2 database
- Coverage includes:
  - Complete registration flow
  - Idempotency behavior
  - Validation error responses
  - Duplicate field detection
  - Password encryption verification
  - Missing idempotency key handling

## API Documentation

Once the application is running, you can access the API documentation at:
- Swagger UI: `http://localhost:8080/swagger-ui.html`

## Database Schema

### Users Table
```sql
CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    user_id VARCHAR(255) NOT NULL UNIQUE,
    full_name VARCHAR(255) NOT NULL,
    username VARCHAR(255) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    phone VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    dob DATE NOT NULL,
    accept_terms BOOLEAN NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    verified_at TIMESTAMP,
    idempotency_key VARCHAR(255) UNIQUE
);
```

## Future Enhancements

- Email verification implementation
- SMS OTP verification
- Rate limiting
- Account activation endpoint
- Password reset functionality
- Audit logging
- Metrics and monitoring
