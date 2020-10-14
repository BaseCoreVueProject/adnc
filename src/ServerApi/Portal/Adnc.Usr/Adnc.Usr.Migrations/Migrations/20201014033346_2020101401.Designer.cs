﻿// <auto-generated />
using System;
using Adnc.Infr.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Adnc.Usr.Migrations.Migrations
{
    [DbContext(typeof(AdncDbContext))]
    [Migration("20201014033346_2020101401")]
    partial class _2020101401
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Adnc.Usr.Core.Entities.SysDept", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ID")
                        .HasColumnType("bigint");

                    b.Property<long?>("CreateBy")
                        .HasColumnName("CreateBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreateTime")
                        .HasColumnName("CreateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("FullName")
                        .HasColumnName("FullName")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4")
                        .HasMaxLength(255);

                    b.Property<long?>("ModifyBy")
                        .HasColumnName("ModifyBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ModifyTime")
                        .HasColumnName("ModifyTime")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("Num")
                        .HasColumnName("Num")
                        .HasColumnType("int");

                    b.Property<long?>("Pid")
                        .HasColumnName("Pid")
                        .HasColumnType("bigint");

                    b.Property<string>("Pids")
                        .HasColumnName("Pids")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4")
                        .HasMaxLength(255);

                    b.Property<string>("SimpleName")
                        .HasColumnName("SimpleName")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4")
                        .HasMaxLength(255);

                    b.Property<string>("Tips")
                        .HasColumnName("Tips")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4")
                        .HasMaxLength(255);

                    b.Property<int?>("Version")
                        .HasColumnName("Version")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("SysDept");
                });

            modelBuilder.Entity("Adnc.Usr.Core.Entities.SysMenu", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ID")
                        .HasColumnType("bigint");

                    b.Property<string>("Code")
                        .HasColumnName("Code")
                        .HasColumnType("varchar(32) CHARACTER SET utf8mb4")
                        .HasMaxLength(32);

                    b.Property<string>("Component")
                        .HasColumnName("Component")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<long?>("CreateBy")
                        .HasColumnName("CreateBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreateTime")
                        .HasColumnName("CreateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<bool?>("Hidden")
                        .HasColumnName("Hidden")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Icon")
                        .HasColumnName("Icon")
                        .HasColumnType("varchar(32) CHARACTER SET utf8mb4")
                        .HasMaxLength(32);

                    b.Property<bool>("IsMenu")
                        .HasColumnName("IsMenu")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool?>("IsOpen")
                        .HasColumnName("IsOpen")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Levels")
                        .HasColumnName("Levels")
                        .HasColumnType("int");

                    b.Property<long?>("ModifyBy")
                        .HasColumnName("ModifyBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ModifyTime")
                        .HasColumnName("ModifyTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnName("Name")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<int>("Num")
                        .HasColumnName("Num")
                        .HasColumnType("int");

                    b.Property<string>("PCode")
                        .HasColumnName("PCode")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<string>("PCodes")
                        .HasColumnName("PCodes")
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.Property<bool>("Status")
                        .HasColumnName("Status")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Tips")
                        .HasColumnName("Tips")
                        .HasColumnType("varchar(32) CHARACTER SET utf8mb4")
                        .HasMaxLength(32);

                    b.Property<string>("Url")
                        .HasColumnName("Url")
                        .HasColumnType("varchar(32) CHARACTER SET utf8mb4")
                        .HasMaxLength(32);

                    b.HasKey("ID");

                    b.ToTable("SysMenu");
                });

            modelBuilder.Entity("Adnc.Usr.Core.Entities.SysRelation", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ID")
                        .HasColumnType("bigint");

                    b.Property<long?>("CreateBy")
                        .HasColumnName("CreateBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreateTime")
                        .HasColumnName("CreateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<long>("MenuId")
                        .HasColumnName("MenuId")
                        .HasColumnType("bigint");

                    b.Property<long>("RoleId")
                        .HasColumnName("RoleId")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("MenuId");

                    b.HasIndex("RoleId");

                    b.ToTable("SysRelation");
                });

            modelBuilder.Entity("Adnc.Usr.Core.Entities.SysRole", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ID")
                        .HasColumnType("bigint");

                    b.Property<long?>("CreateBy")
                        .HasColumnName("CreateBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreateTime")
                        .HasColumnName("CreateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<long?>("DeptId")
                        .HasColumnName("DeptId")
                        .HasColumnType("bigint");

                    b.Property<long?>("ModifyBy")
                        .HasColumnName("ModifyBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ModifyTime")
                        .HasColumnName("ModifyTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnName("Name")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4")
                        .HasMaxLength(255);

                    b.Property<int?>("Num")
                        .HasColumnName("Num")
                        .HasColumnType("int");

                    b.Property<long?>("PID")
                        .HasColumnName("Pid")
                        .HasColumnType("bigint");

                    b.Property<string>("Tips")
                        .HasColumnName("Tips")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4")
                        .HasMaxLength(255);

                    b.Property<int?>("Version")
                        .HasColumnName("Version")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("SysRole");
                });

            modelBuilder.Entity("Adnc.Usr.Core.Entities.SysUser", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ID")
                        .HasColumnType("bigint");

                    b.Property<string>("Account")
                        .HasColumnName("Account")
                        .HasColumnType("varchar(32) CHARACTER SET utf8mb4")
                        .HasMaxLength(32);

                    b.Property<string>("Avatar")
                        .HasColumnName("Avatar")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4")
                        .HasMaxLength(255);

                    b.Property<DateTime?>("Birthday")
                        .HasColumnName("Birthday")
                        .HasColumnType("datetime(6)");

                    b.Property<long?>("CreateBy")
                        .HasColumnName("CreateBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreateTime")
                        .HasColumnName("CreateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<long?>("DeptId")
                        .HasColumnName("DeptId")
                        .HasColumnType("bigint");

                    b.Property<string>("Email")
                        .HasColumnName("Email")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<long?>("ModifyBy")
                        .HasColumnName("ModifyBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ModifyTime")
                        .HasColumnName("ModifyTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnName("Name")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<string>("Password")
                        .HasColumnName("Password")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<string>("Phone")
                        .HasColumnName("Phone")
                        .HasColumnType("varchar(16) CHARACTER SET utf8mb4")
                        .HasMaxLength(16);

                    b.Property<string>("RoleId")
                        .HasColumnName("RoleId")
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.Property<string>("Salt")
                        .HasColumnName("Salt")
                        .HasColumnType("varchar(16) CHARACTER SET utf8mb4")
                        .HasMaxLength(16);

                    b.Property<int>("Sex")
                        .HasColumnName("Sex")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnName("Status")
                        .HasColumnType("int");

                    b.Property<int?>("Version")
                        .HasColumnName("Version")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("DeptId");

                    b.ToTable("SysUser");
                });

            modelBuilder.Entity("Adnc.Usr.Core.Entities.SysUserFinance", b =>
                {
                    b.Property<long>("ID")
                        .HasColumnName("ID")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Amount")
                        .HasColumnName("Amount")
                        .HasColumnType("decimal(18,4)");

                    b.Property<long?>("CreateBy")
                        .HasColumnName("CreateBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CreateTime")
                        .HasColumnName("CreateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<long?>("ModifyBy")
                        .HasColumnName("ModifyBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ModifyTime")
                        .HasColumnName("ModifyTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("RowVersion")
                        .HasColumnType("timestamp(3)")
                        .HasDefaultValueSql("'2000-07-01 22:33:02.559'");

                    b.HasKey("ID");

                    b.ToTable("SysUserFinance");
                });

            modelBuilder.Entity("Adnc.Usr.Core.Entities.SysRelation", b =>
                {
                    b.HasOne("Adnc.Usr.Core.Entities.SysMenu", "Menu")
                        .WithMany("Relations")
                        .HasForeignKey("MenuId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Adnc.Usr.Core.Entities.SysRole", "Role")
                        .WithMany("Relations")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Adnc.Usr.Core.Entities.SysUser", b =>
                {
                    b.HasOne("Adnc.Usr.Core.Entities.SysDept", "Dept")
                        .WithMany("Users")
                        .HasForeignKey("DeptId")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("Adnc.Usr.Core.Entities.SysUserFinance", b =>
                {
                    b.HasOne("Adnc.Usr.Core.Entities.SysUser", "User")
                        .WithOne("UserFinance")
                        .HasForeignKey("Adnc.Usr.Core.Entities.SysUserFinance", "ID")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
