import pytest
from pydantic import ValidationError
from app.schemas.user import RegisterRequest


def test_valid_registration_request():
    """Test valid registration request"""
    data = {
        "fullName": "Somkiat Pui",
        "username": "somkiat.p",
        "email": "somkiat.p@example.com",
        "phone": "+66812345678",
        "password": "Pa$$w0rd2025!",
        "confirmPassword": "Pa$$w0rd2025!",
        "dob": "1995-05-10",
        "acceptTerms": True
    }
    
    request = RegisterRequest(**data)
    assert request.fullName == "Somkiat Pui"
    assert request.username == "somkiat.p"
    assert request.email == "somkiat.p@example.com"


def test_invalid_email():
    """Test registration with invalid email"""
    data = {
        "fullName": "Somkiat Pui",
        "username": "somkiat.p",
        "email": "invalid-email",
        "phone": "+66812345678",
        "password": "Pa$$w0rd2025!",
        "confirmPassword": "Pa$$w0rd2025!",
        "dob": "1995-05-10",
        "acceptTerms": True
    }
    
    with pytest.raises(ValidationError) as exc_info:
        RegisterRequest(**data)
    
    assert "email" in str(exc_info.value)


def test_password_mismatch():
    """Test registration with password mismatch"""
    data = {
        "fullName": "Somkiat Pui",
        "username": "somkiat.p",
        "email": "somkiat.p@example.com",
        "phone": "+66812345678",
        "password": "Pa$$w0rd2025!",
        "confirmPassword": "DifferentPassword!",
        "dob": "1995-05-10",
        "acceptTerms": True
    }
    
    with pytest.raises(ValidationError) as exc_info:
        RegisterRequest(**data)
    
    assert "Passwords do not match" in str(exc_info.value)


def test_weak_password():
    """Test registration with weak password"""
    data = {
        "fullName": "Somkiat Pui",
        "username": "somkiat.p",
        "email": "somkiat.p@example.com",
        "phone": "+66812345678",
        "password": "weakpass",
        "confirmPassword": "weakpass",
        "dob": "1995-05-10",
        "acceptTerms": True
    }
    
    with pytest.raises(ValidationError) as exc_info:
        RegisterRequest(**data)
    
    assert "Password must be 8–64 chars incl. upper/lower/digit/special" in str(exc_info.value)


def test_invalid_phone():
    """Test registration with invalid phone number"""
    data = {
        "fullName": "Somkiat Pui",
        "username": "somkiat.p",
        "email": "somkiat.p@example.com",
        "phone": "123456",  # Invalid format
        "password": "Pa$$w0rd2025!",
        "confirmPassword": "Pa$$w0rd2025!",
        "dob": "1995-05-10",
        "acceptTerms": True
    }
    
    with pytest.raises(ValidationError) as exc_info:
        RegisterRequest(**data)
    
    assert "phone" in str(exc_info.value)


def test_terms_not_accepted():
    """Test registration without accepting terms"""
    data = {
        "fullName": "Somkiat Pui",
        "username": "somkiat.p",
        "email": "somkiat.p@example.com",
        "phone": "+66812345678",
        "password": "Pa$$w0rd2025!",
        "confirmPassword": "Pa$$w0rd2025!",
        "dob": "1995-05-10",
        "acceptTerms": False
    }
    
    with pytest.raises(ValidationError) as exc_info:
        RegisterRequest(**data)
    
    assert "accept the terms" in str(exc_info.value)


def test_underage_user():
    """Test registration with underage user"""
    data = {
        "fullName": "Young User",
        "username": "young.user",
        "email": "young@example.com",
        "phone": "+66812345678",
        "password": "Pa$$w0rd2025!",
        "confirmPassword": "Pa$$w0rd2025!",
        "dob": "2020-01-01",  # Too young
        "acceptTerms": True
    }
    
    with pytest.raises(ValidationError) as exc_info:
        RegisterRequest(**data)
    
    assert "at least 13 years old" in str(exc_info.value)


def test_invalid_username():
    """Test registration with invalid username characters"""
    data = {
        "fullName": "Somkiat Pui",
        "username": "user@name!",  # Invalid characters
        "email": "somkiat.p@example.com",
        "phone": "+66812345678",
        "password": "Pa$$w0rd2025!",
        "confirmPassword": "Pa$$w0rd2025!",
        "dob": "1995-05-10",
        "acceptTerms": True
    }
    
    with pytest.raises(ValidationError) as exc_info:
        RegisterRequest(**data)
    
    assert "Username can only contain" in str(exc_info.value)
