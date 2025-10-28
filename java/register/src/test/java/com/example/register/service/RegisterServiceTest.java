package com.example.register.service;

import java.time.LocalDate;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyString;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import org.mockito.junit.jupiter.MockitoExtension;

import com.example.register.dto.RegisterRequest;
import com.example.register.dto.RegisterResponse;
import com.example.register.entity.User;
import com.example.register.exception.ValidationException;
import com.example.register.repository.UserRepository;

@ExtendWith(MockitoExtension.class)
class RegisterServiceTest {
    
    @Mock
    private UserRepository userRepository;
    
    @InjectMocks
    private RegisterService registerService;
    
    private RegisterRequest validRequest;
    private String idempotencyKey;
    
    @BeforeEach
    void setUp() {
        validRequest = new RegisterRequest();
        validRequest.setFullName("Somkiat Pui");
        validRequest.setUsername("somkiat.p");
        validRequest.setEmail("somkiat.p@example.com");
        validRequest.setPhone("+66812345678");
        validRequest.setPassword("Pa$$w0rd2025!");
        validRequest.setConfirmPassword("Pa$$w0rd2025!");
        validRequest.setDob(LocalDate.of(1995, 5, 10));
        validRequest.setAcceptTerms(true);
        
        idempotencyKey = "test-idempotency-key-123";
    }
    
    @Test
    void testRegister_Success() {
        // Arrange
        when(userRepository.findByIdempotencyKey(anyString())).thenReturn(Optional.empty());
        when(userRepository.existsByUsername(anyString())).thenReturn(false);
        when(userRepository.existsByEmail(anyString())).thenReturn(false);
        when(userRepository.existsByPhone(anyString())).thenReturn(false);
        
        User savedUser = new User();
        savedUser.setUserId("usr_12345");
        savedUser.setFullName(validRequest.getFullName());
        savedUser.setEmail(validRequest.getEmail());
        savedUser.setStatus(User.UserStatus.PENDING_VERIFICATION);
        
        when(userRepository.save(any(User.class))).thenReturn(savedUser);
        
        // Act
        RegisterResponse response = registerService.register(validRequest, idempotencyKey);
        
        // Assert
        assertNotNull(response);
        assertNotNull(response.getUserId());
        assertEquals("pending_verification", response.getStatus());
        assertEquals("email", response.getVerification().getChannel());
        assertNotNull(response.getVerification().getSentAt());
        
        verify(userRepository, times(1)).save(any(User.class));
    }
    
    @Test
    void testRegister_IdempotencyCheck() {
        // Arrange
        User existingUser = new User();
        existingUser.setUserId("usr_existing");
        existingUser.setStatus(User.UserStatus.PENDING_VERIFICATION);
        
        when(userRepository.findByIdempotencyKey(idempotencyKey)).thenReturn(Optional.of(existingUser));
        
        // Act
        RegisterResponse response = registerService.register(validRequest, idempotencyKey);
        
        // Assert
        assertNotNull(response);
        assertEquals("usr_existing", response.getUserId());
        
        verify(userRepository, never()).save(any(User.class));
    }
    
    @Test
    void testRegister_PasswordMismatch() {
        // Arrange
        validRequest.setConfirmPassword("DifferentPassword123!");
        when(userRepository.findByIdempotencyKey(anyString())).thenReturn(Optional.empty());
        
        // Act & Assert
        ValidationException exception = assertThrows(ValidationException.class, () -> {
            registerService.register(validRequest, idempotencyKey);
        });
        
        assertEquals("VALIDATION_FAILED", exception.getErrorCode());
        assertTrue(exception.getFieldErrors().containsKey("confirmPassword"));
        assertEquals("Passwords do not match", exception.getFieldErrors().get("confirmPassword"));
    }
    
    @Test
    void testRegister_UsernameExists() {
        // Arrange
        when(userRepository.findByIdempotencyKey(anyString())).thenReturn(Optional.empty());
        when(userRepository.existsByUsername(validRequest.getUsername())).thenReturn(true);
        
        // Act & Assert
        ValidationException exception = assertThrows(ValidationException.class, () -> {
            registerService.register(validRequest, idempotencyKey);
        });
        
        assertEquals("VALIDATION_FAILED", exception.getErrorCode());
        assertTrue(exception.getFieldErrors().containsKey("username"));
        assertEquals("Username already exists", exception.getFieldErrors().get("username"));
    }
    
    @Test
    void testRegister_EmailExists() {
        // Arrange
        when(userRepository.findByIdempotencyKey(anyString())).thenReturn(Optional.empty());
        when(userRepository.existsByUsername(anyString())).thenReturn(false);
        when(userRepository.existsByEmail(validRequest.getEmail())).thenReturn(true);
        
        // Act & Assert
        ValidationException exception = assertThrows(ValidationException.class, () -> {
            registerService.register(validRequest, idempotencyKey);
        });
        
        assertEquals("VALIDATION_FAILED", exception.getErrorCode());
        assertTrue(exception.getFieldErrors().containsKey("email"));
        assertEquals("Email already registered", exception.getFieldErrors().get("email"));
    }
    
    @Test
    void testRegister_PhoneExists() {
        // Arrange
        when(userRepository.findByIdempotencyKey(anyString())).thenReturn(Optional.empty());
        when(userRepository.existsByUsername(anyString())).thenReturn(false);
        when(userRepository.existsByEmail(anyString())).thenReturn(false);
        when(userRepository.existsByPhone(validRequest.getPhone())).thenReturn(true);
        
        // Act & Assert
        ValidationException exception = assertThrows(ValidationException.class, () -> {
            registerService.register(validRequest, idempotencyKey);
        });
        
        assertEquals("VALIDATION_FAILED", exception.getErrorCode());
        assertTrue(exception.getFieldErrors().containsKey("phone"));
        assertEquals("Phone number already registered", exception.getFieldErrors().get("phone"));
    }
    
    @Test
    void testRegister_MultipleValidationErrors() {
        // Arrange
        validRequest.setConfirmPassword("WrongPassword!");
        when(userRepository.findByIdempotencyKey(anyString())).thenReturn(Optional.empty());
        when(userRepository.existsByUsername(validRequest.getUsername())).thenReturn(true);
        when(userRepository.existsByEmail(validRequest.getEmail())).thenReturn(true);
        
        // Act & Assert
        ValidationException exception = assertThrows(ValidationException.class, () -> {
            registerService.register(validRequest, idempotencyKey);
        });
        
        assertEquals("VALIDATION_FAILED", exception.getErrorCode());
        assertTrue(exception.getFieldErrors().size() >= 2);
    }
}
