using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    public partial class RenameColumnSocialMediaTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SocialMedias_Teachers_TeacherId",
                table: "SocialMedias");

            migrationBuilder.DropColumn(
                name: "TeaecherId",
                table: "SocialMedias");

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "SocialMedias",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SocialMedias_Teachers_TeacherId",
                table: "SocialMedias",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SocialMedias_Teachers_TeacherId",
                table: "SocialMedias");

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "SocialMedias",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TeaecherId",
                table: "SocialMedias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_SocialMedias_Teachers_TeacherId",
                table: "SocialMedias",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id");
        }
    }
}
