using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramics_test
{
    class IppFilter
    {
        public Bitmap Diffusion(Bitmap bitmap, double lambda, double k, int iter)
        {
            int w = bitmap.Width, h = bitmap.Height;
            double[,] picture = new double[h, w];
            double[,] display = new double[h, w];

            //-------------------------------------------------------------------------
            // iter 횟수만큼 비등방성 확산 알고리즘 수행
            //-------------------------------------------------------------------------

            int i, x, y;
            double gradn, grads, grade, gradw;
            double gcn, gcs, gce, gcw;
            double k2 = k * k;

            for (y = 0; y < h; y++)
            {
                for (x = 0; x < w; x++)
                {
                    picture[y, x] = bitmap.GetPixel(x, y).R;
                }
            }

            for (i = 0; i < iter; i++)
            {
                for (y = 1; y < h - 1; y++)
                {
                    for (x = 1; x < w - 1; x++)
                    {
                        gradn = picture[y - 1, x] - picture[y, x];
                        grads = picture[y + 1, x] - picture[y, x];
                        grade = picture[y, x - 1] - picture[y, x];
                        gradw = picture[y, x + 1] - picture[y, x];

                        gcn = gradn / (1.0 + gradn * gradn / k2);
                        gcs = grads / (1.0 + grads * grads / k2);
                        gce = grade / (1.0 + grade * grade / k2);
                        gcw = gradw / (1.0 + gradw * gradw / k2);

                        display[y, x] = picture[y, x] + lambda * (gcn + gcs + gce + gcw);
                        bitmap.SetPixel(x, y, Color.FromArgb((int)display[y, x], (int)display[y, x], (int)display[y, x]));
                    }
                }
            }
            return bitmap;
        }
    }
}
