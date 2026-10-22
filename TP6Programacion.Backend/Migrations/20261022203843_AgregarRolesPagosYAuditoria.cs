using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TP6Programacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRolesPagosYAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rol", table: "Usuarios", type: "integer", nullable: false, defaultValue: 3); // Cliente

            migrationBuilder.Sql("UPDATE \"Usuarios\" SET \"Rol\" = 1 WHERE \"EsAdministrador\" = true;"); // Administrador

            migrationBuilder.Sql("ALTER TABLE \"Usuarios\" ALTER COLUMN \"Rol\" DROP DEFAULT;");

            migrationBuilder.DropColumn(name: "EsAdministrador", table: "Usuarios");

            migrationBuilder.AddColumn<int>(
                name: "Estado", table: "Ventas", type: "integer", nullable: false, defaultValue: 2); // Confirmada

            migrationBuilder.Sql("UPDATE \"Ventas\" SET \"Estado\" = 4 WHERE \"Anulada\" = true;"); // Anulada

            migrationBuilder.Sql("ALTER TABLE \"Ventas\" ALTER COLUMN \"Estado\" DROP DEFAULT;");

            migrationBuilder.DropColumn(name: "Anulada", table: "Ventas");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Ventas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Ventas",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "ClienteId",
                table: "Ventas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");


            migrationBuilder.AddColumn<DateTime>(
                name: "FechaExpiracion",
                table: "Ventas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "Productos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                table: "Productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    Accion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Entidad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntidadId = table.Column<int>(type: "integer", nullable: false),
                    Detalle = table.Column<string>(type: "text", nullable: true),
                    DireccionIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auditorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    Metodo = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReferenciaPago = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ventas_ClienteOUsuario",
                table: "Ventas",
                sql: "\"ClienteId\" IS NOT NULL OR \"UsuarioId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ventas_FechaExpiracion",
                table: "Ventas",
                sql: "\"FechaExpiracion\" IS NULL OR (\"Estado\" = 1 AND \"UsuarioId\" IS NULL AND \"ClienteId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Entidad_EntidadId",
                table: "Auditorias",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Fecha",
                table: "Auditorias",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_UsuarioId",
                table: "Auditorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_VentaId",
                table: "Pagos",
                column: "VentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes");

            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ventas_ClienteOUsuario",
                table: "Ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ventas_FechaExpiracion",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "EsAdministrador",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaExpiracion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Anulada", 
                table: "Ventas");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Ventas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Ventas",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "ClienteId",
                table: "Ventas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Anulada",
                table: "Ventas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsAdministrador",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
