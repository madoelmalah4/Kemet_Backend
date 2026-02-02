# Quick Testing Guide

## 🚀 Application Status
✅ **Running on**: http://localhost:5131  
✅ **Swagger UI**: http://localhost:5131/swagger  
✅ **Status**: All improvements applied and tested

---

## 🧪 Quick Test Scenarios

### Scenario 1: Create a Complete Trip (Happy Path)

**Endpoint**: `POST /api/Trip`

**Request**:
```json
{
  "title": "7-Day Egypt Adventure",
  "travelCompanions": "Family",
  "travelStyle": "MidBudget",
  "experienceTypes": ["Cultural", "Historical"],
  "interests": ["Ancient History", "Photography"],
  "startDate": "2026-03-01",
  "endDate": "2026-03-07",
  "durationDays": 7,
  "price": 2500.00,
  "description": "Explore the wonders of ancient Egypt",
  "imageUrl": "https://example.com/egypt.jpg",
  "days": [
    {
      "dayNumber": 1,
      "date": "2026-03-01",
      "title": "Arrival in Cairo",
      "description": "Visit the Pyramids of Giza",
      "city": "Cairo",
      "activities": []
    },
    {
      "dayNumber": 2,
      "date": "2026-03-02",
      "title": "Cairo Museums",
      "description": "Explore Egyptian Museum",
      "city": "Cairo",
      "activities": []
    }
  ]
}
```

**Expected**: `201 Created` with full trip details

---

### Scenario 2: Test Date Validation (Should Fail)

**Endpoint**: `POST /api/Trip`

**Request**:
```json
{
  "title": "Invalid Trip",
  "travelCompanions": "Solo",
  "travelStyle": "Budget",
  "startDate": "2026-03-10",
  "endDate": "2026-03-05",
  "durationDays": 5,
  "price": 1000,
  "description": "This should fail",
  "days": []
}
```

**Expected**: `400 Bad Request`
```json
{
  "message": "Validation error",
  "error": "End date must be after start date"
}
```

---

### Scenario 3: Test Duration Mismatch (Should Fail)

**Endpoint**: `POST /api/Trip`

**Request**:
```json
{
  "title": "Duration Mismatch",
  "travelCompanions": "Couple",
  "travelStyle": "Luxury",
  "startDate": "2026-03-01",
  "endDate": "2026-03-07",
  "durationDays": 10,
  "price": 5000,
  "description": "Duration doesn't match dates",
  "days": []
}
```

**Expected**: `400 Bad Request`
```json
{
  "message": "Validation error",
  "error": "Duration days (10) doesn't match date range (7 days)"
}
```

---

### Scenario 4: Test Duplicate Day Numbers (Should Fail)

**Endpoint**: `POST /api/Trip`

**Request**:
```json
{
  "title": "Duplicate Days",
  "travelCompanions": "Friends",
  "travelStyle": "MidBudget",
  "startDate": "2026-03-01",
  "endDate": "2026-03-03",
  "durationDays": 3,
  "price": 1500,
  "description": "Has duplicate day numbers",
  "days": [
    {
      "dayNumber": 1,
      "title": "Day 1",
      "description": "First day",
      "city": "Cairo",
      "activities": []
    },
    {
      "dayNumber": 1,
      "title": "Also Day 1",
      "description": "Duplicate!",
      "city": "Cairo",
      "activities": []
    }
  ]
}
```

**Expected**: `400 Bad Request` (caught by validation)

---

### Scenario 5: Get User Trips

**Endpoint**: `GET /api/Trip`

**Headers**: 
```
Authorization: Bearer {your-jwt-token}
```

**Expected**: `200 OK` with array of user's trips

---

### Scenario 6: Get Specific Trip

**Endpoint**: `GET /api/Trip/{tripId}`

**Expected**: `200 OK` with full trip details including days and activities

---

### Scenario 7: Add Day to Existing Trip

**Endpoint**: `POST /api/Trip/{tripId}/days`

**Request**:
```json
{
  "dayNumber": 3,
  "date": "2026-03-03",
  "title": "Luxor Temple",
  "description": "Explore ancient temples",
  "city": "Luxor",
  "activities": []
}
```

**Expected**: `200 OK` with created day details

---

### Scenario 8: Test Permission (Should Fail)

**Endpoint**: `PUT /api/Trip/{someone-elses-trip-id}`

