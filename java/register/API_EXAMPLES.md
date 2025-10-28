# Register API - cURL Examples

## Successful Registration

```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: test-key-12345" \
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

## Registration without Idempotency Key

```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "username": "john.doe",
    "email": "john.doe@example.com",
    "phone": "+66987654321",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "dob": "1990-01-15",
    "acceptTerms": true
  }'
```

## Test Validation Errors

### Missing Required Fields
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "",
    "username": "",
    "email": "invalid-email",
    "phone": "123",
    "password": "weak",
    "confirmPassword": "weak",
    "acceptTerms": false
  }'
```

### Password Mismatch
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: test-key-password-mismatch" \
  -d '{
    "fullName": "Test User",
    "username": "test.user",
    "email": "test@example.com",
    "phone": "+66888888888",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "DifferentPassword123!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```

### Invalid Email Format
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Test User",
    "username": "test.user2",
    "email": "not-an-email",
    "phone": "+66888888889",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "Pa$$w0rd2025!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```

### Invalid Phone Format
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Test User",
    "username": "test.user3",
    "email": "test3@example.com",
    "phone": "123456",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "Pa$$w0rd2025!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```

### Weak Password
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Test User",
    "username": "test.user4",
    "email": "test4@example.com",
    "phone": "+66888888887",
    "password": "weak",
    "confirmPassword": "weak",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```

## Test Idempotency

### First Request
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: same-key-123" \
  -d '{
    "fullName": "Idempotency Test",
    "username": "idem.test",
    "email": "idem@example.com",
    "phone": "+66777777777",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "Pa$$w0rd2025!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```

### Second Request (Same Idempotency Key)
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: same-key-123" \
  -d '{
    "fullName": "Different Name",
    "username": "different.username",
    "email": "different@example.com",
    "phone": "+66666666666",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "Pa$$w0rd2025!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```
Note: Both requests should return the same response (same userId).

## Test Duplicate Detection

### Register First User
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: duplicate-test-1" \
  -d '{
    "fullName": "First User",
    "username": "duplicate.user",
    "email": "duplicate@example.com",
    "phone": "+66555555555",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "Pa$$w0rd2025!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```

### Try Duplicate Username
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: duplicate-test-2" \
  -d '{
    "fullName": "Second User",
    "username": "duplicate.user",
    "email": "different2@example.com",
    "phone": "+66444444444",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "Pa$$w0rd2025!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```

### Try Duplicate Email
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: duplicate-test-3" \
  -d '{
    "fullName": "Third User",
    "username": "unique.user",
    "email": "duplicate@example.com",
    "phone": "+66333333333",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "Pa$$w0rd2025!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```

### Try Duplicate Phone
```bash
curl -X POST http://localhost:8080/api/v1/register \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: duplicate-test-4" \
  -d '{
    "fullName": "Fourth User",
    "username": "another.user",
    "email": "another@example.com",
    "phone": "+66555555555",
    "password": "Pa$$w0rd2025!",
    "confirmPassword": "Pa$$w0rd2025!",
    "dob": "1995-05-10",
    "acceptTerms": true
  }'
```
