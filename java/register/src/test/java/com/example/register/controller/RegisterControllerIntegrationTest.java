package com.example.register.controller;

import java.time.LocalDate;

import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.notNullValue;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import com.example.register.dto.RegisterRequest;
import com.example.register.entity.User;
import com.example.register.repository.UserRepository;
import com.fasterxml.jackson.databind.ObjectMapper;

@SpringBootTest
@AutoConfigureMockMvc
class RegisterControllerIntegrationTest {
    
    @Autowired
    private MockMvc mockMvc;
    
    @Autowired
    private ObjectMapper objectMapper;
    
    @Autowired
    private UserRepository userRepository;
    
    @BeforeEach
    void setUp() {
        userRepository.deleteAll();
    }
    
    @Test
    void testRegister_Success() throws Exception {
        // Arrange
        RegisterRequest request = createValidRequest();
        
        // Act & Assert
        mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", "test-key-001")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isCreated())
                .andExpect(jsonPath("$.userId", notNullValue()))
                .andExpect(jsonPath("$.status", is("pending_verification")))
                .andExpect(jsonPath("$.verification.channel", is("email")))
                .andExpect(jsonPath("$.verification.sentAt", notNullValue()));
    }
    
    @Test
    void testRegister_Idempotency() throws Exception {
        // Arrange
        RegisterRequest request = createValidRequest();
        String idempotencyKey = "test-key-002";
        
        // Act - First request
        String firstResponse = mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", idempotencyKey)
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isCreated())
                .andReturn()
                .getResponse()
                .getContentAsString();
        
        // Act - Second request with same idempotency key
        String secondResponse = mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", idempotencyKey)
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isCreated())
                .andReturn()
                .getResponse()
                .getContentAsString();
        
        // Assert - Both responses should be identical
        assertEquals(firstResponse, secondResponse);
    }
    
    @Test
    void testRegister_ValidationErrors() throws Exception {
        // Arrange
        RegisterRequest request = new RegisterRequest();
        request.setFullName("A"); // Too short
        request.setUsername("ab"); // Too short
        request.setEmail("invalid-email"); // Invalid format
        request.setPhone("123"); // Invalid format
        request.setPassword("weak"); // Too weak
        request.setConfirmPassword("weak");
        request.setDob(LocalDate.now().plusDays(1)); // Future date
        request.setAcceptTerms(false); // Not accepted
        
        // Act & Assert
        mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", "test-key-003")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.error.code", is("VALIDATION_FAILED")))
                .andExpect(jsonPath("$.error.fields", notNullValue()));
    }
    
    @Test
    void testRegister_PasswordMismatch() throws Exception {
        // Arrange
        RegisterRequest request = createValidRequest();
        request.setConfirmPassword("DifferentPassword123!");
        
        // Act & Assert
        mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", "test-key-004")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.error.code", is("VALIDATION_FAILED")))
                .andExpect(jsonPath("$.error.fields.confirmPassword", is("Passwords do not match")));
    }
    
    @Test
    void testRegister_DuplicateUsername() throws Exception {
        // Arrange
        RegisterRequest firstRequest = createValidRequest();
        firstRequest.setUsername("duplicate.user");
        firstRequest.setEmail("first@example.com");
        firstRequest.setPhone("+66811111111");
        
        // Create first user
        mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", "test-key-005")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(firstRequest)))
                .andExpect(status().isCreated());
        
        // Try to create second user with same username
        RegisterRequest secondRequest = createValidRequest();
        secondRequest.setUsername("duplicate.user"); // Same username
        secondRequest.setEmail("second@example.com"); // Different email
        secondRequest.setPhone("+66822222222"); // Different phone
        
        // Act & Assert
        mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", "test-key-006")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(secondRequest)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.error.code", is("VALIDATION_FAILED")))
                .andExpect(jsonPath("$.error.fields.username", is("Username already exists")));
    }
    
    @Test
    void testRegister_DuplicateEmail() throws Exception {
        // Arrange
        RegisterRequest firstRequest = createValidRequest();
        firstRequest.setUsername("user.one");
        firstRequest.setEmail("duplicate@example.com");
        firstRequest.setPhone("+66811111111");
        
        // Create first user
        mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", "test-key-007")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(firstRequest)))
                .andExpect(status().isCreated());
        
        // Try to create second user with same email
        RegisterRequest secondRequest = createValidRequest();
        secondRequest.setUsername("user.two"); // Different username
        secondRequest.setEmail("duplicate@example.com"); // Same email
        secondRequest.setPhone("+66822222222"); // Different phone
        
        // Act & Assert
        mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", "test-key-008")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(secondRequest)))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.error.code", is("VALIDATION_FAILED")))
                .andExpect(jsonPath("$.error.fields.email", is("Email already registered")));
    }
    
    @Test
    void testRegister_PasswordEncryption() throws Exception {
        // Arrange
        RegisterRequest request = createValidRequest();
        String plainPassword = request.getPassword();
        
        // Act
        mockMvc.perform(post("/api/v1/register")
                .header("Idempotency-Key", "test-key-009")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isCreated());
        
        // Assert - Check that password is encrypted in database
        User savedUser = userRepository.findByEmail(request.getEmail()).orElseThrow();
        assertNotEquals(plainPassword, savedUser.getPassword());
        assertTrue(savedUser.getPassword().startsWith("$2a$") || savedUser.getPassword().startsWith("$2b$"));
    }
    
    @Test
    void testRegister_MissingIdempotencyKey() throws Exception {
        // Arrange
        RegisterRequest request = createValidRequest();
        
        // Act & Assert - Should succeed with auto-generated idempotency key
        mockMvc.perform(post("/api/v1/register")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isCreated())
                .andExpect(jsonPath("$.userId", notNullValue()))
                .andExpect(jsonPath("$.status", is("pending_verification")));
    }
    
    private RegisterRequest createValidRequest() {
        RegisterRequest request = new RegisterRequest();
        request.setFullName("Somkiat Pui");
        request.setUsername("somkiat.p");
        request.setEmail("somkiat.p@example.com");
        request.setPhone("+66812345678");
        request.setPassword("Pa$$w0rd2025!");
        request.setConfirmPassword("Pa$$w0rd2025!");
        request.setDob(LocalDate.of(1995, 5, 10));
        request.setAcceptTerms(true);
        return request;
    }
    
    private void assertEquals(String firstResponse, String secondResponse) {
        // Simple string comparison for idempotency test
        assert firstResponse.equals(secondResponse);
    }
    
    private void assertNotEquals(String plainPassword, String encryptedPassword) {
        assert !plainPassword.equals(encryptedPassword);
    }
    
    private void assertTrue(boolean condition) {
        assert condition;
    }
}
