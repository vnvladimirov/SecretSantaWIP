using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecretSanta.Migrations
{
    /// <inheritdoc />
    public partial class AddWishlistAndInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductURL",
                table: "WishlistItems",
                newName: "ProductUrl");

            migrationBuilder.CreateTable(
                name: "ParticipantInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ParticipantId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantInvitations_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantInvitations_ParticipantId",
                table: "ParticipantInvitations",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantInvitations_TokenHash",
                table: "ParticipantInvitations",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipantInvitations");

            migrationBuilder.RenameColumn(
                name: "ProductUrl",
                table: "WishlistItems",
                newName: "ProductURL");
        }
    }
}
