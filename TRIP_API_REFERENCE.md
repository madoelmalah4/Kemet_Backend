# Trip API Quick Reference

## Base URL
```
http://localhost:5131/api/Trip
```

## Authentication
All endpoints require Bearer token authentication:
```
Authorization: Bearer {your-jwt-token}
```

---

## 📋 Endpoints

### 1. Get All Trips
```http
GET /api/Trip
```

**Authorization**: User (own trips) | Admin (all trips)

**Response**: `200 OK`
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "title": "Trip to Egypt",
    "travelCompanions": "Family",
    "travelStyle": "MidBudget",
    "startDate": "2026-03-01",
    "endDate": "2026-03-07",
    "durationDays": 7,
    "price": 2500.00,
    "description": "Amazing trip",
    "imageUrl": "https://...",
    "createdAt": "2026-02-01T12:00:00Z",
    "days": [...]
  }
]
```

---

### 2. Get Trip by ID
```http
GET /api/Trip/{id}
```

**Authorization**: Owner or Admin

**Response**: `200 OK` | `404 Not Found`
```json
{
  "id": "guid",
  "title": "Trip to Egypt",
  "days": [
    {
      "id": "guid",
      "dayNumber": 1,
      "title": "Day 1",
      "activities": [...]
    }
  ]
}
```

---

### 3. Create Trip
```http
POST /api/Trip
Content-Type: application/json
```

**Authorization**: User (creates user trip) | Admin (creates system trip)

**Request Body**:
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
  "description": "Explore ancient wonders",
  "imageUrl": "https://example.com/image.jpg",
  "days": [
    {
      "dayNumber": 1,
      "date": "2026-03-01",
      "title": "Arrival in Cairo",
      "description": "Visit the Pyramids",
      "city": "Cairo",
      "activities": [
        {
          "destinationId": "guid-of-destination",
          "activityType": "Sightseeing",
          "startTime": "09:00:00",
          "durationHours": 4,
          "description": "Guided tour of the Pyramids"
        }
      ]
    }
  ]
}
```

**Validation Rules**:
- ✅ End date must be after start date
- ✅ Duration days must match date range
- ✅ Day numbers must be sequential starting from 1
- ✅ All destination IDs must exist
- ✅ Activity duration must be > 0

**Response**: `201 Created` | `400 Bad Request`

---

### 4. Update Trip
```http
PUT /api/Trip/{id}
Content-Type: application/json
```

**Authorization**: Owner or Admin

**Request Body**: Same as Create Trip (without nested days)

**Response**: `200 OK` | `404 Not Found` | `403 Forbidden`

---

### 5. Delete Trip
```http
DELETE /api/Trip/{id}
```

**Authorization**: Owner or Admin

**Response**: `204 No Content` | `404 Not Found` | `403 Forbidden`

---

## 📅 Day Operations

### 6. Add Day to Trip
```http
POST /api/Trip/{tripId}/days
Content-Type: application/json
```

**Request Body**:
```json
{
  "dayNumber": 2,
  "date": "2026-03-02",
  "title": "Luxor Temple",
  "description": "Explore ancient temples",
  "city": "Luxor",
  "activities": [
    {
      "destinationId": "guid",
      "activityType": "Museum",
      "startTime": "10:00:00",
      "durationHours": 3,
      "description": "Temple tour"
    }
  ]
}
```

**Response**: `200 OK` | `400 Bad Request` | `404 Not Found`

---

### 7. Update Day
```http
PUT /api/Trip/{tripId}/days/{dayId}
Content-Type: application/json
```

**Request Body**: Same as Add Day

**Response**: `200 OK` | `404 Not Found` | `403 Forbidden`

---

### 8. Delete Day
```http
DELETE /api/Trip/{tripId}/days/{dayId}
```

**Response**: `204 No Content` | `404 Not Found` | `403 Forbidden`

---

## 🎯 Activity Operations

### 9. Add Activity to Day
```http
POST /api/Trip/{tripId}/days/{dayId}/activities
Content-Type: application/json
```

