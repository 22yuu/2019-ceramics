using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ceramics_test
{
    class FuzzyBinarization
    {
        private int width, height;
        private int X_mid, X_min = int.MaxValue, X_max = int.MinValue; // 픽셀 화소값(중간, 작은값, 큰값)
        private int a_max, a_min;    // 밝기 조정률
        private double I_mid;
        private int I_max, I_min;    // 최대, 중간, 최소 밝기값
        private static double[] U;

        public Bitmap f_binarization(Bitmap roiBitmap, double a_cut)
        {
            int X_sum = 0;
            int D_max, D_min;

            width = roiBitmap.Width;
            height = roiBitmap.Height;

            // Step #1. 화소값들의 중간값을 구함
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // ROI영역이 Gray Scale을 적용시킨 상태라서 R,G,B의 R값만 비교
                    X_max = Math.Max(X_max, roiBitmap.GetPixel(x, y).R);
                    X_min = Math.Min(X_min, roiBitmap.GetPixel(x, y).R);
                    X_sum += roiBitmap.GetPixel(x, y).R;
                }
            }
            X_mid = (int)(X_sum / (width * height));

            //Step #2. 거리계산
            D_max = Math.Abs(X_max - X_mid);
            D_min = Math.Abs(X_mid - X_min);

            //Step #3. 밝기 조정률(a값)을 이용하여 최대 밝기값, 최소밝기값을 계산
            if (X_mid > (255 / 2.0)) X_mid = 255 - X_mid;

            if (D_min > X_mid) a_min = X_mid;
            else a_min = D_min;

            if (D_max > X_mid) a_max = X_mid;
            else a_max = D_max;

            I_max = X_mid + a_max;
            I_min = X_mid - a_min;
            I_mid = (I_max + I_min) / 2.0;

            U = new double[256];

            for (int x = 0; x < 256; x++)
            {
                U[x] = 0;
            }

            for (int x = I_min; x <= I_max; x++)
            {
                if ((X_mid <= I_min) || (X_mid >= I_max)) U[x] = 0;
                else if (X_mid > I_mid) U[x] = (I_max - X_mid) / (I_max - I_mid);
                else if (X_mid < I_mid) U[x] = (X_mid - I_min) / (I_mid - I_min);
                else if (X_mid == I_mid) U[x] = 1;
            }

            int color_value;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    color_value = roiBitmap.GetPixel(x, y).R;
                    if (U[color_value] >= a_cut) roiBitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    else roiBitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                }
            }

            return roiBitmap;
        }


    }
}
