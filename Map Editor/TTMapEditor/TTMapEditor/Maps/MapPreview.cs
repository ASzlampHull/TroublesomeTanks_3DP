using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TTMapEditor.Managers;
using TTMapEditor.Objects;

namespace TTMapEditor.Maps
{
    enum ObjectType
    {
        Wall,
        Tank,
        Pickup
    }


    internal class MapPreview
    {
        Rectangle mPlayArea { get; set; }
        List<RectWall> mWalls { get; set; }

        List<Tank> mTanks { get; set; }

        List<Pickup> mPickups { get; set; }

        string mFilePath { get; set; }

        // store the raw MapData that came from the JSON (or constructed from the legacy format)
        MapData mMapData { get; set; }


        public MapPreview(string pFilePath)
        {
            int screenWidth = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width;
            int screenHeight = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height;
            mPlayArea = new Rectangle(screenWidth * 2 / 100, screenHeight * 25 / 100, screenWidth * 96 / 100, screenHeight * 73 / 100);
            mFilePath = pFilePath;

            // initialize lists
            mWalls = new List<RectWall>();
            mTanks = new List<Tank>();
            mPickups = new List<Pickup>();

            // Fill lists from file and keep MapData
            LoadMapPreview();
        }

        public Rectangle GetPlayArea()
        {
            return mPlayArea;
        }

        public List<RectWall> GetWalls()
        {
            return mWalls;
        }

        public List<Tank> GetTanks()
        {
            return mTanks;
        }

        public List<Pickup> GetPickups()
        {
            return mPickups;
        }

        // Expose the MapData that represents the map (use to save / edit)
        public MapData GetMapData()
        {
            return mMapData;
        }

        private void LoadMapPreview()
        {
            // Prefer the bin (runtime) output folder first
            string baseDir = AppContext.BaseDirectory; // points to bin/... where the app runs
            string mapsDir = Path.Combine(baseDir, "Maps");

            // If there's no Maps folder in the bin, try to find one upward (useful when running from IDE)
            if (!Directory.Exists(mapsDir))
            {
                DirectoryInfo? dir = new DirectoryInfo(baseDir);
                for (int i = 0; i < 6 && dir != null; i++)
                {
                    string candidate = Path.Combine(dir.FullName, "Maps");
                    if (Directory.Exists(candidate))
                    {
                        mapsDir = candidate;
                        break;
                    }
                    dir = dir.Parent;
                }
            }

            string fullPath;

            // If the provided path is absolute and exists as a file, use it directly
            if (Path.IsPathRooted(mFilePath) && File.Exists(mFilePath))
            {
                fullPath = mFilePath;
            }
            else
            {
                // Remove a leading "Maps\" or "Maps/" from mFilePath if present so we don't duplicate it
                string relativePath = mFilePath;
                string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
                string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;
                if (relativePath.StartsWith(mapsPrefix1) || relativePath.StartsWith(mapsPrefix2))
                {
                    relativePath = relativePath.Substring(5);
                }

                fullPath = Path.Combine(mapsDir, relativePath);

                // If the path resolves to a directory (e.g. a map folder name), look for "map.json" inside it
                if (Directory.Exists(fullPath))
                {
                    string candidate = Path.Combine(fullPath, "map.json");
                    if (File.Exists(candidate))
                    {
                        fullPath = candidate;
                    }
                }
            }

            // As a final attempt, if the path doesn't point to an existing file, try appending "map.json"
            if (!File.Exists(fullPath))
            {
                string alt = fullPath;
                // If fullPath currently points to a directory, alt was already handled above; otherwise try file inside path
                if (!fullPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    alt = Path.Combine(fullPath, "map.json");
                }

                if (File.Exists(alt))
                {
                    fullPath = alt;
                }
                else
                {
                    throw new FileNotFoundException($"Map file not found: {fullPath}");
                }
            }

            string content = File.ReadAllText(fullPath);

            // try JSON first and remember the MapData used to build the preview
            try
            {
                mMapData = JsonSerializer.Deserialize<MapData>(content);
            }
            catch (JsonException)
            {
                mMapData = null;
            }

            if (mMapData != null)
            {
                // build preview lists from mMapData
                BuildPreviewFromMapData(mMapData);
                return;
            }

            // fallback: treat file as legacy line format and parse
            string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            mWalls = ParseLines(lines);

            // construct a MapData from the legacy parsing so callers can still get MapData
            mMapData = new MapData()
            {
                Walls = mWalls.Select(w =>
                {
                    // We cannot reliably extract the original texture name from Texture2D,
                    // so use a sensible default. If your RectWall stores the texture asset name,
                    // replace the line below to use that property instead.
                    string textureName = "block";

                    // convert back to percent-based Position / Size using play area
                    var rect = w.mRectangle;
                    float posX = (rect.X - mPlayArea.X) * 100.0f / mPlayArea.Width;
                    float posY = (rect.Y - mPlayArea.Y) * 100.0f / mPlayArea.Height;
                    float sizeX = rect.Width * 100.0f / mPlayArea.Width;
                    float sizeY = rect.Height * 100.0f / mPlayArea.Height;

                    return new WallData
                    {
                        Texture = textureName,
                        Position = new[] { posX.ToString(CultureInfo.InvariantCulture), posY.ToString(CultureInfo.InvariantCulture) },
                        Size = new[] { sizeX.ToString(CultureInfo.InvariantCulture), sizeY.ToString(CultureInfo.InvariantCulture) }
                    };
                }).ToList(),

                Tanks = new List<TankData>(),
                Pickups = new List<PickupData>()
            };
        }

