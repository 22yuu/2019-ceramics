using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ceramics_test
{
    class ROI
    {
        Color color;
        int width;
        int height;
        Bitmap hBitmap; // histogram 출력용
        Bitmap darkBitmap; // darkhistogram 후 저장용
        int[] histo;
        int start_px, end_px;
        int return_cut_height;

        //test용
        Bitmap bitmap;

        public Bitmap Extraction(Bitmap sBitmap, int[,] sourceArray, Bitmap gBitmap)
        {
            bitmap = gBitmap;
            darkBitmap = darkhistogram(sBitmap, sourceArray);

            sourceArray = new int[darkBitmap.Height, darkBitmap.Width];

            for (int y = 0; y < darkBitmap.Height; y++)
            {
                for (int x = 0; x < darkBitmap.Width; x++)
                {
                    sourceArray[y, x] = darkBitmap.GetPixel(x, y).R;
                }
            }

            darkBitmap = brighthistogram(darkBitmap, sourceArray); // test용
            return bitmap/*brighthistogram(darkBitmap, sourceArray)*/;
        }

        public Bitmap darkhistogram(Bitmap sourceBitmap, int[,] sourceArray)
        {
            width = sourceArray.GetLength(1);
            height = sourceArray.GetLength(0);
            hBitmap = new Bitmap(width, height);
            histo = new int[height];

            for (int y = 0; y < height; y++)
            {
                histo[y] = 0;
                for (int x = 0; x < width; x++)
                {
                    if (sourceArray[y, x] > 50)
                    {
                        histo[y]++;
                    }
                }
            }

            /*histogram 출력용
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    hBitmap.SetPixel(x, y, Color.FromArgb(125, 0, 125));
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < histo[y]; x++)
                {
                    hBitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                }
            }
            */

            start_px = 0;
            end_px = 0;

            for (int y = 0; y < height; y++)
            {
                if (histo[y] != 0)
                {
                    start_px = y;
                    break;
                }
            }

            if (start_px == 0)
            {
                for (int y = start_px; y < height; y++)
                {
                    if (histo[y] == 0)
                    {
                        end_px = y;
                        break;
                    }
                }
            }
            else
            {
                start_px -= 1;
                end_px = height;
            }

            return_cut_height = start_px;

            bitmap = bitmap.Clone(new Rectangle(0, start_px, width, end_px - start_px), bitmap.PixelFormat);

            return sourceBitmap.Clone(new Rectangle(0, start_px, width, end_px - start_px), sourceBitmap.PixelFormat);
        }

        private Bitmap brighthistogram(Bitmap sourceBitmap, int[,] sourceArray)
        {
            width = sourceArray.GetLength(1);
            height = sourceArray.GetLength(0);
            hBitmap = new Bitmap(width, height);
            histo = new int[height];

            for (int y = 0; y < height; y++)
            {
                histo[y] = 0;
                for (int x = 0; x < width; x++)
                {
                    if (y + 5 < height && Math.Abs(sourceArray[y, x] - sourceArray[y+5,x]) > 20 && sourceArray[y,0] > 20)
                    {
                        histo[y]++;
                    }
                }
                if (histo[y] < 30) histo[y] = 0;
            }

            /*그냥 수평 histogram 출력용*/
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        hBitmap.SetPixel(x, y, Color.FromArgb(125, 0, 125));
            //    }
            //}

            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < histo[y]; x++)
            //    {
            //        hBitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
            //    }
            //}
            /*그냥 수평 histogram 출력용*/

            /*roi최종적으로 추출하는 부분*/
            int max_value = 0;
            int max_px = 0;


            if (start_px == 0)
            {
                for (int y = 0; y < height; y++)
                {
                    if (histo[y] > 30 && max_value < histo[y])
                    {
                        max_value = histo[y];
                        max_px = y;

                        int max = 0;
                        for (int i = max_px; i <= max_px + 3; i++)
                        {
                            max = Math.Max(histo[i], max);
                        }

                        if (max_value == max) break;
                    }
                }
                //max_px += 7;
                return_cut_height = max_px;
                sourceBitmap = sourceBitmap.Clone(new Rectangle(0, max_px, width, end_px - max_px), sourceBitmap.PixelFormat);
                bitmap = bitmap.Clone(new Rectangle(0, max_px, width, end_px - max_px), bitmap.PixelFormat);
            }
            else
            {
                for (int y = height-1; y >= 0; y--)
                {
                    if (histo[y] > 30 && max_value < histo[y])
                    {
                        max_value = histo[y];
                        max_px = y;

                        int max = 0;
                        for (int i = max_px; i >= max_px - 3; i--)
                        {
                            max = Math.Max(histo[i], max);
                        }

                        if (max_value == max) break;
                    }
                }
                //max_px -= 4;
                max_px += 5;
                sourceBitmap = sourceBitmap.Clone(new Rectangle(0, 0, width, max_px ), sourceBitmap.PixelFormat);
                bitmap = bitmap.Clone(new Rectangle(0, 0, width, max_px + 5), bitmap.PixelFormat);
            }
            /*roi최종적으로 추출하는 부분*/

            return /*hBitmap*/sourceBitmap;
        }

        public int return_cut_px()
        {
            return return_cut_height;
        }

        public Bitmap Extraction_8(Bitmap testBitmap, int[,] sourceArray, Bitmap gBitmap)
        {
            bitmap = gBitmap;
            darkBitmap = darkhistogram_8(testBitmap, sourceArray);

            //sourceArray = new int[darkBitmap.Height, darkBitmap.Width];

            //for (int y = 0; y < darkBitmap.Height; y++)
            //{
            //    for (int x = 0; x < darkBitmap.Width; x++)
            //    {
            //        sourceArray[y, x] = darkBitmap.GetPixel(x, y).R;
            //    }
            //}

            //darkBitmap = brighthistogram(darkBitmap, sourceArray); // test용
            return darkBitmap/*bitmap*//*brighthistogram(darkBitmap, sourceArray)*/;
        }

        public Bitmap darkhistogram_8(Bitmap sourceBitmap, int[,] sourceArray)
        {
            width = sourceArray.GetLength(1);
            height = sourceArray.GetLength(0);
            hBitmap = new Bitmap(width, height);
            histo = new int[height];

            for (int y = 0; y < height; y++)
            {
                histo[y] = 0;
                for (int x = 0; x < width; x++)
                {
                    if (sourceArray[y, x] > 30)
                    {
                        histo[y]++;
                    }
                }
            }

            start_px = 0;
            end_px = height;

            if (histo[start_px] != 0)
            {
                for (int y = 0; y < height; y++)
                {
                    if (histo[y] < 10)
                    {
                        end_px = y;
                        break;
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    if (histo[y] + 15 >= width)
                    {
                        start_px = y;
                        break;
                    }
                }
            }

            return_cut_height = start_px;

            return sourceBitmap.Clone(new Rectangle(0, start_px, width, end_px - start_px), sourceBitmap.PixelFormat);
        }

        public Bitmap brighthistogram_8(int[,] sourceArray, Bitmap roiBitmap)
        {
            width = sourceArray.GetLength(1);
            height = sourceArray.GetLength(0);
            hBitmap = new Bitmap(width, height);
            histo = new int[height];

            for (int y = 0; y < height; y++)
            {
                histo[y] = 0;
                for (int x = 0; x < width; x++)
                {
                    if (sourceArray[y, x] == 255)
                    {
                        histo[y]++;
                    }
                }
                if (histo[y] <= 100) histo[y] = 0;
            }

            int cut_px = 0;

            if (start_px == 0)
            {
                for (int y = height - 5; y >= 0; y--)
                {
                    if (histo[y] == 0)
                    {
                        cut_px = y;
                        break;
                    }
                }
                return_cut_height = cut_px;
                roiBitmap = roiBitmap.Clone(new Rectangle(0, cut_px, width, end_px - cut_px), roiBitmap.PixelFormat);
            }
            else
            {
                for (int y = 5; y < height; y++)
                {
                    if (histo[y] == 0)
                    {
                        cut_px = y;
                        break;
                    }
                }
                roiBitmap = roiBitmap.Clone(new Rectangle(0, 0, width, cut_px), roiBitmap.PixelFormat);
            }

            return roiBitmap;
        }
    }
}
