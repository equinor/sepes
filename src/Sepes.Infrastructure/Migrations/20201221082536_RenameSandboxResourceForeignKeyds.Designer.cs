﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sepes.Infrastructure.Model.Context;

namespace Sepes.Infrastructure.Migrations
{
    [DbContext(typeof(SepesDbContext))]
    [Migration("20201221082536_RenameSandboxResourceForeignKeyds")]
    partial class RenameSandboxResourceForeignKeyds
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Sepes.Infrastructure.Model.CloudResource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ConfigString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("Deleted")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeletedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastKnownProvisioningState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Region")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceGroupId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceGroupName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResourceType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("SandboxControlled")
                        .HasColumnType("bit");

                    b.Property<int>("SandboxId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("StudyId")
                        .HasColumnType("int");

                    b.Property<string>("Tags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Updated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasIndex("SandboxId");

                    b.HasIndex("StudyId");

                    b.ToTable("CloudResource");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.CloudResourceOperation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BatchId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CarriedOutBySessionId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CloudResourceId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("CreatedBySessionId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DependsOnOperationId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LatestError")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MaxTryCount")
                        .HasColumnType("int");

                    b.Property<string>("OperationType")
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TryCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("Updated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasIndex("CloudResourceId");

                    b.HasIndex("DependsOnOperationId");

                    b.ToTable("CloudResourceOperation");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.Dataset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AreaL1")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AreaL2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Asset")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BADataOwner")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Classification")
                        .IsRequired()
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.Property<string>("CountryOfOrigin")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<int>("DataId")
                        .HasColumnType("int");

                    b.Property<bool?>("Deleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeletedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LRAId")
                        .HasColumnType("int");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("SourceSystem")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StorageAccountId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StorageAccountName")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<int?>("StudyId")
                        .HasColumnType("int");

                    b.Property<string>("Tags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Updated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("Datasets");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.DatasetFirewallRule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<int>("DatasetId")
                        .HasColumnType("int");

                    b.Property<int>("RuleType")
                        .HasColumnType("int");

                    b.Property<DateTime>("Updated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasIndex("DatasetId");

                    b.ToTable("DatasetFirewallRules");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.Region", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<bool>("Disabled")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Key");

                    b.ToTable("Regions");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.RegionVmSize", b =>
                {
                    b.Property<string>("RegionKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("VmSizeKey")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("RegionKey", "VmSizeKey");

                    b.HasIndex("VmSizeKey");

                    b.ToTable("RegionVmSize");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.Sandbox", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<bool?>("Deleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeletedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StudyId")
                        .HasColumnType("int");

                    b.Property<string>("TechnicalContactEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TechnicalContactName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Updated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasIndex("StudyId");

                    b.ToTable("Sandboxes");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.SandboxDataset", b =>
                {
                    b.Property<int>("SandboxId")
                        .HasColumnType("int");

                    b.Property<int>("DatasetId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Added")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("AddedBy")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SandboxId", "DatasetId");

                    b.HasIndex("DatasetId");

                    b.ToTable("SandboxDatasets");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.SandboxPhaseHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Counter")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<int>("Phase")
                        .HasColumnType("int");

                    b.Property<int>("SandboxId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SandboxId");

                    b.ToTable("SandboxPhaseHistory");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.Study", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool?>("Closed")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ClosedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ClosedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LogoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<bool>("Restricted")
                        .HasColumnType("bit");

                    b.Property<string>("ResultsAndLearnings")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StudySpecificDatasetsResourceGroup")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime>("Updated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Vendor")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("WbsCode")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("Studies");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.StudyDataset", b =>
                {
                    b.Property<int>("StudyId")
                        .HasColumnType("int");

                    b.Property<int>("DatasetId")
                        .HasColumnType("int");

                    b.HasKey("StudyId", "DatasetId");

                    b.HasIndex("DatasetId");

                    b.ToTable("StudyDatasets");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.StudyParticipant", b =>
                {
                    b.Property<int>("StudyId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("RoleName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StudyId", "UserId", "RoleName");

                    b.HasIndex("UserId");

                    b.ToTable("StudyParticipants");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("EmailAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ObjectId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Updated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.Variable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool?>("Bool1")
                        .HasColumnType("bit");

                    b.Property<bool?>("Bool2")
                        .HasColumnType("bit");

                    b.Property<bool?>("Bool3")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<int?>("Int1")
                        .HasColumnType("int");

                    b.Property<int?>("Int2")
                        .HasColumnType("int");

                    b.Property<int?>("Int3")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("Str1")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Str2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Str3")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("datetime2");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("Variables");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.VmSize", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Category")
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("DisplayText")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<int>("MaxDataDiskCount")
                        .HasColumnType("int");

                    b.Property<int>("MaxNetworkInterfaces")
                        .HasColumnType("int");

                    b.Property<int>("MemoryGB")
                        .HasColumnType("int");

                    b.Property<int>("NumberOfCores")
                        .HasColumnType("int");

                    b.Property<int>("OsDiskSizeInMB")
                        .HasColumnType("int");

                    b.Property<int>("ResourceDiskSizeInMB")
                        .HasColumnType("int");

                    b.HasKey("Key");

                    b.ToTable("VmSizes");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.CloudResource", b =>
                {
                    b.HasOne("Sepes.Infrastructure.Model.Sandbox", "Sandbox")
                        .WithMany("Resources")
                        .HasForeignKey("SandboxId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Sepes.Infrastructure.Model.Study", null)
                        .WithMany("CloudResources")
                        .HasForeignKey("StudyId");
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.CloudResourceOperation", b =>
                {
                    b.HasOne("Sepes.Infrastructure.Model.CloudResource", "Resource")
                        .WithMany("Operations")
                        .HasForeignKey("CloudResourceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Sepes.Infrastructure.Model.CloudResourceOperation", "DependsOnOperation")
                        .WithMany("DependantOnThisOperation")
                        .HasForeignKey("DependsOnOperationId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.DatasetFirewallRule", b =>
                {
                    b.HasOne("Sepes.Infrastructure.Model.Dataset", "Dataset")
                        .WithMany("FirewallRules")
                        .HasForeignKey("DatasetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.RegionVmSize", b =>
                {
                    b.HasOne("Sepes.Infrastructure.Model.Region", "Region")
                        .WithMany("VmSizeAssociations")
                        .HasForeignKey("RegionKey")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Sepes.Infrastructure.Model.VmSize", "VmSize")
                        .WithMany("RegionAssociations")
                        .HasForeignKey("VmSizeKey")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.Sandbox", b =>
                {
                    b.HasOne("Sepes.Infrastructure.Model.Study", "Study")
                        .WithMany("Sandboxes")
                        .HasForeignKey("StudyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.SandboxDataset", b =>
                {
                    b.HasOne("Sepes.Infrastructure.Model.Dataset", "Dataset")
                        .WithMany("SandboxDatasets")
                        .HasForeignKey("DatasetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Sepes.Infrastructure.Model.Sandbox", "Sandbox")
                        .WithMany("SandboxDatasets")
                        .HasForeignKey("SandboxId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.SandboxPhaseHistory", b =>
                {
                    b.HasOne("Sepes.Infrastructure.Model.Sandbox", "Sandbox")
                        .WithMany("PhaseHistory")
                        .HasForeignKey("SandboxId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.StudyDataset", b =>
                {
                    b.HasOne("Sepes.Infrastructure.Model.Dataset", "Dataset")
                        .WithMany("StudyDatasets")
                        .HasForeignKey("DatasetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Sepes.Infrastructure.Model.Study", "Study")
                        .WithMany("StudyDatasets")
                        .HasForeignKey("StudyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Sepes.Infrastructure.Model.StudyParticipant", b =>
                {
                    b.HasOne("Sepes.Infrastructure.Model.Study", "Study")
                        .WithMany("StudyParticipants")
                        .HasForeignKey("StudyId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Sepes.Infrastructure.Model.User", "User")
                        .WithMany("StudyParticipants")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
