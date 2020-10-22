﻿// <auto-generated />
using System;
using HockeyManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HockeyManager.Migrations
{
    [DbContext(typeof(HockeyContext))]
    partial class HockeyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("HockeyManager.Areas.Identity.Data.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("HockeyManager.Models.Favourites", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("PlayerId");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.HasIndex("UserId");

                    b.ToTable("Favourites");
                });

            modelBuilder.Entity("HockeyManager.Models.HMPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ApiId");

                    b.Property<int>("Assists");

                    b.Property<int>("GamesPlayed");

                    b.Property<int>("Goals");

                    b.Property<int>("PenalityMinutes");

                    b.Property<int?>("PlayerInfoId");

                    b.Property<int>("PlusMinus");

                    b.Property<int>("Points");

                    b.Property<string>("Position");

                    b.Property<int>("Rank");

                    b.Property<int>("Saves");

                    b.Property<int>("Shutouts");

                    b.Property<int?>("TeamId");

                    b.HasKey("Id");

                    b.HasIndex("PlayerInfoId");

                    b.HasIndex("TeamId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("HockeyManager.Models.HMPlayerInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Country");

                    b.Property<DateTimeOffset>("DateOfBirth");

                    b.Property<string>("HeadShotUrl");

                    b.Property<string>("Height");

                    b.Property<string>("Name");

                    b.Property<long>("Weight");

                    b.HasKey("Id");

                    b.ToTable("PlayerInfo");
                });

            modelBuilder.Entity("HockeyManager.Models.HMTeam", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ApiId");

                    b.Property<string>("Conference");

                    b.Property<string>("Division");

                    b.Property<int>("GamesPlayed");

                    b.Property<int>("Loses");

                    b.Property<int>("OvertimeLoses");

                    b.Property<string>("Place");

                    b.Property<int>("Points");

                    b.Property<int?>("PoolId");

                    b.Property<int>("RegulationWins");

                    b.Property<int?>("SeasonId");

                    b.Property<int?>("TeamInfoId");

                    b.Property<string>("UserId");

                    b.Property<int>("Wins");

                    b.HasKey("Id");

                    b.HasIndex("PoolId");

                    b.HasIndex("SeasonId");

                    b.HasIndex("TeamInfoId");

                    b.HasIndex("UserId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("HockeyManager.Models.HMTeamInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Abbreviation");

                    b.Property<string>("Name");

                    b.Property<string>("logoUrl");

                    b.HasKey("Id");

                    b.ToTable("TeamInfo");
                });

            modelBuilder.Entity("HockeyManager.Models.Pool", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("OwnerId");

                    b.Property<bool>("Private");

                    b.Property<int?>("RuleSetId")
                        .IsRequired();

                    b.Property<int>("Size");

                    b.Property<string>("Status");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.HasIndex("RuleSetId");

                    b.ToTable("Pools");
                });

            modelBuilder.Entity("HockeyManager.Models.PoolList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("PoolId");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("PoolId");

                    b.HasIndex("UserId");

                    b.ToTable("PoolList");
                });

            modelBuilder.Entity("HockeyManager.Models.RuleSet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("maxDefensemen");

                    b.Property<int>("maxForwards");

                    b.Property<int>("maxGoalies");

                    b.Property<int>("maxPlayers");

                    b.HasKey("Id");

                    b.ToTable("RuleSets");
                });

            modelBuilder.Entity("HockeyManager.Models.Season", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Seasons");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .HasMaxLength(128);

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("HockeyManager.Models.Favourites", b =>
                {
                    b.HasOne("HockeyManager.Models.HMPlayer", "Player")
                        .WithMany("Favourites")
                        .HasForeignKey("PlayerId");

                    b.HasOne("HockeyManager.Areas.Identity.Data.User", "User")
                        .WithMany("Favourites")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("HockeyManager.Models.HMPlayer", b =>
                {
                    b.HasOne("HockeyManager.Models.HMPlayerInfo", "PlayerInfo")
                        .WithMany("PlayerStats")
                        .HasForeignKey("PlayerInfoId");

                    b.HasOne("HockeyManager.Models.HMTeam", "Team")
                        .WithMany("Players")
                        .HasForeignKey("TeamId");
                });

            modelBuilder.Entity("HockeyManager.Models.HMTeam", b =>
                {
                    b.HasOne("HockeyManager.Models.Pool", "Pool")
                        .WithMany("Teams")
                        .HasForeignKey("PoolId");

                    b.HasOne("HockeyManager.Models.Season", "Season")
                        .WithMany("Teams")
                        .HasForeignKey("SeasonId");

                    b.HasOne("HockeyManager.Models.HMTeamInfo", "TeamInfo")
                        .WithMany("TeamStats")
                        .HasForeignKey("TeamInfoId");

                    b.HasOne("HockeyManager.Areas.Identity.Data.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("HockeyManager.Models.Pool", b =>
                {
                    b.HasOne("HockeyManager.Areas.Identity.Data.User", "Owner")
                        .WithMany("PoolsOwned")
                        .HasForeignKey("OwnerId");

                    b.HasOne("HockeyManager.Models.RuleSet", "RuleSet")
                        .WithMany("Pools")
                        .HasForeignKey("RuleSetId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HockeyManager.Models.PoolList", b =>
                {
                    b.HasOne("HockeyManager.Models.Pool", "Pool")
                        .WithMany("PoolList")
                        .HasForeignKey("PoolId");

                    b.HasOne("HockeyManager.Areas.Identity.Data.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("HockeyManager.Models.Season", b =>
                {
                    b.HasOne("HockeyManager.Areas.Identity.Data.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("HockeyManager.Areas.Identity.Data.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("HockeyManager.Areas.Identity.Data.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("HockeyManager.Areas.Identity.Data.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("HockeyManager.Areas.Identity.Data.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