        private void BuildPreviewFromMapData(MapData map)
        {
            mWalls = new List<RectWall>();
            mTanks = new List<Tank>();
            mPickups = new List<Pickup>();

            if (map.Walls != null)
            {
                foreach (var w in map.Walls)
                {
                    if (w == null) continue;
                    float posX = 0f, posY = 0f;
                    float sizeX = 0f, sizeY = 0f;
                    if (w.Position != null && w.Position.Length >= 2)
                    {
                        float.TryParse(w.Position[0], NumberStyles.Float, CultureInfo.InvariantCulture, out posX);
                        float.TryParse(w.Position[1], NumberStyles.Float, CultureInfo.InvariantCulture, out posY);
                    }
                    if (w.Size != null && w.Size.Length >= 2)
                    {
                        float.TryParse(w.Size[0], NumberStyles.Float, CultureInfo.InvariantCulture, out sizeX);
                        float.TryParse(w.Size[1], NumberStyles.Float, CultureInfo.InvariantCulture, out sizeY);
                    }

                    Vector2 position = new Vector2(
                        mPlayArea.X + ((float)mPlayArea.Width * (posX / 100.0f)),
                        mPlayArea.Y + ((float)mPlayArea.Height * (posY / 100.0f))
                    );
                    Vector2 size = new Vector2(
                        mPlayArea.Width * (sizeX / 100.0f),
                        mPlayArea.Height * (sizeY / 100.0f)
                    );

                    Texture2D tex;
                    try
                    {
                        tex = TTMapEditor.Instance().GetContentManager().Load<Texture2D>(w.Texture);
                    }
                    catch
                    {
                        tex = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("block");
                    }

                    mWalls.Add(new RectWall(tex, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y)));
                }
            }

            if (map.Tanks != null)
            {
                foreach (var t in map.Tanks)
                {
                    if (t?.Position == null || t.Position.Length < 2) continue;
                    float posX = 0f, posY = 0f;
                    float.TryParse(t.Position[0], NumberStyles.Float, CultureInfo.InvariantCulture, out posX);
                    float.TryParse(t.Position[1], NumberStyles.Float, CultureInfo.InvariantCulture, out posY);

                    Vector2 position = new Vector2(
                        mPlayArea.X + ((float)mPlayArea.Width * (posX / 100.0f)),
                        mPlayArea.Y + ((float)mPlayArea.Height * (posY / 100.0f))
                    );

                    // parse rotation if present (map stores degrees); default 0
                    float rotationDeg = 0f;
                    if (!string.IsNullOrEmpty(t.Rotation))
                    {
                        float.TryParse(t.Rotation, NumberStyles.Float, CultureInfo.InvariantCulture, out rotationDeg);
                    }
                    float rotationRad = MathHelper.ToRadians(rotationDeg);

                    // preview size and creation — your Tank preview constructor may differ; adapt as needed
                    int previewSize = 10;
                    Rectangle rect = new Rectangle((int)position.X - previewSize / 2, (int)position.Y - previewSize / 2, previewSize, previewSize);
                    Texture2D tex;
                    try { tex = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("block"); }
                    catch { tex = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("block"); }

                    var previewTank = new Tank(tex, rect);
                    previewTank.Rotation = rotationRad;
                    mTanks.Add(previewTank);
                }
            }

