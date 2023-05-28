using SubmarineTracker.Data;
using static SubmarineTracker.Utils;

namespace SubmarineTracker.Windows;

public partial class BuilderWindow
{
    private void BuildStats(ref Submarines.Submarine sub)
    {
        if (ImGui.BeginChild("SubStats", new Vector2(0, 0)))
        {
            var build = new Submarines.SubmarineBuild(CurrentBuild);

            // Reset to custom build if not equal anymore
            if (sub.IsValid() && !build.EqualsSubmarine(sub))
                CurrentBuild.OriginalSub = 0;

            var startPoint = ExplorationSheet.First(r => r.Map.Row == CurrentBuild.Map + 1);

            var optimizedPoints = CurrentBuild.OptimizedRoute.Prepend(startPoint).ToList();
            var optimizedDuration = Submarines.CalculateDuration(optimizedPoints, build);
            var breakpoints = LootTable.CalculateRequired(CurrentBuild.Sectors);
            var expPerMinute = 0.0;
            if (optimizedDuration != 0 && CurrentBuild.OptimizedDistance != 0)
                expPerMinute = CurrentBuild.OptimizedRoute.Select(p => p.ExpReward).Sum(exp => exp) / (optimizedDuration / 60.0);


            var windowWidth = ImGui.GetWindowWidth();
            var secondRow = windowWidth / 5.1f;
            var thirdRow = windowWidth / 2.9f;
            var fourthRow = windowWidth / 2.0f;
            var sixthRow = windowWidth / 1.5f;
            var seventhRow = windowWidth / 1.25f;

            ImGui.TextUnformatted("Optimized Route:");
            ImGui.SameLine();
            if (CurrentBuild.OptimizedRoute.Any())
            {
                ImGui.TextColored(ImGuiColors.DalamudOrange, string.Join(" -> ", CurrentBuild.OptimizedRoute.Where(p => p.RowId > startPoint.RowId).Select(p => NumToLetter(p.RowId - startPoint.RowId))));
            }
            else
            {
                ImGui.TextColored(ImGuiColors.DalamudOrange, "No Selection");
            }

            ImGui.TextUnformatted("Calculated Stats:");
            ImGui.TextColored(ImGuiColors.HealerGreen, $"Surveillance");
            ImGui.SameLine(secondRow);
            SelectRequiredColor(breakpoints.T2, build.Surveillance, breakpoints.T3);

            ImGui.SameLine(thirdRow);
            ImGui.TextColored(ImGuiColors.HealerGreen, $"Retrieval");
            ImGui.SameLine(fourthRow);
            SelectRequiredColor(breakpoints.Normal, build.Retrieval, breakpoints.Optimal);

            ImGui.SameLine(sixthRow);
            ImGui.TextColored(ImGuiColors.HealerGreen, $"Favor");
            ImGui.SameLine(seventhRow);
            SelectRequiredColor(breakpoints.Favor, build.Favor);

            ImGui.TextColored(ImGuiColors.HealerGreen, $"Speed");
            ImGui.SameLine(secondRow);
            ImGui.TextColored(ImGuiColors.HealerGreen, $"{build.Speed}");

            ImGui.SameLine(thirdRow);
            ImGui.TextColored(ImGuiColors.HealerGreen, $"Range");
            ImGui.SameLine(fourthRow);
            SelectRequiredColor(CurrentBuild.OptimizedDistance, build.Range);

            ImGui.SameLine(sixthRow);
            ImGui.TextColored(ImGuiColors.HealerGreen, $"Repair");
            ImGui.SameLine(seventhRow);
            ImGui.TextUnformatted($"{build.RepairCosts}");

            ImGui.TextColored(ImGuiColors.HealerGreen, $"Duration");
            ImGui.SameLine(secondRow);
            ImGui.TextUnformatted($"{ToTime(TimeSpan.FromSeconds(optimizedDuration))}");

            ImGui.SameLine(thirdRow);
            ImGui.TextColored(ImGuiColors.HealerGreen, $"Exp/Min");
            ImGui.SameLine(fourthRow);
            ImGui.TextUnformatted($"{expPerMinute:F}");
        }
        ImGui.EndChild();
    }

    public void SelectRequiredColor(int minRequired, int current, int maxRequired = -1)
    {
        if (minRequired == 0)
        {
            ImGui.TextUnformatted($"{current}");
        }
        else if (minRequired > current)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, $"{current} ({minRequired})");
        }
        else if (maxRequired == -1)
        {
            if (minRequired == current)
                ImGui.TextColored(ImGuiColors.HealerGreen, $"{current}");
            else
                ImGui.TextColored(ImGuiColors.ParsedGold, $"{current} ({minRequired})");
        }
        else
        {
            if (maxRequired == current)
                ImGui.TextColored(ImGuiColors.HealerGreen, $"{current}");
            else if (current >= minRequired && current < maxRequired)
                ImGui.TextColored(ImGuiColors.ParsedPink, $"{current} ({maxRequired})");
            else
                ImGui.TextColored(ImGuiColors.ParsedGold, $"{current} ({maxRequired})");
        }
    }
}
