using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafeLibrary.ModelConversion
{
    public class CommandListHandler
    {
        public string output = "";
        public string input = "";
        public string ext = ".bfres";
        public bool isSwitch = false;
        public bool yaz0 = false;
        public string version = "";
        public uint alignment = 0;
        public bool single_tex = false;
        public bool use_mesh_meta_info = false;

        public void Execute(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains(".txt"))
                {
                    string textFile = args[i];
                    Execute(GetTextFileArguments(textFile));
                }
                if (args[i].Contains("-i") || args[i].Contains("-input"))
                {
                    input = args[i + 1];
                }
                if (args[i].Contains("-o") || args[i].Contains("-output"))
                {
                    output = args[i + 1];
                }
                if (args[i].Contains("-ext"))
                {
                    ext = args[i + 1];
                    if (!ext.StartsWith("."))
                        ext = $".{ext}";
                }
                if (args[i].Contains("-compress"))
                {
                    yaz0 = true;
                }
                if (args[i].Contains("-switch"))
                {
                    isSwitch = true;
                }
                if (args[i].Contains("-single_tex"))
                {
                    single_tex = true;
                }
                if (args[i].Contains("-version"))
                {
                    version = args[i + 1];
                    if (version.Length != 4)
                        throw new Exception("Version must be 4 characters in length! ie 3404");
                }
                if (args[i].Contains("-align"))
                {
                    alignment = uint.Parse(args[i + 1]);
                }
            }
        }

        private string[] GetTextFileArguments(string fileName)
        {
            List<string> arguments = new List<string>();
            var lines = System.IO.File.ReadAllText(fileName).Split('\n');
            foreach (var line in lines)
            {
                //Skip comments if used
                if (line.StartsWith("#"))
                    continue;

                string output = line.Replace("\n", "");
                output = output.Replace("\r", "");

                arguments.Add(output);
            }
            return arguments.ToArray();
        }
    }
}