            if (map.Pickups != null)
            {
                foreach (var p in map.Pickups)
                {
                    if (p?.Position == null || p.Position.Length < 2) continue;
                    float posX = 0f, posY = 0f;
                    float.TryParse(p.Position[0], NumberStyles.Float, CultureInfo.InvariantCulture, out posX);
                    float.TryParse(p.Position[1], NumberStyles.Float, CultureInfo.InvariantCulture, out posY);

                    Vector2 position = new Vector2(
                        mPlayArea.X + ((float)mPlayArea.Width * (posX / 100.0f)),
                        mPlayArea.Y + ((float)mPlayArea.Height * (posY / 100.0f))
                    );

                    int previewSize = 9;
                    Rectangle rect = new Rectangle((int)position.X - previewSize / 2, (int)position.Y - previewSize / 2, previewSize, previewSize);
                    Texture2D tex;
                    try { tex = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("circle"); }
                    catch { tex = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("circle"); }
                    mPickups.Add(new Pickup(tex, rect));
                }
            }
        }

        // Backwards-compatible line-parsing (kept for legacy formats)
        public List<RectWall> ParseLines(string[] pLines)
        {
            List<RectWall> Walls = new List<RectWall>();

            string texture = null;
            Vector2 positionFallback = Vector2.Zero;
            Vector2 sizeFallback = Vector2.Zero;
            bool isWall = false;

            foreach (string line in pLines)
            {
                if (!isWall)
                {
                    texture = null;
                    positionFallback = Vector2.Zero;
                    sizeFallback = Vector2.Zero;
                }

                if (line.Contains("Walls") || line.StartsWith("wall"))
                {
                    isWall = true;
                    continue;
                }
                else if (line.Contains("Texture") || line.Contains("texture"))
                {
                    texture = line.Split('=')[1].Trim().Trim('"');
                    continue;
                }
                else if (line.Contains("Position") || line.Contains("position"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    positionFallback = new Vector2(float.Parse(components[0], CultureInfo.InvariantCulture), float.Parse(components[1], CultureInfo.InvariantCulture));
                    positionFallback.X = mPlayArea.X + ((float)mPlayArea.Width * (positionFallback.X / 100.0f));
                    positionFallback.Y = mPlayArea.Y + ((float)mPlayArea.Height * (positionFallback.Y / 100.0f));
                    continue;
                }
                else if (line.Contains("Size") || line.Contains("size"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    sizeFallback = new Vector2(float.Parse(components[0], CultureInfo.InvariantCulture), float.Parse(components[1], CultureInfo.InvariantCulture));
                    sizeFallback.X = mPlayArea.Width * (sizeFallback.X / 100.0f);
                    sizeFallback.Y = mPlayArea.Height * (sizeFallback.Y / 100.0f);
                    continue;
                }

                if (isWall)
                {
                    RectWall currentWall = new RectWall(
                        TTMapEditor.Instance().GetContentManager().Load<Texture2D>(texture),
                        new Rectangle((int)positionFallback.X, (int)positionFallback.Y, (int)sizeFallback.X, (int)sizeFallback.Y));
                    Walls.Add(currentWall);
                    isWall = false;
                }
            }
            return Walls;
        }

        public void AddObject(SceneObject pObject)
        {
            switch(pObject)
            {
                case RectWall wall:
                    mWalls.Add(wall);
                    break;
                case Tank tank:
                    mTanks.Add(tank);
                    break;
                case Pickup pickup:
                    mPickups.Add(pickup);
                    break;
            }
        }



        public void RemoveObject(SceneObject pObject)
        {
            switch(pObject)
            {
                case RectWall wall:
                    mWalls.Remove(wall);
                    break;
                case Tank tank:
                    mTanks.Remove(tank);
                    break;
                case Pickup pickup:
                    mPickups.Remove(pickup);
                    break;
            }
        }

        public void SaveMap()
        {
            // Ensure MapData exists and lists are initialized
            if (mMapData == null)
            {
                mMapData = new MapData
                {
                    Walls = new List<WallData>(),
                    Tanks = new List<TankData>(),
                    Pickups = new List<PickupData>()
                };
            }

            // Populate MapData from the current preview
            MapDataFromPreview();

            // Resolve where to save the map (mirror the LoadMapPreview resolution rules)
            string baseDir = AppContext.BaseDirectory;
            string mapsDir = Path.Combine(baseDir, "Maps");

            if (!Directory.Exists(mapsDir))
            {
                DirectoryInfo? dir = new DirectoryInfo(baseDir);
                for (int i = 0; i < 6 && dir != null; i++)
                {
                    string candidate = Path.Combine(dir.FullName, "Maps");
                    if (Directory.Exists(candidate))
                    {
                        mapsDir = candidate;
                        break;
                    }
                    dir = dir.Parent;
                }
            }

            string targetPath;

            // If the provided path is absolute
            if (Path.IsPathRooted(mFilePath))
            {
                // If it's an existing directory, save map.json inside it
                if (Directory.Exists(mFilePath))
                {
                    targetPath = Path.Combine(mFilePath, "map.json");
                }
                else if (!Path.HasExtension(mFilePath))
                {
                    // treat as directory path to create
                    Directory.CreateDirectory(mFilePath);
                    targetPath = Path.Combine(mFilePath, "map.json");
                }
                else
                {
                    // treat as file path
                    string? dirName = Path.GetDirectoryName(mFilePath);
                    if (!string.IsNullOrEmpty(dirName))
                        Directory.CreateDirectory(dirName);
                    targetPath = mFilePath;
                }
            }
            else
            {
                // relative path: strip leading Maps\ if present
                string relativePath = mFilePath;
                string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
                string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;
                if (relativePath.StartsWith(mapsPrefix1) || relativePath.StartsWith(mapsPrefix2))
                {
                    relativePath = relativePath.Substring(5);
                }

                string combined = Path.Combine(mapsDir, relativePath);

                if (Directory.Exists(combined))
                {
                    targetPath = Path.Combine(combined, "map.json");
                }
                else if (!Path.HasExtension(combined))
                {
                    // create directory and save inside
                    Directory.CreateDirectory(combined);
                    targetPath = Path.Combine(combined, "map.json");
                }
                else
                {
                    string? dirName = Path.GetDirectoryName(combined);
                    if (!string.IsNullOrEmpty(dirName))
                        Directory.CreateDirectory(dirName);
                    targetPath = combined;
                }
            }

            // Serialize MapData and write
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(mMapData, options);
            File.WriteAllText(targetPath, json);
        }

        public void MapDataFromPreview()
        {
            mMapData.Tanks.Clear();
            mMapData.Pickups.Clear();
            mMapData.Walls.Clear();
            //Adding walls to map data
            foreach (RectWall wall in mWalls)
            {
                mMapData.Walls.Add(new WallData()
                {
                    Texture = "block",
                    Position = new string[] { ((wall.mRectangle.X - mPlayArea.X) * 100.0f / mPlayArea.Width).ToString(CultureInfo.InvariantCulture), ((wall.mRectangle.Y - mPlayArea.Y) * 100.0f / mPlayArea.Height).ToString(CultureInfo.InvariantCulture) },
                    Size = new string[] { (wall.mRectangle.Width * 100.0f / mPlayArea.Width).ToString(CultureInfo.InvariantCulture), (wall.mRectangle.Height * 100.0f / mPlayArea.Height).ToString(CultureInfo.InvariantCulture) }
                });
            }
            //Adding tanks to map data
            foreach (Tank tank in mTanks)
            {
                float posX = (tank.mRectangle.X + tank.mRectangle.Width / 2 - mPlayArea.X) * 100.0f / mPlayArea.Width;
                float posY = (tank.mRectangle.Y + tank.mRectangle.Height / 2 - mPlayArea.Y) * 100.0f / mPlayArea.Height;
                float rotationDeg = MathHelper.ToDegrees(tank.Rotation);
                mMapData.Tanks.Add(new TankData()
                {
                    Position = new string[] { posX.ToString(CultureInfo.InvariantCulture), posY.ToString(CultureInfo.InvariantCulture) },
                    Rotation = rotationDeg.ToString(CultureInfo.InvariantCulture)
                });
            }
            //Adding pickups to map data
            foreach (Pickup pickup in mPickups)
            {
                float posX = (pickup.mRectangle.X + pickup.mRectangle.Width / 2 - mPlayArea.X) * 100.0f / mPlayArea.Width;
                float posY = (pickup.mRectangle.Y + pickup.mRectangle.Height / 2 - mPlayArea.Y) * 100.0f / mPlayArea.Height;
                mMapData.Pickups.Add(new PickupData()
                {
                    Position = new string[] { posX.ToString(CultureInfo.InvariantCulture), posY.ToString(CultureInfo.InvariantCulture) }
                });
            }
        }
    }
}




