using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seatgenie.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "office_setting_weekdays_allowed",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    allow_monday = table.Column<bool>(type: "boolean", nullable: false),
                    allow_tuesday = table.Column<bool>(type: "boolean", nullable: false),
                    allow_wednesday = table.Column<bool>(type: "boolean", nullable: false),
                    allow_thursday = table.Column<bool>(type: "boolean", nullable: false),
                    allow_friday = table.Column<bool>(type: "boolean", nullable: false),
                    allow_saturday = table.Column<bool>(type: "boolean", nullable: false),
                    allow_sunday = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_office_setting_weekdays_allowed", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    invite_code = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_organization", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verification_token",
                columns: table => new
                {
                    identifier = table.Column<string>(type: "text", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    expires = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_verification_token", x => new { x.identifier, x.token });
                });

            migrationBuilder.CreateTable(
                name: "office_setting",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    allow_scheduling_in_the_past = table.Column<bool>(type: "boolean", nullable: false),
                    duration_scheduling_future = table.Column<int>(type: "integer", nullable: true),
                    office_setting_weekdays_allowed_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_office_setting", x => x.id);
                    table.ForeignKey(
                        name: "f_k_office_setting_office_setting_weekdays_allowed_office_setti~",
                        column: x => x.office_setting_weekdays_allowed_id,
                        principalTable: "office_setting_weekdays_allowed",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "office",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    timezone = table.Column<string>(type: "text", nullable: false, defaultValue: "Etc/GMT"),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    office_setting_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_office", x => x.id);
                    table.ForeignKey(
                        name: "f_k_office_office_setting_office_setting_id",
                        column: x => x.office_setting_id,
                        principalTable: "office_setting",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_office_organization_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "floor",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    floor_plan = table.Column<string>(type: "text", nullable: true),
                    office_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_floor", x => x.id);
                    table.ForeignKey(
                        name: "f_k_floor_office_office_id",
                        column: x => x.office_id,
                        principalTable: "office",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    email_verified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    user_role = table.Column<string>(type: "text", nullable: false),
                    current_office_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_user", x => x.id);
                    table.ForeignKey(
                        name: "f_k_user_office_current_office_id",
                        column: x => x.current_office_id,
                        principalTable: "office",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_user_organization_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "desk",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    public_desk_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    floor_id = table.Column<string>(type: "text", nullable: false),
                    x = table.Column<double>(type: "double precision", nullable: false),
                    y = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_desk", x => x.id);
                    table.ForeignKey(
                        name: "f_k_desk_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "floor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meeting_room",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    floor_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_meeting_room", x => x.id);
                    table.ForeignKey(
                        name: "f_k_meeting_room_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "floor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "office_room",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    floor_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_office_room", x => x.id);
                    table.ForeignKey(
                        name: "f_k_office_room_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "floor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    provider = table.Column<string>(type: "text", nullable: true),
                    provider_account_id = table.Column<string>(type: "text", nullable: true),
                    refresh_token = table.Column<string>(type: "text", nullable: true),
                    access_token = table.Column<string>(type: "text", nullable: true),
                    expires_at = table.Column<int>(type: "integer", nullable: true),
                    token_type = table.Column<string>(type: "text", nullable: true),
                    scope = table.Column<string>(type: "text", nullable: true),
                    id_token = table.Column<string>(type: "text", nullable: true),
                    session_state = table.Column<string>(type: "text", nullable: true),
                    ext_expires_in = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_account", x => x.id);
                    table.ForeignKey(
                        name: "f_k_account_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "onboarding_selection",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    workplacify_preferences = table.Column<List<string>>(type: "text[]", nullable: false),
                    temporary_invite_code = table.Column<string>(type: "text", nullable: true),
                    submitted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_onboarding_selection", x => x.id);
                    table.ForeignKey(
                        name: "f_k_onboarding_selection_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    session_token = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    expires = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_session", x => x.id);
                    table.ForeignKey(
                        name: "f_k_session_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "desk_recommendation",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    desk_id = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    score = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_desk_recommendation", x => x.id);
                    table.ForeignKey(
                        name: "f_k_desk_recommendation_desk_desk_id",
                        column: x => x.desk_id,
                        principalTable: "desk",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_desk_recommendation_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "desk_schedule",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    desk_id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    timezone = table.Column<string>(type: "text", nullable: false, defaultValue: "Etc/GMT"),
                    whole_day = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    start_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_desk_schedule", x => x.id);
                    table.ForeignKey(
                        name: "f_k_desk_schedule_desk_desk_id",
                        column: x => x.desk_id,
                        principalTable: "desk",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_desk_schedule_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_preference",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    preferred_office_id = table.Column<string>(type: "text", nullable: true),
                    preferred_floor_id = table.Column<string>(type: "text", nullable: true),
                    favorite_desk_id = table.Column<string>(type: "text", nullable: true),
                    prefer_monday = table.Column<bool>(type: "boolean", nullable: false),
                    prefer_tuesday = table.Column<bool>(type: "boolean", nullable: false),
                    prefer_wednesday = table.Column<bool>(type: "boolean", nullable: false),
                    prefer_thursday = table.Column<bool>(type: "boolean", nullable: false),
                    prefer_friday = table.Column<bool>(type: "boolean", nullable: false),
                    prefer_saturday = table.Column<bool>(type: "boolean", nullable: false),
                    prefer_sunday = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_user_preference", x => x.id);
                    table.ForeignKey(
                        name: "f_k_user_preference_desk_favorite_desk_id",
                        column: x => x.favorite_desk_id,
                        principalTable: "desk",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_user_preference_floor_preferred_floor_id",
                        column: x => x.preferred_floor_id,
                        principalTable: "floor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_user_preference_office_preferred_office_id",
                        column: x => x.preferred_office_id,
                        principalTable: "office",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_user_preference_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_account_provider_provider_account_id",
                table: "account",
                columns: new[] { "provider", "provider_account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_account_user_id",
                table: "account",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_desk_floor_id_public_desk_id",
                table: "desk",
                columns: new[] { "floor_id", "public_desk_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_desk_recommendation_desk_id",
                table: "desk_recommendation",
                column: "desk_id");

            migrationBuilder.CreateIndex(
                name: "i_x_desk_recommendation_user_id",
                table: "desk_recommendation",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_desk_schedule_desk_id_date",
                table: "desk_schedule",
                columns: new[] { "desk_id", "date" });

            migrationBuilder.CreateIndex(
                name: "i_x_desk_schedule_user_id",
                table: "desk_schedule",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_floor_office_id",
                table: "floor",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "i_x_meeting_room_floor_id",
                table: "meeting_room",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "i_x_office_office_setting_id",
                table: "office",
                column: "office_setting_id");

            migrationBuilder.CreateIndex(
                name: "i_x_office_organization_id",
                table: "office",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "i_x_office_room_floor_id",
                table: "office_room",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "i_x_office_setting_office_setting_weekdays_allowed_id",
                table: "office_setting",
                column: "office_setting_weekdays_allowed_id");

            migrationBuilder.CreateIndex(
                name: "i_x_onboarding_selection_user_id",
                table: "onboarding_selection",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_organization_invite_code",
                table: "organization",
                column: "invite_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_session_session_token",
                table: "session",
                column: "session_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_session_user_id",
                table: "session",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_current_office_id",
                table: "user",
                column: "current_office_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_email",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_user_organization_id",
                table: "user",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_preference_favorite_desk_id",
                table: "user_preference",
                column: "favorite_desk_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_preference_preferred_floor_id",
                table: "user_preference",
                column: "preferred_floor_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_preference_preferred_office_id",
                table: "user_preference",
                column: "preferred_office_id");

            migrationBuilder.CreateIndex(
                name: "i_x_user_preference_user_id",
                table: "user_preference",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_verification_token_token",
                table: "verification_token",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "desk_recommendation");

            migrationBuilder.DropTable(
                name: "desk_schedule");

            migrationBuilder.DropTable(
                name: "meeting_room");

            migrationBuilder.DropTable(
                name: "office_room");

            migrationBuilder.DropTable(
                name: "onboarding_selection");

            migrationBuilder.DropTable(
                name: "session");

            migrationBuilder.DropTable(
                name: "user_preference");

            migrationBuilder.DropTable(
                name: "verification_token");

            migrationBuilder.DropTable(
                name: "desk");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "floor");

            migrationBuilder.DropTable(
                name: "office");

            migrationBuilder.DropTable(
                name: "office_setting");

            migrationBuilder.DropTable(
                name: "organization");

            migrationBuilder.DropTable(
                name: "office_setting_weekdays_allowed");
        }
    }
}
