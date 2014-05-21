using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AutoProcess
{
    class Config
    {
        public List<ConfigSet> ConfigSets = new List<ConfigSet>();

        private Config() { }

        public static Config Load(string configSrc)
        {
            Config c = new Config();

            string[] lines = File.ReadAllLines(configSrc);

            ConfigSet cs = null;

            Modes m = Modes.SelectMode;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i][0] == '<')
                {
                    if (lines[i] == "<set>")
                        cs = new ConfigSet();
                    if (lines[i] == "<\\set>")
                        c.ConfigSets.Add(cs);

                    if (lines[i] == "<types>")
                        m = Modes.WriteTypes;
                    else if (lines[i] == "<\\types>")
                        m = Modes.SelectMode;
                    else if (m == Modes.WriteTypes)
                        cs.Types.Add(lines[i]);

                    if (lines[i] == "<classes>")
                        m = Modes.WriteClasses;
                    else if (lines[i] == "<\\classes>")
                        m = Modes.SelectMode;
                    else if (m == Modes.WriteClasses)
                        cs.Classes.Add(lines[i]);
                }
                else if (lines[i][0] == ':')
                {
                    string[] split = lines[i].Split('=');
                    string trim = lines[1].Trim();

                    if (split[0] == ":name")
                        cs.Name = trim;
                    else if (split[0] == ":root")
                        cs.Root = trim;
                    else if (split[0] == ":bitmaps")
                        cs.Bitmaps = trim;
                }
            }

            return c;
        }
    }

    enum Modes
    {
        SelectMode,
        WriteTypes,
        WriteClasses
    }
}
