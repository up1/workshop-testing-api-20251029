from app.utils.security import hash_password, verify_password


def test_hash_password():
    """Test password hashing"""
    password = "TestPassword123!"
    hashed = hash_password(password)
    
    assert hashed is not None
    assert hashed != password
    assert len(hashed) > 0


def test_verify_password_correct():
    """Test password verification with correct password"""
    password = "TestPassword123!"
    hashed = hash_password(password)
    
    assert verify_password(password, hashed) is True


def test_verify_password_incorrect():
    """Test password verification with incorrect password"""
    password = "TestPassword123!"
    wrong_password = "WrongPassword123!"
    hashed = hash_password(password)
    
    assert verify_password(wrong_password, hashed) is False


def test_different_hashes_same_password():
    """Test that same password produces different hashes (due to salt)"""
    password = "TestPassword123!"
    hash1 = hash_password(password)
    hash2 = hash_password(password)
    
    assert hash1 != hash2
    assert verify_password(password, hash1) is True
    assert verify_password(password, hash2) is True
