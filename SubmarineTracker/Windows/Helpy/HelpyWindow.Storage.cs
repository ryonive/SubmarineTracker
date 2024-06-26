using SubmarineTracker.Data;

namespace SubmarineTracker.Windows.Helpy;

public partial class HelpyWindow
{
    private static readonly Vector2 IconSize = new(32, 32);

    private void StorageTab()
    {
        if (ImGui.BeginTabItem($"{Loc.Localize("Helpy Tab - Storage", "Storage")}##Storage"))
        {
            ImGuiHelpers.ScaledDummy(5.0f);
            if (!Plugin.AllaganToolsConsumer.IsAvailable)
            {
                ImGui.TextColored(ImGuiColors.ParsedOrange, Loc.Localize("Helpy Tab Warning - AllaganTools", "AllaganTools not available."));
                ImGui.EndTabItem();

                return;
            }

            // build cache if needed
            Storage.BuildStorageCache();

            foreach (var (key, fc) in Plugin.DatabaseCache.GetFreeCompanies())
            {
                ImGui.TextColored(ImGuiColors.DalamudViolet, $"{Plugin.NameConverter.GetName(fc)}:");

                ImGuiHelpers.ScaledIndent(10.0f);
                if (ImGui.BeginTable($"##submarineOverview##{key}", 3))
                {
                    ImGui.TableSetupColumn("##icon", 0, 0.1f);
                    ImGui.TableSetupColumn("##count", 0, 0.15f);
                    ImGui.TableSetupColumn("##item");

                    foreach (var cached in Storage.StorageCache[key].Values)
                    {
                        ImGui.TableNextColumn();
                        Helper.DrawScaledIcon(cached.Item.Icon, IconSize);
                        ImGui.TableNextColumn();
                        var count = $"{cached.Count}x";
                        var width = ImGui.CalcTextSize(count).X;
                        var space = ImGui.GetContentRegionAvail().X;
                        ImGui.SameLine(space-width);
                        ImGui.TextUnformatted($"{cached.Count}x");
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(Utils.ToStr(cached.Item.Name));
                        ImGui.TableNextRow();
                    }

                    ImGui.EndTable();
                }
                ImGuiHelpers.ScaledIndent(-10.0f);

                ImGuiHelpers.ScaledDummy(10.0f);
            }
        }
        ImGui.EndTabItem();
    }
}
