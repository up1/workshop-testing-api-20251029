## Register process 
* 1. User opens Register page 
* 2. User enters: Full name, Username, Email, Phone, Password, Confirm Password, Date of Birth, accepts Terms
* 3. Client-side validations highlight obvious issues (format/required) 
* 4. Submit → Server-side validation (uniqueness, rules)
* 5. Account record created in pending_verification state
* 6. Verification email (and/or SMS OTP) sent 
* 7. User verifies → account becomes active 
* 8. Audit/logging

## Technology stack
* Java 25
* Spring Boot 3.5.7
* Spring Validation
* Database with PostgreSQL 18
* Use bcrypt to encrypt password
* Component testing SpringBootTest and mock database with h2
* Unit test with Junit 5

## Project structure
* Flow : controller -> service -> repository

## Register API

Request
```
POST /api/v1/register
Content-Type: application/json
Idempotency-Key: <uuid>
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

Success Response
```
{
  "userId": "usr_12345",
  "status": "pending_verification",
  "verification": {
    "channel": "email",
    "sentAt": "2025-10-28T04:35:00Z"
  }
}
```

Validation Error Response
```
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