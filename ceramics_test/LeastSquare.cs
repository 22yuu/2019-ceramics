using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramics_test
{
    class LeastSquare
    {
        Color color;

        public Bitmap algebraic(Bitmap bitmap)
        {
            int w = bitmap.Width, h = bitmap.Height;
            int[,] A = new int[w * h, 3];
            int[,] AT = new int[3, w * h];
            int[] B = new int[w * h];
            double[] X = new double[3];
            double[,] ResultArray = new double[w, h];
            int t = 0;
            double total = 0.0;

            for (int k = 0; k < 3; k++)
            {
                t = 0;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (k == 0)
                        {
                            A[t, k] = x;
                            AT[k, t] = x;
                        }
                        if (k == 1)
                        {
                            A[t, k] = y;
                            AT[k, t] = y;

                            color = bitmap.GetPixel(x, y);
                            ResultArray[x, y] = color.R;
                            B[t] = color.R;
                            total += color.R;
                        }
                        if (k == 2)
                        {
                            A[t, k] = 1;
                            AT[k, t] = 1;
                        }
                        t++;
                    }
                }
            }

            total = total / (w * h);

            X = calculate(A, AT, B);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    ResultArray[x, y] = Math.Abs(ResultArray[x, y] - ((x * X[0]) + (y * X[1]) + X[2]) + total);
                    if (ResultArray[x, y] < 0)
                    {
                        ResultArray[x, y] = 0;
                    }
                    if (ResultArray[x, y] > 255)
                    {
                        ResultArray[x, y] = 255;
                    }
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    color = Color.FromArgb((int)ResultArray[x, y], (int)ResultArray[x, y], (int)ResultArray[x, y]);
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }

        public double[] calculate(int[,] a, int[,] at, int[] b)
        {
            int w1 = a.GetLength(1), h1 = a.GetLength(0);
            int w2 = at.GetLength(1), h2 = at.GetLength(0);
            double[,] inverse = new double[h2, w1];
            double[,] inverseT = new double[h2, w1];
            double[,] resultA = new double[h2, w2];
            double[] xb = new double[3];
            double det = 0;

            for (int y = 0; y < h2; y++)
            {
                for (int x = 0; x < w1; x++)
                {
                    for (int k = 0; k < h1; k++)
                    {
                        inverse[y, x] += at[y, k] * a[k, x];
                    }
                }
            }

            //여인수, 수반행렬 동시 작업
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (y == 0)
                    {
                        if (x == 0)
                        {
                            inverseT[y, x] = (1) * ((inverse[1, 1] * inverse[2, 2]) - (inverse[1, 2] * inverse[2, 1]));
                        }
                        if (x == 1)
                        {
                            inverseT[y, x] = (-1) * ((inverse[0, 1] * inverse[2, 2]) - (inverse[0, 2] * inverse[2, 1]));
                        }
                        if (x == 2)
                        {
                            inverseT[y, x] = (1) * ((inverse[0, 1] * inverse[1, 2]) - (inverse[0, 2] * inverse[1, 1]));
                        }
                    }
                    if (y == 1)
                    {
                        if (x == 0)
                        {
                            inverseT[y, x] = (-1) * ((inverse[1, 0] * inverse[2, 2]) - (inverse[1, 2] * inverse[2, 0]));
                        }
                        if (x == 1)
                        {
                            inverseT[y, x] = (1) * ((inverse[0, 0] * inverse[2, 2]) - (inverse[0, 2] * inverse[2, 0]));
                        }
                        if (x == 2)
                        {
                            inverseT[y, x] = (-1) * ((inverse[0, 0] * inverse[1, 2]) - (inverse[0, 2] * inverse[1, 0]));
                        }
                    }
                    if (y == 2)
                    {
                        if (x == 0)
                        {
                            inverseT[y, x] = (1) * ((inverse[1, 0] * inverse[2, 1]) - (inverse[1, 1] * inverse[2, 0]));
                        }
                        if (x == 1)
                        {
                            inverseT[y, x] = (-1) * ((inverse[0, 0] * inverse[2, 1]) - (inverse[0, 1] * inverse[2, 0]));
                        }
                        if (x == 2)
                        {
                            inverseT[y, x] = (1) * ((inverse[0, 0] * inverse[1, 1]) - (inverse[0, 1] * inverse[1, 0]));
                        }
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                det += inverse[0, i] * inverseT[i, 0];
            }

            for (int i = 0; i < 3; i++)
            {
                for (int x = 0; x < 3; x++)
                {
                    inverseT[i, x] = inverseT[i, x] / det;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int x = 0; x < w2; x++)
                {
                    for (int k = 0; k < h2; k++)
                    {
                        resultA[i, x] += inverseT[i, k] * at[k, x];
                    }
                    xb[i] += resultA[i, x] * b[x];
                }
            }

            return xb;
        }
    }
}
