package com.example.register.dto;

import java.util.HashMap;
import java.util.Map;

public class ErrorResponse {
    
    private ErrorDetail error;
    
    public ErrorResponse() {
    }
    
    public ErrorResponse(ErrorDetail error) {
        this.error = error;
    }
    
    public ErrorDetail getError() {
        return error;
    }
    
    public void setError(ErrorDetail error) {
        this.error = error;
    }
    
    public static class ErrorDetail {
        private String code;
        private Map<String, String> fields;
        
        public ErrorDetail() {
            this.fields = new HashMap<>();
        }
        
        public ErrorDetail(String code) {
            this.code = code;
            this.fields = new HashMap<>();
        }
        
        public ErrorDetail(String code, Map<String, String> fields) {
            this.code = code;
            this.fields = fields;
        }
        
        public String getCode() {
            return code;
        }
        
        public void setCode(String code) {
            this.code = code;
        }
        
        public Map<String, String> getFields() {
            return fields;
        }
        
        public void setFields(Map<String, String> fields) {
            this.fields = fields;
        }
        
        public void addField(String fieldName, String errorMessage) {
            this.fields.put(fieldName, errorMessage);
        }
    }
}
