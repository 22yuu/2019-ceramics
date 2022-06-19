using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ceramics_test
{
    class CannyEdge
    {
        Bitmap gBitmap;
        public int[,] ConvolveCannyOperator(int[,] grayArray, Bitmap testBitmap) // 캐니 에지 추출
        {
            gBitmap = testBitmap;
            Bitmap bitmap = new Bitmap(gBitmap);
            double[,] gaussianMaskArray = {
                { 1.0/273.0,  4.0/273.0,  7.0/273.0,  4.0/273.0, 1.0/273.0 },
                { 4.0/273.0, 16.0/273.0, 16.0/273.0, 16.0/273.0, 4.0/273.0 },
                { 7.0/273.0, 26.0/273.0, 41.0/273.0, 26.0/273.0, 7.0/273.0 },
                { 4.0/273.0, 16.0/273.0, 16.0/273.0, 16.0/273.0, 4.0/273.0 },
                { 1.0/273.0,  4.0/273.0,  7.0/273.0,  4.0/273.0, 1.0/273.0 }
            };
            double[,] sobelXMaskArray = { { -1.0, 0.0, 1.0 }, { -2.0, 0.0, 2.0 }, { -1.0, 0.0, 1.0 } };
            double[,] sobelYMaskArray = { { -1.0, -2.0, -1.0 }, { 0.0, 0.0, 0.0 }, { 1.0, 2.0, 1.0 } };
            int[,] gaussianArray = ConvolveEdge(grayArray, gaussianMaskArray);
            int[,] sobelXArray = ConvolveEdgeNoBias(gaussianArray, sobelXMaskArray);
            int[,] sobelYArray = ConvolveEdgeNoBias(gaussianArray, sobelYMaskArray);
            double[,] angleArray = new double[bitmap.Height, bitmap.Width];
            double[,] magnitudeArray = new double[bitmap.Height, bitmap.Width];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    angleArray[y, x] = Math.Atan2(sobelYArray[y, x], sobelXArray[y, x]);
                    magnitudeArray[y, x] = Math.Sqrt(sobelXArray[y, x] * sobelXArray[y, x] + sobelYArray[y, x] * sobelYArray[y, x]);
                }
            }
            int[,] NonMaxArray = ConvolveCannyOperationNonMaximum(angleArray, magnitudeArray, bitmap.Width, bitmap.Height, 3, 3);
            int[,] thresholdArray = ConvolveCannyOperatorThreshold(NonMaxArray, bitmap.Width, bitmap.Height, 3, 3, 30, 200);
            return thresholdArray;
        }

        private int[,] ConvolveEdge(int[,] grayArray, double[,] maskArray)
        {
            Bitmap bitmap = new Bitmap(gBitmap);
            int maskWidth = maskArray.GetUpperBound(1) + 1;
            int maskHeight = maskArray.GetUpperBound(0) + 1;
            int[,] resultArray = new int[bitmap.Height, bitmap.Width];
            int xPadding = maskWidth / 2;
            int yPadding = maskHeight / 2;
            double summary;
            for (int y = 0; y < bitmap.Height - 2 * yPadding; y++)
            {
                for (int x = 0; x < bitmap.Width - 2 * xPadding; x++)
                {
                    summary = 0.0;
                    for (int r = 0; r < maskHeight; r++)
                    {
                        for (int c = 0; c < maskWidth; c++)
                        {
                            summary += grayArray[y + r, x + c] * maskArray[r, c];
                        }
                    }
                    summary = Math.Abs(summary);
                    resultArray[y + yPadding, x + xPadding] = (int)Math.Max(0.0, Math.Min(255.0, summary));
                }
            }
            for (int y = 0; y < yPadding; y++)
            {
                for (int x = xPadding; x < bitmap.Width - xPadding; x++)
                {
                    resultArray[y, x] = resultArray[yPadding, x];
                    resultArray[bitmap.Height - 1 - y, x] = resultArray[bitmap.Height - 1 - yPadding, x];
                }
            }
            for (int x = 0; x < xPadding; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    resultArray[y, x] = resultArray[y, xPadding];
                    resultArray[y, bitmap.Width - 1 - x] = resultArray[y, bitmap.Width - 1 - xPadding];
                }
            }
            return resultArray;
        }

        private int[,] ConvolveEdgeNoBias(int[,] grayArray, double[,] maskArray)
        {
            Bitmap bitmap = new Bitmap(gBitmap);
            int maskWidth = maskArray.GetUpperBound(1) + 1;
            int maskHeight = maskArray.GetUpperBound(0) + 1;
            int[,] resultArray = new int[bitmap.Height, bitmap.Width];
            int xPadding = maskWidth / 2;
            int yPadding = maskHeight / 2;
            double summary;
            for (int y = 0; y < bitmap.Height - 2 * yPadding; y++)
            {
                for (int x = 0; x < bitmap.Width - 2 * xPadding; x++)
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
                for (int x = xPadding; x < bitmap.Width - xPadding; x++)
                {
                    resultArray[y, x] = resultArray[yPadding, x];
                    resultArray[bitmap.Height - 1 - y, x] = resultArray[bitmap.Height - 1 - yPadding, x];
                }
            }
            for (int x = 0; x < xPadding; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    resultArray[y, x] = resultArray[y, xPadding];
                    resultArray[y, bitmap.Width - 1 - x] = resultArray[y, bitmap.Width - 1 - xPadding];
                }
            }
            return resultArray;
        }

        private int[,] ConvolveCannyOperationNonMaximum(double[,] angleArray, double[,] magnitudeArray, int Width, int Height, int maskWidth, int maskHeight)
        {
            int[,] resultArray = new int[Height, Width];
            int xPadding = maskWidth / 2;
            int yPadding = maskHeight / 2;
            double[] targetAngleArray = new double[maskHeight * maskWidth];
            double[] targetMagnitudeArray = new double[maskHeight * maskWidth];
            for (int y = 0; y < Height - 2 * yPadding; y++)
            {
                for (int x = 0; x < Width - 2 * xPadding; x++)
                {
                    int index = 0;
                    for (int r = 0; r < maskHeight; r++)
                    {
                        for (int c = 0; c < maskWidth; c++)
                        {
                            targetAngleArray[index] = angleArray[y + r, x + c];
                            targetMagnitudeArray[index++] = magnitudeArray[y + r, x + c];
                        }
                    }
                    resultArray[y + yPadding, x + xPadding] = SuppressNonMaximum(targetAngleArray, targetMagnitudeArray, index);
                }
            }
            for (int y = 0; y < yPadding; y++)
            {
                for (int x = xPadding; x < Width - xPadding; x++)
                {
                    resultArray[y, x] = resultArray[yPadding, x];
                    resultArray[Height - 1 - y, x] = resultArray[Height - 1 - yPadding, x];
                }
            }
            for (int x = 0; x < xPadding; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    resultArray[y, x] = resultArray[y, xPadding];
                    resultArray[y, Width - 1 - x] = resultArray[y, Width - 1 - xPadding];
                }
            }
            return resultArray;
        }

        private int SuppressNonMaximum(double[] targetAngleArray, double[] targetMagnitudeArray, int targetLength)
        {
            int middle = targetLength / 2;
            double iAngle1;
            double iAngle2;
            double iAngle3;
            double iAngle4;
            double cAngle;
            double range = Math.PI / 8.0;
            for (int i = 0; i < middle; i++)
            {
                if (targetMagnitudeArray[middle] > targetMagnitudeArray[i] && targetMagnitudeArray[middle] > targetMagnitudeArray[targetLength - 1 - i])
                {
                    iAngle1 = Math.PI / 4.0 * (i + 1);
                    iAngle2 = Math.PI / 4.0 * (i + 1) + Math.PI;
                    iAngle3 = Math.PI / 4.0 * i - 3.0 * Math.PI / 4.0;
                    iAngle4 = Math.PI / 4.0 * i - 3.0 * Math.PI / 4.0 - Math.PI;
                    cAngle = targetAngleArray[middle];
                    if ((cAngle - range) < iAngle1 && iAngle1 < (cAngle + range))
                    {
                        return (int)targetMagnitudeArray[middle];
                    }
                    else if ((cAngle - range) < iAngle2 && iAngle2 < (cAngle + range))
                    {
                        return (int)targetMagnitudeArray[middle];
                    }
                    else if ((cAngle - range) < iAngle3 && iAngle3 < (cAngle + range))
                    {
                        return (int)targetMagnitudeArray[middle];
                    }
                    else if ((cAngle - range) < iAngle4 && iAngle4 < (cAngle + range))
                    {
                        return (int)targetMagnitudeArray[middle];
                    }
                }
            }
            return 0;
        }

        private int[,] ConvolveCannyOperatorThreshold(int[,] angleArray, int sourceWidth, int sourceHeight, int maskWidth, int maskHeight, int lower, int upper)
        {
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
                            targetArray[index++] = angleArray[y + r, x + c];
                        }
                    }
                    resultArray[y + yPadding, x + xPadding] = LimitThreshold(targetArray, index, lower, upper);
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

        private int LimitThreshold(int[] targetArray, int targetLength, int lower, int upper)
        {
            int middle = targetLength / 2;
            if (targetArray[middle] > upper)
            {
                return 255;
            }
            else if (targetArray[middle] < lower)
            {
                return 0;
            }
            else
            {
                for (int i = 0; i < targetLength; i++)
                {
                    if (targetArray[i] >= targetArray[middle])
                    {
                        return 255;
                    }
                }
            }
            return 0;
        }
    }
}
