# DeskId Nullable Change - Impact Analysis

## Change Summary
Changed `DeskId` from `required` to `nullable` in the `DeskSchedule` entity and model.

### Before:
```csharp
public string? DeskId { get; set; } = default!;  // Marked as required with default!
```

### After:
```csharp
public string? DeskId { get; set; }  // Truly nullable
```

---

## ✅ Build Status
**BUILD SUCCESSFUL** - No compilation errors

---

## Impact Analysis

### 1. **Entity Layer** ✅ SAFE

**File**: `Seatgenie.Api\Entities\Desk.cs`

**Updated**: DeskSchedule entity
```csharp
public class DeskSchedule : IEntity, IAuditable
{
	public string? DeskId { get; set; }  // Changed from default! to truly nullable
}
```

**Impact**: ✅ No breaking changes - already typed as nullable (`string?`)

---

### 2. **Model/DTO Layer** ✅ ALREADY COMPATIBLE

**File**: `Seatgenie.Api\Models\Desk.cs`

**Current State**:
```csharp
public class DeskSchedule
{
	public string? DeskId { get; set; }  // Already nullable
}
```

**Impact**: ✅ No changes needed - model was already nullable

---

### 3. **ReservationInput Model** ⚠️ POTENTIAL ISSUE

**File**: `Seatgenie.Api\Models\Reservation.cs`

**Current State**:
```csharp
public class ReservationInput
{
	public required string DeskId { get; set; }  // Required field
}
```

**Impact**: ⚠️ **DECISION NEEDED**
- ReservationInput requires DeskId
- Should this also be made nullable?
- If not, validation will prevent null DeskId bookings

**Recommendation**: 
- If you want to allow bookings without a desk (e.g., flexible seating), make this nullable
- If desk is always required for booking, keep it as `required`

---

### 4. **Service Layer** ⚠️ REQUIRES ATTENTION

#### **ReservationService.cs**

##### **BookAsync Method** - Line 72-81
```csharp
var entity = new Ent.DeskSchedule
{
	DeskId = input.DeskId,  // ⚠️ Could be null if ReservationInput.DeskId is made nullable
	UserId = userId,
	Date = ToStartOfDay(input.Date),
	// ...
};
```

**Current State**: ✅ Safe (input.DeskId is required)
**If ReservationInput.DeskId becomes nullable**: ⚠️ Need null handling

##### **UpdateAsync Method** - Line 172
```csharp
schedule.DeskId = input.DeskId;  // Assigns from input
```

**Current State**: ✅ Safe
**Potential Issue**: Same as above

##### **CancelAsync Method** - Line 207-208
```csharp
var deskId = schedule.DeskId;  // Reads nullable field
// Later used to fetch desk details
var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(deskId, ct);
```

**Impact**: ⚠️ **REQUIRES NULL CHECK**

**Current Code**:
```csharp
var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(deskId, ct);
```

**Issue**: If `deskId` is null, this could fail

**Recommendation**: Add null check
```csharp
if (!string.IsNullOrEmpty(deskId))
{
	var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(deskId, ct);
	// ... send notification
}
```

---

### 5. **Repository Layer** ✅ SAFE

#### **ReservationRepository.cs**

##### **GetDeskScheduleAsync** - Line 31
```csharp
var query = Set.AsNoTracking().Where(s => s.DeskId == deskId);
```
**Impact**: ✅ Safe - EF Core handles nullable comparisons

##### **GetFloorAvailabilityAsync** - Line 52-53
```csharp
.Where(s => s.Date >= start && s.Date < end && desks.Select(d => d.Id).Contains(s.DeskId))
.Select(s => s.DeskId)
```
**Impact**: ⚠️ **POTENTIAL ISSUE**
- If DeskId is null, `.Contains()` comparison might behave unexpectedly
- Null DeskIds won't match any desk in the floor

**Recommendation**: Add null filter
```csharp
.Where(s => s.Date >= start && s.Date < end 
	&& s.DeskId != null 
	&& desks.Select(d => d.Id).Contains(s.DeskId))
```

##### **HasConflictAsync** - Line 74
```csharp
.AnyAsync(s => s.DeskId == deskId && s.Date >= start && s.Date < end, ct);
```
**Impact**: ✅ Safe - equality check works with nullables

##### **GetFavoriteDeskAsync** - Line 111
```csharp
.GroupBy(s => s.DeskId)
```
**Impact**: ✅ Safe - GroupBy handles nulls (will create a group with null key)

---

### 6. **AnalyticsRepository.cs** ⚠️ REQUIRES REVIEW

#### **GetUtilizationAsync** - Line 44-45
```csharp
.Where(s => s.Date >= start && s.Date < end && deskIds.Contains(s.DeskId))
.Select(s => new { s.DeskId, s.Date })
```

