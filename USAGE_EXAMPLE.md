# DeskRepository - GetDeskWithFloorAndOfficeAsync Usage Example

## Method Overview
The `GetDeskWithFloorAndOfficeAsync` method fetches a desk along with its related floor and office information in a single query using Entity Framework Core's eager loading.

## Method Signature
```csharp
Task<DeskWithFloorAndOffice?> GetDeskWithFloorAndOfficeAsync(string deskId, CancellationToken ct = default)
```

## Usage Example

### In a Service or Controller
```csharp
public class ExampleService
{
	private readonly IDeskRepository _deskRepository;

	public ExampleService(IDeskRepository deskRepository)
	{
		_deskRepository = deskRepository;
	}

	public async Task<string> GetDeskLocationInfo(string deskId, CancellationToken ct)
	{
		var result = await _deskRepository.GetDeskWithFloorAndOfficeAsync(deskId, ct);

		if (result == null)
		{
			return "Desk not found";
		}

		return $"Desk: {result.Desk.Name ?? result.Desk.PublicDeskId}\n" +
			   $"Floor: {result.Floor?.Name ?? "Unknown"}\n" +
			   $"Office: {result.Office?.Name ?? "Unknown"}\n" +
			   $"Location: {result.Office?.Latitude}, {result.Office?.Longitude}";
	}
}
```

### Using in ReservationService (BookAsync method)
```csharp
public async Task<DeskSchedule> BookAsync(ReservationInput input, CancellationToken ct = default)
{
	// ... existing code ...

	// Fetch desk with complete location details
	var deskDetails = await _deskRepository.GetDeskWithFloorAndOfficeAsync(input.DeskId, ct);

	if (deskDetails != null)
	{
		// Now you can access:
		string deskName = deskDetails.Desk.Name ?? deskDetails.Desk.PublicDeskId;
		string floorName = deskDetails.Floor?.Name ?? "Unknown";
		string officeName = deskDetails.Office?.Name ?? "Unknown";

		// Use in notification payload or logging
		Console.WriteLine($"Booking desk {deskName} on floor {floorName} at office {officeName}");
	}

	// ... rest of the code ...
}
```

## Return Type: DeskWithFloorAndOffice
```csharp
public class DeskWithFloorAndOffice
{
	public required Desk Desk { get; init; }        // The desk entity
	public Floor? Floor { get; init; }               // The related floor (nullable)
	public Office? Office { get; init; }             // The related office (nullable)
}
```

## Benefits
1. **Single Query**: Uses eager loading to fetch all related data in one database query
2. **Null-Safe**: Returns null if desk is not found, nullable properties for floor and office
3. **Read-Only**: Uses `AsNoTracking()` for better performance in read scenarios
4. **Type-Safe**: Returns a strongly-typed result object

## Performance
- Uses Entity Framework Core's `.Include()` and `.ThenInclude()` for efficient eager loading
- Executes as a single SQL query with JOINs
- More efficient than making separate queries for desk, floor, and office
