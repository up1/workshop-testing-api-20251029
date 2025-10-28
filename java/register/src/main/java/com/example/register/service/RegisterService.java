package com.example.register.service;

import java.time.LocalDateTime;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.example.register.dto.RegisterRequest;
import com.example.register.dto.RegisterResponse;
import com.example.register.entity.User;
import com.example.register.exception.ValidationException;
import com.example.register.repository.UserRepository;

@Service
public class RegisterService {
    
    private final UserRepository userRepository;
    private final BCryptPasswordEncoder passwordEncoder;
    
    @Autowired
    public RegisterService(UserRepository userRepository) {
        this.userRepository = userRepository;
        this.passwordEncoder = new BCryptPasswordEncoder();
    }
    
    @Transactional
    public RegisterResponse register(RegisterRequest request, String idempotencyKey) {
        // Check idempotency
        Optional<User> existingUser = userRepository.findByIdempotencyKey(idempotencyKey);
        if (existingUser.isPresent()) {
            return buildResponse(existingUser.get());
        }
        
        // Validate request
        validateRegistrationRequest(request);
        
        // Create user
        User user = new User();
        user.setUserId(generateUserId());
        user.setFullName(request.getFullName());
        user.setUsername(request.getUsername());
        user.setEmail(request.getEmail());
        user.setPhone(request.getPhone());
        user.setPassword(passwordEncoder.encode(request.getPassword()));
        user.setDob(request.getDob());
        user.setAcceptTerms(request.getAcceptTerms());
        user.setIdempotencyKey(idempotencyKey);
        user.setStatus(User.UserStatus.PENDING_VERIFICATION);
        user.setCreatedAt(LocalDateTime.now());
        
        // Save user
        user = userRepository.save(user);
        
        // Send verification email (simulated)
        sendVerificationEmail(user);
        
        return buildResponse(user);
    }
    
    private void validateRegistrationRequest(RegisterRequest request) {
        Map<String, String> errors = new HashMap<>();
        
        // Check password match
        if (!request.getPassword().equals(request.getConfirmPassword())) {
            errors.put("confirmPassword", "Passwords do not match");
        }
        
        // Check username uniqueness
        if (userRepository.existsByUsername(request.getUsername())) {
            errors.put("username", "Username already exists");
        }
        
        // Check email uniqueness
        if (userRepository.existsByEmail(request.getEmail())) {
            errors.put("email", "Email already registered");
        }
        
        // Check phone uniqueness
        if (userRepository.existsByPhone(request.getPhone())) {
            errors.put("phone", "Phone number already registered");
        }
        
        if (!errors.isEmpty()) {
            throw new ValidationException("VALIDATION_FAILED", errors);
        }
    }
    
    private String generateUserId() {
        return "usr_" + UUID.randomUUID().toString().replace("-", "").substring(0, 10);
    }
    
    private void sendVerificationEmail(User user) {
        // This is a placeholder for actual email sending logic
        // In production, this would integrate with an email service
        System.out.println("Sending verification email to: " + user.getEmail());
    }
    
    private RegisterResponse buildResponse(User user) {
        RegisterResponse.VerificationInfo verificationInfo = new RegisterResponse.VerificationInfo(
            "email",
            user.getCreatedAt()
        );
        
        return new RegisterResponse(
            user.getUserId(),
            user.getStatus().name().toLowerCase(),
            verificationInfo
        );
    }
}
