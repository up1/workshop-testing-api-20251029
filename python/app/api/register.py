from fastapi import APIRouter, Depends, HTTPException, Header, status
from sqlalchemy.orm import Session
from app.database import get_db
from app.schemas.user import RegisterRequest, RegisterResponse, VerificationInfo
from app.services.user_service import UserService
import logging

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api/v1", tags=["registration"])


@router.post(
    "/register",
    response_model=RegisterResponse,
    status_code=status.HTTP_201_CREATED,
    responses={
        201: {"description": "User registered successfully"},
        400: {"description": "Validation error"},
        409: {"description": "User already exists"}
    }
)
async def register_user(
    user_data: RegisterRequest,
    db: Session = Depends(get_db),
    idempotency_key: str = Header(None, alias="Idempotency-Key")
):
    """
    Register a new user account
    
    - **fullName**: User's full name
    - **username**: Unique username (3-30 chars, alphanumeric with ._-)
    - **email**: Valid email address
    - **phone**: Phone number in international format (e.g., +66812345678)
    - **password**: 8-64 chars including upper/lower/digit/special
    - **confirmPassword**: Must match password
    - **dob**: Date of birth (must be 13+ years old)
    - **acceptTerms**: Must be true
    """
    try:
        logger.info(f"Registration request received for username: {user_data.username}")
        
        if idempotency_key:
            logger.info(f"Idempotency-Key: {idempotency_key}")
        
        # Create user
        user = UserService.create_user(db, user_data)
        
        # Send verification email
        verification_info = UserService.send_verification_email(user)
        
        # Prepare response
        response = RegisterResponse(
            userId=user.id,
            status=user.status,
            verification=VerificationInfo(
                channel=verification_info["channel"],
                sentAt=verification_info["sentAt"]
            )
        )
        
        logger.info(f"User registered successfully: {user.id}")
        return response
        
    except ValueError as e:
        logger.warning(f"Registration failed: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT,
            detail={
                "error": {
                    "code": "USER_EXISTS",
                    "message": str(e)
                }
            }
        )
    except Exception as e:
        logger.error(f"Unexpected error during registration: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail={
                "error": {
                    "code": "INTERNAL_ERROR",
                    "message": "An unexpected error occurred"
                }
            }
        )
