using Microsoft.EntityFrameworkCore;

#nullable disable

namespace SAI.Service.Core.Infrastructure.Persistencia.Migraciones;

/// <summary>
/// Construye el modelo de EF Core del esquema de identidad (ADR-16) para las clases
/// generadas <c>SaiDbContextModelSnapshot</c> y
/// <c>EsquemaInicialAcceso.Designer</c>. Se factoriza en un único método para no
/// duplicar la definición del modelo entre la instantánea y el diseñador (ambos deben
/// producir el mismo modelo objetivo). Reproduce a mano lo que EF Core generaría para
/// <c>IdentityDbContext&lt;AdministradorUser&gt;</c> sobre SQLite.
/// </summary>
internal static class ModeloIdentityBuilder
{
    public const string ProductVersion = "10.0.0";

    public static void Construir(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", ProductVersion);

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
        {
            b.Property<string>("Id").HasColumnType("TEXT");
            b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("TEXT");
            b.Property<string>("Name").HasMaxLength(256).HasColumnType("TEXT");
            b.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("TEXT");

            b.HasKey("Id");
            b.HasIndex("NormalizedName").IsUnique().HasDatabaseName("RoleNameIndex");
            b.ToTable("AspNetRoles", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("ClaimType").HasColumnType("TEXT");
            b.Property<string>("ClaimValue").HasColumnType("TEXT");
            b.Property<string>("RoleId").IsRequired().HasColumnType("TEXT");

            b.HasKey("Id");
            b.HasIndex("RoleId");
            b.ToTable("AspNetRoleClaims", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("ClaimType").HasColumnType("TEXT");
            b.Property<string>("ClaimValue").HasColumnType("TEXT");
            b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");

            b.HasKey("Id");
            b.HasIndex("UserId");
            b.ToTable("AspNetUserClaims", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
        {
            b.Property<string>("LoginProvider").HasColumnType("TEXT");
            b.Property<string>("ProviderKey").HasColumnType("TEXT");
            b.Property<string>("ProviderDisplayName").HasColumnType("TEXT");
            b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");

            b.HasKey("LoginProvider", "ProviderKey");
            b.HasIndex("UserId");
            b.ToTable("AspNetUserLogins", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
        {
            b.Property<string>("UserId").HasColumnType("TEXT");
            b.Property<string>("RoleId").HasColumnType("TEXT");

            b.HasKey("UserId", "RoleId");
            b.HasIndex("RoleId");
            b.ToTable("AspNetUserRoles", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
        {
            b.Property<string>("UserId").HasColumnType("TEXT");
            b.Property<string>("LoginProvider").HasColumnType("TEXT");
            b.Property<string>("Name").HasColumnType("TEXT");
            b.Property<string>("Value").HasColumnType("TEXT");

            b.HasKey("UserId", "LoginProvider", "Name");
            b.ToTable("AspNetUserTokens", (string)null);
        });

        modelBuilder.Entity("SAI.Service.Core.Infrastructure.Persistencia.AdministradorUser", b =>
        {
            b.Property<string>("Id").HasColumnType("TEXT");
            b.Property<int>("AccessFailedCount").HasColumnType("INTEGER");
            b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("TEXT");
            b.Property<string>("Email").HasMaxLength(256).HasColumnType("TEXT");
            b.Property<bool>("EmailConfirmed").HasColumnType("INTEGER");
            b.Property<bool>("LockoutEnabled").HasColumnType("INTEGER");
            b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("TEXT");
            b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("TEXT");
            b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("TEXT");
            b.Property<string>("PasswordHash").HasColumnType("TEXT");
            b.Property<string>("PhoneNumber").HasColumnType("TEXT");
            b.Property<bool>("PhoneNumberConfirmed").HasColumnType("INTEGER");
            b.Property<string>("SecurityStamp").HasColumnType("TEXT");
            b.Property<bool>("TwoFactorEnabled").HasColumnType("INTEGER");
            b.Property<string>("UserName").HasMaxLength(256).HasColumnType("TEXT");

            b.HasKey("Id");
            b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
            b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex");
            b.ToTable("AspNetUsers", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
        {
            b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                .WithMany()
                .HasForeignKey("RoleId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
        {
            b.HasOne("SAI.Service.Core.Infrastructure.Persistencia.AdministradorUser", null)
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
        {
            b.HasOne("SAI.Service.Core.Infrastructure.Persistencia.AdministradorUser", null)
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
        {
            b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                .WithMany()
                .HasForeignKey("RoleId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("SAI.Service.Core.Infrastructure.Persistencia.AdministradorUser", null)
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
        {
            b.HasOne("SAI.Service.Core.Infrastructure.Persistencia.AdministradorUser", null)
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
    }
}
