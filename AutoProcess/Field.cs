using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace AutoProcess
{
    //Contains a bunch of methods for processing 2D-arrays of data.
    //There may be bugs in the methods relating to whole image statistics. Should probably test more.
    class Field
    {
        public float[,] FieldValues;
        public int Width;
        public int Height;

        int[] XD = new int[] { -1, 0, 1, 1, 1, 0, -1, -1 };
        int[] YD = new int[] { -1, -1, -1, 0, 1, 1, 1, 0 };

        private Field(float[,] fieldValues)
        {
            Width = fieldValues.GetLength(0);
            Height = fieldValues.GetLength(1);

            FieldValues = new float[Width, Height];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    FieldValues[x, y] = fieldValues[x, y];
        }

        public static Field FromBmp(Bitmap data)
        {
            float[,] output = new float[data.Width, data.Height];

            for (int y = 0; y < data.Height; y++)
                for (int x = 0; x < data.Width; x++)
                    output[x, y] = (float)(data.GetPixel(x, y).R + data.GetPixel(x, y).G + data.GetPixel(x, y).B) / 765f;

            return new Field(output);
        }
        public static Field FromBmp(string filePath)
        {
            Bitmap data = (Bitmap)Bitmap.FromFile(filePath);

            float[,] output = new float[data.Width, data.Height];

            for (int y = 0; y < data.Height; y++)
                for (int x = 0; x < data.Width; x++)
                    output[x, y] = (float)(data.GetPixel(x, y).R + data.GetPixel(x, y).G + data.GetPixel(x, y).B) / 765f;

            return new Field(output);
        }
        public void ToBmp(string filePath)
        {
            Bitmap b = new Bitmap(Width, Height);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    int intensity = (int)(FieldValues[x, y] * 255);
                    Color c = Color.FromArgb(intensity, intensity, intensity);
                    b.SetPixel(x, y, c);
                }

            b.Save(filePath, ImageFormat.Bmp);
        }

        public void Partition(float threshold)
        {
            for (int y = 0; y < Width; y++)
                for (int x = 0; x < Height; x++)
                    if (FieldValues[x, y] > threshold)
                        FieldValues[x, y] = 1;
                    else
                        FieldValues[x, y] = 0;
        }
        public Field PartitionRange(float lowerThreshold, float upperThreshold)
        {
            float[,] output = new float[Width, Height];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (FieldValues[x, y] <= lowerThreshold)
                        output[x, y] = 0;
                    else if (FieldValues[x, y] >= upperThreshold)
                        output[x, y] = 1;
                    else
                        output[x, y] = (FieldValues[x, y] - lowerThreshold) / (upperThreshold - lowerThreshold);

            return FromValues(output);
        }

        public void Replace(float target, float replacement)
        {
            for (int y = 0; y < Width; y++)
                for (int x = 0; x < Height; x++)
                    if (FieldValues[x, y] == target)
                        FieldValues[x, y] = replacement;
        }

        public static Field Mask(Field data, Field mask)
        {
            int height = data.Height;
            int width = data.Width;

            float[,] output = new float[width, height];
            float[,] inputData = data.FieldValues;
            float[,] maskData = mask.FieldValues;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (maskData[x, y] == 0)
                        output[x, y] = inputData[x, y];

            Field f = new Field(output);
            return f;
        }

        public static Field FromValues(float[,] values)
        {
            return new Field(values);
        }

        public void Fill(int x, int y, float fillColor, float threshold)
        {
            List<Point> points = new List<Point>();
            points.Add(new Point(x, y));

            if (FieldValues[x, y] == fillColor)
                return;

            FieldValues[x, y] = fillColor;

            while (points.Count > 0)
                TailFill(points[0].X, points[0].Y, FieldValues[x, y], fillColor, threshold, points);
        }
        private void TailFill(int x, int y, float fillStart, float fillColor, float threshold, List<Point> points)
        {
            points.RemoveAt(0);

            for (int k = 0; k < 8; k++)
            {
                int sx = x + XD[k];
                int sy = y + YD[k];

                if (sy > -1 && sy < Height && sx > -1 && sx < Width)
                {
                    Point p = new Point(sx, sy);

                    if (FieldValues[sx, sy] < threshold)
                    {
                        FieldValues[sx, sy] = fillColor;
                        points.Add(p);
                    }
                }
            }
        }

        public void Stroke(float tolerance, int strokeRadius, float strokeColor)
        {
            float[,] temp = new float[Width, Height];
            List<Point> pointsToStroke = new List<Point>();

            for (int y = 0; y < Height - 1; y++)
                for (int x = 0; x < Width - 1; x++)
                    temp[x, y] = FieldValues[x, y];

            for (int y = strokeRadius; y < Height - 1 - strokeRadius; y++)
                for (int x = strokeRadius; x < Width - 1 - strokeRadius; x++)
                    if (FieldValues[x, y] == 1)
                        for (int k = 0; k < 8; k++)
                            if (Math.Abs(FieldValues[x + XD[k], y + YD[k]] - FieldValues[x, y]) < tolerance)
                                for (int yi = -strokeRadius / 2; yi < strokeRadius / 2; yi++)
                                    for (int xi = -strokeRadius / 2; xi < strokeRadius / 2; xi++)
                                    {
                                        temp[x + xi, y + yi] = strokeColor;
                                    }

            FieldValues = temp;
        }

        public float GetSampleMean()
        {
            float sampleMean = 0;

            for (int y = 0; y < Width; y++)
                for (int x = 0; x < Height; x++)
                    sampleMean += FieldValues[x, y];

            sampleMean /= Width * Height;

            return sampleMean;
        }

        public float GetVariance(float sampleMean)
        {
            float sampleVariance = 0;

            for (int y = 0; y < Width; y++)
                for (int x = 0; x < Height; x++)
                    sampleVariance += (float)Math.Pow(FieldValues[x, y] - sampleMean, 2);

            sampleVariance /= Width * Height - 1;

            return sampleVariance;
        }

        internal void ZeroEdges()
        {
            for (int y = 0; y < Height; y++)
            {
                FieldValues[0, y] = 0;
                FieldValues[Width - 1, y] = 0;
            }

            for (int x = 0; x < Width; x++)
            {
                FieldValues[x, 0] = 0;
                FieldValues[x, Height - 1] = 0;
            }
        }

        public void Blot(float value)
        {
            float[,] output = new float[Width, Height];

            for (int y = 0; y < Width; y++)
                for (int x = 0; x < Height; x++)
                    output[x, y] = FieldValues[x, y];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (FieldValues[x, y] == value)
                        for (int k = 0; k < 8; k++)
                        {
                            int sx = x + XD[k];
                            int sy = y + YD[k];

                            if (0 < sx && sx < Width && 0 < sy && sy < Height)
                            {
                                output[sx, sy] = 0;
                            }
                        }

            FieldValues = output;
        }

        public void Contract(float target, float threshold, int selection, float replacement)
        {
            float[,] output = new float[Width, Height];

            for (int y = 0; y < Width; y++)
                for (int x = 0; x < Height; x++)
                    output[x, y] = FieldValues[x, y];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (FieldValues[x, y] == target)
                    {
                        int count = 0;

                        for (int k = 0; k < 8; k++)
                        {
                            int sx = x + XD[k];
                            int sy = y + YD[k];

                            if (0 < sx && sx < Width && 0 < sy && sy < Height)
                                if (FieldValues[sx, sy] >= threshold)
                                    count++;

                            if (count >= selection)
                                output[x, y] = replacement;
                        }
                    }

            FieldValues = output;
        }

        public void Invert()
        {
            float[,] output = new float[Width, Height];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    output[x, y] = 1 - FieldValues[x, y];

            FieldValues = output;
        }

        public void Mask(Field fMask)
        {
            float[,] output = new float[Width, Height];

            float[,] mask = fMask.FieldValues;

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (mask[x, y] > 0)
                        output[x, y] = FieldValues[x, y];

            FieldValues = output;
        }

        public void Rescale(float magnitude, float floor)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    FieldValues[x, y] *= magnitude;
                    FieldValues[x, y] += floor;
                }
        }

        public string ComputeMaskedStatistics(Field maskField)
        {
            float mean = ComputeMaskedMean(maskField);
            float variance = ComputeMaskedVariance(mean, maskField);
            float skewness = ComputeMaskedSkewness(mean, variance, maskField);
            float kurtosis = ComputeMaskedKurtosis(mean, variance, maskField);

            string output = "";
            output += mean.ToString() + "\t";
            output += variance.ToString() + "\t";
            output += skewness.ToString() + "\t";
            output += kurtosis.ToString() + "\t";
            return output;
        }

        public float ComputeMaskedMean(Field maskField)
        {
            float[,] maskingValues = maskField.FieldValues;

            float mean = 0;
            int counter = 0;

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (maskingValues[x, y] > 0)
                    {
                        counter++;
                        mean += FieldValues[x, y];
                    }

            mean /= counter;
            return mean;
        }
        public float ComputeMaskedVariance(float mean, Field maskField)
        {
            float[,] maskingValues = maskField.FieldValues;

            float variance = 0;
            int counter = 0;

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (maskingValues[x, y] > 0)
                    {
                        counter++;
                        variance += (float)Math.Pow(FieldValues[x, y] - mean, 2);
                    }

            variance /= counter;
            return variance;
        }
        public float ComputeMaskedSkewness(float mean, float variance, Field maskField)
        {
            float[,] maskingValues = maskField.FieldValues;

            float skewness = 0;
            int counter = 0;

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (maskingValues[x, y] > 0)
                    {
                        counter++;
                        skewness += (float)Math.Pow(FieldValues[x, y] - mean, 3f);
                    }

            skewness /= counter;
            skewness /= (float)Math.Pow(variance, 1.5f);
            return skewness;
        }
        public float ComputeMaskedKurtosis(float mean, float variance, Field maskField)
        {
            float[,] maskingValues = maskField.FieldValues;

            float kurtosis = 0;
            int counter = 0;

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (maskingValues[x, y] > 0)
                    {
                        counter++;
                        kurtosis += (float)Math.Pow(FieldValues[x, y] - mean, 4f);
                    }

            kurtosis /= counter;
            kurtosis /= (float)Math.Pow(variance, 2f);
            return kurtosis;
        }

        public float[] ComputeSubdivisionMeans(int xHeight, int xWidth)
        {
            List<float> means = new List<float>();

            int xcell = Width / xWidth;
            int ycell = Height / xHeight;

            for (int y = 0; y < xHeight; y++)
                for (int x = 0; x < xWidth; x++)
                {
                    float mean = 0;

                    for (int y2 = 0; y2 < ycell; y2++)
                        for (int x2 = 0; x2 < xcell; x2++)
                            mean += FieldValues[x * xcell + x2, y * ycell + y2];

                    mean /= xcell * ycell;

                    means.Add(mean);
                }

            return means.ToArray();
        }
        public float[] ComputeSubdivisionVariances(int xWidth, int xHeight, float[] means)
        {
            List<float> variances = new List<float>();

            int xcell = Width / xWidth;
            int ycell = Height / xHeight;

            for (int y = 0; y < xHeight; y++)
                for (int x = 0; x < xWidth; x++)
                {
                    float variance = 0;

                    for (int y2 = 0; y2 < ycell; y2++)
                        for (int x2 = 0; x2 < xcell; x2++)
                            variance += (float)Math.Pow(FieldValues[x * xcell + x2, y * ycell + y2] - means[x + y * xWidth], 2);

                    variance /= xcell * ycell;

                    variances.Add(variance);
                }

            return variances.ToArray();
        }
        public float[] ComputeSubdivisionSkewness(int xWidth, int xHeight, float[] means, float[] variances)
        {
            List<float> skewnesses = new List<float>();

            int xcell = Width / xWidth;
            int ycell = Height / xHeight;

            for (int y = 0; y < xHeight; y++)
                for (int x = 0; x < xWidth; x++)
                {
                    float skewness = 0;

                    for (int y2 = 0; y2 < ycell; y2++)
                        for (int x2 = 0; x2 < xcell; x2++)
                            skewness += (float)Math.Pow(FieldValues[x * xcell + x2, y * ycell + y2] - means[x + y * xWidth], 3);

                    skewness /= xcell * ycell;
                    skewness /= (float)Math.Pow(variances[x + y * xWidth], 1.5f);

                    skewnesses.Add(skewness);
                }

            return skewnesses.ToArray();
        }
        public float[] ComputeSubdivisionKurtosis(int xWidth, int xHeight, float[] means, float[] variances)
        {
            List<float> kurtosises = new List<float>();

            int xcell = Width / xWidth;
            int ycell = Height / xHeight;

            for (int y = 0; y < xHeight; y++)
                for (int x = 0; x < xWidth; x++)
                {
                    float kurtosis = 0;

                    for (int y2 = 0; y2 < ycell; y2++)
                        for (int x2 = 0; x2 < xcell; x2++)
                            kurtosis += (float)Math.Pow(FieldValues[x * xcell + x2, y * ycell + y2] - means[x + y * xWidth], 4f);

                    kurtosis /= xcell * ycell;
                    kurtosis /= (float)Math.Pow(variances[x + y * xWidth], 2f);

                    kurtosises.Add(kurtosis);
                }

            return kurtosises.ToArray();
        }
    }
}