**Request Body**:
```json
{
  "destinationId": "guid",
  "activityType": "Sightseeing",
  "startTime": "14:00:00",
  "durationHours": 2.5,
  "description": "Optional description"
}
```

**Activity Types**:
- `Sightseeing`
- `Food`
- `Museum`
- `Adventure`
- `Relaxation`
- `Shopping`
- `NightLife`
- `Other`

**Response**: `200 OK` | `400 Bad Request` | `404 Not Found`

---

### 10. Update Activity
```http
PUT /api/Trip/{tripId}/days/{dayId}/activities/{activityId}
Content-Type: application/json
```

**Request Body**: Same as Add Activity

**Response**: `200 OK` | `404 Not Found` | `403 Forbidden`

---

### 11. Delete Activity
```http
DELETE /api/Trip/{tripId}/days/{dayId}/activities/{activityId}
```

**Response**: `204 No Content` | `404 Not Found` | `403 Forbidden`

---

## 🚨 Error Responses

### Validation Error (400)
```json
{
  "message": "Validation error",
  "error": "Day number 1 already exists for this trip"
}
```

### Unauthorized (401)
```json
{
  "message": "Invalid user credentials"
}
```

### Forbidden (403)
```json
{
  "message": "You don't have permission to access this resource"
}
```

### Not Found (404)
```json
{
  "message": "Trip with ID {guid} not found"
}
```

### Server Error (500)
```json
{
  "message": "An error occurred while creating the trip",
  "error": "Technical details..."
}
```

---

## 📊 Enums Reference

### TravelCompanion
```csharp
Solo | Couple | Family | Friends
```

### TravelStyle
```csharp
Budget | MidBudget | Luxury
```

### ActivityType
```csharp
Sightseeing | Food | Museum | Adventure | 
Relaxation | Shopping | NightLife | Other
```

---

## 💡 Best Practices

### 1. Creating Complete Trips
✅ **DO**: Create trip with all days and activities in one request
```json
POST /api/Trip
{
  "title": "...",
  "days": [...]  // Include all days
}
```

❌ **DON'T**: Create trip then add days one by one (slower)

### 2. Date Handling
✅ **DO**: Use ISO 8601 format
```json
"startDate": "2026-03-01T00:00:00Z"
```

❌ **DON'T**: Use locale-specific formats

### 3. Time Handling
✅ **DO**: Use TimeSpan format for activity times
```json
"startTime": "09:30:00"  // 9:30 AM
```

### 4. Error Handling
✅ **DO**: Check response status and parse error messages
```javascript
if (response.status === 400) {
  const error = await response.json();
  console.error(error.message, error.error);
}
```

---

## 🔍 Testing with cURL

### Create Trip
```bash
curl -X POST http://localhost:5131/api/Trip \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Trip",
    "travelCompanions": "Solo",
    "travelStyle": "Budget",
    "startDate": "2026-03-01",
    "endDate": "2026-03-03",
    "durationDays": 3,
    "price": 500,
    "description": "Test",
    "days": []
  }'
```

### Get All Trips
```bash
curl -X GET http://localhost:5131/api/Trip \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 📈 Performance Tips

1. **Use Pagination** (when implemented)
   - Don't fetch all trips at once
   - Request only what you need

2. **Cache Responses**
   - Cache trip lists on client side
   - Invalidate on create/update/delete

3. **Batch Operations**
   - Create trips with full hierarchy
   - Reduces round trips to server

4. **Use Proper Indexes**
   - Already implemented on server
   - Queries are optimized

---

## 🔐 Security Notes

1. **Authentication Required**: All endpoints require valid JWT token
2. **Authorization**: Users can only access their own trips (except admins)
3. **Input Validation**: All inputs are validated server-side
4. **SQL Injection**: Protected by EF Core parameterized queries
5. **Rate Limiting**: 100 requests per minute per IP

---

**Last Updated**: 2026-02-02  
**API Version**: 1.0  
**Status**: ✅ Production Ready
