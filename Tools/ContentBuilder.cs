using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Tools
{
    internal class ContentBuilder
    {
        public enum BuildMode
        {
            [Description("/build")]
            Build,
            [Description("/rebuild")]
            Rebuild,
            [Description("/incremental")]
            Incremental
        }

        private string GetMode(BuildMode buildMode)
        {
            var enumType = typeof(BuildMode);
            var memberInfos = enumType.GetMember(buildMode.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = ((DescriptionAttribute)valueAttributes[0]).Description;
            return description;
        }

        internal const string mgcb = "mgcb";
        internal string OutputDir { get; set; } = string.Empty;
        internal string IntermediateDir { get; set; } = "obj";
        internal BuildMode Mode { get; set; } = BuildMode.Incremental;
        internal virtual string Importer { get { return string.Empty; } }
        internal virtual string Processor { get { return string.Empty; } }
        internal virtual string ProcessorParam { get { return string.Empty; } }
        internal List<string> Targets { get; set; } = new List<string>();

        public ContentBuilder() { }

        public ContentBuilder(string outputDir, string intermediateDir)
        {
            OutputDir = outputDir;
            IntermediateDir = intermediateDir;
        }

        /// <summary>
        /// If arg is string.Empty, return string.Empty and ignore the prefix.
        /// </summary>
        private string ValidateArg(string prefix, string arg)
        {
            string result = arg != string.Empty ? $"{prefix}{arg}" : string.Empty;
            return result;
        }

        /// <summary>
        /// Get build arguments.
        /// </summary>
        private string GetArgs()
        {
            string outputDir = ValidateArg("/outputDir:", OutputDir);
            string intermediateDir = ValidateArg("/intermediateDir:", IntermediateDir);
            string importer = ValidateArg("/importer:", Importer);
            string processor = ValidateArg("/processor:", Processor);
            string processorParam = ValidateArg("/processorParam:", ProcessorParam);
            return $@"{outputDir} {intermediateDir} {GetMode(Mode)} {importer} {processor} {processorParam}";
        }

        private string GetTargets()
        {
            string targets = string.Empty;
            foreach (var content in Targets)
            {
                targets += $"/build:{content} ";
            }
            return targets;
        }

        internal void Build()
        {
            using (Process process = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = mgcb;
                startInfo.Arguments = $@"{GetArgs()} {GetTargets()}";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.Start();
                StreamReader myStreamReader = process.StandardError;
                Console.WriteLine(myStreamReader.ReadLine());
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Rebuild the entire Content folder.
        /// </summary>
        internal static void RebuildContent()
        {
            RebuildContent(string.Empty);
        }

        /// <summary>
        /// Rebuild a Content sub-folder.
        /// </summary>
        internal static void RebuildContent(string subFolder)
        {
            "Rebuilding content...".Log();
            var appPath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
            Debug.Assert(appDirectory == @"D:\SunbirdMB\bin\Debug\");
            var contentPath = subFolder == string.Empty ? Path.Combine(appDirectory, "Content") : Path.Combine(appDirectory, "Content", subFolder);
            var files = Directory.GetFiles(contentPath, "*.png", SearchOption.AllDirectories);
            var tcb = new TextureContentBuilder();
            foreach (var file in files)
            {
                var target = file.Replace(appDirectory, "");
                tcb.Targets.Add(target);
            }
            tcb.Build();
        }

        /// <summary>
        /// Rebuild a particular file in the Content folder.
        /// </summary>
        internal static void BuildFile(string filePath)
        {
            "Building content...".Log();
            var appPath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = appPath.TrimEnd(Path.GetFileName(appPath));
            Debug.Assert(appDirectory == @"D:\SunbirdMB\bin\Debug\");
            var tcb = new TextureContentBuilder();
            if (ValidateDirectory(appDirectory, filePath))
            {
                var target = filePath.Replace(appDirectory, "");
                tcb.Targets.Add(target);
                tcb.Build();
            }
            else
            {
                throw new IOException("File not in Content directory.");
            }
        }

        private static bool ValidateDirectory(string appDirectory, string pathToValidate)
        {
            var contentDirectory = Path.Combine(appDirectory, "Content");
            var result = pathToValidate.Replace(contentDirectory, "");
            if (result.Length == pathToValidate.Length)
            {
                return false;
            }
            return true;
        }

    }

    internal class TextureContentBuilder : ContentBuilder
    {
        internal override string Importer { get { return "TextureImporter"; } }
        internal override string Processor { get { return "TextureProcessor"; } }
        internal override string ProcessorParam { get { return "ColorKeyEnabled=false"; } }

        public TextureContentBuilder() : base() { }

        public TextureContentBuilder(string outputDir, string intermediateDir) : base(outputDir, intermediateDir) { }
    }
}
