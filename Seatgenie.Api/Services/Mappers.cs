using Seatgenie.Api.Models;
using Ent = Seatgenie.Api.Entities;

namespace Seatgenie.Api.Services;

/// <summary>
/// Translation between persistence entities (<see cref="Ent"/>) and API DTOs
/// (<see cref="Seatgenie.Api.Models"/>). Entity → DTO maps for reads; DTO → entity
/// "apply" helpers for writes (mutating an existing tracked entity in place).
/// </summary>
public static class Mappers
{
    // ---------------------------------------------------------------- Organization
    public static Organization ToDto(this Ent.Organization e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        InviteCode = e.InviteCode,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static Ent.Organization ToEntity(this OrganizationInput dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
    };

    public static void Apply(this OrganizationInput dto, Ent.Organization e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
    }

    // ---------------------------------------------------------------- Office
    public static Office ToDto(this Ent.Office e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        Longitude = e.Longitude,
        Latitude = e.Latitude,
        Timezone = e.Timezone,
        OrganizationId = e.OrganizationId,
        OfficeSettingId = e.OfficeSettingId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static OfficeDetail ToDetailDto(this Ent.Office e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        Longitude = e.Longitude,
        Latitude = e.Latitude,
        Timezone = e.Timezone,
        OrganizationId = e.OrganizationId,
        OfficeSettingId = e.OfficeSettingId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Floors = e.Floors.Select(f => f.ToDetailDto()).ToList(),
    };

    public static Ent.Office ToEntity(this OfficeInput dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        Longitude = dto.Longitude,
        Latitude = dto.Latitude,
        Timezone = dto.Timezone ?? "Etc/GMT",
    };

    public static void Apply(this OfficeInput dto, Ent.Office e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.Longitude = dto.Longitude;
        e.Latitude = dto.Latitude;
        if (dto.Timezone is not null) e.Timezone = dto.Timezone;
    }

    // ---------------------------------------------------------------- OfficeSetting
    public static OfficeSetting ToDto(this Ent.OfficeSetting e) => new()
    {
        Id = e.Id,
        AllowSchedulingInThePast = e.AllowSchedulingInThePast,
        DurationSchedulingFuture = e.DurationSchedulingFuture,
        WeekdaysAllowed = e.WeekdaysAllowed?.ToDto(),
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static WeekdaysAllowed ToDto(this Ent.OfficeSettingWeekdaysAllowed e) => new()
    {
        Id = e.Id,
        AllowMonday = e.AllowMonday,
        AllowTuesday = e.AllowTuesday,
        AllowWednesday = e.AllowWednesday,
        AllowThursday = e.AllowThursday,
        AllowFriday = e.AllowFriday,
        AllowSaturday = e.AllowSaturday,
        AllowSunday = e.AllowSunday,
    };

    public static Ent.OfficeSetting ToEntity(this OfficeSettingInput dto) => new()
    {
        AllowSchedulingInThePast = dto.AllowSchedulingInThePast,
        DurationSchedulingFuture = dto.DurationSchedulingFuture,
    };

    public static Ent.OfficeSettingWeekdaysAllowed ToEntity(this WeekdaysAllowed dto) => new()
    {
        AllowMonday = dto.AllowMonday,
        AllowTuesday = dto.AllowTuesday,
        AllowWednesday = dto.AllowWednesday,
        AllowThursday = dto.AllowThursday,
        AllowFriday = dto.AllowFriday,
        AllowSaturday = dto.AllowSaturday,
        AllowSunday = dto.AllowSunday,
    };

    // ---------------------------------------------------------------- Floor
    public static Floor ToDto(this Ent.Floor e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        FloorPlan = e.FloorPlan,
        OfficeId = e.OfficeId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static FloorDetail ToDetailDto(this Ent.Floor e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        FloorPlan = e.FloorPlan,
        OfficeId = e.OfficeId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Desks = e.Desks.Select(d => d.ToDto()).ToList(),
    };

    public static Ent.Floor ToEntity(this FloorInput dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        FloorPlan = dto.FloorPlan,
    };

    public static void Apply(this FloorInput dto, Ent.Floor e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.FloorPlan = dto.FloorPlan;
    }

    // ---------------------------------------------------------------- Rooms
    public static Room ToDto(this Ent.MeetingRoom e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        FloorId = e.FloorId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static Room ToDto(this Ent.OfficeRoom e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        FloorId = e.FloorId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    // ---------------------------------------------------------------- Desk
    public static Desk ToDto(this Ent.Desk e) => new()
    {
        Id = e.Id,
        PublicDeskId = e.PublicDeskId,
        Name = e.Name,
        Description = e.Description,
        FloorId = e.FloorId,
        X = e.X,
        Y = e.Y,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static Ent.Desk ToEntity(this DeskInput dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        X = dto.X,
        Y = dto.Y,
    };

    public static void Apply(this DeskInput dto, Ent.Desk e)
    {
        e.Name = dto.Name;
        e.Description = dto.Description;
        e.X = dto.X;
        e.Y = dto.Y;
    }

    // ---------------------------------------------------------------- DeskSchedule
    public static DeskSchedule ToDto(this Ent.DeskSchedule e) => new()
    {
        Id = e.Id,
        DeskId = e.DeskId,
        UserId = e.UserId,
        Date = e.Date,
        Timezone = e.Timezone,
        WholeDay = e.WholeDay,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static DeskScheduleWithDesk ToWithDeskDto(this Ent.DeskSchedule e) => new()
    {
        Id = e.Id,
        DeskId = e.DeskId,
        UserId = e.UserId,
        Date = e.Date,
        Timezone = e.Timezone,
        WholeDay = e.WholeDay,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Desk = e.Desk?.ToDto(),
    };

    // ---------------------------------------------------------------- Recommendation
    public static DeskRecommendation ToDto(this Ent.DeskRecommendation e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        DeskId = e.DeskId,
        Date = e.Date,
        Score = e.Score,
        Reason = e.Reason,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    // ---------------------------------------------------------------- UserPreference
    public static UserPreference ToDto(this Ent.UserPreference e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        PreferredOfficeId = e.PreferredOfficeId,
        PreferredFloorId = e.PreferredFloorId,
        FavoriteDeskId = e.FavoriteDeskId,
        PreferMonday = e.PreferMonday,
        PreferTuesday = e.PreferTuesday,
        PreferWednesday = e.PreferWednesday,
        PreferThursday = e.PreferThursday,
        PreferFriday = e.PreferFriday,
        PreferSaturday = e.PreferSaturday,
        PreferSunday = e.PreferSunday,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    // ---------------------------------------------------------------- User
    public static User ToDto(this Ent.User e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Email = e.Email,
        EmailVerified = e.EmailVerified,
        Image = e.Image,
        OrganizationId = e.OrganizationId,
        UserRole = e.UserRole,
        CurrentOfficeId = e.CurrentOfficeId,
    };

    // ---------------------------------------------------------------- Onboarding
    public static OnboardingSelection ToDto(this Ent.OnboardingSelection e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        WorkplacifyPreferences = e.WorkplacifyPreferences
            .Select(ParsePreference)
            .Where(p => p is not null)
            .Select(p => p!.Value)
            .ToList(),
        TemporaryInviteCode = e.TemporaryInviteCode,
        Submitted = e.Submitted,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };

    public static Ent.OnboardingSelection ToEntity(this OnboardingSelectionInput dto) => new()
    {
        WorkplacifyPreferences = dto.WorkplacifyPreferences.Select(p => p.ToString()).ToList(),
        TemporaryInviteCode = dto.TemporaryInviteCode,
    };

    public static void Apply(this OnboardingSelectionInput dto, Ent.OnboardingSelection e)
    {
        e.WorkplacifyPreferences = dto.WorkplacifyPreferences.Select(p => p.ToString()).ToList();
        e.TemporaryInviteCode = dto.TemporaryInviteCode;
    }

    private static WorkplacifyPreference? ParsePreference(string value)
        => Enum.TryParse<WorkplacifyPreference>(value, out var parsed) ? parsed : null;
}
