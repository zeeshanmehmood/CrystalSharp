// <auto-generated />
using System;
using CrystalSharp.Tests.Common.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CrystalSharp.Tests.Common.Migrations.MySqlMigrations
{
    [DbContext(typeof(MySqlAppDbContext))]
    [Migration("20240207011922_MySqlInitTestingDb")]
    partial class MySqlInitTestingDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.25")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate.Supplier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

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

                    b.Property<long>("Version")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Supplier");
                });

            modelBuilder.Entity("CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate.Supplier", b =>
                {
                    b.OwnsOne("CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate.SupplierInfo", "SupplierInfo", b1 =>
                        {
                            b1.Property<int>("SupplierId")
                                .HasColumnType("int");

                            b1.Property<string>("Code")
                                .HasColumnType("longtext");

                            b1.Property<string>("Email")
                                .HasColumnType("longtext");

                            b1.HasKey("SupplierId");

                            b1.ToTable("Supplier");

                            b1.WithOwner()
                                .HasForeignKey("SupplierId");
                        });

                    b.Navigation("SupplierInfo");
                });
#pragma warning restore 612, 618
        }
    }
}
