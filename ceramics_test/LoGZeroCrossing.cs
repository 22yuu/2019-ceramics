using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ceramics_test
{
    class LoGZeroCrossing
    {
        Bitmap logBitmap;
        int width;
        int height;

        public Bitmap calculate(int[,] grayArray)
        {
            width = grayArray.GetLength(1);
            height = grayArray.GetLength(0);

            double[,] gaussianLaplacianMaskArray = { // σ = 1.4
                { 0.0, 0.0, 3.0,   2.0,   2.0,   2.0, 3.0, 0.0, 0.0 },
                { 0.0, 2.0, 3.0,   5.0,   5.0,   5.0, 3.0, 2.0, 0.0 },
                { 3.0, 3.0, 5.0,   3.0,   0.0,   3.0, 5.0, 3.0, 3.0 },
                { 2.0, 5.0, 3.0, -12.0, -23.0, -12.0, 3.0, 5.0, 2.0 },
                { 2.0, 5.0, 0.0, -23.0, -40.0, -23.0, 0.0, 5.0, 2.0 },
                { 2.0, 5.0, 3.0, -12.0, -23.0, -12.0, 3.0, 5.0, 2.0 },
                { 3.0, 3.0, 5.0,   3.0,   0.0,   3.0, 5.0, 3.0, 3.0 },
                { 0.0, 2.0, 3.0,   5.0,   5.0,   5.0, 3.0, 2.0, 0.0 },
                { 0.0, 0.0, 3.0,   2.0,   2.0,   2.0, 3.0, 0.0, 0.0 }
            };
            int[,] gaussianLaplacianArray = ConvolveEdgeNoBias(grayArray, gaussianLaplacianMaskArray);
            int[,] resultArray = ZeroCrossing(gaussianLaplacianArray, 3, 3);

            logBitmap = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    logBitmap.SetPixel(x, y, Color.FromArgb(resultArray[y,x], resultArray[y, x], resultArray[y, x]));
                }
            }

            return logBitmap;
        }

        private int[,] ConvolveEdgeNoBias(int[,] grayArray, double[,] maskArray)
        {
            int maskWidth = maskArray.GetLength(1);
            int maskHeight = maskArray.GetLength(0);
            int[,] resultArray = new int[height, width];
            int xPadding = maskWidth / 2;
            int yPadding = maskHeight / 2;
            double summary;
            for (int y = 0; y < height - 2 * yPadding; y++)
            {
                for (int x = 0; x < width - 2 * xPadding; x++)
                {
                    summary = 0.0;
                    for (int r = 0; r < maskHeight; r++)
                    {
                        for (int c = 0; c < maskWidth; c++)
                        {
                            summary += grayArray[y + r, x + c] * maskArray[r, c];
                        }
                    }
                    resultArray[y + yPadding, x + xPadding] = (int)summary;
                }
            }
            for (int y = 0; y < yPadding; y++)
            {
                for (int x = xPadding; x < width - xPadding; x++)
                {
                    resultArray[y, x] = resultArray[yPadding, x];
                    resultArray[height - 1 - y, x] = resultArray[height - 1 - yPadding, x];
                }
            }
            for (int x = 0; x < xPadding; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    resultArray[y, x] = resultArray[y, xPadding];
                    resultArray[y, width - 1 - x] = resultArray[y, width - 1 - xPadding];
                }
            }
            return resultArray;
        }

        int[,] ZeroCrossing(int[,] sourceArray, int maskWidth, int maskHeight) // 영교차
        {
            int sourceWidth = sourceArray.GetUpperBound(1) + 1;
            int sourceHeight = sourceArray.GetUpperBound(0) + 1;
            int[,] resultArray = new int[sourceHeight, sourceWidth];
            int xPadding = maskWidth / 2;
            int yPadding = maskHeight / 2;
            int[] targetArray = new int[maskHeight * maskWidth];
            for (int y = 0; y < sourceHeight - 2 * yPadding; y++)
            {
                for (int x = 0; x < sourceWidth - 2 * xPadding; x++)
                {
                    int index = 0;
                    for (int r = 0; r < maskHeight; r++)
                    {
                        for (int c = 0; c < maskWidth; c++)
                        {
                            targetArray[index++] = sourceArray[y + r, x + c];
                        }
                    }
                    resultArray[y + yPadding, x + xPadding] = Calculate(targetArray, index);
                }
            }
            for (int y = 0; y < yPadding; y++)
            {
                for (int x = xPadding; x < sourceWidth - xPadding; x++)
                {
                    resultArray[y, x] = resultArray[yPadding, x];
                    resultArray[sourceHeight - 1 - y, x] = resultArray[sourceHeight - 1 - yPadding, x];
                }
            }
            for (int x = 0; x < xPadding; x++)
            {
                for (int y = 0; y < sourceHeight; y++)
                {
                    resultArray[y, x] = resultArray[y, xPadding];
                    resultArray[y, sourceWidth - 1 - x] = resultArray[y, sourceWidth - 1 - xPadding];
                }
            }
            return resultArray;
        }

        int Calculate(int[] targetArray, int targetLength)
        {
            int middle = targetLength / 2;
            if (targetArray[middle] * targetArray[1] < 0 || targetArray[middle] * targetArray[middle - 1] < 0)
            {
                return 255;
            }
            return 0;
        }
    }
}
