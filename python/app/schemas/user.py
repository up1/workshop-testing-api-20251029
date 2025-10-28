from datetime import date, datetime
from pydantic import BaseModel, EmailStr, Field, field_validator, model_validator
import re


class RegisterRequest(BaseModel):
    fullName: str = Field(..., min_length=1, max_length=100)
    username: str = Field(..., min_length=3, max_length=30)
    email: EmailStr
    phone: str = Field(..., pattern=r'^\+\d{10,15}$')
    password: str = Field(..., min_length=8, max_length=64)
    confirmPassword: str = Field(..., min_length=8, max_length=64)
    dob: date
    acceptTerms: bool

    @field_validator('username')
    @classmethod
    def validate_username(cls, v: str) -> str:
        if not re.match(r'^[a-zA-Z0-9._-]+$', v):
            raise ValueError('Username can only contain letters, numbers, dots, underscores, and hyphens')
        return v

    @field_validator('password')
    @classmethod
    def validate_password(cls, v: str) -> str:
        if not re.search(r'[A-Z]', v):
            raise ValueError('Password must be 8–64 chars incl. upper/lower/digit/special')
        if not re.search(r'[a-z]', v):
            raise ValueError('Password must be 8–64 chars incl. upper/lower/digit/special')
        if not re.search(r'\d', v):
            raise ValueError('Password must be 8–64 chars incl. upper/lower/digit/special')
        if not re.search(r'[!@#$%^&*(),.?":{}|<>]', v):
            raise ValueError('Password must be 8–64 chars incl. upper/lower/digit/special')
        return v

    @model_validator(mode='after')
    def check_passwords_match(self):
        if self.password != self.confirmPassword:
            raise ValueError('Passwords do not match')
        return self

    @field_validator('acceptTerms')
    @classmethod
    def validate_accept_terms(cls, v: bool) -> bool:
        if not v:
            raise ValueError('You must accept the terms and conditions')
        return v

    @field_validator('dob')
    @classmethod
    def validate_dob(cls, v: date) -> date:
        today = date.today()
        age = today.year - v.year - ((today.month, today.day) < (v.month, v.day))
        if age < 13:
            raise ValueError('You must be at least 13 years old to register')
        if age > 120:
            raise ValueError('Invalid date of birth')
        return v


class VerificationInfo(BaseModel):
    channel: str
    sentAt: datetime


class RegisterResponse(BaseModel):
    userId: str
    status: str
    verification: VerificationInfo


class ValidationErrorField(BaseModel):
    code: str
    fields: dict[str, str]


class ValidationErrorResponse(BaseModel):
    error: ValidationErrorField
