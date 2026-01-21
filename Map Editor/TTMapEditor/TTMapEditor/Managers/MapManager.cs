using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TTMapEditor.Managers
{
    internal static class MapManager
    {
        public static string createNewMap(string pMapName)
        {
            if (string.IsNullOrWhiteSpace(pMapName))
                throw new ArgumentException("map name must not be empty", nameof(pMapName));

            // make a safe folder name
            pMapName = SanitizeFileName(pMapName);

            // Try to find an existing "Maps" folder up the directory tree (useful when running from bin/Debug)
            string mapsRoot = FindMapsRoot(Environment.CurrentDirectory) ?? Path.Combine(Environment.CurrentDirectory, "Maps");

            // Ensure the maps root exists
            if (!Directory.Exists(mapsRoot))
            {
                Directory.CreateDirectory(mapsRoot);
            }

            // Strip an existing " (n)" suffix from the requested name so we always start from the base name.
            // e.g. "MyMap (1)" -> "MyMap"
            string baseName = Regex.Match(pMapName, @"^(.*?)(?: \(\d+\))?$").Groups[1].Value;

            // Find a unique directory name by appending " (n)" when necessary.
            string mapDirectory;
            int suffix = 0;
            string uniqueName = baseName;
            mapDirectory = Path.Combine(mapsRoot, uniqueName);
            while (Directory.Exists(mapDirectory))
            {
                suffix++;
                uniqueName = $"{baseName} ({suffix})";
                mapDirectory = Path.Combine(mapsRoot, uniqueName);
            }

            // Create directory and default map.json
            Directory.CreateDirectory(mapDirectory);

            MapData newMap = new MapData()
            {
                Walls = new List<WallData>(),
                Tanks = new List<TankData>(),
                Pickups = new List<PickupData>()
            };

            string json = JsonSerializer.Serialize(newMap, new JsonSerializerOptions() { WriteIndented = true });
            string mapFilePath = Path.Combine(mapDirectory, "map.json");
            File.WriteAllText(mapFilePath, json);

            // Return the created map folder name (not the full path)
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
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                sb.Append(invalid.Contains(c) ? '_' : c);
            }
            return sb.ToString().Trim();
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

    }

    public class TankData
    {
        public string[] Position { get; set; }

        public string Rotation { get; set; }

    }

    public class PickupData
    { 
        public string[] Position { get; set; }
    }

}
