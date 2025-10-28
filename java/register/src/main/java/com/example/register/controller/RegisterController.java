package com.example.register.controller;

import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.validation.FieldError;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.register.dto.ErrorResponse;
import com.example.register.dto.RegisterRequest;
import com.example.register.dto.RegisterResponse;
import com.example.register.exception.ValidationException;
import com.example.register.service.RegisterService;

import jakarta.validation.Valid;

@RestController
@RequestMapping("/api/v1")
public class RegisterController {
    
    private final RegisterService registerService;
    
    @Autowired
    public RegisterController(RegisterService registerService) {
        this.registerService = registerService;
    }
    
    @PostMapping("/register")
    public ResponseEntity<RegisterResponse> register(
            @Valid @RequestBody RegisterRequest request,
            @RequestHeader(value = "Idempotency-Key", required = false) String idempotencyKey) {
        
        // Generate idempotency key if not provided
        if (idempotencyKey == null || idempotencyKey.isEmpty()) {
            idempotencyKey = UUID.randomUUID().toString();
        }
        
        RegisterResponse response = registerService.register(request, idempotencyKey);
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }
    
    @ExceptionHandler(ValidationException.class)
    public ResponseEntity<ErrorResponse> handleValidationException(ValidationException ex) {
        ErrorResponse.ErrorDetail errorDetail = new ErrorResponse.ErrorDetail(
            ex.getErrorCode(),
            ex.getFieldErrors()
        );
        ErrorResponse errorResponse = new ErrorResponse(errorDetail);
        return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(errorResponse);
    }
    
    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<ErrorResponse> handleMethodArgumentNotValid(MethodArgumentNotValidException ex) {
        Map<String, String> errors = new HashMap<>();
        ex.getBindingResult().getAllErrors().forEach((error) -> {
            String fieldName = ((FieldError) error).getField();
            String errorMessage = error.getDefaultMessage();
            errors.put(fieldName, errorMessage);
        });
        
        ErrorResponse.ErrorDetail errorDetail = new ErrorResponse.ErrorDetail("VALIDATION_FAILED", errors);
        ErrorResponse errorResponse = new ErrorResponse(errorDetail);
        return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(errorResponse);
    }
    
    @ExceptionHandler(Exception.class)
    public ResponseEntity<ErrorResponse> handleGenericException(Exception ex) {
        Map<String, String> errors = new HashMap<>();
        errors.put("message", ex.getMessage());
        
        ErrorResponse.ErrorDetail errorDetail = new ErrorResponse.ErrorDetail("INTERNAL_ERROR", errors);
        ErrorResponse errorResponse = new ErrorResponse(errorDetail);
        return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(errorResponse);
    }
}
