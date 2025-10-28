package com.example.register.dto;

import java.time.LocalDateTime;

public class RegisterResponse {
    
    private String userId;
    private String status;
    private VerificationInfo verification;
    
    // Constructors
    public RegisterResponse() {
    }
    
    public RegisterResponse(String userId, String status, VerificationInfo verification) {
        this.userId = userId;
        this.status = status;
        this.verification = verification;
    }
    
    // Getters and Setters
    public String getUserId() {
        return userId;
    }
    
    public void setUserId(String userId) {
        this.userId = userId;
    }
    
    public String getStatus() {
        return status;
    }
    
    public void setStatus(String status) {
        this.status = status;
    }
    
    public VerificationInfo getVerification() {
        return verification;
    }
    
    public void setVerification(VerificationInfo verification) {
        this.verification = verification;
    }
    
    // Nested class for verification info
    public static class VerificationInfo {
        private String channel;
        private LocalDateTime sentAt;
        
        public VerificationInfo() {
        }
        
        public VerificationInfo(String channel, LocalDateTime sentAt) {
            this.channel = channel;
            this.sentAt = sentAt;
        }
        
        public String getChannel() {
            return channel;
        }
        
        public void setChannel(String channel) {
            this.channel = channel;
        }
        
        public LocalDateTime getSentAt() {
            return sentAt;
        }
        
        public void setSentAt(LocalDateTime sentAt) {
            this.sentAt = sentAt;
        }
    }
}
