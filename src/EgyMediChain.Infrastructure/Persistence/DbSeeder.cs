using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;

namespace EgyMediChain.Infrastructure.Persistence;

public static class DbSeeder
{
    private static readonly Random Rng = new(42);

    private static readonly string[] Governorates =
    {
        "Cairo", "Giza", "Alexandria", "Dakahlia", "Sharqia", "Gharbia", "Menoufia",
        "Qalyubia", "Beheira", "Assiut", "Sohag", "Aswan", "Luxor", "Port Said",
        "Ismailia", "Suez", "Fayoum", "Beni Suef", "Minya", "Damietta"
    };

    private static readonly string[] FactoryCompanyNames =
    {
        "Delta Pharma Factory", "EIPICO Factory", "Misr Co. for Pharma", "Upper Egypt Factory",
        "Alexandria Medicines", "Nile Pharma", "Cairo Chemical Industries", "October Pharma Co.",
        "Suez Pharmaceutical Factory", "Rameda Factory", "Amoun Pharma Factory", "Sigma Factory",
        "Pharco Factory", "Adwia Factory", "Marcyrl Factory", "Hikma Egypt Factory",
        "Memphis Pharma Factory", "Al Andalous Factory", "Minapharm Factory", "Chemipharm Factory"
    };

    private static readonly string[] WarehouseNames =
    {
        "Cairo Medical Storage", "Portsaid Distribution", "Assiut Central Warehouse",
        "Delta Storage Warehouse", "Alex Warehouse", "Giza Main Depot", "Mansoura Regional Store",
        "Tanta Central Depot", "Ismailia Warehouse", "Aswan Storage Facility", "Beni Suef Depot",
        "Fayoum Regional Warehouse", "Minya Central Store", "Sohag Storage Hub", "Damietta Depot"
    };

    private static readonly string[] PharmacyNames =
    {
        "Alexandria Drug Store", "Mansoura Pharmacy", "Giza City Pharmacy", "Heliopolis Pharmacy",
        "Tanta Pharmacy", "Nasr City Pharmacy", "Zamalek Pharmacy", "Maadi Pharmacy",
        "Dokki Pharmacy", "Sheraton Pharmacy", "Smouha Pharmacy", "Mohandessin Pharmacy",
        "Rehab Pharmacy", "October Pharmacy", "Agouza Pharmacy", "Sidi Gaber Pharmacy"
    };

    private static readonly string[] ProductNames =
    {
        "Panadol Extra", "Brufen 400mg", "Augmentin 1g", "Cipro 500mg", "Flagyl 500mg",
        "Voltaren 75mg", "Amoxicillin 500mg", "Diclofenac 50mg", "Losec 20mg", "Zithromax 500mg",
        "Concor 5mg", "Glucophage 500mg", "Lipitor 20mg", "Nexium 40mg", "Ventolin Inhaler",
        "Cataflam 50mg", "Neurobion Forte", "Congestal", "Adol Extra", "Tensopin 5mg"
    };

    private static readonly string[] DosageForms = { "Tablet", "Coated Tablet", "Capsule", "Syrup", "Injection" };
    private static readonly string[] FirstNames = { "Ahmed", "Mona", "Yasser", "Heba", "Khaled", "Sara", "Omar", "Tamer", "Mostafa", "Sarah", "Amr", "Nour", "Youssef", "Dina", "Karim", "Rania" };
    private static readonly string[] LastNames = { "Ali", "Samir", "Mohamed", "Mostafa", "Hassan", "Mahmoud", "Refaat", "Fathy", "Nabil", "Ahmed", "Youssef", "Kamal", "Adel", "Fahmy" };

    // Fixed, memorable demo accounts - one per role, so the email itself tells you which
    // role it logs in as. Password is the same for all of them to keep testing simple.
    public const string DemoPassword = "Passw0rd!123";

    public static readonly (string Email, string FullName, SystemRole Role, EntityKind EntityType)[] RoleTestAccounts =
    {
        ("superadmin@egymedichain.com",     "Dr. Saif (Super Admin)",   SystemRole.SuperAdmin,     EntityKind.Ministry),
        ("ministryadmin@egymedichain.com",  "Ahmed Ali (Ministry Admin)", SystemRole.MinistryAdmin,  EntityKind.Ministry),
        ("ministryviewer@egymedichain.com", "Sara Mahmoud (Ministry Viewer)", SystemRole.MinistryViewer, EntityKind.Ministry),
        ("factoryuser@egymedichain.com",    "Ahmed Ali (Factory Admin)", SystemRole.FactoryUser,    EntityKind.Factory),
        ("warehouseuser@egymedichain.com",  "Ahmed Hassan (Warehouse Admin)", SystemRole.WarehouseUser,  EntityKind.Warehouse),
        ("pharmacyuser@egymedichain.com",   "Yasser Mohamed (Pharmacy Admin)", SystemRole.PharmacyUser,   EntityKind.Pharmacy)
    };

