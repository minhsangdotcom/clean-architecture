using System.Text.Json;
using Application.Common.Interfaces.Seeder;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Regions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Seeders;

public class RegionSeeder(IEfUnitOfWork unitOfWork, ILogger<RegionSeeder> logger) : IDbSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (
            await unitOfWork.Repository<Province>().AnyAsync(cancellationToken: cancellationToken)
            && await unitOfWork
                .Repository<District>()
                .AnyAsync(cancellationToken: cancellationToken)
            && await unitOfWork.Repository<Commune>().AnyAsync(cancellationToken: cancellationToken)
        )
        {
            return;
        }

        string path = AppContext.BaseDirectory;
        string fullPath = Path.Combine(path, "Data", "Seeders", "Resources");

        string provinceFilePath = Path.Combine(fullPath, "Provinces.json");
        IEnumerable<Province>? provinces = Read<Province>(provinceFilePath);
        await unitOfWork.Repository<Province>().AddRangeAsync(provinces ?? [], cancellationToken);

        string districtFilePath = Path.Combine(fullPath, "Districts.json");
        IEnumerable<District>? districts = Read<District>(districtFilePath);
        await unitOfWork.Repository<District>().AddRangeAsync(districts ?? [], cancellationToken);

        string communeFilePath = Path.Combine(fullPath, "Wards.json");
        IEnumerable<Commune>? communes = Read<Commune>(communeFilePath);
        await unitOfWork.Repository<Commune>().AddRangeAsync(communes ?? [], cancellationToken);
        logger.LogInformation("Seeding region data has finished....");

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static List<T>? Read<T>(string path)
        where T : class
    {
        using FileStream json = File.OpenRead(path);
        return JsonSerializer.Deserialize<List<T>>(json);
    }
}