**Impact**: ⚠️ **POTENTIAL ISSUE**
- Similar to ReservationRepository - null DeskIds won't be in deskIds list
- Could cause incorrect utilization calculations

**Recommendation**: Add null filter
```csharp
.Where(s => s.Date >= start && s.Date < end 
	&& s.DeskId != null 
	&& deskIds.Contains(s.DeskId))
```

#### **GetHeadcountAsync** - Line 72
```csharp
join d in _db.Desks.AsNoTracking() on s.DeskId equals d.Id
```

**Impact**: ⚠️ **POTENTIAL ISSUE**
- Inner join will exclude schedules with null DeskId
- This might be desired behavior or might lose data

**Recommendation**: Consider if schedules without desks should be counted

---

### 7. **ChatbotService.cs** ✅ SAFE

#### **TryBookAsync** - Line 295
```csharp
new ReservationInput { DeskId = candidate, Date = date, WholeDay = wholeDay }
```

**Impact**: ✅ Safe - `candidate` is already null-checked in the loop

---

### 8. **Database Schema** ⚠️ REQUIRES MIGRATION

#### **DbContext Configuration** - Line 215-217
```csharp
modelBuilder.Entity<DeskSchedule>(b =>
{
	// ...
	b.HasIndex(x => new { x.DeskId, x.Date });

	b.HasOne(x => x.Desk)
		.WithMany(d => d.DeskSchedules)
		.HasForeignKey(x => x.DeskId)
		.OnDelete(DeleteBehavior.Cascade);
});
```

**Impact**: ⚠️ **DATABASE MIGRATION REQUIRED**

**Current State**: DeskId is NOT NULL in database
**New State**: DeskId should be nullable

**Action Required**:
1. Generate migration: `dotnet ef migrations add MakeDeskIdNullable`
2. Review migration to ensure it alters the column correctly
3. Apply migration: `dotnet ef database update`

**SQL Expected**:
```sql
ALTER TABLE desk_schedule 
ALTER COLUMN desk_id DROP NOT NULL;
```

---

## Summary of Required Actions

### ✅ Completed
1. ✅ Updated DeskSchedule entity
2. ✅ Build successful

### ⚠️ Recommended Changes

#### **HIGH PRIORITY** (Prevent Runtime Errors)

1. **Add null check in CancelAsync** (ReservationService.cs ~line 207)
```csharp
if (!string.IsNullOrEmpty(deskId))
{
	var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(deskId, ct);
	// notification code
}
```

2. **Add null filter in GetFloorAvailabilityAsync** (ReservationRepository.cs ~line 52)
```csharp
.Where(s => s.Date >= start && s.Date < end 
	&& s.DeskId != null 
	&& desks.Select(d => d.Id).Contains(s.DeskId))
```

3. **Add null filter in GetUtilizationAsync** (AnalyticsRepository.cs ~line 44)
```csharp
.Where(s => s.Date >= start && s.Date < end 
	&& s.DeskId != null 
	&& deskIds.Contains(s.DeskId))
```

4. **Database Migration**
```bash
dotnet ef migrations add MakeDeskScheduleDeskIdNullable
dotnet ef database update
```

#### **MEDIUM PRIORITY** (Business Logic Review)

5. **Review GetHeadcountAsync** - Decide if schedules without desks should be counted

6. **Review BookAsync notification logic** - Handle case where DeskId is null

#### **OPTIONAL** (Based on Business Requirements)

7. **Consider making ReservationInput.DeskId nullable** if you want to support flexible/unassigned bookings

---

## Testing Recommendations

### Unit Tests to Add/Update
1. Test booking creation with null DeskId
2. Test booking update with null DeskId
3. Test cancellation with null DeskId
4. Test floor availability calculation with null DeskIds in schedule
5. Test analytics with null DeskIds

### Integration Tests
1. Create booking without desk → verify it persists
2. Update booking to remove desk → verify notification handling
3. Cancel booking without desk → verify graceful handling

---

## Risk Assessment

| Area | Risk Level | Impact |
|------|-----------|---------|
| Entity/Model | 🟢 LOW | Already nullable, no code changes needed |
| Service Layer | 🟡 MEDIUM | Need null checks in notification logic |
| Repository Queries | 🟡 MEDIUM | Need null filters in analytics queries |
| Database | 🔴 HIGH | Requires migration to modify schema |
| API Contracts | 🟢 LOW | Already uses nullable types |

---

## Conclusion

**Overall Impact**: 🟡 **MEDIUM RISK**

The change is **structurally safe** (builds successfully) but requires:
1. ✅ Database migration (mandatory)
2. ⚠️ Null handling in 3-4 key methods (recommended)
3. 📋 Business logic review for analytics (optional)

**Recommendation**: 
- Apply the HIGH PRIORITY changes before deploying
- Create database migration
- Add tests for null DeskId scenarios
- Review business requirements for analytics behavior
