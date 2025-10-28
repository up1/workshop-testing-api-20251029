package com.example.register.repository;

import java.util.Optional;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.example.register.entity.User;

@Repository
public interface UserRepository extends JpaRepository<User, Long> {
    
    boolean existsByUsername(String username);
    
    boolean existsByEmail(String email);
    
    boolean existsByPhone(String phone);
    
    Optional<User> findByIdempotencyKey(String idempotencyKey);
    
    Optional<User> findByUserId(String userId);
    
    Optional<User> findByEmail(String email);
}