    // Runs on every startup (not just first seed) so the fixed demo accounts always exist
    // and always have a known password, even if the rest of the data was seeded earlier.
    public static void EnsureRoleTestAccounts(AppDbContext db)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword, 12);
        var changed = false;

        // Link the demo accounts to specific, recognizable seeded records (matching the
        // Factory/Warehouse Portal mockups) instead of "whichever row happens to be first".
        var demoFactoryId = db.Factories.Where(f => f.OfficialFactoryName == "EIPICO Factory").Select(f => (int?)f.Id).FirstOrDefault()
                             ?? db.Factories.Select(f => (int?)f.Id).FirstOrDefault();
        var demoWarehouseId = db.Warehouses.Where(w => w.OfficialWarehouseName == "Cairo Medical Storage").Select(w => (int?)w.Id).FirstOrDefault()
                               ?? db.Warehouses.Select(w => (int?)w.Id).FirstOrDefault();
        var demoPharmacyId = db.Pharmacies.Where(p => p.OfficialPharmacyName == "Alexandria Drug Store").Select(p => (int?)p.Id).FirstOrDefault()
                              ?? db.Pharmacies.Select(p => (int?)p.Id).FirstOrDefault();

        if (demoFactoryId.HasValue)
        {
            var f = db.Factories.Find(demoFactoryId.Value);
            if (f != null && f.FactoryStatus != FacilityStatus.Active) { f.FactoryStatus = FacilityStatus.Active; changed = true; }
        }
        if (demoWarehouseId.HasValue)
        {
            var w = db.Warehouses.Find(demoWarehouseId.Value);
            if (w != null && w.WarehouseStatus != FacilityStatus.Active) { w.WarehouseStatus = FacilityStatus.Active; changed = true; }
        }
        if (demoPharmacyId.HasValue)
        {
            var p = db.Pharmacies.Find(demoPharmacyId.Value);
            if (p != null && p.PharmacyStatus != FacilityStatus.Active) { p.PharmacyStatus = FacilityStatus.Active; changed = true; }
        }

        foreach (var (email, fullName, role, entityType) in RoleTestAccounts)
        {
            int? entityId = entityType switch
            {
                EntityKind.Factory => demoFactoryId,
                EntityKind.Warehouse => demoWarehouseId,
                EntityKind.Pharmacy => demoPharmacyId,
                _ => null
            };

            
            
            var user = db.SystemUsers.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                db.SystemUsers.Add(new SystemUser
                {
                    FullName = fullName,
                    Email = email,
                    MobileNumber = $"0100{Rng.Next(1000000, 9999999)}",
                    NationalId = $"{Rng.NextInt64(10000000000000, 29999999999999)}",
                    Role = role,
                    EntityType = entityType,
                    EntityId = entityId,
                    EmailConfirmed = true,
                    IsActive = true,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                changed = true;
            }
            else
            {
                // keep the known password + role + entity link stable even if something else changed it
                user.PasswordHash = passwordHash;
                user.Role = role;
                user.EntityType = entityType;
                user.EntityId = entityId;
                user.IsActive = true;
                user.EmailConfirmed = true;
                changed = true;
            }
        }

        if (changed) db.SaveChanges();
    }

    public static void Seed(AppDbContext db)
    {
        if (db.SystemUsers.Any())
        {
            EnsureRoleTestAccounts(db);
            SeedDemoOperationalData(db);
            return; // rest of the data already seeded
        }

        var users = SeedUsers();
        db.SystemUsers.AddRange(users);
        db.SaveChanges();

        var factories = SeedFactories();
        db.Factories.AddRange(factories);
        db.SaveChanges();

        var warehouses = SeedWarehouses();
        db.Warehouses.AddRange(warehouses);
        db.SaveChanges();

        var pharmacies = SeedPharmacies(warehouses);
        db.Pharmacies.AddRange(pharmacies);
        db.SaveChanges();

        var products = SeedProducts();
        db.MedicineProducts.AddRange(products);
        db.SaveChanges();

        var batches = SeedBatches(products, factories, users);
        db.Batches.AddRange(batches);
        db.SaveChanges();

        var unitCodes = SeedUnitCodes(batches);
        db.UnitCodes.AddRange(unitCodes);
        db.SaveChanges();

        var shipments = SeedShipments(batches, factories, warehouses, pharmacies);
        db.Shipments.AddRange(shipments);
        db.SaveChanges();

        var inventory = SeedInventory(batches, warehouses, pharmacies);
        db.InventoryStocks.AddRange(inventory);
        db.SaveChanges();

        var alerts = SeedAlerts(batches, factories, warehouses, pharmacies);
        db.Alerts.AddRange(alerts);
        db.SaveChanges();

        var scans = SeedScans(unitCodes, products, batches);
        db.PublicVerificationScans.AddRange(scans);
        db.SaveChanges();

        var registrationRequests = SeedRegistrationRequests(users);
        db.RegistrationRequests.AddRange(registrationRequests);
        db.SaveChanges();

        var licenses = SeedLicenses(factories);
        db.EntityLicenses.AddRange(licenses);
        db.SaveChanges();

        var auditLogs = SeedAuditLogs(users);
        db.AuditLogs.AddRange(auditLogs);
        db.SaveChanges();

        EnsureRoleTestAccounts(db);
        SeedDemoOperationalData(db);
    }

    private static DateTime RandDate(int daysBackMin, int daysBackMax) =>
        DateTime.UtcNow.AddDays(-Rng.Next(daysBackMin, daysBackMax)).AddHours(Rng.Next(0, 23));

    private static string RandName() => $"{FirstNames[Rng.Next(FirstNames.Length)]} {LastNames[Rng.Next(LastNames.Length)]}";

    private static List<SystemUser> SeedUsers()
    {
        var list = new List<SystemUser>();
        list.Add(new SystemUser
        {
            FullName = "Dr. Saif", Email = "saif.superadmin@health.gov.eg", MobileNumber = "01000000001",
            NationalId = "29901011234567", Role = SystemRole.SuperAdmin, EntityType = EntityKind.Ministry,
            EmailConfirmed = true, IsActive = true, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!", 12),
            LastLoginAt = RandDate(0, 1), CreatedAt = RandDate(300, 400)
        });
        for (int i = 0; i < 31; i++)
        {
            var role = i % 6 == 0 ? SystemRole.MinistryAdmin : i % 6 == 1 ? SystemRole.MinistryViewer :
                       i % 6 == 2 ? SystemRole.FactoryUser : i % 6 == 3 ? SystemRole.WarehouseUser :
                       i % 6 == 4 ? SystemRole.PharmacyUser : SystemRole.MinistryAdmin;
            var name = RandName();
            list.Add(new SystemUser
            {
                FullName = name,
                Email = $"{name.Replace(" ", ".").ToLower()}{i}@health.gov.eg",
                MobileNumber = $"01{Rng.Next(0, 2)}{Rng.Next(10000000, 99999999)}",
                NationalId = Rng.Next(0, 2) == 0 ? $"{Rng.NextInt64(10000000000000, 29999999999999)}" : null,
                Role = role,
                EntityType = EntityKind.Ministry,
                EmailConfirmed = Rng.Next(0, 10) > 1,
                IsActive = Rng.Next(0, 10) > 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Temp@12345", 12),
                LastLoginAt = Rng.Next(0, 10) > 1 ? RandDate(0, 30) : null,
                CreatedAt = RandDate(60, 400),
                UpdatedAt = RandDate(0, 60)
            });
        }
        return list;
    }

    private static List<Factory> SeedFactories()
    {
        var list = new List<Factory>();
        var statuses = new[] { FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Suspended, FacilityStatus.Inactive };
        var productionTypesOptions = new[] { "Tablets, Capsules, Injectables, Liquids", "Tablets, Syrups", "Capsules, Injectables", "Tablets, Ointments, Syrups" };
        var storageTypesOptions = new[] { "Ambient, Cold Storage", "Ambient Only", "Ambient, Cold Storage, Quarantine" };
        var certOptions = new[] { "ISO 9001:2015, ISO 14001:2015, GMP", "GMP, WHO-GMP", "ISO 9001:2015, GMP" };

        for (int i = 0; i < 45; i++)
        {
            var name = FactoryCompanyNames[i % FactoryCompanyNames.Length] + (i >= FactoryCompanyNames.Length ? $" #{i}" : "");
            var gov = Governorates[Rng.Next(Governorates.Length)];
            var isDemo = name == "EIPICO Factory";

            var factory = new Factory
            {
                FactoryCode = isDemo ? "FAC-2024-021" : $"FAC-2024-{(i + 1):000}",
                OfficialFactoryName = name,
                LegalCompanyName = name.Replace("Factory", "Pharmaceutical Co."),
                DosageFormsProduced = string.Join(", ", DosageForms.OrderBy(_ => Rng.Next()).Take(Rng.Next(1, 4))),
                Governorate = isDemo ? "Cairo" : gov,
                City = isDemo ? "Cairo" : gov,
                DistrictArea = isDemo ? "Industrial Zone A3" : $"Industrial Zone {(char)('A' + Rng.Next(0, 5))}",
                FullAddress = isDemo ? "10th of Ramadan City, Industrial Zone A3, Egypt" : $"Industrial Zone {(char)('A' + Rng.Next(0, 5))}, {gov}, Egypt",
                Phone = isDemo ? "+20 123 456 7890" : $"+20 1{Rng.Next(0, 2)} {Rng.Next(1000, 9999)} {Rng.Next(1000, 9999)}",
                Email = isDemo ? "factory@eipico.com" : $"factory@{name.Replace(" ", "").ToLower()}.com",
                FactoryLicenseNumber = $"FAC-2024-{(i + 1):000}",
                TechnicalOperatingLicenseNumber = $"TOL-2024-{(i + 11):000}",
                CommercialRegistrationNumber = isDemo ? "125478" : $"{Rng.Next(100000, 999999)}",
                TaxCardNumber = isDemo ? "548796321" : $"{Rng.Next(100000000, 999999999)}",
                LicenseIssueDate = RandDate(200, 900),
                LicenseExpiryDate = DateTime.UtcNow.AddDays(Rng.Next(-60, 900)),
                HasQualityControlLab = Rng.Next(0, 10) > 2,
                HasFinishedGoodsStore = Rng.Next(0, 10) > 1,
                HasColdStorage = isDemo ? true : Rng.Next(0, 10) > 4,
                HasQuarantineArea = Rng.Next(0, 10) > 3,
                EstablishedYear = isDemo ? 2005 : Rng.Next(1990, 2020),
                TotalProductionLines = isDemo ? 12 : Rng.Next(3, 20),
                MainProductionTypes = isDemo ? "Tablets, Capsules, Injectables, Liquids" : productionTypesOptions[Rng.Next(productionTypesOptions.Length)],
                StorageTypes = isDemo ? "Ambient, Cold Storage" : storageTypesOptions[Rng.Next(storageTypesOptions.Length)],
                QualityCertificates = isDemo ? "ISO 9001:2015, ISO 14001:2015, GMP" : certOptions[Rng.Next(certOptions.Length)],
                Description = isDemo
                    ? "EIPICO is a leading pharmaceutical manufacturer in Egypt committed to quality and compliance."
                    : $"{name} is a licensed pharmaceutical manufacturer operating in {gov}, Egypt.",
                RegistrationRequestNo = isDemo ? "REG-2024-00125" : $"REG-2024-{Rng.Next(10000, 99999)}",
                RegistrationSubmittedAt = RandDate(100, 400),
                RegistrationApprovedAt = RandDate(50, 100),
                RegistrationApprovedBy = "Ministry of Health",
                RegistrationExpiryDate = DateTime.UtcNow.AddDays(Rng.Next(300, 800)),
                RegistrationNotes = "All documents and information verified successfully.",
                FactoryStatus = isDemo ? FacilityStatus.Active : statuses[Rng.Next(statuses.Length)],
                TotalBatches = Rng.Next(5, 60),
                CreatedAt = RandDate(100, 500),
                UpdatedAt = RandDate(0, 90)
            };
            list.Add(factory);
        }
        return list;
    }

    private static List<Warehouse> SeedWarehouses()
    {
        var list = new List<Warehouse>();
        var statuses = new[] { FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Suspended, FacilityStatus.Inactive };
        var types = new[] { "Main Warehouse", "Regional Warehouse" };
        for (int i = 0; i < 40; i++)
        {
            var name = WarehouseNames[i % WarehouseNames.Length] + (i >= WarehouseNames.Length ? $" #{i}" : "");
            var gov = Governorates[Rng.Next(Governorates.Length)];
            var isDemo = name == "Cairo Medical Storage";

            list.Add(new Warehouse
            {
                WarehouseCode = isDemo ? "WH-CAI-001" : $"WH-2024-{(i + 1):000}",
                OfficialWarehouseName = name,
                WarehouseType = isDemo ? "Main Warehouse" : types[Rng.Next(types.Length)],
                Governorate = isDemo ? "Cairo" : gov,
                City = isDemo ? "Cairo" : gov,
                DistrictArea = isDemo ? "10th of Ramadan District" : $"{Rng.Next(1, 20)} Ahmed Fakhry St., Naar City",
                FullAddress = isDemo ? "Industrial Zone A3, Block 15, 10th of Ramadan City, Cairo, Egypt" : $"{Rng.Next(1, 20)} Ahmed Fakhry St., Naar City, {gov}, Egypt",
                Phone = isDemo ? "+20 10 1234 5678" : $"+20 1{Rng.Next(0, 2)} {Rng.Next(1000, 9999)} {Rng.Next(1000, 9999)}",
                Email = isDemo ? "ahmed.hassan@cairomed.com" : $"warehouse@{name.Replace(" ", "").ToLower()}.com",
                WarehouseLicenseNumber = isDemo ? "WH-CAI-001-LIC" : $"WH-2024-{(i + 1):000}",
                LicenseIssueDate = isDemo ? new DateTime(2023, 5, 12) : RandDate(200, 900),
                LicenseExpiryDate = isDemo ? new DateTime(2025, 5, 12) : DateTime.UtcNow.AddDays(Rng.Next(-60, 900)),
                HasColdStorage = isDemo ? true : Rng.Next(0, 10) > 3,
                HasQuarantineArea = isDemo ? true : Rng.Next(0, 10) > 3,
                HasDeliveryService = isDemo ? true : Rng.Next(0, 10) > 2,
                WarehouseStatus = isDemo ? FacilityStatus.Active : statuses[Rng.Next(statuses.Length)],
                CreatedAt = RandDate(100, 500),
                UpdatedAt = RandDate(0, 90)
            });
        }
        return list;
    }

    private static List<Pharmacy> SeedPharmacies(List<Warehouse> warehouses)
    {
        var list = new List<Pharmacy>();
        var statuses = new[] { FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Active, FacilityStatus.Suspended, FacilityStatus.Inactive };
        for (int i = 0; i < 90; i++)
        {
            var name = PharmacyNames[i % PharmacyNames.Length] + (i >= PharmacyNames.Length ? $" #{i}" : "");
            var gov = Governorates[Rng.Next(Governorates.Length)];
            var wh = warehouses[Rng.Next(warehouses.Count)];
            var isDemo = name == "Alexandria Drug Store";
            list.Add(new Pharmacy
            {
                PharmacyCode = isDemo ? "PH-2024-001" : $"PH-2024-{(i + 1):000}",
                OfficialPharmacyName = name,
                PharmacyType = "Retail Pharmacy",
                Governorate = isDemo ? "Alexandria" : gov,
                City = isDemo ? "Alexandria" : gov,
                DistrictArea = isDemo ? "Smouha" : $"{Rng.Next(1, 30)} Smouha St.",
                FullAddress = isDemo ? "12 Smouha St., Alexandria, Egypt" : $"{Rng.Next(1, 30)} Smouha St., {gov}, Egypt",
                Phone = isDemo ? "+20 12 3456 7890" : $"+20 1{Rng.Next(0, 2)} {Rng.Next(1000, 9999)} {Rng.Next(1000, 9999)}",
                Email = isDemo ? "pharmacy@alexdrugstore.com" : $"pharmacy@{name.Replace(" ", "").ToLower()}.com",
                DefaultWarehouseId = wh.Id == 0 ? null : wh.Id,
                DefaultWarehouse = wh,
                HasColdStorage = isDemo ? true : Rng.Next(0, 10) > 4,
                PharmacyLicenseNumber = $"PH-2024-{(i + 1):000}",
                LicenseIssueDate = RandDate(200, 900),
                LicenseExpiryDate = DateTime.UtcNow.AddDays(Rng.Next(-60, 900)),
                PharmacistSyndicateId = $"PS-{Rng.Next(100000, 999999)}",
                PharmacyStatus = isDemo ? FacilityStatus.Active : statuses[Rng.Next(statuses.Length)],
                CreatedAt = RandDate(100, 500),
                UpdatedAt = RandDate(0, 90)
            });
        }
        return list;
    }

    private static List<MedicineProduct> SeedProducts()
    {
        var list = new List<MedicineProduct>();
        foreach (var p in ProductNames)
        {
            list.Add(new MedicineProduct
            {
                ProductName = p,
                GTIN = $"0622210{Rng.Next(100000, 999999)}",
                DosageForm = DosageForms[Rng.Next(DosageForms.Length)],
                Strength = $"{Rng.Next(1, 500)} mg",
                RequiresColdChain = Rng.Next(0, 10) > 7,
                ProductStatus = "Active"
            });
        }
        return list;
    }

    private static List<Batch> SeedBatches(List<MedicineProduct> products, List<Factory> factories, List<SystemUser> users)
    {
        var list = new List<Batch>();
        var batchStatuses = new[] { BatchStatus.InSupplyChain, BatchStatus.InWarehouse, BatchStatus.InPharmacy, BatchStatus.Quarantined, BatchStatus.Recalled, BatchStatus.Available, BatchStatus.InProduction };
        var stages = new[] { SupplyChainStage.AtFactory, SupplyChainStage.InTransit, SupplyChainStage.Stored, SupplyChainStage.Available, SupplyChainStage.Quarantined, SupplyChainStage.Recalled };
        var locations = new[] { "Cairo Medical Storage", "Port Said Warehouse", "Alexandria Drug Store", "Assiut Central Warehouse", "Delta Warehouse", "Alex Warehouse", "Mansoura Pharmacy" };

        for (int i = 0; i < 150; i++)
        {
            var product = products[Rng.Next(products.Count)];
            var factory = factories[Rng.Next(factories.Count)];
            var status = batchStatuses[Rng.Next(batchStatuses.Length)];
            var qty = Rng.Next(1, 20) * 5000;
            var totalUnits = qty;
            var inWarehouse = Rng.Next(0, totalUnits / 2);
            var inPharmacy = Rng.Next(0, Math.Max(1, totalUnits - inWarehouse));
            list.Add(new Batch
            {
                BatchNumber = $"BAT-2024-{(i + 1):000}",
                MedicineProductId = product.Id == 0 ? null : product.Id,
                MedicineProduct = product,
                FactoryId = factory.Id == 0 ? null : factory.Id,
                Factory = factory,
                Quantity = qty,
                ManufacturingDate = RandDate(200, 600),
                ExpiryDate = DateTime.UtcNow.AddDays(Rng.Next(-30, 700)),
                BatchStatus = status,
                SupplyChainStage = stages[Rng.Next(stages.Length)],
                CurrentLocation = locations[Rng.Next(locations.Length)],
                CreatedBy = users[Rng.Next(users.Count)].Email,
                CreatedByUserId = users[Rng.Next(users.Count)].Id == 0 ? null : users[Rng.Next(users.Count)].Id,
                Notes = Rng.Next(0, 10) > 6 ? "Standard production run, no special handling notes." : null,
                CreatedAt = RandDate(30, 400),
                UpdatedAt = RandDate(0, 30),
                TotalUnitCodes = totalUnits,
                GeneratedUnitCodes = totalUnits,
                InWarehouseUnitCodes = inWarehouse,
                InPharmacyUnitCodes = inPharmacy,
                SuspiciousUnitCodes = Rng.Next(0, 15),
                BlockedUnitCodes = status == BatchStatus.Quarantined ? Rng.Next(50, 400) : Rng.Next(0, 20),
                RecalledUnitCodes = status == BatchStatus.Recalled ? totalUnits : 0,
                ScanCountTotal = Rng.Next(0, totalUnits),
                OpenAlertsCount = Rng.Next(0, 3)
            });
        }
        return list;
    }

    private static List<UnitCode> SeedUnitCodes(List<Batch> batches)
    {
        var list = new List<UnitCode>();
        var holderTypes = new[] { "Factory", "Warehouse", "Pharmacy" };
        var unitStatuses = new[] { UnitStatus.InWarehouse, UnitStatus.InPharmacy, UnitStatus.Generated, UnitStatus.Blocked, UnitStatus.Suspicious, UnitStatus.Recalled };
        foreach (var batch in batches.Where((_, idx) => idx % 2 == 0).Take(80))
        {
            for (int i = 0; i < 3; i++)
            {
                list.Add(new UnitCode
                {
                    UnitCodeValue = $"{batch.BatchNumber}-U{i:0000}",
                    SerialNumber = $"SN-{Rng.Next(100000000, 999999999)}",
                    GTIN = batch.MedicineProduct?.GTIN,
                    CodeValueHash = Convert.ToHexString(Guid.NewGuid().ToByteArray()).ToLower(),
                    ExpiryDate = batch.ExpiryDate,
                    BatchId = batch.Id == 0 ? null : batch.Id,
                    Batch = batch,
                    UnitStatus = unitStatuses[Rng.Next(unitStatuses.Length)],
                    CurrentHolderType = holderTypes[Rng.Next(holderTypes.Length)],
                    CurrentHolderName = "Cairo Medical Storage",
                    ScanCount = Rng.Next(0, 6),
                    FirstScannedAt = Rng.Next(0, 10) > 3 ? RandDate(0, 60) : null,
                    CreatedAt = RandDate(30, 300)
                });
            }
        }
        return list;
    }

    private static List<Shipment> SeedShipments(List<Batch> batches, List<Factory> factories, List<Warehouse> warehouses, List<Pharmacy> pharmacies)
    {
        var list = new List<Shipment>();
        var types = new[] { ShipmentType.FactoryToWarehouse, ShipmentType.WarehouseToPharmacy, ShipmentType.WarehouseToWarehouse };
        var statuses = new[] { ShipmentStatus.InTransit, ShipmentStatus.Received, ShipmentStatus.PartiallyReceived, ShipmentStatus.Rejected, ShipmentStatus.Cancelled, ShipmentStatus.PendingInspection };
        for (int i = 0; i < 120; i++)
        {
            var batch = batches[Rng.Next(batches.Count)];
            var type = types[Rng.Next(types.Length)];
            var status = statuses[Rng.Next(statuses.Length)];
            var expected = Rng.Next(1, 10) * 3000;
            var factory = factories[Rng.Next(factories.Count)];
            var srcWarehouse = warehouses[Rng.Next(warehouses.Count)];
            var destWarehouse = warehouses[Rng.Next(warehouses.Count)];
            var pharmacy = pharmacies[Rng.Next(pharmacies.Count)];
            var received = status is ShipmentStatus.Received ? expected
                : status == ShipmentStatus.PartiallyReceived ? Rng.Next(1, expected)
                : status == ShipmentStatus.Rejected ? 0
                : (long?)null;

            list.Add(new Shipment
            {
                TransferCode = $"TRF-2034-{(1100 + i)}",
                BatchId = batch.Id == 0 ? null : batch.Id,
                Batch = batch,
                ShipmentType = type,
                Source = type == ShipmentType.FactoryToWarehouse ? factory.OfficialFactoryName : srcWarehouse.OfficialWarehouseName,
                Destination = type == ShipmentType.WarehouseToPharmacy ? pharmacy.OfficialPharmacyName : destWarehouse.OfficialWarehouseName,
                SourceFactoryId = type == ShipmentType.FactoryToWarehouse ? factory.Id : null,
                SourceWarehouseId = type != ShipmentType.FactoryToWarehouse ? srcWarehouse.Id : null,
                DestinationWarehouseId = type != ShipmentType.WarehouseToPharmacy ? destWarehouse.Id : null,
                DestinationPharmacyId = type == ShipmentType.WarehouseToPharmacy ? pharmacy.Id : null,
                ExpectedQuantity = expected,
                ReceivedQuantity = received,
                ShipmentStatus = status,
                RequiresColdChain = batch.MedicineProduct?.RequiresColdChain ?? false,
                InspectionResult = status switch
                {
                    ShipmentStatus.Received => "Accepted",
                    ShipmentStatus.PartiallyReceived => "PartiallyAccepted",
                    ShipmentStatus.Rejected => "Rejected",
                    _ => null
                },
                DispatchDate = RandDate(0, 60),
                ReceivedDate = status is ShipmentStatus.Received or ShipmentStatus.PartiallyReceived or ShipmentStatus.Rejected ? RandDate(0, 30) : null
            });
        }
        return list;
    }

    private static List<InventoryStock> SeedInventory(List<Batch> batches, List<Warehouse> warehouses, List<Pharmacy> pharmacies)
    {
        var list = new List<InventoryStock>();
        var statuses = new[] { InventoryStatus.Active, InventoryStatus.Quarantined, InventoryStatus.Recalled, InventoryStatus.Blocked };
        for (int i = 0; i < 100; i++)
        {
            var batch = batches[Rng.Next(batches.Count)];
            var isWarehouse = Rng.Next(0, 2) == 0;
            var total = Rng.Next(5, 60) * 1000;
            var available = Rng.Next(0, total);
            var wh = warehouses[Rng.Next(warehouses.Count)];
            var ph = pharmacies[Rng.Next(pharmacies.Count)];
            list.Add(new InventoryStock
            {
                BatchId = batch.Id == 0 ? null : batch.Id,
                Batch = batch,
                HolderType = isWarehouse ? "Warehouse" : "Pharmacy",
                HolderName = isWarehouse ? wh.OfficialWarehouseName : ph.OfficialPharmacyName,
                WarehouseId = isWarehouse ? (wh.Id == 0 ? null : wh.Id) : null,
                PharmacyId = !isWarehouse ? (ph.Id == 0 ? null : ph.Id) : null,
                TotalReceivedQuantity = total,
                AvailableQuantity = available,
                ReservedQuantity = Rng.Next(0, Math.Max(1, total - available)),
                QuarantinedQuantity = Rng.Next(0, 1000),
                InventoryStatus = statuses[Rng.Next(statuses.Length)],
                LastUpdated = RandDate(0, 30)
            });
        }
        return list;
    }

    private static List<Alert> SeedAlerts(List<Batch> batches, List<Factory> factories, List<Warehouse> warehouses, List<Pharmacy> pharmacies)
    {
        var list = new List<Alert>();
        var types = new[] { AlertType.ColdChainIssue, AlertType.QuantityMismatch, AlertType.SuspiciousScan, AlertType.LicenseExpiry, AlertType.BlockedUnitScan, AlertType.ComplianceIssue, AlertType.Recall, AlertType.DuplicateSerial, AlertType.DamagedPackage };
        var severities = new[] { AlertSeverity.Low, AlertSeverity.Medium, AlertSeverity.High, AlertSeverity.Critical };
        var statuses = new[] { AlertStatus.Open, AlertStatus.UnderReview, AlertStatus.Resolved, AlertStatus.Dismissed };
        var entityKinds = new[] { EntityKind.Factory, EntityKind.Warehouse, EntityKind.Pharmacy };
        var messages = new[]
        {
            "Temperature excursion detected during transit.",
            "Received quantity does not match expected amount.",
            "Multiple scans from different governorates.",
            "Factory operating license will expire in 15 days.",
            "Blocked unit code scanned by public.",
            "Batch has been recalled by Ministry.",
            "Product quality issue - recall initiated.",
            "Safety issue reported - recall.",
            "Damaged boxes reported at warehouse."
        };
        for (int i = 0; i < 45; i++)
        {
            var batch = batches[Rng.Next(batches.Count)];
            var kind = entityKinds[Rng.Next(entityKinds.Length)];
            var entityName = kind switch
            {
                EntityKind.Factory => factories[Rng.Next(factories.Count)].OfficialFactoryName,
                EntityKind.Warehouse => warehouses[Rng.Next(warehouses.Count)].OfficialWarehouseName,
                _ => pharmacies[Rng.Next(pharmacies.Count)].OfficialPharmacyName
            };
            var status = statuses[Rng.Next(statuses.Length)];
            list.Add(new Alert
            {
                AlertCode = $"ALERT-2024-{(91 - i):0000}",
                AlertType = types[Rng.Next(types.Length)],
                Severity = severities[Rng.Next(severities.Length)],
                EntityType = kind,
                EntityName = entityName,
                BatchId = batch.Id == 0 ? null : batch.Id,
                Batch = batch,
                Message = messages[Rng.Next(messages.Length)],
                AlertStatus = status,
                CreatedAt = RandDate(0, 60),
                ResolvedAt = status is AlertStatus.Resolved or AlertStatus.Dismissed ? RandDate(0, 20) : null
            });
        }
        return list;
    }

    private static List<PublicVerificationScan> SeedScans(List<UnitCode> unitCodes, List<MedicineProduct> products, List<Batch> batches)
    {
        var list = new List<PublicVerificationScan>();
        var results = new[] { VerificationResult.Authentic, VerificationResult.NotFound, VerificationResult.DuplicateScan, VerificationResult.Recalled, VerificationResult.Expired, VerificationResult.Blocked, VerificationResult.Suspicious };
        var reasons = new[] { "Valid Product", "Serial number not registered in the system.", "Scanned multiple times in short time.", "Batch is recalled.", "Product expired.", "Blocked unit code.", "Multiple locations in short time." };
        for (int i = 0; i < 150; i++)
        {
            var hasUnit = Rng.Next(0, 10) > 2 && unitCodes.Count > 0;
            var unit = hasUnit ? unitCodes[Rng.Next(unitCodes.Count)] : null;
            var product = products[Rng.Next(products.Count)];
            var batch = batches[Rng.Next(batches.Count)];
            var gov = Governorates[Rng.Next(Governorates.Length)];
            list.Add(new PublicVerificationScan
            {
                ScanCode = $"SCAN-2024-{(15021 - i)}",
                ScannedGTIN = product.GTIN,
                ScannedSerialNumber = unit?.SerialNumber ?? "SN-UNKNOWN-001",
                ScannedBatchNumber = batch.BatchNumber,
                UnitCodeId = unit?.Id,
                UnitCode = unit,
                ProductName = hasUnit ? product.ProductName : null,
                VerificationResult = results[Rng.Next(results.Length)],
                Reason = reasons[Rng.Next(reasons.Length)],
                Governorate = gov,
                City = gov,
                ScannedAt = RandDate(0, 45)
            });
        }
        return list;
    }

    private static List<RegistrationRequest> SeedRegistrationRequests(List<SystemUser> users)
    {
        var list = new List<RegistrationRequest>();
        var statuses = new[] { RegistrationStatus.Pending, RegistrationStatus.UnderReview, RegistrationStatus.NeedsMoreDocuments, RegistrationStatus.Approved, RegistrationStatus.Rejected, RegistrationStatus.Cancelled };
        var kinds = new[] { EntityKind.Factory, EntityKind.Warehouse, EntityKind.Pharmacy };
        var docTypes = new[] { "Factory License Copy", "Commercial Registration", "Tax Card", "Technical License", "Authorization Letter", "Syndicate Card Copy" };
        var docStatuses = new[] { DocumentStatus.UnderReview, DocumentStatus.Complete, DocumentStatus.NeedsReplacement, DocumentStatus.Rejected };

        var entityNamesByKind = new Dictionary<EntityKind, string[]>
        {
            [EntityKind.Factory] = FactoryCompanyNames,
            [EntityKind.Warehouse] = WarehouseNames,
            [EntityKind.Pharmacy] = PharmacyNames
        };

        for (int i = 0; i < 60; i++)
        {
            var kind = kinds[Rng.Next(kinds.Length)];
            var name = entityNamesByKind[kind][Rng.Next(entityNamesByKind[kind].Length)];
            var rep = users[Rng.Next(users.Count)];
            var status = statuses[Rng.Next(statuses.Length)];
            var req = new RegistrationRequest
            {
                RequestCode = $"REQ-{(60 - i):0000}",
                EntityType = kind,
                EntityName = name,
                RepresentativeName = rep.FullName,
                Email = rep.Email,
                SubmittedAt = RandDate(0, 60),
                EmailConfirmed = Rng.Next(0, 10) > 2,
                DocumentsOverallStatus = docStatuses[Rng.Next(docStatuses.Length)],
                RegistrationStatus = status,
                AdminNotes = status == RegistrationStatus.NeedsMoreDocuments ? "Please provide an updated Authorization Letter on company letterhead." : null,
                RejectionReason = status == RegistrationStatus.Rejected ? "Documents did not meet regulatory requirements." : null,
                SystemUserId = rep.Id == 0 ? null : rep.Id,
                SystemUser = rep,
                Documents = new List<EntityDocument>()
            };
            var docCount = Rng.Next(3, 7);
            for (int d = 0; d < docCount; d++)
            {
                req.Documents.Add(new EntityDocument
                {
                    DocumentType = docTypes[d % docTypes.Length],
                    FileName = $"{docTypes[d % docTypes.Length].Replace(" ", "_").ToLower()}.pdf",
                    FileUrl = $"/files/documents/{Guid.NewGuid()}.pdf",
                    UploadedAt = RandDate(0, 55),
                    DocumentStatus = docStatuses[Rng.Next(docStatuses.Length)],
                    ReviewedBy = Rng.Next(0, 10) > 4 ? "Dr. Saif" : null,
                    ReviewedAt = Rng.Next(0, 10) > 4 ? RandDate(0, 40) : null,
                    RejectionReason = Rng.Next(0, 10) > 8 ? "Document unclear, please re-upload." : null
                });
            }
            list.Add(req);
        }
        return list;
    }

    private static List<EntityLicense> SeedLicenses(List<Factory> factories)
    {
        var list = new List<EntityLicense>();
        var genericTypes = new[] { "Manufacturing License", "GMP Certificate", "Environmental License", "Fire Safety License" };

        foreach (var factory in factories)
        {
            var isDemo = factory.OfficialFactoryName == "EIPICO Factory";
            if (isDemo)
            {
                list.Add(new EntityLicense { EntityType = EntityKind.Factory, EntityId = factory.Id, LicenseType = "Manufacturing License", LicenseNumber = "LIC-MFG-2024-001", IssueDate = new DateTime(2024, 5, 1), ExpiryDate = new DateTime(2026, 4, 30), Status = "Active" });
                list.Add(new EntityLicense { EntityType = EntityKind.Factory, EntityId = factory.Id, LicenseType = "GMP Certificate", LicenseNumber = "GMP-2024-022", IssueDate = new DateTime(2024, 4, 15), ExpiryDate = new DateTime(2026, 4, 14), Status = "Active" });
                list.Add(new EntityLicense { EntityType = EntityKind.Factory, EntityId = factory.Id, LicenseType = "Environmental License", LicenseNumber = "ENV-2024-015", IssueDate = new DateTime(2024, 3, 10), ExpiryDate = new DateTime(2026, 3, 9), Status = "Active" });
                list.Add(new EntityLicense { EntityType = EntityKind.Factory, EntityId = factory.Id, LicenseType = "Fire Safety License", LicenseNumber = "FIRE-2024-008", IssueDate = new DateTime(2024, 2, 20), ExpiryDate = new DateTime(2026, 2, 19), Status = "Active" });
                continue;
            }

            var count = Rng.Next(2, 5);
            foreach (var type in genericTypes.OrderBy(_ => Rng.Next()).Take(count))
            {
                var issue = RandDate(200, 600);
                list.Add(new EntityLicense
                {
                    EntityType = EntityKind.Factory,
                    EntityId = factory.Id,
                    LicenseType = type,
                    LicenseNumber = $"{type.Split(' ')[0].ToUpper().Substring(0, 3)}-{Rng.Next(2023, 2025)}-{Rng.Next(1, 999):000}",
                    IssueDate = issue,
                    ExpiryDate = issue.AddYears(2),
                    Status = Rng.Next(0, 10) > 1 ? "Active" : "Expired"
                });
            }
        }
        return list;
    }

    // Guarantees the fixed demo accounts (EIPICO Factory / Cairo Medical Storage / Alexandria Drug
    // Store) always have a rich, realistic set of batches/shipments/alerts to show - instead of
    // relying on chance from the general random seeding above. Runs once, after everything else,
    // using the real DB-assigned IDs.
    private static void SeedDemoOperationalData(AppDbContext db)
    {
        var factory = db.Factories.FirstOrDefault(f => f.OfficialFactoryName == "EIPICO Factory");
        var warehouse = db.Warehouses.FirstOrDefault(w => w.OfficialWarehouseName == "Cairo Medical Storage");
        var altWarehouse1 = db.Warehouses.FirstOrDefault(w => w.OfficialWarehouseName == "Delta Storage Warehouse");
        var altWarehouse2 = db.Warehouses.FirstOrDefault(w => w.OfficialWarehouseName == "Alex Warehouse");
        var pharmacy = db.Pharmacies.FirstOrDefault(p => p.OfficialPharmacyName == "Alexandria Drug Store");
        var altPharmacy = db.Pharmacies.FirstOrDefault(p => p.OfficialPharmacyName == "Mansoura Pharmacy");
        if (factory == null || warehouse == null) return;

        var demoProducts = new (string Name, string Gtin, string Dosage, string Strength, bool Cold)[]
        {
            ("Paracetamol 500mg", "06222100123456", "Tablet", "500mg", false),
            ("Amoxicillin 500mg", "06222100987654", "Capsule", "500mg", false),
            ("Ibuprofen 400mg", "06222100555544", "Tablet", "400mg", false),
            ("Ciprofloxacin 500mg", "06222100777711", "Tablet", "500mg", true),
            ("Metronidazole 500mg", "06222100333322", "Tablet", "500mg", false),
            ("Diclofenac 50mg", "06222100444455", "Tablet", "50mg", false),
            ("Azithromycin 500mg", "06222100666677", "Tablet", "500mg", true),
            ("Clarithromycin 250mg", "06222100888844", "Tablet", "250mg", false),
            ("Ofloxacin 200mg", "06222100111222", "Tablet", "200mg", false)
        };

        var products = new List<MedicineProduct>();
        foreach (var (name, gtin, dosage, strength, cold) in demoProducts)
        {
            var product = db.MedicineProducts.FirstOrDefault(p => p.GTIN == gtin);
            if (product == null)
            {
                product = new MedicineProduct { ProductName = name, GTIN = gtin, DosageForm = dosage, Strength = strength, RequiresColdChain = cold, ProductStatus = "Active" };
                db.MedicineProducts.Add(product);
                db.SaveChanges();
            }
            products.Add(product);
        }

        var batchPlans = new (string Number, BatchStatus Status, SupplyChainStage Stage, long Qty)[]
        {
            ("BAT-2024-001", BatchStatus.Registered, SupplyChainStage.AtFactory, 100000),
            ("BAT-2024-002", BatchStatus.ReadyForWarehouseDispatch, SupplyChainStage.AtFactory, 80000),
            ("BAT-2024-003", BatchStatus.Draft, SupplyChainStage.AtFactory, 60000),
            ("BAT-2024-004", BatchStatus.ReadyForWarehouseDispatch, SupplyChainStage.AtFactory, 120000),
            ("BAT-2024-005", BatchStatus.Registered, SupplyChainStage.AtFactory, 90000),
            ("BAT-2024-006", BatchStatus.Draft, SupplyChainStage.AtFactory, 50000),
            ("BAT-2024-007", BatchStatus.Quarantined, SupplyChainStage.Quarantined, 70000),
            ("BAT-2024-008", BatchStatus.Recalled, SupplyChainStage.Recalled, 40000),
            ("BAT-2024-009", BatchStatus.Expired, SupplyChainStage.AtFactory, 30000)
        };

        var batches = new List<Batch>();
        for (int i = 0; i < batchPlans.Length; i++)
        {
            var (number, status, stage, qty) = batchPlans[i];
            var existing = db.Batches.FirstOrDefault(b => b.BatchNumber == number && b.FactoryId == factory.Id);
            if (existing != null) { batches.Add(existing); continue; }

            var codesGenerated = status != BatchStatus.Draft;
            var batch = new Batch
            {
                MedicineProductId = products[i].Id,
                MedicineProduct = products[i],
                FactoryId = factory.Id,
                Factory = factory,
                BatchNumber = number,
                Quantity = qty,
                ManufacturingDate = RandDate(60, 200),
                ExpiryDate = status == BatchStatus.Expired ? DateTime.UtcNow.AddDays(-30) : DateTime.UtcNow.AddDays(Rng.Next(200, 700)),
                BatchStatus = status,
                SupplyChainStage = stage,
                CurrentLocation = factory.OfficialFactoryName,
                CreatedBy = "ahmed.ali@eipico.com",
                CreatedAt = RandDate(30, 90),
                UpdatedAt = RandDate(0, 20),
                TotalUnitCodes = codesGenerated ? qty : 0,
                GeneratedUnitCodes = codesGenerated ? qty : 0,
                InWarehouseUnitCodes = 0,
                InPharmacyUnitCodes = 0,
                SuspiciousUnitCodes = 0,
                BlockedUnitCodes = status == BatchStatus.Quarantined ? qty : 0,
                RecalledUnitCodes = status == BatchStatus.Recalled ? qty : 0,
                ScanCountTotal = 0,
                OpenAlertsCount = status is BatchStatus.Quarantined or BatchStatus.Recalled ? 1 : 0
            };
            db.Batches.Add(batch);
            db.SaveChanges();
            batches.Add(batch);
        }

        // Shipments: factory -> warehouses (spread across Cairo Medical Storage + two alternates)
        var shipmentPlans = new (int BatchIdx, Warehouse? Dest, ShipmentStatus Status, long Expected, long? Received)[]
        {
            (1, warehouse, ShipmentStatus.InTransit, 80000, null),
            (0, altWarehouse1, ShipmentStatus.Received, 100000, 100000),
            (2, altWarehouse2, ShipmentStatus.PartiallyReceived, 60000, 30000),
            (3, altWarehouse1, ShipmentStatus.InTransit, 120000, null),
            (4, warehouse, ShipmentStatus.Received, 90000, 90000),
            (5, altWarehouse1, ShipmentStatus.Rejected, 50000, 0),
            (6, altWarehouse2, ShipmentStatus.Received, 70000, 70000),
            (7, altWarehouse1, ShipmentStatus.PartiallyReceived, 40000, 20000),
            (8, warehouse, ShipmentStatus.Cancelled, 30000, null)
        };

        var demoShipments = new List<Shipment>();
        for (int i = 0; i < shipmentPlans.Length; i++)
        {
            var (batchIdx, dest, status, expected, received) = shipmentPlans[i];
            if (dest == null) continue;
            var code = $"TRF-2024-0{170 + i}";
            if (db.Shipments.Any(s => s.TransferCode == code)) continue;

            var batch = batches[batchIdx];
            var shipment = new Shipment
            {
                TransferCode = code,
                BatchId = batch.Id,
                Batch = batch,
                ShipmentType = ShipmentType.FactoryToWarehouse,
                Source = factory.OfficialFactoryName,
                Destination = dest.OfficialWarehouseName,
                SourceFactoryId = factory.Id,
                DestinationWarehouseId = dest.Id,
                ExpectedQuantity = expected,
                ReceivedQuantity = received,
                ShipmentStatus = status,
                RequiresColdChain = batch.MedicineProduct?.RequiresColdChain ?? false,
                DispatchDate = RandDate(0, 20),
                ReceivedDate = status is ShipmentStatus.Received or ShipmentStatus.PartiallyReceived ? RandDate(0, 10) : null
            };
            db.Shipments.Add(shipment);
            demoShipments.Add(shipment);

            if (status is ShipmentStatus.Received or ShipmentStatus.PartiallyReceived && received.HasValue)
            {
                db.InventoryStocks.Add(new InventoryStock
                {
                    BatchId = batch.Id,
                    HolderType = "Warehouse",
                    HolderName = dest.OfficialWarehouseName,
                    WarehouseId = dest.Id,
                    TotalReceivedQuantity = received.Value,
                    AvailableQuantity = received.Value,
                    ReservedQuantity = 0,
                    QuarantinedQuantity = 0,
                    InventoryStatus = InventoryStatus.Active,
                    LastUpdated = RandDate(0, 10)
                });
            }
        }
        db.SaveChanges();

        // A couple of warehouse -> pharmacy shipments so the "Outgoing to Pharmacy" tab has data too.
        if (pharmacy != null)
        {
            var wh1Stock = db.InventoryStocks.FirstOrDefault(i => i.WarehouseId == warehouse.Id);
            if (wh1Stock != null && !db.Shipments.Any(s => s.TransferCode == "DSP-2024-0088"))
            {
                db.Shipments.Add(new Shipment
                {
                    TransferCode = "DSP-2024-0088",
                    BatchId = wh1Stock.BatchId,
                    ShipmentType = ShipmentType.WarehouseToPharmacy,
                    Source = warehouse.OfficialWarehouseName,
                    Destination = pharmacy.OfficialPharmacyName,
                    SourceWarehouseId = warehouse.Id,
                    DestinationPharmacyId = pharmacy.Id,
                    ExpectedQuantity = 20000,
                    ReceivedQuantity = 20000,
                    ShipmentStatus = ShipmentStatus.Received,
                    RequiresColdChain = false,
                    DispatchDate = RandDate(0, 15),
                    ReceivedDate = RandDate(0, 10)
                });
                db.InventoryStocks.Add(new InventoryStock
                {
                    BatchId = wh1Stock.BatchId,
                    HolderType = "Pharmacy",
                    HolderName = pharmacy.OfficialPharmacyName,
                    PharmacyId = pharmacy.Id,
                    TotalReceivedQuantity = 20000,
                    AvailableQuantity = 20000,
                    ReservedQuantity = 0,
                    QuarantinedQuantity = 0,
                    InventoryStatus = InventoryStatus.Active,
                    LastUpdated = RandDate(0, 10)
                });
            }
            if (altPharmacy != null && wh1Stock != null && !db.Shipments.Any(s => s.TransferCode == "DSP-2024-0091"))
            {
                db.Shipments.Add(new Shipment
                {
                    TransferCode = "DSP-2024-0091",
                    BatchId = wh1Stock.BatchId,
                    ShipmentType = ShipmentType.WarehouseToPharmacy,
                    Source = warehouse.OfficialWarehouseName,
                    Destination = altPharmacy.OfficialPharmacyName,
                    SourceWarehouseId = warehouse.Id,
                    DestinationPharmacyId = altPharmacy.Id,
                    ExpectedQuantity = 15000,
                    ReceivedQuantity = null,
                    ShipmentStatus = ShipmentStatus.InTransit,
                    RequiresColdChain = false,
                    DispatchDate = RandDate(0, 5)
                });
            }
            db.SaveChanges();
        }

        // Alerts tied specifically to the demo factory / warehouse so the Alerts pages aren't empty.
        if (!db.Alerts.Any(a => a.EntityName == factory.OfficialFactoryName && a.AlertCode == "ALERT-2024-0091"))
        {
            db.Alerts.AddRange(
                new Alert { AlertCode = "ALERT-2024-0091", AlertType = AlertType.Recall, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[1].Id, ShipmentId = demoShipments.ElementAtOrDefault(0)?.Id, Message = "This batch has been recalled by Ministry due to quality issue.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 5) },
                new Alert { AlertCode = "ALERT-2024-0090", AlertType = AlertType.ColdChainIssue, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[3].Id, ShipmentId = demoShipments.ElementAtOrDefault(3)?.Id, Message = "Temperature excursion detected during transit.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 6) },
                new Alert { AlertCode = "ALERT-2024-0089", AlertType = AlertType.QuantityMismatch, Severity = AlertSeverity.Medium, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[2].Id, ShipmentId = demoShipments.ElementAtOrDefault(2)?.Id, Message = "Received quantity does not match expected quantity.", AlertStatus = AlertStatus.UnderReview, CreatedAt = RandDate(0, 7) },
                new Alert { AlertCode = "ALERT-2024-0088", AlertType = AlertType.ComplianceIssue, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[0].Id, ShipmentId = demoShipments.ElementAtOrDefault(1)?.Id, Message = "GMP compliance document is missing.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 8) },
                new Alert { AlertCode = "ALERT-2024-0087", AlertType = AlertType.LicenseExpiry, Severity = AlertSeverity.Low, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, Message = "Factory license will expire in 12 days.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 9) },
                new Alert { AlertCode = "ALERT-2024-0086", AlertType = AlertType.ExpiredBatch, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[4].Id, Message = "Batch has expired.", AlertStatus = AlertStatus.Resolved, CreatedAt = RandDate(10, 15), ResolvedAt = RandDate(0, 9) },
                new Alert { AlertCode = "ALERT-2024-0085", AlertType = AlertType.DocumentMissing, Severity = AlertSeverity.Medium, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[5].Id, Message = "COA document is missing for this batch.", AlertStatus = AlertStatus.UnderReview, CreatedAt = RandDate(0, 10) },
                new Alert { AlertCode = "ALERT-2024-0084", AlertType = AlertType.Recall, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[6].Id, Message = "Batch recalled due to contamination risk.", AlertStatus = AlertStatus.Open, CreatedAt = RandDate(0, 11) },
                new Alert { AlertCode = "ALERT-2024-0083", AlertType = AlertType.ComplianceIssue, Severity = AlertSeverity.High, EntityType = EntityKind.Factory, EntityName = factory.OfficialFactoryName, BatchId = batches[8].Id, Message = "Stability study report is missing.", AlertStatus = AlertStatus.Dismissed, CreatedAt = RandDate(12, 18), ResolvedAt = RandDate(0, 11) }
            );
            db.SaveChanges();
        }
    }

    private static List<AuditLog> SeedAuditLogs(List<SystemUser> users)
    {
        var list = new List<AuditLog>();
        var actions = Enum.GetValues<AuditAction>();
        var resourceTypes = new[] { "Batch", "Factory", "Warehouse", "Pharmacy", "EntityDocument", "RegistrationRequest", "SystemUser", "Alert" };
        var oldVals = new[] { "Active", "Pending", "Under Review", "In Supply Chain" };
        var newVals = new[] { "Suspended", "Approved", "Quarantined", "Recalled", "Rejected" };
        var results = new[]
        {
            AuditResult.Success, AuditResult.Success, AuditResult.Success, AuditResult.Success,
            AuditResult.Success, AuditResult.Success, AuditResult.Success, AuditResult.Success,
            AuditResult.Warning, AuditResult.Failed
        };
        for (int i = 0; i < 120; i++)
        {
            var user = users[Rng.Next(users.Count)];
            list.Add(new AuditLog
            {
                LogCode = $"LOG-2024-{(55678 - i)}",
                UserId = user.Id == 0 ? null : user.Id,
                User = user,
                UserDisplayName = user.FullName,
                Role = user.Role,
                Action = (AuditAction)actions.GetValue(Rng.Next(actions.Length))!,
                ResourceType = resourceTypes[Rng.Next(resourceTypes.Length)],
                ResourceId = $"{resourceTypes[Rng.Next(resourceTypes.Length)].ToUpper().Substring(0, 3)}-2024-{Rng.Next(1, 999):000}",
                OldValue = oldVals[Rng.Next(oldVals.Length)],
                NewValue = newVals[Rng.Next(newVals.Length)],
                Result = results[Rng.Next(results.Length)],
                IpAddress = $"{Rng.Next(41, 197)}.{Rng.Next(1, 255)}.{Rng.Next(1, 255)}.{Rng.Next(1, 255)}",
                CreatedAt = RandDate(0, 60)
            });
        }
        return list;
    }
}
