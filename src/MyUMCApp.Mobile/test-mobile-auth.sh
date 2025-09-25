#!/bin/bash

# Test script to demonstrate mobile authentication functionality
API_BASE="http://localhost:5294/api/auth"

echo "=== MyUMC Mobile Authentication Demo ==="
echo "Testing API at: $API_BASE"
echo

# Test 1: Register a new user
echo "1. Testing User Registration..."
REGISTER_RESULT=$(curl -s -X POST "$API_BASE/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "mobile@myumc.com",
    "password": "MobileTest123!",
    "confirmPassword": "MobileTest123!",
    "firstName": "Mobile",
    "lastName": "User",
    "organization": "Mobile UMC",
    "churchRole": "Member"
  }')

echo "Registration Result:"
echo "$REGISTER_RESULT" | jq .
echo

# Test 2: Login with the new user
echo "2. Testing User Login..."
LOGIN_RESULT=$(curl -s -X POST "$API_BASE/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "mobile@myumc.com",
    "password": "MobileTest123!"
  }')

echo "Login Result:"
echo "$LOGIN_RESULT" | jq .

# Extract token for authenticated requests
TOKEN=$(echo "$LOGIN_RESULT" | jq -r '.token')
echo "Extracted Token: ${TOKEN:0:50}..."
echo

# Test 3: Get user profile (authenticated)
echo "3. Testing Get User Profile (authenticated)..."
PROFILE_RESULT=$(curl -s -X GET "$API_BASE/profile" \
  -H "Authorization: Bearer $TOKEN")

echo "Profile Result:"
echo "$PROFILE_RESULT" | jq .
echo

# Test 4: Validate token
echo "4. Testing Token Validation..."
VALIDATE_RESULT=$(curl -s -X GET "$API_BASE/validate-token" \
  -H "Authorization: Bearer $TOKEN")

echo "Token Validation Result:"
echo "$VALIDATE_RESULT" | jq .
echo

# Test 5: Forgot password
echo "5. Testing Forgot Password..."
FORGOT_RESULT=$(curl -s -X POST "$API_BASE/forgot-password" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "mobile@myumc.com"
  }')

echo "Forgot Password Result:"
echo "$FORGOT_RESULT" | jq .
echo

# Test 6: Logout
echo "6. Testing Logout..."
LOGOUT_RESULT=$(curl -s -X POST "$API_BASE/logout" \
  -H "Authorization: Bearer $TOKEN")

echo "Logout Result:"
echo "$LOGOUT_RESULT" | jq .
echo

# Test 7: Try to access profile after logout (should fail)
echo "7. Testing Profile Access After Logout (should fail)..."
PROFILE_AFTER_LOGOUT=$(curl -s -X GET "$API_BASE/profile" \
  -H "Authorization: Bearer $TOKEN")

echo "Profile After Logout Result:"
echo "$PROFILE_AFTER_LOGOUT" | jq .
echo

echo "=== Mobile Authentication Demo Complete ==="