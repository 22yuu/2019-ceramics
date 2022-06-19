using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ceramics_test
{
    public partial class Form1 : Form
    {
        Image image;
        Bitmap gBitmap; // 원본영상 비트맵
        Bitmap sBitmap; // 퍼지스테칭 적용 비트맵
        Bitmap roiBitmap; // 관심영역적용 비트맵
        Bitmap lsmBitmap; // 최소자승법 적용 비트맵
        Bitmap fcmBitmap; // fcm 적용 비트맵
        Bitmap testBitmap; // 8미리 테스트용 비트맵
        int[,] grayArray; // 그레이 적용 배열
        int[,] stretchingArray; // 퍼지스트레칭 적용 배열
        int[,] testArray;
        double a_cut;
        int return_height_px;

        public Form1() // 퍼지스트레칭 -> ROI영역 추출.
        {
            InitializeComponent();
            this.Text = "세라믹";
            textBox2.ScrollBars = ScrollBars.Vertical;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Graphics gr = CreateGraphics();
            openFileDialog1.Title = "영상파일 열기";
            openFileDialog1.Filter = "All File(*.*) |*.*| Bitmap File(*.bmp) | *.bmp | Jpeg File(*.jpg) | *.jpg";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string strFilename = openFileDialog1.FileName;
                image = Image.FromFile(strFilename);

                int x, y, brightness;
                Color color, gray;

                gBitmap = new Bitmap(image, new Size(pictureBox1.Width, pictureBox1.Height));

                IppFilter ipp = new IppFilter();
                gBitmap = ipp.Diffusion(gBitmap, 0.25, 4, 10);

                grayArray = new int[gBitmap.Height, gBitmap.Width];

                for (y = 0; y < gBitmap.Height; y++)
                {
                    for (x = 0; x < gBitmap.Width; x++)
                    {
                        color = gBitmap.GetPixel(x, y);
                        brightness = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                        gray = Color.FromArgb(brightness, brightness, brightness);
                        gBitmap.SetPixel(x, y, gray);
                        grayArray[y, x] = brightness;
                    }
                }

                pictureBox1.Image = gBitmap;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Save Image";
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.Filter = "All Files(*.*) |*.*| Bitmap File(*.bmp) |*.bmp| Jpeg File(*.jpg) |*.jpg";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string strFilename = saveFileDialog1.FileName;
                string strLowerFilename = strFilename.ToLower();
                roiBitmap.Save(strLowerFilename);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void fuzzyStretchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //stretchingArray = new int[gBitmap.Height, gBitmap.Width];

            //FuzzyStretching fuzzyStretching = new FuzzyStretching();

            //sBitmap = fuzzyStretching.Stretching(gBitmap);
            //pictureBox2.Image = sBitmap;

            //for (int y = 0; y < gBitmap.Height; y++)
            //{
            //    for (int x = 0; x < gBitmap.Width; x++)
            //    {
            //        stretchingArray[y, x] = sBitmap.GetPixel(x, y).R;
            //    }
            //}

            // 그냥 roi딴거에 스트레칭하는용
            stretchingArray = new int[roiBitmap.Height, roiBitmap.Width];

            FuzzyStretching fuzzyStretching = new FuzzyStretching();

            roiBitmap = fuzzyStretching.Stretching(roiBitmap);
            pictureBox2.Image = roiBitmap;

            for (int y = 0; y < roiBitmap.Height; y++)
            {
                for (int x = 0; x < roiBitmap.Width; x++)
                {
                    stretchingArray[y, x] = roiBitmap.GetPixel(x, y).R;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "8mm";

            ROI roi = new ROI();

            //roiBitmap = new Bitmap(roi.Extraction_8(testBitmap, testArray, gBitmap));
            roiBitmap = new Bitmap(roi.darkhistogram_8(gBitmap, grayArray));
            //pictureBox3.Image = roiBitmap;

            int[,] tempArray = new int[roiBitmap.Height, roiBitmap.Width];
            for (int y = 0; y < roiBitmap.Height; y++)
            {
                for (int x = 0; x < roiBitmap.Width; x++)
                {
                    tempArray[y, x] = roiBitmap.GetPixel(x, y).R;
                }
            }

            double[,] mask = { { -1.0, -1.0, -1.0 }, { -1.0, 8.0, -1.0 }, { -1.0, -1.0, -1.0 } };
            double[,] gmask = { { 2.0/115.0, 4.0/115.0, 5.0/115.0, 5.0/115.0, 2.0/115.0},
                                { 4.0/115.0, 9.0/115.0, 12.0/115.0, 9.0/115.0, 4.0/115.0},
                                { 5.0/115.0, 12.0/115.0, 15.0/115.0, 12.0/115.0, 5.0/115.0},
                                { 4.0/115.0, 9.0/115.0, 12.0/115.0, 9.0/115.0, 4.0/115.0},
                                { 2.0/115.0, 4.0/115.0, 5.0/115.0, 5.0/115.0, 2.0/115.0}};

            int[,] blurArray = convolve2(tempArray, roiBitmap.Width, roiBitmap.Height, gmask, 5, 5);
            int[,] LaplacianArray = convolveNoBias(blurArray, roiBitmap.Width, roiBitmap.Height, mask, 3, 3);
            int[,] ResultArray = convolveLp(LaplacianArray, roiBitmap.Width, roiBitmap.Height, 3, 3);
            displayResultArray1(ResultArray, roiBitmap.Width, roiBitmap.Height);

            roiBitmap = roi.brighthistogram_8(testArray, roiBitmap);
            return_height_px = roi.return_cut_px();

            pictureBox2.Image = roiBitmap;


            // test
            // 최소자승법
            LeastSquare LS = new LeastSquare();
            roiBitmap = LS.algebraic(roiBitmap);

            // 퍼지스트레칭
            FuzzyStretching fuzzyStretching = new FuzzyStretching();
            roiBitmap = fuzzyStretching.Stretching(roiBitmap);

            // FCM
            FuzzyClusteringMeans fcm = new FuzzyClusteringMeans();
            fcmBitmap = fcm.clustering(roiBitmap);
            a_cut = fcm.return_a_cut();

            // 퍼지이진화
            FuzzyBinarization fuzzybinari = new FuzzyBinarization();
            roiBitmap = fuzzybinari.f_binarization(roiBitmap, a_cut);

            // 결함 표시
            pictureBox2.Image = displayFaulty(gBitmap, roiBitmap, roiBitmap.Width, roiBitmap.Height);


            sb.AppendLine("8mm 추출완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        StringBuilder sb = new StringBuilder();
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "10mm";
            displayFaulty_10_11();
            sb.AppendLine("10mm 추출완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "11mm";
            displayFaulty_10_11();
            sb.AppendLine("11mm 추출완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Text = "16mm";
            displayFaulty_16_22();
            sb.AppendLine("16mm 추출완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = "22mm";
            displayFaulty_16_22();
            sb.AppendLine("22mm 추출완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void rOI추출ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stretchingArray = new int[gBitmap.Height, gBitmap.Width];

            FuzzyStretching fuzzyStretching = new FuzzyStretching();

            sBitmap = fuzzyStretching.Stretching(gBitmap);
            pictureBox2.Image = sBitmap;

            for (int y = 0; y < gBitmap.Height; y++)
            {
                for (int x = 0; x < gBitmap.Width; x++)
                {
                    stretchingArray[y, x] = sBitmap.GetPixel(x, y).R;
                }
            }

            ROI roi = new ROI();
            //pictureBox2.Image = roi.histogram(stretchingArray); // 10,11,16,22 mm 용 roi
            //pictureBox2.Image = roi.Extraction(sBitmap, stretchingArray);
            //pictureBox2.Image = roi.histogram(grayArray); // 8mm용 roi(8mm는 스트레칭 하지 않고 roi영역 추출)            
            //pictureBox2.Image = roi.Extraction(gBitmap);

            /*histogram test*/
            //pictureBox1.Image = roi.darkhistogram(sBitmap, stretchingArray);
            //roiBitmap = roi.Extraction(sBitmap, stretchingArray);
            roiBitmap = roi.Extraction(sBitmap, stretchingArray, gBitmap);
            return_height_px = roi.return_cut_px();

            pictureBox2.Image = roiBitmap;
        }

        private void dBSCANToolStripMenuItem_Click(object sender, EventArgs e)  // 8mm 임시 테스트용
        {
            ROI roi = new ROI();

            //roiBitmap = new Bitmap(roi.Extraction_8(testBitmap, testArray, gBitmap));
            roiBitmap = new Bitmap(roi.darkhistogram_8(gBitmap, grayArray));
            //pictureBox3.Image = roiBitmap;

            int[,] tempArray = new int[roiBitmap.Height, roiBitmap.Width];
            for (int y = 0; y < roiBitmap.Height; y++)
            {
                for (int x = 0; x < roiBitmap.Width; x++)
                {
                    tempArray[y, x] = roiBitmap.GetPixel(x, y).R;
                }
            }

            double[,] mask = { { -1.0, -1.0, -1.0 }, { -1.0, 8.0, -1.0 }, { -1.0, -1.0, -1.0 } };
            double[,] gmask = { { 2.0/115.0, 4.0/115.0, 5.0/115.0, 5.0/115.0, 2.0/115.0},
                                { 4.0/115.0, 9.0/115.0, 12.0/115.0, 9.0/115.0, 4.0/115.0},
                                { 5.0/115.0, 12.0/115.0, 15.0/115.0, 12.0/115.0, 5.0/115.0},
                                { 4.0/115.0, 9.0/115.0, 12.0/115.0, 9.0/115.0, 4.0/115.0},
                                { 2.0/115.0, 4.0/115.0, 5.0/115.0, 5.0/115.0, 2.0/115.0}};

            int[,] blurArray = convolve2(tempArray, roiBitmap.Width, roiBitmap.Height, gmask, 5, 5);
            int[,] LaplacianArray = convolveNoBias(blurArray, roiBitmap.Width, roiBitmap.Height, mask, 3, 3);
            int[,] ResultArray = convolveLp(LaplacianArray, roiBitmap.Width, roiBitmap.Height, 3, 3);
            displayResultArray1(ResultArray, roiBitmap.Width, roiBitmap.Height);

            roiBitmap = roi.brighthistogram_8(testArray, roiBitmap);
            return_height_px = roi.return_cut_px();



            //pictureBox2.Image = roiBitmap;

            // 퍼지스트레칭
            FuzzyStretching fuzzyStretching = new FuzzyStretching();
            roiBitmap = fuzzyStretching.Stretching(roiBitmap);

            // 최소자승법
            LeastSquare LS = new LeastSquare();
            roiBitmap = LS.algebraic(roiBitmap);

            // FCM
            FuzzyClusteringMeans fcm = new FuzzyClusteringMeans();
            fcmBitmap = fcm.clustering(roiBitmap);
            a_cut = fcm.return_a_cut();

            // 퍼지이진화
            FuzzyBinarization fuzzybinari = new FuzzyBinarization();
            roiBitmap = fuzzybinari.f_binarization(roiBitmap, a_cut);

            // 결함 표시
            pictureBox3.Image = displayFaulty(gBitmap, roiBitmap, roiBitmap.Width, roiBitmap.Height);
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e) // 
        {
            LoGZeroCrossing log = new LoGZeroCrossing();
            pictureBox2.Image = log.calculate(grayArray);
        }

        private void 최소자승법배경밝기제거ToolStripMenuItem_Click(object sender, EventArgs e) // 배경밝기 제거
        {
            /*
            LeastSquareMethod LSM = new LeastSquareMethod();
            //lsmBitmap = LSM.calculate(roiBitmap);
            roiBitmap = LSM.calculate(roiBitmap);
            //lsmBitmap = LSM.calculate(gBitmap);
            pictureBox2.Image = roiBitmap;
            */

            LeastSquare LS = new LeastSquare();
            roiBitmap = LS.algebraic(roiBitmap);
            pictureBox2.Image = roiBitmap;
            //lsmBitmap = LS.algebraic(gBitmap);
            //pictureBox2.Image = lsmBitmap;
        }

        private void fCMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FuzzyClusteringMeans fcm = new FuzzyClusteringMeans();
            fcmBitmap = fcm.clustering(roiBitmap);
            a_cut = fcm.return_a_cut();
            pictureBox2.Image = fcmBitmap;
        }

        private void fuzzyBinarizationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FuzzyBinarization fuzzybinari = new FuzzyBinarization();
            roiBitmap = fuzzybinari.f_binarization(roiBitmap, a_cut);
            pictureBox2.Image = roiBitmap;
        }

        private void 결함표시ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            pictureBox2.Image = displayFaulty(gBitmap, roiBitmap, roiBitmap.Width, roiBitmap.Height);
        }

        private Bitmap displayFaulty(Bitmap gBitmap, Bitmap Bitmap, int Width, int Height)
        {
            Bitmap bitmap = new Bitmap(gBitmap);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (roiBitmap.GetPixel(x, y).R == 255)
                    {
                        bitmap.SetPixel(x, return_height_px + y, Color.FromArgb(255, 0, 0));
                    }
                }
            }

            return bitmap;
        }

        private void displayFaulty_16_22()  // 퍼지스트레칭 -> 최소자승법 -> FCM -> 퍼지이진화
        {
            // ROI추출
            stretchingArray = new int[gBitmap.Height, gBitmap.Width];

            FuzzyStretching fuzzyStretching = new FuzzyStretching();

            sBitmap = fuzzyStretching.Stretching(gBitmap);
            pictureBox2.Image = sBitmap;

            for (int y = 0; y < gBitmap.Height; y++)
            {
                for (int x = 0; x < gBitmap.Width; x++)
                {
                    stretchingArray[y, x] = sBitmap.GetPixel(x, y).R;
                }
            }

            ROI roi = new ROI();
            roiBitmap = roi.Extraction(sBitmap, stretchingArray, gBitmap);
            return_height_px = roi.return_cut_px();

            // 퍼지스트레칭
            roiBitmap = fuzzyStretching.Stretching(roiBitmap);

            // 최소자승법
            LeastSquare LS = new LeastSquare();
            roiBitmap = LS.algebraic(roiBitmap);

            // FCM
            FuzzyClusteringMeans fcm = new FuzzyClusteringMeans();
            fcmBitmap = fcm.clustering(roiBitmap);
            a_cut = fcm.return_a_cut();

            // 퍼지이진화
            FuzzyBinarization fuzzybinari = new FuzzyBinarization();
            roiBitmap = fuzzybinari.f_binarization(roiBitmap, a_cut);

            // 결함 표시
            pictureBox2.Image = displayFaulty(gBitmap, roiBitmap, roiBitmap.Width, roiBitmap.Height);
        }

        private void displayFaulty_10_11() // 최소자승법 -> 퍼지스트레칭 -> FCM -> 퍼지이진화
        {
            // ROI추출
            stretchingArray = new int[gBitmap.Height, gBitmap.Width];

            FuzzyStretching fuzzyStretching = new FuzzyStretching();

            sBitmap = fuzzyStretching.Stretching(gBitmap);
            pictureBox2.Image = sBitmap;

            for (int y = 0; y < gBitmap.Height; y++)
            {
                for (int x = 0; x < gBitmap.Width; x++)
                {
                    stretchingArray[y, x] = sBitmap.GetPixel(x, y).R;
                }
            }

            ROI roi = new ROI();
            roiBitmap = roi.Extraction(sBitmap, stretchingArray, gBitmap);
            return_height_px = roi.return_cut_px();

            // 최소자승법
            LeastSquare LS = new LeastSquare();
            roiBitmap = LS.algebraic(roiBitmap);

            // 퍼지스트레칭
            roiBitmap = fuzzyStretching.Stretching(roiBitmap);

            // FCM
            FuzzyClusteringMeans fcm = new FuzzyClusteringMeans();
            fcmBitmap = fcm.clustering(roiBitmap);
            a_cut = fcm.return_a_cut();

            // 퍼지이진화
            FuzzyBinarization fuzzybinari = new FuzzyBinarization();
            roiBitmap = fuzzybinari.f_binarization(roiBitmap, a_cut);

            // 결함 표시
            pictureBox2.Image = displayFaulty(gBitmap, roiBitmap, roiBitmap.Width, roiBitmap.Height);
        }



        void displayResultArray1(int[,] resultArray, int Width, int Height)
        {
            int x, y;
            Color color;
            Bitmap bitmap = new Bitmap(Width, Height);
            testArray = new int[Height, Width];
            for (y = 0; y < Height; y++)
                for (x = 0; x < Width; x++)
                {
                    testArray[y, x] = resultArray[y, x];
                    color = Color.FromArgb(resultArray[y, x], resultArray[y, x], resultArray[y, x]);
                    bitmap.SetPixel(x, y, color);
                }
            testBitmap = new Bitmap(bitmap);

            //pictureBox2.Image = testBitmap;
            Invalidate();
        }

        private void sobelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] mask1 = { { -1.0, 0.0, 1.0 }, { -2.0, 0.0, 2.0 }, { -1.0, 0.0, 1.0 } };
            double[,] mask2 = { { -1.0, -2.0, -1.0 }, { 0.0, 0.0, 0.0 }, { 1.0, 2.0, 1.0 } };
            int[,] ResultArray = convolveXY(grayArray, gBitmap.Width, gBitmap.Height, mask1, mask2, 3, 3);
            displayResultArray1(ResultArray, gBitmap.Width, gBitmap.Height);
        }

        int[,] convolveXY(int[,] G, int Width, int Height, double[,] M1, double[,] M2, int maskCol, int maskRow)
        {
            int[,] R = new int[Height, Width];
            int x, y, r, c;
            int xPad = maskCol / 2;
            int yPad = maskRow / 2;
            double sum, sum1, sum2;
            for (y = 0; y < Height - 2 * yPad; y++)
                for (x = 0; x < Width - 2 * xPad; x++)
                {
                    sum1 = sum2 = 0.0;
                    for (r = 0; r < maskRow; r++)
                        for (c = 0; c < maskCol; c++)
                        {
                            sum1 += G[y + r, x + c] * M1[r, c];
                            sum2 += G[y + r, x + c] * M2[r, c];
                        }

                    sum = Math.Abs(sum1) + Math.Abs(sum2);
                    if (sum > 255.0) sum = 255.0;
                    if (sum < 0.0) sum = 0.0;
                    R[y + yPad, x + xPad] = (int)sum;
                }

            /* 영상의 가장자리는 아무 변화율이 없다. 가장자리의 중심값을 구해서 영상의 가장자리 부분도 회선을 시킨다???*/
            for (y = 0; y < yPad; y++)
            {
                for (x = 0; x < xPad; x++)
                {
                    R[y, x] = R[yPad, x];
                    R[Height - 1 - y, x] = R[Height - 1 - yPad, x];
                }
            }
            for (x = 0; x < xPad; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    R[y, x] = R[y, xPad];
                    R[y, Width - 1 - x] = R[y, Width - 1 - xPad];
                }
            }
            return R;
        }

        private void 스무딩라플라시안ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] mask = { { -1.0, -1.0, -1.0 }, { -1.0, 8.0, -1.0 }, { -1.0, -1.0, -1.0 } };
            double[,] gmask = { { 2.0/115.0, 4.0/115.0, 5.0/115.0, 5.0/115.0, 2.0/115.0},
                                { 4.0/115.0, 9.0/115.0, 12.0/115.0, 9.0/115.0, 4.0/115.0},
                                { 5.0/115.0, 12.0/115.0, 15.0/115.0, 12.0/115.0, 5.0/115.0},
                                { 4.0/115.0, 9.0/115.0, 12.0/115.0, 9.0/115.0, 4.0/115.0},
                                { 2.0/115.0, 4.0/115.0, 5.0/115.0, 5.0/115.0, 2.0/115.0}};

            int[,] blurArray = convolve2(grayArray, gBitmap.Width, gBitmap.Height, gmask, 5, 5);
            int[,] LaplacianArray = convolveNoBias(blurArray, gBitmap.Width, gBitmap.Height, mask, 3, 3);
            int[,] ResultArray = convolveLp(LaplacianArray, gBitmap.Width, gBitmap.Height, 3, 3);
            displayResultArray1(ResultArray, gBitmap.Width, gBitmap.Height);
        }

        int[,] convolve2(int[,] G, int Width, int Height, double[,] M, int maskCol, int maskRow)
        {
            int[,] R = new int[Height, Width];
            int x, y, r, c;
            int xPad = maskCol / 2;
            int yPad = maskRow / 2;
            double sum;
            for (y = 0; y < Height - 2 * yPad; y++)
                for (x = 0; x < Width - 2 * xPad; x++)
                {
                    sum = 0.0;
                    for (r = 0; r < maskRow; r++)
                        for (c = 0; c < maskCol; c++)
                            sum += G[y + r, x + c] * M[r, c];
                    sum = Math.Abs(sum);
                    if (sum > 255.0) sum = 255.0;
                    if (sum < 0.0) sum = 0.0;
                    R[y + yPad, x + xPad] = (int)sum;
                }

            /* 영상의 가장자리는 아무 변화율이 없다. 가장자리의 중심값을 구해서 영상의 가장자리 부분도 회선을 시킨다???*/
            for (y = 0; y < yPad; y++)
            {
                for (x = 0; x < xPad; x++)
                {
                    R[y, x] = R[yPad, x];
                    R[Height - 1 - y, x] = R[Height - 1 - yPad, x];
                }
            }
            for (x = 0; x < xPad; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    R[y, x] = R[y, xPad];
                    R[y, Width - 1 - x] = R[y, Width - 1 - xPad];
                }
            }
            return R;
        }

        int[,] convolveNoBias(int[,] G, int Width, int Height, double[,] M, int maskCol, int maskRow)
        {
            int[,] R = new int[Height, Width];
            int x, y;
            int r, c;
            int xPad = maskCol / 2;
            int yPad = maskRow / 2;
            double sum;
            for (y = 0; y < Height - 2 * yPad; y++)
                for (x = 0; x < Width - 2 * xPad; x++)
                {
                    sum = 0.0;
                    for (r = 0; r < maskRow; r++)
                        for (c = 0; c < maskCol; c++)
                            sum += G[y + r, x + c] * M[r, c];
                    R[y + yPad, x + xPad] = (int)sum;
                }
            for (y = 0; y < yPad; y++)
            {
                for (x = xPad; x < Width - xPad; x++)
                {
                    R[y, x] = R[yPad, x];
                    R[Height - 1 - y, x] = R[Height - 1 - yPad, x];
                }
            }
            for (x = 0; x < xPad; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    R[y, x] = R[y, xPad];
                    R[Height - 1 - y, x] = R[Height - 1 - yPad, x];
                }
            }
            return R;
        }

        int[,] convolveLp(int[,] G, int Width, int Height, int maskCol, int maskRow)
        {
            int[,] R = new int[Height, Width];
            int x, y;
            int r, c;
            int xPad = maskCol / 2;
            int yPad = maskRow / 2;
            int[] target = new int[maskRow * maskCol];

            for (y = 0; y < Height - 2 * yPad; y++)
                for (x = 0; x < Width - 2 * xPad; x++)
                {
                    int index = 0;
                    for (r = 0; r < maskRow; r++)
                        for (c = 0; c < maskCol; c++)
                            target[index++] = G[y + r, x + c];
                    R[y + yPad, x + xPad] = zeroCrossing(target, index);
                }
            for (x = 0; x < xPad; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    R[y, x] = R[yPad, x];
                    R[Height - 1 - y, x] = R[Height - 1 - yPad, x];
                }
            }
            for (x = 0; x < xPad; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    R[y, x] = R[y, xPad];
                    R[Height - 1 - y, x] = R[Height - 1 - yPad, x];
                }
            }
            return R;
        }

        int zeroCrossing(int[] target, int tsize)
        {
            int mid = tsize / 2;
            if (target[mid] * target[1] < 0 || target[mid] * target[mid - 1] < 0)
            {
                return 255;
            }
            return 0;
        }

        private void 라플라시안ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] mask = { { -1.0, -1.0, -1.0 }, { -1.0, 8.0, -1.0 }, { -1.0, -1.0, -1.0 } };
            int[,] ResultArray = convolve2(grayArray, gBitmap.Width, gBitmap.Height, mask, 3, 3);
            displayResultArray1(ResultArray, gBitmap.Width, gBitmap.Height);
        }

        private void 프리윗ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[,] mask1 = { { -1.0, 0.0, 1.0 }, { -1.0, 0.0, 1.0 }, { -1.0, 0.0, 1.0 } };
            double[,] mask2 = { { -1.0, -1.0, -1.0 }, { 0.0, 0.0, 0.0 }, { 1.0, 1.0, 1.0 } };
            int[,] ResultArray = convolveXY(grayArray, gBitmap.Width, gBitmap.Height, mask1, mask2, 3, 3);
            displayResultArray1(ResultArray, gBitmap.Width, gBitmap.Height);
        }

        private void cannyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CannyEdge canny = new CannyEdge();
            int[,] cannyArray;
            DisplayResultArray(cannyArray = canny.ConvolveCannyOperator(grayArray, gBitmap));
        }

        void DisplayResultArray(int[,] resultArray) // 이진 영상 출력
        {
            Bitmap bitmap = new Bitmap(gBitmap);
            Color color;
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    color = Color.FromArgb(resultArray[y, x], resultArray[y, x], resultArray[y, x]);
                    bitmap.SetPixel(x, y, color);
                }
            }
            pictureBox2.Image = bitmap;
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double lambda = double.Parse(toolStripTextBox1.Text);
            double k = double.Parse(toolStripTextBox2.Text);
            int iter = int.Parse(toolStripTextBox3.Text);

            if (lambda < 0 || lambda > 0.25)
            {
                MessageBox.Show("0.0~0.25 사이값을 입력하세요.");
            }
            else if (iter < 0 || iter > 100)
            {
                MessageBox.Show("0~100 사이의 값을 입력하세요.");
            }
            else
            {
                IppFilter ipp = new IppFilter();
                gBitmap = ipp.Diffusion(gBitmap, lambda, k, iter);
                pictureBox2.Image = gBitmap;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // ROI추출
            stretchingArray = new int[gBitmap.Height, gBitmap.Width];

            FuzzyStretching fuzzyStretching = new FuzzyStretching();

            sBitmap = fuzzyStretching.Stretching(gBitmap);
            pictureBox2.Image = sBitmap;

            for (int y = 0; y < gBitmap.Height; y++)
            {
                for (int x = 0; x < gBitmap.Width; x++)
                {
                    stretchingArray[y, x] = sBitmap.GetPixel(x, y).R;
                }
            }

            ROI roi = new ROI();
            roiBitmap = roi.Extraction(sBitmap, stretchingArray, gBitmap);
            return_height_px = roi.return_cut_px();
            pictureBox2.Image = roiBitmap;

            sb.AppendLine("ROI 추출완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // 최소자승법
            LeastSquare LS = new LeastSquare();
            roiBitmap = LS.algebraic(roiBitmap);
            pictureBox2.Image = roiBitmap;

            sb.AppendLine("최소자승법 적용완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            // 퍼지스트레칭
            FuzzyStretching fuzzyStretching = new FuzzyStretching();
            roiBitmap = fuzzyStretching.Stretching(roiBitmap);
            pictureBox2.Image = roiBitmap;

            sb.AppendLine("퍼지스트레칭 적용완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // 퍼지스트레칭
            FuzzyStretching fuzzyStretching = new FuzzyStretching();
            roiBitmap = fuzzyStretching.Stretching(roiBitmap);
            pictureBox2.Image = roiBitmap;

            sb.AppendLine("퍼지스트레칭 적용완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            // 최소자승법
            LeastSquare LS = new LeastSquare();
            roiBitmap = LS.algebraic(roiBitmap);
            pictureBox2.Image = roiBitmap;

            sb.AppendLine("최소자승법 적용완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // FCM
            FuzzyClusteringMeans fcm = new FuzzyClusteringMeans();
            fcmBitmap = fcm.clustering(roiBitmap);
            a_cut = fcm.return_a_cut();
            pictureBox2.Image = roiBitmap;

            sb.AppendLine("FCM 적용완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // 퍼지이진화
            FuzzyBinarization fuzzybinari = new FuzzyBinarization();
            roiBitmap = fuzzybinari.f_binarization(roiBitmap, a_cut);
            pictureBox2.Image = roiBitmap;

            sb.AppendLine("퍼지이진화 적용완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            // 결함 표시
            pictureBox2.Image = displayFaulty(gBitmap, roiBitmap, roiBitmap.Width, roiBitmap.Height);

            sb.AppendLine("결함 추출완료.");
            textBox2.Text = sb.ToString();
            textBox2.Select(textBox2.Text.Length, 0);
            textBox2.ScrollToCaret();
        }

        private void otsuBinarizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = roiBitmap;
           
            int h = roiBitmap.Height;
            int w = roiBitmap.Width;

            int row = 0, col = 0, index = 0;
            int[] histo = new int[256];
            int T = 0;
            int[,] result_image = new int[h, w];

            for (index = 0; index < 256; index++)
            {
                histo[index] = 0;
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    histo[roiBitmap.GetPixel(x,y).R]++;
                }
            }

            double Wb = 0.0;
            double Wf = 0.0;
            double Mb = 0.0;
            double Mf = 0.0;
            int Sb = 0;
            int Sf = 0;

            for (index = 0; index < 256; index++)
            {
                Sf = Sf + index * histo[index];
            }

            int Cb = histo[0];
            int Cf = (h * w) - histo[0];

            double max_var = 0.0;
            double variance = 0.0;


            for (index = 0; index < 256; index++)
            {
                Cb = Cb + histo[index];
                Cf = Cf - histo[index];

                if (Cb == 0 || Cf == 0)
                {
                    continue;
                }

                Sb = Sb + index * histo[index];
                Sf = Sf - index * histo[index];

                Mb = (double)Sb / Cb;
                Mf = (double)Sf / Cf;

                Wb = (double)Cb / (h * w);
                Wf = (double)Cf / (h * w);

                variance = Wb * Wf * Math.Pow((Mb - Mf), 2.0);

                if (variance > max_var)
                {
                    T = index;
                    max_var = variance;
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (roiBitmap.GetPixel(x, y).R < T)
                        result_image[y, x] = 0;
                    else
                        result_image[y, x] = 255;
                }
            }
            
            Color color;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    color = Color.FromArgb(result_image[y, x], result_image[y, x], result_image[y, x]);
                    roiBitmap.SetPixel(x, y, color);
                }
            }

            pictureBox3.Image = displayFaulty(gBitmap, roiBitmap, roiBitmap.Width, roiBitmap.Height);

            pictureBox2.Image = roiBitmap;
        }

        private void maxminBinarizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap maxminBitmap = new Bitmap(roiBitmap);
            //gBitmap14 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            int[,] binaryimg = new int[roiBitmap.Width, roiBitmap.Height];

            int x, y, i, j;
            int max = 0;
            int min = 255;


            for (x = 0; x < roiBitmap.Height; x++)
            {
                for (y = 0; y < roiBitmap.Width; y++)
                {
                    if (roiBitmap.GetPixel(y, x).R >= max)
                        max = roiBitmap.GetPixel(y, x).R;
                    if (roiBitmap.GetPixel(y, x).R <= min)
                        min = roiBitmap.GetPixel(y, x).R;
                }
            }

            for (x = 0; x < roiBitmap.Height; x++)
            {
                for (y = 0; y < roiBitmap.Width; y++)
                {
                    if (roiBitmap.GetPixel(y, x).R >= max)
                        binaryimg[y, x] = 255;
                    else
                        binaryimg[y, x] = 0;
                }
            }
            for (i = 0; i < roiBitmap.Height; i++)
            {
                for (j = 0; j < roiBitmap.Width; j++)
                {
                    //m_OutputImage[i*m_Re_width + j] = (unsigned char)m_tempImage[i,j];
                    byte c = (byte)binaryimg[j, i];

                    Color color = Color.FromArgb(c, c, c);
                    roiBitmap.SetPixel(j, i, color);
                }
            }

            pictureBox2.Image = roiBitmap;
        }

        private void 기존FuzzyBinarizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int X_sum = 0;
            int D_max, D_min;
            int X_mid, X_min = int.MaxValue, X_max = int.MinValue;
            int a_max, a_min;
            double I_mid;
            int I_max, I_min;
            double[] U;

            int width = roiBitmap.Width;
            int height = roiBitmap.Height;

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
                    if (U[color_value] >= 0.6) roiBitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    else roiBitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                }
            }

            pictureBox3.Image = displayFaulty(gBitmap, roiBitmap, roiBitmap.Width, roiBitmap.Height);
        }
    }
}