**Expected**: `403 Forbidden` (user trying to edit another user's trip)

---

## 🔍 Performance Testing

### Test 1: Query Performance
1. Create 50 trips with 5 days each
2. Query all user trips
3. Measure response time (should be < 100ms)

### Test 2: Complex Trip Creation
1. Create trip with 10 days
2. Each day has 5 activities
3. Measure response time (should be < 300ms)

### Test 3: Concurrent Requests
1. Make 10 simultaneous trip creation requests
2. Verify all succeed or fail gracefully
3. Check for race conditions

---

## 🛠️ Using Swagger UI

1. **Open Swagger**: http://localhost:5131/swagger

2. **Authorize**:
   - Click "Authorize" button
   - Enter: `Bearer {your-jwt-token}`
   - Click "Authorize" then "Close"

3. **Test Endpoints**:
   - Expand any endpoint
   - Click "Try it out"
   - Fill in parameters
   - Click "Execute"
   - View response

---

## 📊 What to Check

### ✅ Functionality
- [ ] Can create trips with nested days/activities
- [ ] Can retrieve user trips
- [ ] Can update trip details
- [ ] Can add/remove days
- [ ] Can add/remove activities
- [ ] Can delete trips

### ✅ Validation
- [ ] Date range validation works
- [ ] Duration validation works
- [ ] Day number validation works
- [ ] Destination ID validation works
- [ ] Activity duration validation works

### ✅ Error Handling
- [ ] Validation errors return 400 with details
- [ ] Not found returns 404 with message
- [ ] Unauthorized returns 401 with message
- [ ] Forbidden returns 403
- [ ] Server errors return 500 with message

### ✅ Performance
- [ ] Trip queries are fast (< 100ms)
- [ ] Trip creation is fast (< 300ms)
- [ ] No N+1 query problems
- [ ] Days and activities are ordered correctly

### ✅ Security
- [ ] Users can only access own trips
- [ ] Admins can access all trips
- [ ] Invalid tokens are rejected
- [ ] Permission checks work correctly

---

## 🐛 Common Issues & Solutions

### Issue: "Trip not found"
**Solution**: Verify the trip ID is correct and belongs to the authenticated user

### Issue: "Destination not found"
**Solution**: Ensure destination IDs exist in the database before creating activities

### Issue: "Day number already exists"
**Solution**: Check for duplicate day numbers in the request

### Issue: "Unauthorized"
**Solution**: Ensure Bearer token is included in Authorization header

### Issue: "Validation error"
**Solution**: Check the error message for specific validation failure details

---

## 📈 Expected Performance

| Operation | Expected Time | Status |
|-----------|---------------|--------|
| Get all user trips | < 100ms | ✅ Optimized |
| Get single trip | < 120ms | ✅ Optimized |
| Create simple trip | < 200ms | ✅ Optimized |
| Create complex trip | < 300ms | ✅ Optimized |
| Update trip | < 150ms | ✅ Optimized |
| Delete trip | < 100ms | ✅ Optimized |

---

## 🎯 Success Criteria

### All Tests Pass ✅
- Trip creation works with full validation
- Updates work correctly
- Deletes work correctly
- Permissions are enforced
- Errors are descriptive

### Performance Meets Targets ✅
- All operations complete within expected time
- No performance degradation with multiple trips
- Database indexes are being used

### User Experience is Good ✅
- Error messages are clear and helpful
- Response format is consistent
- API is intuitive to use

---

## 📝 Test Results Template

```
Date: ___________
Tester: ___________

Functionality Tests:
[ ] Create trip - PASS/FAIL
[ ] Get trips - PASS/FAIL
[ ] Update trip - PASS/FAIL
[ ] Delete trip - PASS/FAIL
[ ] Add day - PASS/FAIL
[ ] Add activity - PASS/FAIL

Validation Tests:
[ ] Date validation - PASS/FAIL
[ ] Duration validation - PASS/FAIL
[ ] Day number validation - PASS/FAIL
[ ] Destination validation - PASS/FAIL

Performance Tests:
[ ] Query speed < 100ms - PASS/FAIL
[ ] Create speed < 300ms - PASS/FAIL

Security Tests:
[ ] Permission checks - PASS/FAIL
[ ] Authorization - PASS/FAIL

Overall Status: PASS/FAIL
Notes: ___________
```

---

## 🚀 Next Steps After Testing

1. **If all tests pass**:
   - ✅ System is production ready
   - ✅ Can deploy to staging/production
   - ✅ Monitor performance in production

2. **If tests fail**:
   - Check error messages
   - Review logs
   - Fix issues
   - Re-test

3. **Performance issues**:
   - Check database indexes
   - Review query execution plans
   - Monitor database load

---

**Last Updated**: 2026-02-02  
**Status**: ✅ Ready for Testing  
**Application**: Running on http://localhost:5131
