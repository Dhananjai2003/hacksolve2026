using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Entities;
using UserRole = Seatgenie.Api.Models.UserRole;

namespace Seatgenie.Api.Data;

/// <summary>
/// EF Core context for the SeatGenie schema, targeting Azure Database for PostgreSQL.
/// </summary>
public class SeatGenieDbContext : DbContext
{
    public SeatGenieDbContext(DbContextOptions<SeatGenieDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();
    public DbSet<Office> Offices => Set<Office>();
    public DbSet<OfficeSetting> OfficeSettings => Set<OfficeSetting>();
    public DbSet<OfficeSettingWeekdaysAllowed> OfficeSettingWeekdaysAllowed => Set<OfficeSettingWeekdaysAllowed>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<MeetingRoom> MeetingRooms => Set<MeetingRoom>();
    public DbSet<OfficeRoom> OfficeRooms => Set<OfficeRoom>();
    public DbSet<Desk> Desks => Set<Desk>();
    public DbSet<DeskSchedule> DeskSchedules => Set<DeskSchedule>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<OnboardingSelection> OnboardingSelections => Set<OnboardingSelection>();
    public DbSet<DeskQuality> DeskQualities => Set<DeskQuality>();
    public DbSet<DeskQualityMapping> DeskQualityMappings => Set<DeskQualityMapping>();
    public DbSet<UserSeatPreference> UserSeatPreferences => Set<UserSeatPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(b =>
        {
            b.ToTable("organization");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.InviteCode).IsUnique();
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("user");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Email).IsUnique();
            // Stored as text to match the diagram; the MEMBER default is applied at the
            // application layer via the entity's property initializer.
            b.Property(x => x.UserRole)
                .HasConversion<string>();

            b.HasOne(x => x.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.CurrentOffice)
                .WithMany()
                .HasForeignKey(x => x.CurrentOfficeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("account");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.Provider, x.ProviderAccountId }).IsUnique();
            b.HasOne(x => x.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Session>(b =>
        {
            b.ToTable("session");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.SessionToken).IsUnique();
            b.HasOne(x => x.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VerificationToken>(b =>
        {
            b.ToTable("verification_token");
            b.HasKey(x => new { x.Identifier, x.Token });
            b.HasIndex(x => x.Token).IsUnique();
        });

        modelBuilder.Entity<Office>(b =>
        {
            b.ToTable("office");
            b.HasKey(x => x.Id);
            b.Property(x => x.Timezone).HasDefaultValue("Etc/GMT");

            b.HasOne(x => x.Organization)
                .WithMany(o => o.Offices)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.OfficeSetting)
                .WithMany(s => s.Offices)
                .HasForeignKey(x => x.OfficeSettingId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OfficeSetting>(b =>
        {
            b.ToTable("office_setting");
            b.HasKey(x => x.Id);
            b.HasOne(x => x.WeekdaysAllowed)
                .WithMany()
                .HasForeignKey(x => x.OfficeSettingWeekdaysAllowedId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OfficeSettingWeekdaysAllowed>(b =>
        {
            b.ToTable("office_setting_weekdays_allowed");
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Floor>(b =>
        {
            b.ToTable("floor");
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Office)
                .WithMany(o => o.Floors)
                .HasForeignKey(x => x.OfficeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MeetingRoom>(b =>
        {
            b.ToTable("meeting_room");
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Floor)
                .WithMany(f => f.MeetingRooms)
                .HasForeignKey(x => x.FloorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OfficeRoom>(b =>
        {
            b.ToTable("office_room");
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Floor)
                .WithMany(f => f.OfficeRooms)
                .HasForeignKey(x => x.FloorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Desk>(b =>
        {
            b.ToTable("desk");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.FloorId, x.PublicDeskId }).IsUnique();
            b.HasOne(x => x.Floor)
                .WithMany(f => f.Desks)
                .HasForeignKey(x => x.FloorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeskQuality>(b =>
        {
            b.ToTable("desk_quality");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("quality_id");
            b.Property(x => x.Name).HasColumnName("quality_name");
        });

        modelBuilder.Entity<DeskQualityMapping>(b =>
        {
            b.ToTable("desk_quality_mapping");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("mapping_id");
            // One desk ↔ many qualities; no duplicate (desk, quality) pairs.
            b.HasIndex(x => new { x.DeskId, x.QualityId }).IsUnique();

            b.HasOne(x => x.Desk)
                .WithMany(d => d.QualityMappings)
                .HasForeignKey(x => x.DeskId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Quality)
                .WithMany()
                .HasForeignKey(x => x.QualityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserSeatPreference>(b =>
        {
            b.ToTable("user_seat_preferences");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("seat_preference_id");

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Quality)
                .WithMany()
                .HasForeignKey(x => x.QualityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeskSchedule>(b =>
        {
            b.ToTable("desk_schedule");
            b.HasKey(x => x.Id);
            b.Property(x => x.Timezone).HasDefaultValue("Etc/GMT");
            b.Property(x => x.WholeDay).HasDefaultValue(false);
            b.HasIndex(x => new { x.DeskId, x.Date });

            b.HasOne(x => x.Desk)
                .WithMany(d => d.DeskSchedules)
                .HasForeignKey(x => x.DeskId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.User)
                .WithMany(u => u.DeskSchedules)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserPreference>(b =>
        {
            b.ToTable("user_preference");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.UserId).IsUnique();

            b.HasOne(x => x.User)
                .WithOne(u => u.UserPreference)
                .HasForeignKey<UserPreference>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.PreferredOffice)
                .WithMany()
                .HasForeignKey(x => x.PreferredOfficeId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.PreferredFloor)
                .WithMany()
                .HasForeignKey(x => x.PreferredFloorId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.FavoriteDesk)
                .WithMany()
                .HasForeignKey(x => x.FavoriteDeskId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OnboardingSelection>(b =>
        {
            b.ToTable("onboarding_selection");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.UserId).IsUnique();
            b.Property(x => x.WorkplacifyPreferences).HasColumnType("text[]");
            b.Property(x => x.Submitted).HasDefaultValue(false);

            b.HasOne(x => x.User)
                .WithOne(u => u.OnboardingSelection)
                .HasForeignKey<OnboardingSelection>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.UseSnakeCaseNames();
    }
}
