using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AutoProcess
{
    //Organizes the grid based subdivision and statistical processing of the source images.
    class SubdivisionProcess
    {
        public static void Run(string outputFileDirectory, string rootDir, string typeName, string className, int classIndex)
        {
            string outputFileName = className + "_" + typeName + "_Stats.nna";
            string outputFilePath = outputFileDirectory + outputFileName;

            if (!File.Exists(outputFilePath))
            {
                string dataDir = outputFileDirectory + className + "\\" + typeName + "\\";

                string[] strings = Directory.GetFiles(dataDir);

                List<string> stringVectors = new List<string>();

                foreach (string s in strings)
                {
                    string[] splits = s.Split('(');
                    splits = splits[splits.Length - 1].Split(')');
                    string dims = splits[0];

                    int width = Convert.ToInt16(dims.Split('x')[0]);
                    int height = Convert.ToInt16(dims.Split('x')[1]);

                    int dataLength = 4;
                    int byteCount = width * height * dataLength * 5;

                    FileStream fs = File.OpenRead(s);
                    BinaryReader br = new BinaryReader(fs);

                    byte[] allbytes = new byte[byteCount];

                    float[,] readData = new float[width, height];

                    allbytes = br.ReadBytes(byteCount);

                    float total = 0;

                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                        {
                            byte[] floaty = new byte[4];

                            for (int i = 0; i < 4; i++)
                                floaty[i] = allbytes[(x + y * width) * 4 + i];

                            readData[x, y] = BitConverter.ToSingle(floaty, 0);
                            total += readData[x, y];
                        }

                    Field f = Field.FromValues(readData);

                    float[] means = f.ComputeSubdivisionMeans(8, 8);
                    float[] variances = f.ComputeSubdivisionVariances(8, 8, means);
                    float[] skewnesses = f.ComputeSubdivisionSkewness(8, 8, means, variances);
                    float[] kurtosises = f.ComputeSubdivisionKurtosis(8, 8, means, variances);

                    for (int i = 0; i < means.Length; i++)
                    {
                        if (float.IsNaN(skewnesses[i]))
                            skewnesses[i] = 0;

                        if (float.IsNaN(kurtosises[i]))
                            kurtosises[i] = 0;
                    }

                    string statVector = "";

                    for (int i = 0; i < means.Length; i++)
                        //statVector += means[i] + "\t";
                        statVector += means[i] + "\t" + variances[i] + "\t" + skewnesses[i] + "\t" + kurtosises[i] + "\t";

                    statVector += classIndex;

                    stringVectors.Add(statVector);
                }

                Console.WriteLine("Writing " + outputFileName);
                File.WriteAllLines(outputFilePath, stringVectors.ToArray());
            }
        }
    }
}
