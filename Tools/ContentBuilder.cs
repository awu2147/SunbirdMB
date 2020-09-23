using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Tools
{
    internal class ContentBuilder
    {
        public enum Mode
        {
            [Description("/build")]
            Build,
            [Description("/rebuild")]
            Rebuild,
            [Description("/incremental")]
            Incremental
        }

        private string GetMode(Mode buildMode)
        {
            var enumType = typeof(Mode);
            var memberInfos = enumType.GetMember(buildMode.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = ((DescriptionAttribute)valueAttributes[0]).Description;
            return description;
        }

        internal const string mgcb = "mgcb";
        internal string OutputDir { get; set; } = string.Empty;
        internal string IntermediateDir { get; set; } = "obj";
        internal Mode BuildMode { get; set; } = Mode.Rebuild;
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
            return $@"{outputDir} {intermediateDir} {GetMode(BuildMode)} {importer} {processor} {processorParam}";
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
            }
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
