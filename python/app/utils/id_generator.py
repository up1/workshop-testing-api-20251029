import uuid


def generate_user_id() -> str:
    """Generate a unique user ID with usr_ prefix"""
    return f"usr_{uuid.uuid4().hex[:10]}"
