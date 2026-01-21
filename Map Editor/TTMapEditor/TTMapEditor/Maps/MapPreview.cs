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
    internal class MapPreview
    {
        Rectangle mPlayArea { get; set; }
        List<RectWall> mWalls { get; set; }

        string mFilePath { get; set; }

        public MapPreview(string pFilePath)
        {
            int screenWidth = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width;
            int screenHeight = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height;
            mPlayArea = new Rectangle(screenWidth * 2 / 100, screenHeight * 25 / 100, screenWidth * 96 / 100, screenHeight * 73 / 100);
            mFilePath = pFilePath;
            mWalls = GetWalls();
        }

        public Rectangle GetPlayArea()
        {
            return mPlayArea;
        }

        public List<RectWall> GetWalls()
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

            // If the provided path is absolute and exists, use it directly
            if (Path.IsPathRooted(mFilePath) && File.Exists(mFilePath))
            {
                return ParseLines(File.ReadAllLines(mFilePath));
            }

            // Remove a leading "Maps\" or "Maps/" from mFilePath if present so we don't duplicate it
            string relativePath = mFilePath;
            string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
            string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;
            if (relativePath.StartsWith(mapsPrefix1) || relativePath.StartsWith(mapsPrefix2))
            {
                relativePath = relativePath.Substring(5);
            }

            string fullPath = Path.Combine(mapsDir, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Map file not found: {fullPath}");
            }

            string[] lines = File.ReadAllLines(fullPath);
            return ParseLines(lines);
        }

        public List<RectWall> ParseLines(string[] pLines)
        {
            int screenWidth = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width;
            int screenHeight = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height;
            Rectangle playArea = new Rectangle(screenWidth * 2 / 100, screenHeight * 25 / 100, screenWidth * 96 / 100, screenHeight * 73 / 100);
            List<RectWall> Walls = new List<RectWall>();

            // Join lines to determine format (JSON vs old-line format)
            string content = string.Join(Environment.NewLine, pLines).TrimStart();

            // If JSON detected, deserialize and convert to RectWall list
            if (content.StartsWith("{") || content.StartsWith("["))
            {
                try
                {
                    // Reuse MapData from Managers (matches the JSON structure you posted)
                    MapData map = JsonSerializer.Deserialize<MapData>(content);
                    if (map?.Walls != null)
                    {
                        foreach (var w in map.Walls)
                        {
                            if (w == null) continue;
                            string textureName = w.Texture;
                            // parse positions/sizes using invariant culture
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
                                playArea.X + ((float)playArea.Width * (posX / 100.0f)),
                                playArea.Y + ((float)playArea.Height * (posY / 100.0f))
                            );
                            Vector2 size = new Vector2(
                                playArea.Width * (sizeX / 100.0f),
                                playArea.Height * (sizeY / 100.0f)
                            );

                            Texture2D tex = null;
                            try
                            {
                                tex = TTMapEditor.Instance().GetContentManager().Load<Texture2D>(textureName);
                            }
                            catch
                            {
                                // If texture load fails, you may want to fallback to a default texture or skip.
                                tex = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("block");
                            }

                            RectWall currentWall = new RectWall(tex, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y));
                            Walls.Add(currentWall);
                        }
                    }
                }
                catch (JsonException)
                {
                    // Fall back to line parsing if JSON parse fails
                }

                return Walls;
            }

            // --- existing line based parsing (fallback) ---
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
                    positionFallback.X = playArea.X + ((float)playArea.Width * (positionFallback.X / 100.0f));
                    positionFallback.Y = playArea.Y + ((float)playArea.Height * (positionFallback.Y / 100.0f));
                    continue;
                }
                else if (line.Contains("Size") || line.Contains("size"))
                {
                    string[] components = line.Split('=')[1].Trim().Split(',');
                    sizeFallback = new Vector2(float.Parse(components[0], CultureInfo.InvariantCulture), float.Parse(components[1], CultureInfo.InvariantCulture));
                    sizeFallback.X = playArea.Width * (sizeFallback.X / 100.0f);
                    sizeFallback.Y = playArea.Height * (sizeFallback.Y / 100.0f);
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
    }
}



