using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace AutoProcess
{
    class GenerateDatFiles
    {
        static Bitmap bData;
        static Bitmap bMask;

        public static void Run(string targetDir, string maskDir, string sourceDir)
        {
            Console.WriteLine("Began BMP -> DAT conversion of all files in:");
            Console.WriteLine(sourceDir);

            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            Dictionary<string, List<string>> maskByPrefix = FillDictionary(maskDir);
            Dictionary<string, List<string>> dataByPrefix = FillDictionary(sourceDir);
            Dictionary<string, List<float[,]>> storedMaskedData = new Dictionary<string, List<float[,]>>();

            foreach (string s2 in maskByPrefix.Keys)
            {
                for (int j = 0; j < dataByPrefix[s2].Count; j++)
                    for (int i = 0; i < maskByPrefix[s2].Count; i++)
                        DoInnerLoop(i, targetDir, dataByPrefix[s2][j], maskByPrefix[s2][j], sourceDir, s2, storedMaskedData);

                Console.WriteLine("Completed BMP -> DAT: " + s2);
            }
        }

        static void DoInnerLoop(int index, string targetDir, string dataSrc, string maskSrc, string dataDir, string prefix, Dictionary<string, List<float[,]>> storedMaskedData)
        {
            bData = (Bitmap)Bitmap.FromFile(dataSrc);
            bMask = (Bitmap)Bitmap.FromFile(maskSrc);

            int width = bData.Width;
            int height = bData.Height;

            string fileString = targetDir + prefix + "(" + width + "x" + height + ")_M_" + (index + 1) + ".dat";

            if (!File.Exists(fileString))
            {
                float[,] data = MaskingRoutine(bData, bMask);

                if (!storedMaskedData.Keys.Contains(prefix))
                    storedMaskedData.Add(prefix, new List<float[,]>());

                storedMaskedData[prefix].Add(data);

                int dataLength = 4;
                int byteCount = width * height * dataLength * 5;

                FileStream fs = File.Create(fileString, byteCount, FileOptions.SequentialScan);
                BinaryWriter bw = new BinaryWriter(fs);

                byte[] allbytes = new byte[byteCount];

                float total = 0;

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        byte[] f = BitConverter.GetBytes(data[x, y]);
                        f.CopyTo(allbytes, (x + y * width) * dataLength);

                        total += data[x, y];
                    }

                bw.Write(allbytes, 0, byteCount);

                bw.Close();
                fs.Close();
            }

            bData.Dispose();
            bMask.Dispose();
        }

        static Dictionary<string, List<string>> FillDictionary(string dir)
        {
            List<string> dirStr = new List<string>(Directory.GetFiles(dir));
            Dictionary<string, List<string>> strDict = new Dictionary<string, List<string>>();

            foreach (string s in dirStr)
            {
                string[] splits = s.Split('.');
                splits = splits[0].Split('\\');
                string[] tryJSplit = splits[splits.Length - 1].Split('{');
                string identifier = "";

                if (tryJSplit.Length > 1)
                    identifier = tryJSplit[0];
                else
                    identifier = splits[splits.Length - 1];

                if (!strDict.ContainsKey(identifier))
                    strDict.Add(identifier, new List<string>());

                strDict[identifier].Add(s);
            }

            return strDict;
        }

        static Dictionary<string, List<string>> FillDictionary(params string[] dir)
        {
            Dictionary<string, List<string>> output = new Dictionary<string, List<string>>();

            foreach (string s in dir)
            {
                Dictionary<string, List<string>> temp = FillDictionary(s);

                foreach (string s1 in temp.Keys)
                {
                    if (!output.ContainsKey(s1))
                        output.Add(s1, new List<string>());

                    foreach (string s2 in temp[s1])
                        output[s1].Add(s2);
                }
            }

            return output;
        }

        static void MaskingRoutine(string identifier, string dataPath, string maskPath, string outputPath, int i)
        {
            Field fMask = Field.FromBmp(maskPath);
            Field fData = Field.FromBmp(dataPath);

            fMask.ZeroEdges();
            fMask.Fill(1, 1, 1, .1f);

            fMask.Invert();

            for (int k = 0; k < 5; k++)
                fMask.Blot(0);

            fData.Mask(fMask);
            fData.ToBmp(outputPath + identifier + "." + i + ".bmp");
        }

        private static float[,] MaskingRoutine(Bitmap dataField, Bitmap maskField)
        {
            Field data = Field.FromBmp(dataField);
            Field mask = Field.FromBmp(maskField);

            float[,] output = new float[data.Width, data.Height];
            float[,] maskV = mask.FieldValues;
            float[,] dataV = data.FieldValues;

            for (int y = 0; y < data.Height; y++)
                for (int x = 0; x < data.Width; x++)
                    if (maskV[x, y] == 0)
                        output[x, y] = dataV[x, y];

            return output;
        }
    }
}
