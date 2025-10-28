import pytest
from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from app.main import app
from app.database import Base, get_db


# Create in-memory SQLite database for testing
SQLALCHEMY_DATABASE_URL = "sqlite:///:memory:"
engine = create_engine(SQLALCHEMY_DATABASE_URL, connect_args={"check_same_thread": False})
TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)


def override_get_db():
    """Override database dependency for testing"""
    try:
        db = TestingSessionLocal()
        yield db
    finally:
        db.close()


@pytest.fixture
def client():
    """Create test client with test database"""
    Base.metadata.create_all(bind=engine)
    app.dependency_overrides[get_db] = override_get_db
    
    with TestClient(app) as test_client:
        yield test_client
    
    Base.metadata.drop_all(bind=engine)
    app.dependency_overrides.clear()


def test_register_success(client):
    """Test successful user registration"""
    response = client.post(
        "/api/v1/register",
        headers={"Idempotency-Key": "test-key-123"},
        json={
            "fullName": "Somkiat Pui",
            "username": "somkiat.p",
            "email": "somkiat.p@example.com",
            "phone": "+66812345678",
            "password": "Pa$$w0rd2025!",
            "confirmPassword": "Pa$$w0rd2025!",
            "dob": "1995-05-10",
            "acceptTerms": True
        }
    )
    
    assert response.status_code == 201
    data = response.json()
    assert "userId" in data
    assert data["userId"].startswith("usr_")
    assert data["status"] == "pending_verification"
    assert data["verification"]["channel"] == "email"
    assert "sentAt" in data["verification"]


def test_register_validation_error_invalid_email(client):
    """Test registration with invalid email"""
    response = client.post(
        "/api/v1/register",
        json={
            "fullName": "Somkiat Pui",
            "username": "somkiat.p",
            "email": "invalid-email",
            "phone": "+66812345678",
            "password": "Pa$$w0rd2025!",
            "confirmPassword": "Pa$$w0rd2025!",
            "dob": "1995-05-10",
            "acceptTerms": True
        }
    )
    
    assert response.status_code == 400
    data = response.json()
    assert data["error"]["code"] == "VALIDATION_FAILED"
    assert "email" in data["error"]["fields"]


def test_register_validation_error_weak_password(client):
    """Test registration with weak password"""
    response = client.post(
        "/api/v1/register",
        json={
            "fullName": "Somkiat Pui",
            "username": "somkiat.p",
            "email": "somkiat.p@example.com",
            "phone": "+66812345678",
            "password": "weak",
            "confirmPassword": "weak",
            "dob": "1995-05-10",
            "acceptTerms": True
        }
    )
    
    assert response.status_code == 400
    data = response.json()
    assert data["error"]["code"] == "VALIDATION_FAILED"
    assert "password" in data["error"]["fields"]


def test_register_duplicate_username(client):
    """Test registration with duplicate username"""
    # First registration
    client.post(
        "/api/v1/register",
        json={
            "fullName": "Somkiat Pui",
            "username": "somkiat.p",
            "email": "somkiat.p@example.com",
            "phone": "+66812345678",
            "password": "Pa$$w0rd2025!",
            "confirmPassword": "Pa$$w0rd2025!",
            "dob": "1995-05-10",
            "acceptTerms": True
        }
    )
    
    # Second registration with same username
    response = client.post(
        "/api/v1/register",
        json={
            "fullName": "Another User",
            "username": "somkiat.p",  # Same username
            "email": "another@example.com",
            "phone": "+66812345679",
            "password": "Pa$$w0rd2025!",
            "confirmPassword": "Pa$$w0rd2025!",
            "dob": "1995-05-10",
            "acceptTerms": True
        }
    )
    
    assert response.status_code == 409
    data = response.json()
    assert data["error"]["code"] == "USER_EXISTS"


def test_register_password_mismatch(client):
    """Test registration with password mismatch"""
    response = client.post(
        "/api/v1/register",
        json={
            "fullName": "Somkiat Pui",
            "username": "somkiat.p",
            "email": "somkiat.p@example.com",
            "phone": "+66812345678",
            "password": "Pa$$w0rd2025!",
            "confirmPassword": "DifferentPassword!",
            "dob": "1995-05-10",
            "acceptTerms": True
        }
    )
    
    assert response.status_code == 400
    data = response.json()
    assert data["error"]["code"] == "VALIDATION_FAILED"


def test_health_check(client):
    """Test health check endpoint"""
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json()["status"] == "healthy"


def test_root_endpoint(client):
    """Test root endpoint"""
    response = client.get("/")
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "healthy"
    assert "service" in data
