using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTMapEditor.Maps;

namespace TTMapEditor.Saving
{
    public class MapEditingMapService
    {
        readonly string mMapsRoot;

        public MapEditingMapService(string pMapsRoot)
        {
            mMapsRoot = pMapsRoot;
        }

        public MapPreview CreatePreviewForExistingMap(string pMapFile)
        {
            string resolvedPath = ResolveMapPath(pMapFile);
            return new MapPreview(resolvedPath);
        }

        public MapPreview CreatePreviewForNewMap(string pMapFile)
        {
            string targetPath = ResolveNewMapPath(pMapFile);
            return new MapPreview(targetPath);
        }

        public void SaveMap(MapPreview pPreview, string pName)
        {
            pPreview.SaveMap(pName);
        }

        string ResolveMapPath(string pMapFile)
        {
            if(string.IsNullOrEmpty(pMapFile))
            {
                Directory.CreateDirectory(mMapsRoot);
                return Path.GetFullPath(Path.Combine(mMapsRoot, "map.json"));
            }

            if(Path.IsPathRooted(pMapFile))
            {
                if(Directory.Exists(pMapFile) || !Path.HasExtension(pMapFile))
                {
                    Directory.CreateDirectory(pMapFile);
                    return Path.GetFullPath(Path.Combine(pMapFile, "map.json"));
                }

                return Path.GetFullPath(pMapFile);
            }

            string relative = NormalizeMapsPrefix(pMapFile);

            string candidate = Path.Combine(mMapsRoot, relative);

            if (Directory.Exists(candidate) || !Path.HasExtension(candidate))
            {
                    Directory.CreateDirectory(candidate);
                    return Path.GetFullPath(Path.Combine(candidate, "map.json"));
            }

            string? dir = Path.GetDirectoryName(candidate);
            if(!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return Path.GetFullPath(candidate);
        }

        string ResolveNewMapPath(string pMapFile)
        {
            string mapsRoot = mMapsRoot;

            string relative = pMapFile ?? string.Empty;
            relative = NormalizeMapsPrefix(relative);

            string candidate;
            if(string.IsNullOrWhiteSpace(relative))
            {
                candidate = mapsRoot;
            }
            else if(Path.IsPathRooted(relative) || relative.Contains(Path.DirectorySeparatorChar) || relative.Contains(Path.AltDirectorySeparatorChar))
            {
                candidate = relative;
            }
            else
            {
                candidate = Path.Combine(mapsRoot, relative);
            }

            string targetPath;
            if(Directory.Exists(candidate) || !Path.HasExtension(candidate))
            {
                targetPath = Path.Combine(candidate, "map.json");
            }
            else
            {
                targetPath = candidate;
            }

            return Path.GetFullPath(targetPath);
        }

        static string NormalizeMapsPrefix(string pPath)
        {
            string relative = pPath;
            string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
            string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;

            if(relative.StartsWith(mapsPrefix1) || relative.StartsWith(mapsPrefix2))
            {
                relative = relative.Substring(5);
            }

            return relative;
        }



    }
}
