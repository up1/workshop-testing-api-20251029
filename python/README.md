# User Registration API

A robust user registration REST API built with FastAPI, PostgreSQL, and comprehensive testing.

## Features

- User registration with email/SMS verification
- Password hashing with bcrypt
- Pydantic validation
- Idempotency support
- Comprehensive logging
- Unit, integration, and API tests

## Tech Stack

- Python 3.13
- FastAPI 0.120.1
- Pydantic 2.12
- PostgreSQL 18
- bcrypt for password encryption
- Pytest for testing

## Setup

1. Install dependencies:
```bash
pip install -r requirements.txt
```

2. Configure environment:
```bash
cp .env.example .env
# Edit .env with your database credentials
```

3. Run tests:
```bash
pytest
pytest --cov=app tests/
pytest --cov=app --cov-report=html tests/
```
## Open coverage report in file `htmlcov/index.html`


4. Run test by specify folder
```bash
pytest tests/unit/
pytest tests/component/
```

5. Create database
```
docker compose down
docker compose up -d
docker compose ps
```

6. Run the application:
```bash
uvicorn app.main:app --reload
```

7. API testing
* Postman and newman
* robot framework with requests library

## API Documentation
Once running, visit:
- Swagger UI: http://localhost:8000/docs
- ReDoc: http://localhost:8000/redoc

## API Endpoint

### POST /api/v1/register

Register a new user account.

**Request:**
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

**Success Response (201):**
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
