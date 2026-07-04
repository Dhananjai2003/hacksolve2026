# Event ID Implementation Summary

## Overview
Implemented event tracking for booking notifications by adding `notify_event_id` field to the desk_schedule table and updating the notification flow to capture and use event IDs.

---

## Database Changes

### Entity: `DeskSchedule` (table: desk_schedule)
**New Field Added:**
```csharp
public string? NotifyEventId { get; set; }
```

### Model: `DeskSchedule` (DTO)
**New Field Added:**
```csharp
public string? NotifyEventId { get; set; }
```

---

## Notification Flow

### 1. **CREATE Operation** (BookAsync)

#### Steps:
1. Create booking in database
2. Send notification to API with `eventId: null`
3. Receive response with new `eventId`
4. Update booking record with the received `eventId`

#### Request Payload:
```json
{
  "deskName": "Desk 101",
  "floorName": "First Floor",
  "officeName": "Main Office",
  "bookingDateTime": "2024-01-15T10:00:00Z",
  "userId": "user-123",
  "userEmail": "john.doe@example.com",
  "status": "Created",
  "eventId": null
}
```

#### Expected Response:
```json
{
  "status": "success",
  "action": "created",
  "eventId": "evt_abc123xyz"
}
```

#### Result:
✅ Booking stored in database with `notify_event_id = "evt_abc123xyz"`

---

### 2. **UPDATE Operation** (UpdateAsync)

#### Steps:
1. Retrieve existing booking (including `NotifyEventId`)
2. Update booking details
3. Send notification with **existing eventId**
4. Notification API uses eventId to update the same event

#### Request Payload:
```json
{
  "deskName": "Desk 102",
  "floorName": "Second Floor",
  "officeName": "Main Office",
  "bookingDateTime": "2024-01-16T14:00:00Z",
  "userId": "user-123",
  "userEmail": "john.doe@example.com",
  "status": "Updated",
  "eventId": "evt_abc123xyz"
}
```

#### Expected Response:
```json
{
  "status": "success",
  "action": "updated",
  "eventId": "evt_abc123xyz"
}
```

---

### 3. **CANCEL Operation** (CancelAsync)

#### Steps:
1. Retrieve booking details (including `NotifyEventId`)
2. Delete booking from database
3. Send notification with **existing eventId**
4. Notification API uses eventId to cancel/close the event

#### Request Payload:
```json
{
  "deskName": "Desk 101",
  "floorName": "First Floor",
  "officeName": "Main Office",
  "bookingDateTime": "2024-01-15T10:00:00Z",
  "userId": "user-123",
  "userEmail": "john.doe@example.com",
  "status": "Cancelled",
  "eventId": "evt_abc123xyz"
}
```

#### Expected Response:
```json
{
  "status": "success",
  "action": "cancelled",
  "eventId": "evt_abc123xyz"
}
```

---

## Implementation Details

### SendBookingNotificationAsync Method

**Updated Signature:**
```csharp
private async Task<string?> SendBookingNotificationAsync(
	string deskName, 
	string? floorName, 
	string? officeName, 
	DateTime? bookingDateTime, 
	string userId, 
	string? userEmail,
	string status,
	string? eventId,           // NEW: Input event ID for update/cancel
	CancellationToken ct)
```

**Return Value:**
- Returns `string?` - The event ID from the API response
- Returns `null` if the API call fails

### Key Changes:

1. **Method now returns event ID** instead of void
2. **Accepts eventId parameter** for update/cancel operations
3. **Reads response** from notification API
4. **Deserializes response** to extract eventId

---

## Data Models

### BookingNotificationPayload (Request)
```csharp
internal record BookingNotificationPayload
{
	[JsonPropertyName("deskName")]
	public required string DeskName { get; init; }

	[JsonPropertyName("floorName")]
	public string? FloorName { get; init; }

	[JsonPropertyName("officeName")]
	public string? OfficeName { get; init; }

	[JsonPropertyName("bookingDateTime")]
	public required DateTime BookingDateTime { get; init; }

	[JsonPropertyName("userId")]
	public required string UserId { get; init; }

	[JsonPropertyName("userEmail")]
	public required string UserEmail { get; init; }

	[JsonPropertyName("status")]
	public required string Status { get; init; }

	[JsonPropertyName("eventId")]
	public string? EventId { get; init; }          // NEW FIELD
}
```

### NotificationApiResponse (Response)
```csharp
internal record NotificationApiResponse
{
	[JsonPropertyName("status")]
	public string? Status { get; init; }

	[JsonPropertyName("action")]
	public string? Action { get; init; }

	[JsonPropertyName("eventId")]
	public string? EventId { get; init; }
}
```

---

## Operation Flow Summary

### Create Booking Flow:
```
1. BookAsync() called
2. Create booking → Database
3. Send notification (eventId: null) → Notification API
4. Receive response with eventId
5. Update booking.NotifyEventId → Database
6. Return result to caller
```

### Update Booking Flow:
```
1. UpdateAsync() called
2. Retrieve existing booking (get NotifyEventId)
3. Update booking → Database
4. Send notification (eventId: existing) → Notification API
5. API updates the same event
6. Return result to caller
```

### Cancel Booking Flow:
```
1. CancelAsync() called
2. Retrieve booking details (get NotifyEventId)
3. Delete booking → Database
4. Send notification (eventId: existing) → Notification API
5. API cancels/closes the event
6. Return result to caller
```

---

## Error Handling

- All notification operations are wrapped in try-catch
- Failed notifications don't fail the booking operations
- Returns `null` for eventId if notification fails
- Booking continues even if eventId cannot be stored

---

## Benefits

✅ **Event Tracking**: Complete lifecycle tracking of booking events  
✅ **Integration**: External systems can track the same event across create/update/cancel  
✅ **Audit Trail**: Event IDs provide a correlation between booking records and notifications  
✅ **Idempotency**: Update and cancel operations use the same event ID  
✅ **External Sync**: Notification system can maintain its own event state  

---

## API Endpoint

**URL**: `https://e5a503132f76e9f089421d9ff3ed6a.5a.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/33970fed29c444c69266670a2e622572/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=f5ySfheuWdLQ7h_cpnqwHHFH7q2ymjdMMm5dREA9zfk`

**Method**: POST  
**Content-Type**: application/json

---

## Testing Checklist

- [ ] Create booking - verify eventId is stored
- [ ] Update booking - verify same eventId is used
- [ ] Cancel booking - verify same eventId is used
- [ ] Failed notification - verify booking still succeeds
- [ ] Check database for notify_event_id column
- [ ] Verify API response parsing

---

**✅ Implementation Complete! Build Successful!**
