using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Tools
{
    internal class ContentBuilder
    {
        internal const string mgcb = "mgcb";
        internal string WorkingDir { get; set; }
        internal string OutputDir { get; set; }
        internal string IntermediateDir { get; set; }
        internal string Importer { get; set; }
        internal string Processor { get; set; }
        internal string ProcessorParam { get; set; }
        internal List<string> Targets { get; set; } = new List<string>();

        public ContentBuilder() { }

        public ContentBuilder(string workingDir, string outputDir, string intermediateDir)
        {
            WorkingDir = workingDir;
            OutputDir = outputDir;
            IntermediateDir = intermediateDir;
        }

        private string ValidateArg(string prefix, string arg)
        {
            string result = arg != string.Empty ? $"{prefix}{arg}" : string.Empty;
            return result;
        }

        private string GetArgs()
        {
            string outputDir = ValidateArg("/outputDir:", OutputDir);
            string intermediateDir = ValidateArg("/intermediateDir:", IntermediateDir);
            string importer = ValidateArg("/importer:", Importer);
            string processor = ValidateArg("/processor:", Processor);
            string processorParam = ValidateArg("/processorParam:", ProcessorParam);
            return $"{outputDir} {intermediateDir} {importer} {processor} {outputDir} {processorParam}";
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
            //ProcessStartInfo startInfo = new ProcessStartInfo();
            ////startInfo.WorkingDirectory = WorkingDir;
            //startInfo.FileName = mgcb;
            //startInfo.Arguments = $@"{GetArgs()} {GetTargets()}";
            //startInfo.RedirectStandardOutput = true;
            //startInfo.RedirectStandardError = true;
            //startInfo.UseShellExecute = true;
            //startInfo.CreateNoWindow = true;
            //Process buildProcess = new Process();
            //buildProcess.StartInfo = startInfo;
            //buildProcess.EnableRaisingEvents = true;
            //buildProcess.Start();

            Process.Start(mgcb, $@"{GetArgs()} {GetTargets()}");
        }

        internal void Rebuild()
        {
            Process.Start(mgcb, @"/rebuild");
        }
    }

    internal class TextureContentBuilder : ContentBuilder
    {
        public TextureContentBuilder(string workingDir, string outputDir, string intermediateDir) : base(workingDir, outputDir, intermediateDir)
        {
            Importer = "TextureImporter";
            Processor = "TextureProcessor";
            ProcessorParam = "ColorKeyEnabled=false";
        }
    }
}
