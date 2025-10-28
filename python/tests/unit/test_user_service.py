import pytest
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from app.database import Base
from app.schemas.user import RegisterRequest
from app.services.user_service import UserService


# Create in-memory SQLite database for testing
SQLALCHEMY_DATABASE_URL = "sqlite:///:memory:"
engine = create_engine(SQLALCHEMY_DATABASE_URL, connect_args={"check_same_thread": False})
TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)


@pytest.fixture
def db():
    """Create database session for testing"""
    Base.metadata.create_all(bind=engine)
    db = TestingSessionLocal()
    try:
        yield db
    finally:
        db.close()
        Base.metadata.drop_all(bind=engine)


def test_create_user_success(db):
    """Test successful user creation"""
    user_data = RegisterRequest(
        fullName="Somkiat Pui",
        username="somkiat.p",
        email="somkiat.p@example.com",
        phone="+66812345678",
        password="Pa$$w0rd2025!",
        confirmPassword="Pa$$w0rd2025!",
        dob="1995-05-10",
        acceptTerms=True
    )
    
    user = UserService.create_user(db, user_data)
    
    assert user.id is not None
    assert user.id.startswith("usr_")
    assert user.username == "somkiat.p"
    assert user.email == "somkiat.p@example.com"
    assert user.status == "pending_verification"
    assert user.password_hash != "Pa$$w0rd2025!"  # Password should be hashed


def test_create_user_duplicate_username(db):
    """Test user creation with duplicate username"""
    user_data = RegisterRequest(
        fullName="Somkiat Pui",
        username="somkiat.p",
        email="somkiat.p@example.com",
        phone="+66812345678",
        password="Pa$$w0rd2025!",
        confirmPassword="Pa$$w0rd2025!",
        dob="1995-05-10",
        acceptTerms=True
    )
    
    # Create first user
    UserService.create_user(db, user_data)
    
    # Try to create second user with same username
    user_data2 = RegisterRequest(
        fullName="Another User",
        username="somkiat.p",  # Same username
        email="another@example.com",
        phone="+66812345679",
        password="Pa$$w0rd2025!",
        confirmPassword="Pa$$w0rd2025!",
        dob="1995-05-10",
        acceptTerms=True
    )
    
    with pytest.raises(ValueError) as exc_info:
        UserService.create_user(db, user_data2)
    
    assert "Username already exists" in str(exc_info.value)


def test_create_user_duplicate_email(db):
    """Test user creation with duplicate email"""
    user_data = RegisterRequest(
        fullName="Somkiat Pui",
        username="somkiat.p",
        email="somkiat.p@example.com",
        phone="+66812345678",
        password="Pa$$w0rd2025!",
        confirmPassword="Pa$$w0rd2025!",
        dob="1995-05-10",
        acceptTerms=True
    )
    
    # Create first user
    UserService.create_user(db, user_data)
    
    # Try to create second user with same email
    user_data2 = RegisterRequest(
        fullName="Another User",
        username="another.user",
        email="somkiat.p@example.com",  # Same email
        phone="+66812345679",
        password="Pa$$w0rd2025!",
        confirmPassword="Pa$$w0rd2025!",
        dob="1995-05-10",
        acceptTerms=True
    )
    
    with pytest.raises(ValueError) as exc_info:
        UserService.create_user(db, user_data2)
    
    assert "Email already exists" in str(exc_info.value)


def test_send_verification_email(db):
    """Test verification email sending"""
    user_data = RegisterRequest(
        fullName="Somkiat Pui",
        username="somkiat.p",
        email="somkiat.p@example.com",
        phone="+66812345678",
        password="Pa$$w0rd2025!",
        confirmPassword="Pa$$w0rd2025!",
        dob="1995-05-10",
        acceptTerms=True
    )
    
    user = UserService.create_user(db, user_data)
    verification = UserService.send_verification_email(user)
    
    assert verification["channel"] == "email"
    assert verification["sentAt"] is not None
