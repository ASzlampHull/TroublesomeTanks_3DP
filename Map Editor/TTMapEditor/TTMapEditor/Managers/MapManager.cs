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
    /// <summary>
    /// Provides helper functionality for creating and managing map files
    /// used by the map editor. Handles safe naming, root folder discovery,
    /// and JSON serialization of new maps.
    /// </summary>
    public static class MapManager
    {
        /// <summary>
        /// Fallback name used when a provided map name is null, empty, or
        /// sanitizes to an empty string.
        /// </summary>
        private const string DefaultMapName = "Unnamed";

        /// <summary>
        /// Creates a new, empty map JSON file with a unique file name and
        /// returns the resulting map name (without extension).
        /// </summary>
        /// <param name="pMapName">
        /// User-provided name for the map. May contain invalid filename
        /// characters or path components; these will be sanitized.
        /// </param>
        /// <returns>
        /// The unique map name actually used for the file (no .json extension),
        /// including any numeric suffix appended to avoid collisions.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="pMapName"/> is null, empty, or whitespace.
        /// </exception>
        public static string createNewMap(string pMapName)
        {
            if (string.IsNullOrWhiteSpace(pMapName))
            {
                throw new ArgumentException("map name must not be empty", nameof(pMapName));
            }

            // Use only the file-name portion (prevent creation of directories if caller passed a path).
            string rawName = Path.GetFileName(pMapName);

            // Make a safe file name that can be used on the file system.
            string safeBase = SanitizeFileName(rawName);

            // Prefer a nearby "Maps" root if one already exists; otherwise use ./Maps under the current directory.
            string mapsRoot = FindMapsRoot(Environment.CurrentDirectory) ?? Path.Combine(Environment.CurrentDirectory, "Maps");

            // Ensure the maps root directory exists.
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

            // Create default MapData and write a single JSON file (no per-map folder structure).
            MapData newMap = new MapData
            {
                Walls = new List<WallData>(),
                Tanks = new List<TankData>(),
                Pickups = new List<PickupData>()
            };

            string json = JsonSerializer.Serialize(newMap, new JsonSerializerOptions { WriteIndented = true });

            // Persist the new, empty map to disk.
            File.WriteAllText(mapFilePath, json);

            // Return the created map base name (without extension).
            return uniqueName;
        }

        /// <summary>
        /// Searches upwards from a starting directory for a folder named "Maps".
        /// Limits traversal to <paramref name="maxLevels"/> ancestor levels.
        /// </summary>
        /// <param name="startDirectory">Directory from which to start the search.</param>
        /// <param name="maxLevels">Maximum number of parent levels to search.</param>
        /// <returns>
        /// The full path to the discovered "Maps" folder, or <c>null</c> if none is found.
        /// </returns>
        private static string? FindMapsRoot(string startDirectory, int maxLevels = 6)
        {
            DirectoryInfo? dir = new DirectoryInfo(startDirectory);
            for (int i = 0; i < maxLevels && dir != null; i++)
            {
                string candidate = Path.Combine(dir.FullName, "Maps");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                dir = dir.Parent;
            }

            return null;
        }

        /// <summary>
        /// Produces a file-system-safe filename from an arbitrary string by
        /// replacing invalid characters with underscores and trimming whitespace.
        /// If the final result is empty, <see cref="DefaultMapName"/> is used.
        /// </summary>
        /// <param name="name">Original user-provided name.</param>
        /// <returns>A sanitized filename-safe string.</returns>
        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return DefaultMapName;
            }

            char[] invalid = Path.GetInvalidFileNameChars();
            StringBuilder sb = new StringBuilder(name.Length);

            foreach (char c in name)
            {
                // Replace invalid path characters with underscore to avoid IO errors.
                sb.Append(invalid.Contains(c) ? '_' : c);
            }

            string sanitized = sb.ToString().Trim();
            return string.IsNullOrWhiteSpace(sanitized) ? DefaultMapName : sanitized;
        }
    }

    /// <summary>
    /// Serializable container for all map content (walls, tanks, and pickups).
    /// This is the format persisted to and loaded from map JSON files.
    /// </summary>
    public class MapData
    {
        public List<WallData> Walls { get; set; }

        public List<TankData> Tanks { get; set; }

        public List<PickupData> Pickups { get; set; }
    }

    /// <summary>
    /// Describes a single wall segment in the map, including its texture,
    /// position, size, and rotation.
    /// </summary>
    public class WallData
    {

        public string Texture { get; set; }

        public string[] Position { get; set; }

        public string[] Size { get; set; }

        public string Rotation { get; set; }
    }

    /// <summary>
    /// Represents a tank spawn point in the map, including its position
    /// and facing rotation.
    /// </summary>
    public class TankData
    {
        public string[] Position { get; set; }

        public string Rotation { get; set; }
    }

    /// <summary>
    /// Represents a pickup instance in the map, including where it is placed
    /// and which pickup types are active at this location.
    /// </summary>
    public class PickupData
    {
        public string[] Position { get; set; }

        public Dictionary<PickupType, bool> ActivatedPickups { get; set; }
    }
}
