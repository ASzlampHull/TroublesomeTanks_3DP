using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TTMapEditor.Objects;

namespace TTMapEditor.Managers
{
    public static class MapManager
    {

        private const string DefaultMapName = "Unnamed";

        public static string createNewMap(string pMapName)
        {
            if (string.IsNullOrWhiteSpace(pMapName))
                throw new ArgumentException("map name must not be empty", nameof(pMapName));

            // Use only the file-name portion (prevent creation of directories if caller passed a path)
            string rawName = Path.GetFileName(pMapName);
            // make a safe file name
            string safeBase = SanitizeFileName(rawName);

            // Prefer configured maps root if available (fallback to upward search or ./Maps)
            string mapsRoot = FindMapsRoot(Environment.CurrentDirectory) ?? Path.Combine(Environment.CurrentDirectory, "Maps");

            // Ensure the maps root exists
            if (!Directory.Exists(mapsRoot))
            {
                Directory.CreateDirectory(mapsRoot);
            }

            // Strip an existing " (n)" suffix from the requested name so we always start from the base name.
            string baseName = Regex.Match(safeBase, @"^(.*?)(?: \(\d+\))?$").Groups[1].Value;

            // Find a unique file name by appending " (n)" when necessary.
            int suffix = 0;
            string uniqueName = baseName;
            string mapFilePath = Path.Combine(mapsRoot, $"{uniqueName}.json");
            while (File.Exists(mapFilePath))
            {
                suffix++;
                uniqueName = $"{baseName} ({suffix})";
                mapFilePath = Path.Combine(mapsRoot, $"{uniqueName}.json");
            }

            // Create default MapData and write single JSON file (no folder)
            MapData newMap = new MapData()
            {
                Walls = new List<WallData>(),
                Tanks = new List<TankData>(),
                Pickups = new List<PickupData>()
            };

            string json = JsonSerializer.Serialize(newMap, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(mapFilePath, json);

            // Return the created map base name (without extension)
            return uniqueName;
        }

        // Search upward for a "Maps" folder (returns null if none found within N levels)
        private static string? FindMapsRoot(string startDirectory, int maxLevels = 6)
        {
            DirectoryInfo? dir = new DirectoryInfo(startDirectory);
            for (int i = 0; i < maxLevels && dir != null; i++)
            {
                string candidate = Path.Combine(dir.FullName, "Maps");
                if (Directory.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }
            return null;
        }

        // Replace invalid filename chars with underscore
        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return DefaultMapName;

            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                sb.Append(invalid.Contains(c) ? '_' : c);
            }

            string sanitized = sb.ToString().Trim();
            return string.IsNullOrWhiteSpace(sanitized) ? DefaultMapName : sanitized;
        }
    }

    public class MapData
    {
        public List<WallData> Walls { get; set; }

        public List<TankData> Tanks { get; set; }

        public List<PickupData> Pickups { get; set; }
    }

    public class WallData
    {
        public string Texture { get; set; }

        public string[] Position { get; set; }

        public string[] Size { get; set; }
        public string Rotation { get; set; }

    }

    public class TankData
    {
        public string[] Position { get; set; }

        public string Rotation { get; set; }

    }

    public class PickupData
    { 
        public string[] Position { get; set; }

        public Dictionary<PickupType,bool> ActivatedPickups { get; set; }
    }

}
