// <auto-generated />
using System;
using CrystalSharp.Tests.Common.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CrystalSharp.Tests.Common.Migrations.MySqlMigrations
{
    [DbContext(typeof(MySqlAppDbReadModelStoreContext))]
    partial class MySqlAppDbReadModelStoreContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.25")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("CrystalSharp.Tests.Common.MySql.ReadModels.SupplierReadModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Code")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("EntityStatus")
                        .HasColumnType("int");

                    b.Property<Guid>("GlobalUId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime?>("ModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("SupplierReadModel");
                });
#pragma warning restore 612, 618
        }
    }
}
