using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareFlow.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DiagnosticServices",
                columns: new[] { "DiagnosticServiceId", "DailyCapacity", "ServiceName" },
                values: new object[,]
                {
                    { 1, 20, "Complete Blood Count (CBC)" },
                    { 2, 10, "Chest X-Ray" },
                    { 3, 8, "Ultrasound" },
                    { 4, 12, "Electrocardiogram (ECG)" },
                    { 5, 15, "Blood Chemistry" }
                });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "PatientId", "FirstName", "LastName" },
                values: new object[,]
                {
                    { 1, "Juan", "Dela Cruz" },
                    { 2, "Maria", "Santos" },
                    { 3, "Pedro", "Reyes" },
                    { 4, "Ana", "Garcia" },
                    { 5, "Carlos", "Mendoza" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DiagnosticServices",
                keyColumn: "DiagnosticServiceId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DiagnosticServices",
                keyColumn: "DiagnosticServiceId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DiagnosticServices",
                keyColumn: "DiagnosticServiceId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "DiagnosticServices",
                keyColumn: "DiagnosticServiceId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "DiagnosticServices",
                keyColumn: "DiagnosticServiceId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 5);
        }
    }
}
