using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AutoProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            //Loads up the data provided in Config.txt, for the organized batch processing of many images.
            Config c = Config.Load(args[0]);

            //Loops through all the config sets defined by the Config.txt file.
            foreach (ConfigSet set in c.ConfigSets)
                foreach (string className in set.Classes)
                {
                    string maskDir = set.Bitmaps + className + "\\" + "Masks\\";

                    foreach (string typeName in set.Types)
                    {
                        //Sets up source and target directory strings given the "type" and "class" of the images specified by the strings "s1" and "s2".
                        string sourceDir = set.Bitmaps + className + "\\" + typeName + "\\";
                        string targetDir = set.Root + "Data\\" + set.Name + "\\" + className + "\\" + typeName + "\\";

                        //Creates directories for the converted files.
                        if (!Directory.Exists(targetDir))
                            Directory.CreateDirectory(targetDir);

                        //Converts the BMP sources into DAT raw data for faster loading and processing in the future.
                        //Ideally this would be replaced with a direct conversion from Dicom to DAT.
                        GenerateDatFiles.Run(targetDir, maskDir, sourceDir);
                    }
                }

            //Using all the configuration sets, create output files for a particular type of processing.
            foreach (ConfigSet set in c.ConfigSets)
                foreach (string s1 in set.Types)
                    for (int classIndex = 0; classIndex < set.Classes.Count; classIndex++)
                    {
                        string outputFileDirectory = set.Root + "Data\\" + set.Name + "\\";
                        SubdivisionProcess.Run(outputFileDirectory, set.Root, s1, set.Classes[classIndex], classIndex);
                    }

            //Using all of the output files for each "type" and "class" of input data sets, create training, testing, and validation sets
            foreach (ConfigSet set in c.ConfigSets)
            {
                string dataDir = set.Root + "Data\\" + set.Name + "\\";
                string[] files = Directory.GetFiles(dataDir);

                Dictionary<string, List<string>> dataRelations = new Dictionary<string, List<string>>();

                //Organize the output files
                foreach (string s in files)
                {
                    string[] nameSplit = s.Split('\\');
                    string name = nameSplit[nameSplit.Length - 1].Split('.')[0];
                    nameSplit = name.Split('_');

                    if (!dataRelations.ContainsKey(nameSplit[1]))
                        dataRelations.Add(nameSplit[1], new List<string>());

                    dataRelations[nameSplit[1]].Add(s);
                }

                //For each collection of output files that are related by "class" generate the .nna sets.
                foreach (string key in dataRelations.Keys)
                    GenerateNNAFiles(set.Root, 0.5, set.Name, key, dataRelations[key].ToArray());
            }
        }

        private static void GenerateNNAFiles(string root, double abRatio, string classifier, string prefix, params string[] dataSources)
        {
            List<string> setA = new List<string>();
            List<string> setB = new List<string>();

            double total = 0;

            List<List<string>> listStrings = new List<List<string>>();

            foreach (string s in dataSources)
            {
                string[] all = File.ReadAllLines(s);
                listStrings.Add(all.ToList());
                total += all.Length;
            }

            if (total > 0)
            {

                for (int i = 0; i < listStrings.Count; i++)
                {
                    double ratio = 0;
                    double transferCount = 0;
                    double originalMax = listStrings[i].Count;

                    while (ratio < abRatio)
                    {
                        setA.Add(listStrings[i][0]);
                        listStrings[i].RemoveAt(0);
                        transferCount++;
                        ratio = transferCount / originalMax;
                    }

                    while (listStrings[i].Count > 0)
                    {
                        setB.Add(listStrings[i][0]);
                        listStrings[i].RemoveAt(0);
                    }
                }

                string writeDir = root + "Data\\";

                File.WriteAllLines(writeDir + classifier + "_" + prefix + "_SetA.nna", setA.ToArray());
                File.WriteAllLines(writeDir + classifier + "_" + prefix + "_SetB.nna", setB.ToArray());
            }
            else
                Console.WriteLine("There were no files to process for " + classifier + " " + prefix);
        }
    }
}
