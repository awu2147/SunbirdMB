using Microsoft.Win32;
using SunbirdMB.Interfaces;
using SunbirdMB.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Framework
{
    internal static class Importer
    {
        internal static void CopyBuildImport(string importDirectory, IImporter importer)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (Path.GetExtension(openFileDialog.FileName) == ".png")
                {
                    var newFilePath = Path.Combine(importDirectory, Path.GetFileName(openFileDialog.FileName));
                    if (File.Exists(newFilePath))
                    {
                        "Cannot copy to directory, file already exists.".Log();
                    }
                    else
                    {
                        File.Copy(openFileDialog.FileName, newFilePath);
                        // We don't need to rebuild everything, only the .png we just imported.
                        ContentBuilder.BuildFile(newFilePath);
                        importer.Import(newFilePath);
                    }
                }
                else
                {
                    "Incorrect file format".Log();
                }
            }
        }
    }
}
