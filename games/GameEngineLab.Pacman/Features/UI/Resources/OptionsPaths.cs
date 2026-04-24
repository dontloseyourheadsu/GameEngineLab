using System.IO;

namespace GameEngineLab.Pacman.Features.UI.Resources;

public static class OptionsPaths
{
    public static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "Data", "settings.json");
}
