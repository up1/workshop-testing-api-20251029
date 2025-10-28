package com.example.register.exception;

import java.util.Map;

public class ValidationException extends RuntimeException {
    
    private final String errorCode;
    private final Map<String, String> fieldErrors;
    
    public ValidationException(String errorCode, Map<String, String> fieldErrors) {
        super("Validation failed");
        this.errorCode = errorCode;
        this.fieldErrors = fieldErrors;
    }
    
    public String getErrorCode() {
        return errorCode;
    }
    
    public Map<String, String> getFieldErrors() {
        return fieldErrors;
    }
}
