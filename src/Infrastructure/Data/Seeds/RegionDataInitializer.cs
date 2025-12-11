using System.Text.Json;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Regions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Seeds;

public class RegionDataInitializer
{
    public static async Task InitializeAsync(IServiceProvider provider)
    {
        IEfUnitOfWork unitOfWork = provider.GetRequiredService<IEfUnitOfWork>();
        ILogger logger = provider.GetRequiredService<ILogger<RegionDataInitializer>>();

        if (
            await unitOfWork.Repository<Province>().AnyAsync()
            && await unitOfWork.Repository<District>().AnyAsync()
            && await unitOfWork.Repository<Commune>().AnyAsync()
        )
        {
            return;
        }

        // bin/Debug/{net_version}/
        string path = AppContext.BaseDirectory;
        string fullPath = Path.Combine(path, "Data", "Seeds");

        string provinceFilePath = Path.Combine(fullPath, "Provinces.json");
        IEnumerable<Province>? provinces = Read<Province>(provinceFilePath);
        await unitOfWork.Repository<Province>().AddRangeAsync(provinces ?? []);

        string districtFilePath = Path.Combine(fullPath, "Districts.json");
        IEnumerable<District>? districts = Read<District>(districtFilePath);
        await unitOfWork.Repository<District>().AddRangeAsync(districts ?? []);

        string communeFilePath = Path.Combine(fullPath, "Wards.json");
        IEnumerable<Commune>? communes = Read<Commune>(communeFilePath);
        await unitOfWork.Repository<Commune>().AddRangeAsync(communes ?? []);
        logger.LogInformation("Seeding region data has finished....");

        await unitOfWork.SaveChangesAsync();
    }

    private static List<T>? Read<T>(string path)
        where T : class
    {
        using FileStream json = File.OpenRead(path);
        return JsonSerializer.Deserialize<List<T>>(json);
    }
}
