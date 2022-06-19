using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ceramics_test
{
    class LeastSquareMethod // 1차 평면 -> f(x,y) = ax + by + c
    {
        public Bitmap calculate(Bitmap roiBitmap)
        {
            int width = roiBitmap.Width;
            int height = roiBitmap.Height;
            double[,] lsmArray = new double[height, width];
            double[] X; // residual^2(어떤 데이터가 추정된 모델로 얼마나 떨어진 값인가를 나타냄)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    lsmArray[y, x] = roiBitmap.GetPixel(x, y).R;
                }
            }

            X = PseudoInverse(lsmArray, width, height);


            double z;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    z = (int)(X[0] * x + X[1] * y + X[2]);
                    Console.Write(z + ", ");
                    //Console.Write(z + ", " );
                    lsmArray[y, x] = lsmArray[y,x] - Math.Abs(z);
                    if (lsmArray[y, x] > 255) lsmArray[y, x] = 255.0;
                    else if (lsmArray[y, x] < 0) lsmArray[y, x] = 0.0;
                    roiBitmap.SetPixel(x,y,Color.FromArgb((int)lsmArray[y,x], (int)lsmArray[y, x], (int)lsmArray[y, x]));
                }
                Console.WriteLine();
            }


            return roiBitmap;
        }

        private double[] PseudoInverse(double[,] sampleArray, int width, int height)
        {
            double[,] A = new double[width * height, 3];
            double[,] At = new double[3, width * height];
            double[] X = new double[3];
            double[] Z = new double[width * height];
            double[,] temp1 = new double[3, 3]; // (A^t * A)
            double[] temp2 = new double[3]; // (A^t * B)
            double[,] temp3 = new double[3, 3]; // temp1^-1
            int x, y, r = 0, c = 0;

            Console.Write("width : " + width + ", height : " + height);
            Console.WriteLine();
                
            for (y = 0; y < width * height; y++)
            {
                if (c == width)
                {
                    r++; // y
                    c = 0; // x
                }
                A[y, 0] = c;
                A[y, 1] = r;
                A[y, 2] = 1;

                //Console.Write(A[y, 0] + ", " + A[y, 1] + ", " + A[y, 2]);
                //Console.WriteLine();

                Z[y] = sampleArray[r, c++];
                //Console.WriteLine("Z : " + Z[y]);
            }

            double sum1, sum2;
            Console.WriteLine("temp1 ");
            for (y = 0; y < 3; y++)  // (A^t * A)
            {
                for (x = 0; x < 3; x++)
                {
                    sum1 = 0;
                    for (r = 0; r < width * height; r++)
                    {
                        sum1 += A[r, y] * A[r, x];
                    }
                    temp1[y, x] = sum1;
                    Console.Write(temp1[y,x]+", ");
                }
                Console.WriteLine();
            }
            
            Console.WriteLine("*****D*****");

            Console.WriteLine(temp1[0, 0] * temp1[1, 1] * temp1[2, 2] + temp1[1, 0] * temp1[2, 1] * temp1[0, 2] + temp1[2, 0] * temp1[0, 1] * temp1[1, 2]);
            Console.WriteLine(temp1[0, 0] * temp1[2, 1] * temp1[1, 2] - temp1[2, 1] * temp1[1, 1] * temp1[0, 2] - temp1[1, 0] * temp1[0, 1] * temp1[2, 2]);

            //int d1, d2;
            //d1 = temp1[0, 0] * temp1[1, 1] * temp1[2, 2] + temp1[0, 1] * temp1[1, 2] * temp1[2, 0] + temp1[0, 2] * temp1[1, 0] * temp1[2, 1];
            //d2 = temp1[0, 2] * temp1[1, 1] * temp1[2, 0] - temp1[0, 1] * temp1[1, 0] * temp1[2, 2] - temp1[0, 0] * temp1[1, 2] * temp1[2, 1];
            double D;
            //D = 1.0 / (d1 - d2);
            D = 1.0 / ((temp1[0, 0] * temp1[1, 1] * temp1[2, 2]) + (temp1[1, 0] * temp1[2, 1] * temp1[0, 2]) + (temp1[2, 0] * temp1[0, 1] * temp1[1, 2]) 
                        - (temp1[0, 0] * temp1[2, 1] * temp1[1, 2]) - (temp1[2, 1] * temp1[1, 1] * temp1[0, 2]) - (temp1[1, 0] * temp1[0, 1] * temp1[2, 2]));

            temp3[0, 0] = D * (temp1[1, 1] * temp1[2, 2] - temp1[1, 2] * temp1[2, 1]);
            temp3[0, 1] = D * (temp1[0, 2] * temp1[2, 1] - temp1[0, 1] * temp1[2, 2]);
            temp3[0, 2] = D * (temp1[0, 1] * temp1[1, 2] - temp1[0, 2] * temp1[1, 1]);
            temp3[1, 0] = D * (temp1[1, 2] * temp1[2, 0] - temp1[1, 0] * temp1[2, 2]);
            temp3[1, 1] = D * (temp1[0, 0] * temp1[2, 2] - temp1[0, 2] * temp1[2, 0]);
            temp3[1, 2] = D * (temp1[0, 2] * temp1[1, 0] - temp1[0, 0] * temp1[1, 2]);
            temp3[2, 0] = D * (temp1[1, 0] * temp1[2, 1] - temp1[1, 1] * temp1[2, 0]);
            temp3[2, 1] = D * (temp1[0, 1] * temp1[2, 1] - temp1[0, 0] * temp1[2, 1]);
            temp3[2, 2] = D * (temp1[0, 0] * temp1[1, 1] - temp1[0, 1] * temp1[1, 0]);

            Console.WriteLine("D : " + D);

            Console.WriteLine("temp3 ");

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.Write(temp3[i, j] + ", ");
                }
                Console.WriteLine();
            }

            // 행렬 곱 순서대로 안한거
            //Console.WriteLine("temp2 ");
            //for (x = 0; x < 3; x++)  // (A ^ t * B)
            //{
            //    sum2 = 0;
            //    for (y = 0; y < width * height; y++)
            //    {
            //        sum2 += A[y, x] * Z[y];
            //    }
            //    temp2[x] = sum2;
            //    Console.Write(temp2[x] + " , ");
            //}
            //Console.WriteLine();

            //double sum;
            //for (x = 0; x < 3; x++)
            //{
            //    sum = 0.0;
            //    for (y = 0; y < 3; y++)
            //    {
            //        sum += temp3[x, y] * temp2[y];
            //    }
            //    X[x] = sum;
            //}


            // 여기서 부터 행렬 순서대로 곱한거
            for (x = 0; x < 3; x++)
            {
                for (y = 0; y < width * height; y++)
                {
                    At[x, y] = A[y, x];
                }
            }

            double[,] temp = new double[3, width * height];
            double sum = 0.0;
            for (x = 0; x < 3; x++)
            {
                for (y = 0; y < width * height; y++)
                {
                    sum = 0.0;
                    for (int z = 0; z < 3; z++)
                    {
                        sum += temp3[x, z] * At[z, y];
                    }
                    temp[x, y] = sum;
                }
            }

            for (x = 0; x < 3; x++)
            {
                sum = 0.0;
                for (y = 0; y < width * height; y++)
                {
                    sum += temp[x, y] * Z[y];
                }
                X[x] = sum;
            }

            Console.WriteLine("X");
            Console.Write("1 : " + X[0] + ", 2 : " + X[1] + ", 3 : " + X[2]);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            return X;
        }
    }
}
