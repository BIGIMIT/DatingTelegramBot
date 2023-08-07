﻿// <auto-generated />
using System;
using DatingTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DatingTelegramBot.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DatingTelegramBot.Models.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FileId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Path")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.HasIndex("UserId");

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("DatingTelegramBot.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Age")
                        .HasColumnType("int");

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CurrentHandler")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PreferGender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TurnOff")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("ChatId")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DatingTelegramBot.Models.UserView", b =>
                {
                    b.Property<int>("ViewerId")
                        .HasColumnType("int");

                    b.Property<int>("ViewedId")
                        .HasColumnType("int");

                    b.Property<bool?>("Like")
                        .HasColumnType("bit");

                    b.HasKey("ViewerId", "ViewedId");

                    b.HasIndex("ViewedId");

                    b.ToTable("UserViews");
                });

            modelBuilder.Entity("DatingTelegramBot.Models.Photo", b =>
                {
                    b.HasOne("DatingTelegramBot.Models.User", "User")
                        .WithMany("Photos")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("DatingTelegramBot.Models.UserView", b =>
                {
                    b.HasOne("DatingTelegramBot.Models.User", "Viewed")
                        .WithMany("ViewerUsers")
                        .HasForeignKey("ViewedId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DatingTelegramBot.Models.User", "Viewer")
                        .WithMany("ViewedUsers")
                        .HasForeignKey("ViewerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Viewed");

                    b.Navigation("Viewer");
                });

            modelBuilder.Entity("DatingTelegramBot.Models.User", b =>
                {
                    b.Navigation("Photos");

                    b.Navigation("ViewedUsers");

                    b.Navigation("ViewerUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
