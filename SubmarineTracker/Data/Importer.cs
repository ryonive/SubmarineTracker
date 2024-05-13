﻿// ReSharper disable FieldCanBeMadeReadOnly.Global
// MessagePack can't deserialize into readonly

using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Lumina.Excel.GeneratedSheets;
using MessagePack;

namespace SubmarineTracker.Data;

[MessagePackObject]
public class CalculatedData
{
    [Key(0)] public uint MaxSector;
    [Key(1)] public Dictionary<int, Route[]> Maps = [];
}

[MessagePackObject]
public struct Route
{
    [Key(0)] public uint Distance;
    [Key(1)] public uint[] Sectors;
}

public static class Importer
{
    public const string Filename = "CalculatedData.msgpack";

    public static CalculatedData CalculatedData = new();
    public static Dictionary<int, FrozenDictionary<int, Route>> HashedRoutes = new();

    static Importer()
    {
        Load();
        ImportDetailed();

        HashedRoutes.Clear();
        foreach (var (map, routes) in CalculatedData.Maps)
        {
            var dict = new Dictionary<int, Route>();
            foreach (var route in routes)
                dict.Add(Utils.GetUniqueHash(route.Sectors), route);

            HashedRoutes.Add(map, dict.ToFrozenDictionary());
        }
    }

    private static void Load()
    {
        try
        {
            using var fileStream = File.OpenRead(Path.Combine(Plugin.PluginDir, Filename));
            CalculatedData = MessagePackSerializer.Deserialize<CalculatedData>(fileStream);
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, "Failed loading calculated data.");
            CalculatedData = new CalculatedData();
        }
    }

    #region MessagePackCreation
    #if DEBUG
    public static void Export()
    {
        try
        {
            var dict = new Dictionary<int, Route[]>();
            foreach (var mapId in Plugin.Data.GetExcelSheet<SubmarineMap>()!.Where(m => m.RowId != 0).Select(m => m.RowId))
                dict.Add((int) mapId, Voyage.FindAllRoutes(mapId));

            CalculatedData = new CalculatedData {Maps = dict};

            var path = Path.Combine(Plugin.PluginDir, Filename);
            if (File.Exists(path))
                File.Delete(path);

            File.WriteAllBytes(path, MessagePackSerializer.Serialize(CalculatedData));
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e, "Can't build routes.");
        }
    }

    // ReSharper disable once UnusedType.Global
    public class SectorDetailed
    {
        [Name("Sector name")] public uint Sector { get; set; }
        [Name("T1 high surv proc")] public uint T1HighSurv { get; set; }
        [Name("T2 high surv proc")] public string T2HighSurv { get; set; }
        [Name("T3 high surv proc")] public string T3HighSurv { get; set; }
        [Name("Favor high surv proc")] public string HighFavor { get; set; }
        [Name("Favor proc chance")] public string MidFavor { get; set; }
        [Name("T1 mid surv proc")] public string T1MidSurv { get; set; }
        [Name("T2 mid surv proc")] public string T2MidSurv { get; set; }
    }

    // ReSharper disable once UnusedType.Global
    public class ItemDetailed
    {
        [Name("Sector name")] public string Sector { get; set; }
        [Name("Item name")] public string Item { get; set; }
        [Name("Loot tier")] public string Tier { get; set; }
        [Name("High surv drop chance")] public string T3SurvDrop { get; set; }
        [Name("High surv tier proc chance")] public string T3SurvTierDrop { get; set; }
        [Name("High surv drop chance (within tier)")] public string T3SurvDropTier { get; set; }
        [Name("Overall drop chance (within tier)")] public string OverallDropTier { get; set; }
        [Name("Poor min")] public string PoorMin { get; set; }
        [Name("Poor max")] public string PoorMax { get; set; }
        [Name("Normal min")] public string NormalMin { get; set; }
        [Name("Normal max")] public string NormalMax { get; set; }
        [Name("Optimal min")] public string OptimalMin { get; set; }
        [Name("Optimal max")] public string OptimalMax { get; set; }
    }

    public record ItemDetail(uint Sector, string Tier, string Poor, string Normal, string Optimal)
    {
        public ItemDetail(uint sector, ItemDetailed detailed) : this(sector, detailed.Tier, $"{detailed.PoorMin} - {detailed.PoorMax}", $"{detailed.NormalMin} - {detailed.NormalMax}", $"{detailed.OptimalMin} - {detailed.OptimalMax}") { }
    }

    public static readonly Dictionary<uint, List<ItemDetail>> ItemDetails = new();

    private const string ItemPath = "Items (detailed).csv";
    private const string SectorPath = "Sectors (detailed).csv";

    public static void ImportDetailed()
    {
        var itemSheet = Plugin.Data.GetExcelSheet<Item>()!;
        var subSheet = Plugin.Data.GetExcelSheet<SubmarineExploration>()!;

        using var reader = new FileInfo(Path.Combine(Plugin.PluginDir, "Resources", ItemPath)).OpenText();
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });
        foreach (var itemDetailed in csv.GetRecords<ItemDetailed>())
        {
            itemDetailed.Sector = itemDetailed.Sector switch
            {
                "The Lilac Sea 1" => "Lilac Sea 1",
                "The Lilac Sea 2" => "Lilac Sea 2",
                _ => itemDetailed.Sector
            };

            var itemRow = itemSheet.First(i => i.Name == itemDetailed.Item).RowId;
            var subRow = subSheet.First(s => string.Equals(Utils.UpperCaseStr(s.Destination), itemDetailed.Sector, StringComparison.InvariantCultureIgnoreCase)).RowId;

            var detail = new ItemDetail(subRow, itemDetailed.Tier,
                                        $"{itemDetailed.PoorMin} - {itemDetailed.PoorMax}",
                                        $"{itemDetailed.NormalMin} - {itemDetailed.NormalMax}",
                                        $"{itemDetailed.OptimalMin} - {itemDetailed.OptimalMax}");
            if (!ItemDetails.TryAdd(itemRow, [detail]))
                ItemDetails[itemRow].Add(detail);
        }
    }
    #endif
    #endregion
}
