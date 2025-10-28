from datetime import datetime, timezone
from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError
from app.models.user import User
from app.schemas.user import RegisterRequest
from app.utils.security import hash_password
from app.utils.id_generator import generate_user_id
import logging

logger = logging.getLogger(__name__)


class UserService:
    @staticmethod
    def create_user(db: Session, user_data: RegisterRequest) -> User:
        """
        Create a new user in the database
        
        Args:
            db: Database session
            user_data: User registration data
            
        Returns:
            Created User object
            
        Raises:
            ValueError: If username, email, or phone already exists
        """
        # Check if username already exists
        existing_user = db.query(User).filter(
            (User.username == user_data.username) |
            (User.email == user_data.email) |
            (User.phone == user_data.phone)
        ).first()
        
        if existing_user:
            if existing_user.username == user_data.username:
                raise ValueError("Username already exists")
            if existing_user.email == user_data.email:
                raise ValueError("Email already exists")
            if existing_user.phone == user_data.phone:
                raise ValueError("Phone number already exists")
        
        # Hash password
        password_hash = hash_password(user_data.password)
        
        # Generate user ID
        user_id = generate_user_id()
        
        # Create user
        user = User(
            id=user_id,
            full_name=user_data.fullName,
            username=user_data.username,
            email=user_data.email,
            phone=user_data.phone,
            password_hash=password_hash,
            dob=user_data.dob,
            status="pending_verification",
            accept_terms=user_data.acceptTerms
        )
        
        try:
            db.add(user)
            db.commit()
            db.refresh(user)
            logger.info(f"User created successfully: {user_id}")
            return user
        except IntegrityError as e:
            db.rollback()
            logger.error(f"Database integrity error: {str(e)}")
            raise ValueError("User with this information already exists")
    
    @staticmethod
    def send_verification_email(user: User) -> dict:
        """
        Simulate sending verification email
        
        Args:
            user: User object
            
        Returns:
            Dictionary with verification details
        """
        logger.info(f"Sending verification email to {user.email}")
        # In production, this would send an actual email
        return {
            "channel": "email",
            "sentAt": datetime.now(timezone.utc)
        }
