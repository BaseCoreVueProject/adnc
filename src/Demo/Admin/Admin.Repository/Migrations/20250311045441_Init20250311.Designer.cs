// <auto-generated />
using System;
using Adnc.Infra.Repository.EfCore.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Adnc.Demo.Admin.Repository.Migrations
{
    [DbContext(typeof(MySqlDbContext))]
    [Migration("20250311045441_Init20250311")]
    partial class Init20250311
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.HasCharSet(modelBuilder, "utf8mb4 ");
            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Adnc.Demo.Admin.Repository.Entities.Menu", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasColumnOrder(1)
                        .HasComment("");

                    b.Property<bool>("AlwaysShow")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("alwaysshow")
                        .HasComment("只有一个子路由是否始终显示");

                    b.Property<string>("Component")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)")
                        .HasColumnName("component")
                        .HasComment("組件配置");

                    b.Property<long>("CreateBy")
                        .HasColumnType("bigint")
                        .HasColumnName("createby")
                        .HasColumnOrder(100)
                        .HasComment("创建人");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("createtime")
                        .HasColumnOrder(101)
                        .HasComment("创建时间/注册时间");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16)")
                        .HasColumnName("icon")
                        .HasComment("图标");

                    b.Property<bool>("KeepAlive")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("keepalive")
                        .HasComment("是否开启页面缓存");

                    b.Property<long>("ModifyBy")
                        .HasColumnType("bigint")
                        .HasColumnName("modifyby")
                        .HasColumnOrder(102)
                        .HasComment("最后更新人");

                    b.Property<DateTime>("ModifyTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("modifytime")
                        .HasColumnOrder(103)
                        .HasComment("最后更新时间");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasColumnName("name")
                        .HasComment("名称");

                    b.Property<int>("Ordinal")
                        .HasColumnType("int")
                        .HasColumnName("ordinal")
                        .HasComment("序号");

                    b.Property<string>("Params")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)")
                        .HasColumnName("params")
                        .HasComment("路由参数");

                    b.Property<long>("ParentId")
                        .HasColumnType("bigint")
                        .HasColumnName("parentid")
                        .HasComment("父菜单Id");

                    b.Property<string>("ParentIds")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)")
                        .HasColumnName("parentids")
                        .HasComment("父菜单Id组合");

                    b.Property<string>("Perm")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasColumnName("perm")
                        .HasComment("权限编码");

                    b.Property<string>("Redirect")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)")
                        .HasColumnName("redirect")
                        .HasComment("跳转路由路径");

                    b.Property<string>("RouteName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)")
                        .HasColumnName("routename")
                        .HasComment("路由名称");

                    b.Property<string>("RoutePath")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)")
                        .HasColumnName("routepath")
                        .HasComment("路由路径");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16)")
                        .HasColumnName("type")
                        .HasComment("菜单类型");

                    b.Property<bool>("Visible")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("visible")
                        .HasComment("是否显示");

                    b.HasKey("Id")
                        .HasName("pk_sys_menu");

                    b.ToTable("sys_menu", null, t =>
                        {
                            t.HasComment("菜单");
                        });
                });

            modelBuilder.Entity("Adnc.Demo.Admin.Repository.Entities.Organization", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasColumnOrder(1)
                        .HasComment("");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16)")
                        .HasColumnName("code")
                        .HasComment("");

                    b.Property<long>("CreateBy")
                        .HasColumnType("bigint")
                        .HasColumnName("createby")
                        .HasColumnOrder(100)
                        .HasComment("创建人");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("createtime")
                        .HasColumnOrder(101)
                        .HasComment("创建时间/注册时间");

                    b.Property<long>("ModifyBy")
                        .HasColumnType("bigint")
                        .HasColumnName("modifyby")
                        .HasColumnOrder(102)
                        .HasComment("最后更新人");

                    b.Property<DateTime>("ModifyTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("modifytime")
                        .HasColumnOrder(103)
                        .HasComment("最后更新时间");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasColumnName("name")
                        .HasComment("");

                    b.Property<int>("Ordinal")
                        .HasColumnType("int")
                        .HasColumnName("ordinal")
                        .HasComment("");

                    b.Property<long>("ParentId")
                        .HasColumnType("bigint")
                        .HasColumnName("parentid")
                        .HasComment("");

                    b.Property<string>("ParentIds")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)")
                        .HasColumnName("parentids")
                        .HasComment("");

                    b.Property<bool>("Status")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("status")
                        .HasComment("");

                    b.HasKey("Id")
                        .HasName("pk_sys_organization");

                    b.ToTable("sys_organization", null, t =>
                        {
                            t.HasComment("部门");
                        });
                });

            modelBuilder.Entity("Adnc.Demo.Admin.Repository.Entities.Role", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasColumnOrder(1)
                        .HasComment("");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasColumnName("code")
                        .HasComment("");

                    b.Property<long>("CreateBy")
                        .HasColumnType("bigint")
                        .HasColumnName("createby")
                        .HasColumnOrder(100)
                        .HasComment("创建人");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("createtime")
                        .HasColumnOrder(101)
                        .HasComment("创建时间/注册时间");

                    b.Property<int>("DataScope")
                        .HasColumnType("int")
                        .HasColumnName("datascope")
                        .HasComment("");

                    b.Property<long>("ModifyBy")
                        .HasColumnType("bigint")
                        .HasColumnName("modifyby")
                        .HasColumnOrder(102)
                        .HasComment("最后更新人");

                    b.Property<DateTime>("ModifyTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("modifytime")
                        .HasColumnOrder(103)
                        .HasComment("最后更新时间");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasColumnName("name")
                        .HasComment("");

                    b.Property<int>("Ordinal")
                        .HasColumnType("int")
                        .HasColumnName("ordinal")
                        .HasComment("");

                    b.Property<int>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status")
                        .HasComment("");

                    b.HasKey("Id")
                        .HasName("pk_sys_role");

                    b.ToTable("sys_role", null, t =>
                        {
                            t.HasComment("角色");
                        });
                });

            modelBuilder.Entity("Adnc.Demo.Admin.Repository.Entities.RoleMenuRelation", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasColumnOrder(1)
                        .HasComment("");

                    b.Property<long>("MenuId")
                        .HasColumnType("bigint")
                        .HasColumnName("menuid")
                        .HasComment("");

                    b.Property<long>("RoleId")
                        .HasColumnType("bigint")
                        .HasColumnName("roleid")
                        .HasComment("");

                    b.HasKey("Id")
                        .HasName("pk_sys_role_menu_relation");

                    b.ToTable("sys_role_menu_relation", null, t =>
                        {
                            t.HasComment("菜单角色关系");
                        });
                });

            modelBuilder.Entity("Adnc.Demo.Admin.Repository.Entities.RoleUserRelation", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasColumnOrder(1)
                        .HasComment("");

                    b.Property<long>("RoleId")
                        .HasColumnType("bigint")
                        .HasColumnName("roleid")
                        .HasComment("");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("userid")
                        .HasComment("");

                    b.HasKey("Id")
                        .HasName("pk_sys_role_user_relation");

                    b.ToTable("sys_role_user_relation", null, t =>
                        {
                            t.HasComment("用户角色关系");
                        });
                });

            modelBuilder.Entity("Adnc.Demo.Admin.Repository.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasColumnOrder(1)
                        .HasComment("");

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16)")
                        .HasColumnName("account")
                        .HasComment("账号");

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)")
                        .HasColumnName("avatar")
                        .HasComment("头像路径");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("birthday")
                        .HasComment("生日");

                    b.Property<long>("CreateBy")
                        .HasColumnType("bigint")
                        .HasColumnName("createby")
                        .HasColumnOrder(100)
                        .HasComment("创建人");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("createtime")
                        .HasColumnOrder(101)
                        .HasComment("创建时间/注册时间");

                    b.Property<long>("DeptId")
                        .HasColumnType("bigint")
                        .HasColumnName("deptid")
                        .HasComment("部门Id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasColumnName("email")
                        .HasComment("email");

                    b.Property<int>("Gender")
                        .HasColumnType("int")
                        .HasColumnName("gender")
                        .HasComment("性别");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false)
                        .HasColumnName("isdeleted")
                        .HasColumnOrder(99)
                        .HasComment("");

                    b.Property<string>("Mobile")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("varchar(11)")
                        .HasColumnName("mobile")
                        .HasComment("手机号");

                    b.Property<long>("ModifyBy")
                        .HasColumnType("bigint")
                        .HasColumnName("modifyby")
                        .HasColumnOrder(102)
                        .HasComment("最后更新人");

                    b.Property<DateTime>("ModifyTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("modifytime")
                        .HasColumnOrder(103)
                        .HasComment("最后更新时间");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16)")
                        .HasColumnName("name")
                        .HasComment("姓名");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasColumnName("password")
                        .HasComment("密码");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasMaxLength(6)
                        .HasColumnType("varchar(6)")
                        .HasColumnName("salt")
                        .HasComment("密码盐");

                    b.Property<int>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status")
                        .HasComment("状态");

                    b.HasKey("Id")
                        .HasName("pk_sys_user");

                    b.ToTable("sys_user", null, t =>
                        {
                            t.HasComment("管理员");
                        });
                });

            modelBuilder.Entity("Adnc.Shared.Repository.EfCoreEntities.EventTracker", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasComment("");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));

                    b.Property<long>("CreateBy")
                        .HasColumnType("bigint")
                        .HasColumnName("createby")
                        .HasComment("创建人");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("createtime")
                        .HasComment("创建时间/注册时间");

                    b.Property<long>("EventId")
                        .HasColumnType("bigint")
                        .HasColumnName("eventid")
                        .HasComment("");

                    b.Property<string>("TrackerName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("trackername")
                        .HasComment("");

                    b.HasKey("Id")
                        .HasName("pk_sys_eventtracker");

                    b.HasIndex(new[] { "EventId", "TrackerName" }, "uk_eventid_trackername")
                        .IsUnique()
                        .HasDatabaseName("ix_sys_eventtracker_eventid_trackername");

                    b.ToTable("sys_eventtracker", null, t =>
                        {
                            t.HasComment("事件跟踪/处理信息");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
