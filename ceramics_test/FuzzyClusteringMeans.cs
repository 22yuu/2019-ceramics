using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ceramics_test
{
    class FuzzyClusteringMeans
    {
        public static int CLUSTER = 7;
        public static int DATA;
        private const double error = 0.0001;
        public static double[,] weight;
        public static double[,] old_weight;
        public static double[,] final_weight;
        public static double[] v;
        public static double[,] d;
        public static double[] roiArray;
        public static int[,] C_weight;
        StringBuilder sb = new StringBuilder();
        public static double e;
        private double a_cut;

        public Bitmap clustering(Bitmap roiBitmap)
        {
            int Width = roiBitmap.Width;
            int Height = roiBitmap.Height;
            roiArray = new double[Height * Width];

            int index =0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    roiArray[index++] = roiBitmap.GetPixel(x, y).R;
                }
            }

            DATA = Width * Height;
            //COORD = pattern.GetLength(1);
            weight = new double[DATA, CLUSTER];
            old_weight = new double[DATA, CLUSTER];
            final_weight = new double[DATA, CLUSTER];
            v = new double[CLUSTER];
            d = new double[DATA, CLUSTER];
            C_weight = new int[DATA, CLUSTER];

            int cycle = 0;

            Initialize_weight();

            do
            {
                calculate_v();

                change_weight();

                check_error();

                cycle++;

            } while (e > error);

            Console.WriteLine("최종 cycle 횟수 : " + cycle);

            //Console.WriteLine("▼최종 가중치(weight)▼");
            //for (int data = 0; data < DATA; data++)
            //{
            //    Console.Write(data + "의 weight = ");
            //    for (int cluster = 0; cluster < CLUSTER; cluster++)
            //    {
            //        if (weight[data, cluster] >= 0.99)
            //        {
            //            Console.Write(Math.Truncate(weight[data, cluster] * 10) / 10 + ", ");
            //            C_weight[data, cluster] = 1;
            //        }
            //        else if(weight[data, cluster] <= 0.1)
            //        {
            //            Console.Write(Math.Truncate(weight[data, cluster] * 10) / 10 + ", ");
            //            C_weight[data, cluster] = 0;
            //        }
            //        else
            //        {
            //            Console.Write(Math.Truncate(weight[data, cluster] * 10) / 10+", ");
            //        }
            //    }
            //    Console.WriteLine();
            //}

            for (int i = 0; i < CLUSTER; i++)
            {
                Console.WriteLine((i+1)+"번째 중심값 : " + v[i]);

            }

            /* 그냥 클러스터 구분하기위해 색깔 입힌거
            Random random = new Random();
            int ran;
            int maxindex = 0;
            double max = 0;
            index = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    max = double.MinValue;
                    for (int i = 0; i < CLUSTER; i++)
                    {
                        if(max < weight[index, i])
                        {
                            max = weight[index, i];
                            maxindex = i;
                        }
                        max = Math.Max(max, weight[index, i]);
                    }
                    index++;
                    ran = random.Next(0,256);
                    if(maxindex == 0)
                    {
                        roiBitmap.SetPixel(x, y, Color.Red);
                    }
                    else if(maxindex == 1)
                    {
                        roiBitmap.SetPixel(x, y, Color.Orange);
                    }
                    else if(maxindex == 2)
                    {
                        roiBitmap.SetPixel(x, y, Color.Yellow);
                    }
                    else if(maxindex == 3)
                    {
                        roiBitmap.SetPixel(x, y, Color.Green);
                    }
                    else if(maxindex == 4)
                    {
                        roiBitmap.SetPixel(x, y, Color.Blue);
                    }
                    else if (maxindex == 5)
                    {
                        roiBitmap.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        roiBitmap.SetPixel(x, y, Color.Purple);
                    }
                }
            }
            */

            // 퍼지이진화에서 a_cut기준을 잡기위해 각 클러스터의 중심색깔의 평균을 구함.
            double sum = 0.0;
            for (int cluster = 0; cluster < CLUSTER; cluster++)
            {
                sum += v[cluster];
            }
            a_cut = sum / CLUSTER;
            a_cut /= 255; // 색갈을 1.0이하로 잡기위해 255로 나눔.

            return roiBitmap;
        }

        private void Initialize_weight()
        {
            int max_weight;
            int random;

            Random r = new Random();

            for (int data = 0; data < DATA; data++) // 초기 가중치 설정
            {
                max_weight = 100000;
                for (int cluster = 0; cluster < CLUSTER; cluster++)
                {
                    if (cluster == (CLUSTER - 1) && max_weight != 0)
                    {
                        weight[data, cluster] = max_weight * 0.00001;
                    }
                    else
                    {
                        random = r.Next(0, max_weight + 1); // 0~ 100000 난수
                        max_weight -= random;
                        weight[data, cluster] = random * 0.00001;
                    }
                }
            }
        }

        private void calculate_v()
        {
            double p_weight, p2_weight;

            for (int data = 0; data < DATA; data++)
            {
                for (int cluster = 0; cluster < CLUSTER; cluster++)
                {
                    old_weight[data, cluster] = weight[data, cluster];
                }
            }

            final_weight = old_weight;

            for (int cluster = 0; cluster < CLUSTER; cluster++)
            {
                p_weight = 0.0;
                p2_weight = 0.0;

                for (int data = 0; data < DATA; data++)
                {
                    p_weight += Math.Pow(weight[data, cluster], 2) * roiArray[data];
                    p2_weight += Math.Pow(weight[data, cluster], 2);
                }
                v[cluster] = p_weight / p2_weight;
            }
        }

        private void change_weight()
        {
            double d_value, sum;

            for (int cluster = 0; cluster < CLUSTER; cluster++)
            {
                for (int data = 0; data < DATA; data++)
                {
                    d_value = 0.0;
                    d_value += Math.Pow(roiArray[data] - v[cluster], 2);
                    d[data, cluster] = 1 / d_value;
                }
            }

            for (int data = 0; data < DATA; data++)
            {
                for (int cluster1 = 0; cluster1 < CLUSTER; cluster1++)
                {
                    sum = 0.0;
                    for (int clutser2 = 0; clutser2 < CLUSTER; clutser2++)
                    {
                        sum += d[data, clutser2];
                    }
                    weight[data, cluster1] = d[data, cluster1] / sum;
                }
            }
        }

        private void check_error()
        {
            e = 0.0;
            for (int data = 0; data < DATA; data++)
            {
                for (int cluster = 0; cluster < CLUSTER; cluster++)
                {
                    e += Math.Abs(weight[data, cluster] - old_weight[data, cluster]);
                }
            }
        }

        public double return_a_cut()
        {
            return a_cut;
        }
    }
}
