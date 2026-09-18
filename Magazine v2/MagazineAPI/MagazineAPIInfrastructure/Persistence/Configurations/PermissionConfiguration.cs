using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Id)
            .ValueGeneratedOnAdd();

        builder.Property(permission => permission.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(permission => permission.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(permission => permission.Description)
            .HasMaxLength(500);

        builder.HasIndex(permission => permission.Code)
            .IsUnique();

        builder.HasData(
            new Permission { Id = PermissionIds.ProductsRead, Code = PermissionCodes.ProductsRead, Name = "Podgląd produktów", Description = "Wyświetlanie katalogu i szczegółów produktów." },
            new Permission { Id = PermissionIds.ProductsManage, Code = PermissionCodes.ProductsManage, Name = "Zarządzanie produktami", Description = "Dodawanie, edycja i wycofywanie produktów." },
            new Permission { Id = PermissionIds.InventoryRead, Code = PermissionCodes.InventoryRead, Name = "Podgląd stanów", Description = "Wyświetlanie stanów i rezerwacji magazynowych." },
            new Permission { Id = PermissionIds.InventoryManage, Code = PermissionCodes.InventoryManage, Name = "Zarządzanie stanami", Description = "Bezpośrednia edycja stanów magazynowych." },
            new Permission { Id = PermissionIds.WarehousesRead, Code = PermissionCodes.WarehousesRead, Name = "Podgląd magazynów", Description = "Wyświetlanie magazynów oraz lokalizacji." },
            new Permission { Id = PermissionIds.WarehousesManage, Code = PermissionCodes.WarehousesManage, Name = "Zarządzanie magazynami", Description = "Dodawanie i edycja magazynów oraz lokalizacji." },
            new Permission { Id = PermissionIds.ContractorsRead, Code = PermissionCodes.ContractorsRead, Name = "Podgląd kontrahentów", Description = "Wyświetlanie dostawców i odbiorców." },
            new Permission { Id = PermissionIds.ContractorsManage, Code = PermissionCodes.ContractorsManage, Name = "Zarządzanie kontrahentami", Description = "Dodawanie i edycja kontrahentów." },
            new Permission { Id = PermissionIds.UsersRead, Code = PermissionCodes.UsersRead, Name = "Podgląd użytkowników", Description = "Wyświetlanie kont użytkowników." },
            new Permission { Id = PermissionIds.UsersManage, Code = PermissionCodes.UsersManage, Name = "Zarządzanie użytkownikami", Description = "Edycja i usuwanie kont innych niż administratorzy." },
            new Permission { Id = PermissionIds.DictionariesManage, Code = PermissionCodes.DictionariesManage, Name = "Zarządzanie słownikami", Description = "Edycja kategorii i jednostek miary." },
            new Permission { Id = PermissionIds.StockDocumentsManage, Code = PermissionCodes.StockDocumentsManage, Name = "Tworzenie dokumentów PZ i WZ", Description = "Tworzenie, edycja i usuwanie szkiców przyjęć oraz wydań." },
            new Permission { Id = PermissionIds.StockDocumentsApprove, Code = PermissionCodes.StockDocumentsApprove, Name = "Zatwierdzanie dokumentów PZ i WZ", Description = "Zatwierdzanie przyjęć i wydań zmieniających stan magazynowy." },
            new Permission { Id = PermissionIds.StockDocumentsRead, Code = PermissionCodes.StockDocumentsRead, Name = "Podgląd dokumentów PZ i WZ", Description = "Wyświetlanie list dokumentów i ich historii." },
            new Permission { Id = PermissionIds.StockDocumentsReceive, Code = PermissionCodes.StockDocumentsReceive, Name = "Odbiór przesyłek", Description = "Skanowanie kodu, podgląd zawartości i przyjmowanie przesyłek PZ." },
            new Permission { Id = PermissionIds.StockShipmentsCreate, Code = PermissionCodes.StockShipmentsCreate, Name = "Tworzenie wysyłek międzyoddziałowych", Description = "Tworzenie szkiców WZ kierowanych do innego oddziału." });
    }
}
