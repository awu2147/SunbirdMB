using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Framework
{
    /// <summary>
    /// This class should be initalize at start-up.
    /// </summary>
    public static class UriHelper
    {
        public static string AppPath;
        public static string AppDirectory;
        public static string ContentDirectory;
        public static string CubeDirectory;
        public static string CubeTopDirectory;
        public static string CubeBaseDirectory;
        public static string DecoDirectory;
        public static string Deco1x1x1Directory;
        public static string Deco1x1x2Directory;
        public static string Deco1x1x3Directory;

        public static void Intiailize()
        {
            AppPath = Assembly.GetExecutingAssembly().Location;
            Debug.Assert(AppPath == @"D:\SunbirdMB\bin\Debug\SunbirdMB.exe");

            AppDirectory = AppPath.TrimEnd(Path.GetFileName(AppPath));
            Debug.Assert(AppDirectory == @"D:\SunbirdMB\bin\Debug\");

            ContentDirectory = Path.Combine(AppDirectory, "Content");
            Debug.Assert(ContentDirectory == @"D:\SunbirdMB\bin\Debug\Content");

            CubeDirectory = Path.Combine(ContentDirectory, "Cubes");
            Debug.Assert(CubeDirectory == @"D:\SunbirdMB\bin\Debug\Content\Cubes");

            CubeTopDirectory = Path.Combine(CubeDirectory, "Top");
            Debug.Assert(CubeTopDirectory == @"D:\SunbirdMB\bin\Debug\Content\Cubes\Top");

            CubeBaseDirectory = Path.Combine(CubeDirectory, "Base");
            Debug.Assert(CubeBaseDirectory == @"D:\SunbirdMB\bin\Debug\Content\Cubes\Base");

            DecoDirectory = Path.Combine(ContentDirectory, "Decos");
            Debug.Assert(DecoDirectory == @"D:\SunbirdMB\bin\Debug\Content\Decos");

            Deco1x1x1Directory = Path.Combine(DecoDirectory, "1x1x1");
            Debug.Assert(Deco1x1x1Directory == @"D:\SunbirdMB\bin\Debug\Content\Decos\1x1x1");

            Deco1x1x2Directory = Path.Combine(DecoDirectory, "1x1x2");
            Debug.Assert(Deco1x1x2Directory == @"D:\SunbirdMB\bin\Debug\Content\Decos\1x1x2");

            Deco1x1x3Directory = Path.Combine(DecoDirectory, "1x1x3");
            Debug.Assert(Deco1x1x3Directory == @"D:\SunbirdMB\bin\Debug\Content\Decos\1x1x3");
        }

        public static string MakeContentRelative(this string path)
        {
            string relativePath = Path.ChangeExtension(path.Replace(AppDirectory + @"Content\", ""), null);
            if (path.Length == relativePath.Length)
            {
                "Failed to create content relative path. An attempt was made to import from outside of the Content directory.".Log();
                return string.Empty;
            }
            return relativePath;
        }
    }
}
