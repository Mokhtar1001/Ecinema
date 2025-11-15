using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECinema.Migrations
{
    /// <inheritdoc />
    public partial class AddTraffic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Traffic",
                table: "Movies",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ForgetPasswordVM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserNameOREmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForgetPasswordVM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewPasswordVM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfirmPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewPasswordVM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValidateOTPVM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidateOTPVM", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForgetPasswordVM");

            migrationBuilder.DropTable(
                name: "NewPasswordVM");

            migrationBuilder.DropTable(
                name: "ValidateOTPVM");

            migrationBuilder.DropColumn(
                name: "Traffic",
                table: "Movies");
        }
    }
}
