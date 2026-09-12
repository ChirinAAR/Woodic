using System;
using System.IO;
using System.Text.Json;

namespace Woodic.Services
{
    public class UserSettings
    {
        public bool IsDarkTheme { get; set; } = true;
    }

    public static class SettingsService
    {
        private static string GetSettingsFilePath()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(baseDir, "woodic_settings.json");
            }
            catch
            {
                string appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Woodic"
                );
                if (!Directory.Exists(appData))
                {
                    Directory.CreateDirectory(appData);
                }
                return Path.Combine(appData, "woodic_settings.json");
            }
        }

        public static UserSettings LoadSettings()
        {
            try
            {
                string filePath = GetSettingsFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var settings = JsonSerializer.Deserialize<UserSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar configuración: {ex.Message}");
            }

            return new UserSettings { IsDarkTheme = true };
        }

        public static void SaveSettings(UserSettings settings)
        {
            try
            {
                string filePath = GetSettingsFilePath();
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar configuración: {ex.Message}");
            }
        }
    }
}
