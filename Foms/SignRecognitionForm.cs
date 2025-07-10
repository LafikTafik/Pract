using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tesseract;
using Emgu.CV.OCR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pix = Tesseract.Pix;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NAMI.Foms
{
    public partial class SignRecognitionForm : Form
    {
        private Mat originalImage = new Mat();

        public SignRecognitionForm()
        {
            InitializeComponent();
        }

        private void roundedButton3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var image = CvInvoke.Imread(ofd.FileName, ImreadModes.Color);
                    if (image.IsEmpty)
                    {
                        MessageBox.Show("Ошибка загрузки изображения");
                        return;
                    }

                    Mat resultImage;
                    string signName = DetectTrafficSign(image, out resultImage);

                    picboxsign.Image = resultImage.ToBitmap();
                    labelSign.Text = $"Знак: {signName}";
                }
            }
        }
        private Mat Preprocess(Mat input)
        {
            // Переводим в оттенки серого
            Mat gray = new Mat();
            CvInvoke.CvtColor(input, gray, ColorConversion.Bgr2Gray);

            // Улучшаем контраст
            Mat equalized = new Mat();
            CvInvoke.EqualizeHist(gray, equalized);

            // Бинаризация — выделяем объекты
            Mat binary = new Mat();
            CvInvoke.Threshold(equalized, binary, 200, 255, ThresholdType.BinaryInv);

            // Морфологические операции для улучшения формы
            Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            CvInvoke.MorphologyEx(binary, binary, MorphOp.Open, element, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar());

            return binary;
        }

        private string DetectTrafficSign(Mat image, out Mat resultImage)
        {
            resultImage = image.Clone();

            if (image.IsEmpty)
                return "Неизвестный";

            // Предварительная обработка изображения
            Mat gray = new Mat();
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(gray, gray, new Size(5, 5), 0);
            CvInvoke.Threshold(gray, gray, 150, 255, ThresholdType.BinaryInv);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(gray, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint contour = contours[i];
                double perimeter = CvInvoke.ArcLength(contour, true);
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contour, approx, 0.04 * perimeter, true);

                int corners = approx.Size;
                Rectangle roi = CvInvoke.BoundingRectangle(approx);

                // Фильтр маленьких/некорректных контуров
                if (roi.Width < 80 || roi.Height < 80 ||
                    roi.X < 0 || roi.Y < 0 ||
                    roi.X + roi.Width > image.Cols || roi.Y + roi.Height > image.Rows)
                {
                    continue;
                }

                using (Mat cropped = new Mat())
                {
                    // Вырезаем ROI
                    int top = Math.Max(0, roi.Y);
                    int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                    int left = Math.Max(0, roi.X);
                    int right = Math.Max(0, image.Cols - roi.X - roi.Width);

                    CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));
                    using (Mat processed = Preprocess(cropped))
                    {
                        // Анализируем признаки
                        string shape = GetShape(contour, roi);
                        string color = GetDominantColor(image, roi);
                        bool hasWhiteCenter = HasWhiteCenter(cropped, roi);
                        bool hasRedBorder = HasRedBorder(cropped, roi);
                        bool isSpeedLimitNumber = HasSpeedLimitDigit(cropped, roi);
                        bool hasPedestrianPattern = HasPedestrianPattern(cropped, roi);
                        bool isInvertedTriangle = IsInvertedTriangle(approx);

                        string numberText = RecognizeSpeedLimitNumber(cropped);
                        bool isSpeedLimit = !string.IsNullOrEmpty(numberText);

                        System.Diagnostics.Debug.WriteLine($"Форма: {shape}, Цвет: {color}");
                        System.Diagnostics.Debug.WriteLine($"Белый центр: {hasWhiteCenter}, Красная окантовка: {hasRedBorder}");

                        // Проверяем по комбинации признаков
                        if (shape == "octagon" && color == "red")
                        {
                            CvInvoke.Rectangle(resultImage, roi, new MCvScalar(0, 255, 0), 2);
                            return "Стоп";
                        }

                        if (shape == "circle" && isSpeedLimit && hasRedBorder && hasWhiteCenter)
                        {
                            if (!string.IsNullOrEmpty(numberText))
                            {
                                CvInvoke.Rectangle(resultImage, roi, new MCvScalar(0, 255, 0), 2);
                                return $"Ограничение скорости {numberText}";
                            }
                        }

                        if (shape == "triangle" && color == "red" && hasWhiteCenter && isInvertedTriangle)
                        {
                            CvInvoke.Rectangle(resultImage, roi, new MCvScalar(0, 255, 0), 2);
                            return "Уступите дорогу";
                        }

                        if (shape == "square" && color == "blue" && hasPedestrianPattern)
                        {
                            CvInvoke.Rectangle(resultImage, roi, new MCvScalar(0, 255, 0), 2);
                            return "Пешеходный переход";
                        }
                    }
                }
            }

            // 🔁 НИ ОДИН ЗНАК НЕ РАСПОЗНАН → попробуй TemplateMatching
            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint contour = contours[i];
                Rectangle roi = CvInvoke.BoundingRectangle(contours[i]);

                if (roi.Width < 80 || roi.Height < 80) continue;

                using (Mat cropped = new Mat())
                {
                    int top = Math.Max(0, roi.Y);
                    int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                    int left = Math.Max(0, roi.X);
                    int right = Math.Max(0, image.Cols - roi.X - roi.Width);

                    CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                    // Теперь сравниваем с шаблонами
                    string matchedSign = MatchWithTemplates(cropped);

                    if (matchedSign != "Неизвестный")
                    {
                        CvInvoke.Rectangle(resultImage, roi, new MCvScalar(0, 255, 0), 2);
                        return matchedSign;
                    }
                }
            }

            return "Неизвестный";
        }

        private string GetShape(VectorOfPoint contour, Rectangle rect)
        {
            
            double perimeter = CvInvoke.ArcLength(contour, true);
            VectorOfPoint approx = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(contour, approx, 0.04 * perimeter, true);


            int corners = approx.Size;
            double aspectRatio = (double)rect.Width / rect.Height;

            // Проверка формы через HuMoments
            Moments moments = CvInvoke.Moments(approx);
            double[] huMoments = CvInvoke.HuMoments(moments);

            // Проверка для круга: высокая округлость + соотношение сторон близко к 1
            if (corners >= 7 && Math.Abs(aspectRatio - 1) < 0.15)
                return "circle";

            // Восьмиугольник — только если форма явно имеет 8 углов
            if (corners == 8 && Math.Abs(aspectRatio - 1) < 0.2)
                return "octagon";

            // Треугольник
            if (corners == 3)
                return "triangle";

            // Четырёхугольник
            if (corners == 4)
            {
                if (Math.Abs(aspectRatio - 1) < 0.2)
                    return "square";
                else if (aspectRatio > 1.5)
                    return "horizontal_rectangle";
                else if (aspectRatio < 0.6)
                    return "vertical_rectangle";
            }

            return "unknown";
        }


        private void roundedButton1_Click(object sender, EventArgs e)
        {
            this.Close(); // или Hide(), если хотите сохранить состояние
        }

        private int CountNonZero(Mat mask)
        {
            VectorOfPoint points = new VectorOfPoint();
            CvInvoke.FindNonZero(mask, points);
            return points.Size;
        }

        private string GetDominantColor(Mat image, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                // Корректное вырезание ROI
                int top = Math.Max(0, roi.Y);
                int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                int left = Math.Max(0, roi.X);
                int right = Math.Max(0, image.Cols - roi.X - roi.Width);

                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                if (cropped.IsEmpty)
                    return "unknown";

                // Переводим в HSV для анализа цвета
                Mat hsv = new Mat();
                CvInvoke.CvtColor(cropped, hsv, ColorConversion.Bgr2Hsv);

                // Диапазоны цветов в HSV
                ScalarArray lowerRed1 = new ScalarArray(new MCvScalar(0, 100, 100));
                ScalarArray upperRed1 = new ScalarArray(new MCvScalar(10, 255, 255));
                ScalarArray lowerRed2 = new ScalarArray(new MCvScalar(170, 100, 100));
                ScalarArray upperRed2 = new ScalarArray(new MCvScalar(180, 255, 255));

                ScalarArray lowerBlue = new ScalarArray(new MCvScalar(100, 150, 50));
                ScalarArray upperBlue = new ScalarArray(new MCvScalar(140, 255, 255));

                ScalarArray lowerWhite = new ScalarArray(new MCvScalar(0, 0, 200));
                ScalarArray upperWhite = new ScalarArray(new MCvScalar(180, 20, 255));

                ScalarArray lowerYellow = new ScalarArray(new MCvScalar(20, 100, 100));
                ScalarArray upperYellow = new ScalarArray(new MCvScalar(30, 255, 255));


                // Маски для каждого цвета
                Mat maskRed1 = new Mat(), maskRed2 = new Mat();
                CvInvoke.InRange(hsv, lowerRed1, upperRed1, maskRed1);
                CvInvoke.InRange(hsv, lowerRed2, upperRed2, maskRed2);
                CvInvoke.Add(maskRed1, maskRed2, maskRed1); // объединяем оба диапазона красного

                Mat maskBlue = new Mat();
                CvInvoke.InRange(hsv, lowerBlue, upperBlue, maskBlue);

                Mat maskWhite = new Mat();
                CvInvoke.InRange(hsv, lowerWhite, upperWhite, maskWhite);

                Mat maskYellow = new Mat();
                CvInvoke.InRange(hsv, lowerYellow, upperYellow, maskYellow);


                // Подсчёт пикселей
                int redCount = CountNonZero(maskRed1);
                int blueCount = CountNonZero(maskBlue);
                int whiteCount = CountNonZero(maskWhite);
                int yellowCount = CountNonZero(maskYellow);

                double total = roi.Width * roi.Height;

                // Нормализация по площади
                double redRatio = redCount / total;
                double blueRatio = blueCount / total;
                double whiteRatio = whiteCount / total;
                double yellowRatio = yellowCount / total;


                // Вывод отладочной информации (полезно при тестировании)
                System.Diagnostics.Debug.WriteLine($"Цветовой анализ → красный: {redRatio:F2}, синий: {blueRatio:F2}, белый: {whiteRatio:F2}");

                // Пороги могут быть разными, если нужна большая точность
                const double colorThreshold = 0.2;

                if (redRatio > colorThreshold && redCount > whiteCount && redCount > blueCount && redCount > yellowCount)
                    return "red";

                if (blueRatio > colorThreshold && blueCount > redCount && blueCount > whiteCount && blueCount > yellowCount)
                    return "blue";

                if (whiteRatio > colorThreshold && whiteCount > redCount && whiteCount > blueCount && whiteCount > yellowCount)
                    return "white";

                if (yellowRatio > colorThreshold && yellowCount > redCount && yellowCount > blueCount && yellowCount > whiteCount)
                    return "yellow";


                return "unknown";
            }
        }


        private bool IsInvertedTriangle(VectorOfPoint approx)
        {
            if (approx.Size != 3)
                return false;

            Point p1 = approx[0];
            Point p2 = approx[1];
            Point p3 = approx[2];

            // Проверяем, что одна из вершин находится в самом низу
            int lowestY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
            bool isBottomPoint = (p1.Y == lowestY || p2.Y == lowestY || p3.Y == lowestY);

            // Вычисляем длины всех сторон
            double lengthA = Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            double lengthB = Math.Sqrt((p2.X - p3.X) * (p2.X - p3.X) + (p2.Y - p3.Y) * (p2.Y - p3.Y));
            double lengthC = Math.Sqrt((p1.X - p3.X) * (p1.X - p3.X) + (p1.Y - p3.Y) * (p1.Y - p3.Y));

            double baseLength = Math.Min(Math.Min(lengthA, lengthB), lengthC);

            return isBottomPoint && baseLength < Math.Max(lengthA, Math.Max(lengthB, lengthC)) * 0.8;
        }

        private bool HasDirectionArrow(Mat image, Rectangle rect)
        {
            using (Mat cropped = new Mat())
            {

                CvInvoke.CopyMakeBorder(image, cropped, rect.Y, image.Rows - rect.Bottom, rect.X, image.Cols - rect.Width, BorderType.Constant, new MCvScalar(0));

                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(gray, gray, 200, 255, ThresholdType.Binary);

                VectorOfVectorOfPoint arrowContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(gray, arrowContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < arrowContours.Size; i++)
                {
                    VectorOfPoint approx = new VectorOfPoint();
                    double peri = CvInvoke.ArcLength(arrowContours[i], true);
                    CvInvoke.ApproxPolyDP(arrowContours[i], approx, 0.04 * peri, true);

                    if (approx.Size >= 5 && approx.Size <= 10)
                        return true;
                }

                return false;
            }
        }

        private bool HasStraightArrow(Mat image, Rectangle rect)
        {
            using (Mat cropped = new Mat())
            {
                CvInvoke.CopyMakeBorder(image, cropped, rect.Y, image.Rows - rect.Bottom, rect.X, image.Cols - rect.Width, BorderType.Constant, new MCvScalar(0));

                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(gray, gray, 200, 255, ThresholdType.Binary);

                VectorOfVectorOfPoint arrowContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(gray, arrowContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < arrowContours.Size; i++)
                {
                    VectorOfPoint approx = new VectorOfPoint();
                    double peri = CvInvoke.ArcLength(arrowContours[i], true);
                    CvInvoke.ApproxPolyDP(arrowContours[i], approx, 0.04 * peri, true);

                    // Стрелка вверх → около 6-8 углов
                    if (approx.Size >= 6 && approx.Size <= 9)
                        return true;
                }

                return false;
            }
        }

        private bool HasWhiteCenter(Mat image, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                int top = Math.Max(0, roi.Y);
                int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                int left = Math.Max(0, roi.X);
                int right = Math.Max(0, image.Cols - roi.X - roi.Width);

                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);

                Mat whiteMask = new Mat();
                CvInvoke.Threshold(gray, whiteMask, 200, 255, ThresholdType.Binary);

                int whitePixels = CountNonZero(whiteMask);
                double ratio = whitePixels / (double)(roi.Width * roi.Height);

                return ratio > 0.6;
            }
        }

        private bool HasRedBorder(Mat image, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                int top = Math.Max(0, roi.Y);
                int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                int left = Math.Max(0, roi.X);
                int right = Math.Max(0, image.Cols - roi.X - roi.Width);
                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                Mat hsv = new Mat();
                CvInvoke.CvtColor(cropped, hsv, ColorConversion.Bgr2Hsv);

                ScalarArray lowerRed1 = new ScalarArray(new MCvScalar(0, 100, 100));
                ScalarArray upperRed1 = new ScalarArray(new MCvScalar(10, 255, 255));
                ScalarArray lowerRed2 = new ScalarArray(new MCvScalar(170, 100, 100));
                ScalarArray upperRed2 = new ScalarArray(new MCvScalar(180, 255, 255));

                Mat maskRed1 = new Mat(), maskRed2 = new Mat();
                CvInvoke.InRange(hsv, lowerRed1, upperRed1, maskRed1);
                CvInvoke.InRange(hsv, lowerRed2, upperRed2, maskRed2);
                CvInvoke.Add(maskRed1, maskRed2, maskRed1);
                Mat element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
                CvInvoke.Erode(maskRed1, maskRed1, element, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar());
                int redCount = CountNonZero(maskRed1);
                double ratio = redCount / (double)(roi.Width * roi.Height);

                return ratio > 0.3;
            }
        }

        private bool HasSpeedLimitDigit(Mat image, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                CvInvoke.CopyMakeBorder(image, cropped, roi.Y, image.Rows - roi.Bottom, roi.X, image.Cols - roi.Right, BorderType.Constant, new MCvScalar(0));

                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(gray, gray, 200, 255, ThresholdType.Binary);

                VectorOfVectorOfPoint digitContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(gray, digitContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < digitContours.Size; i++)
                {
                    VectorOfPoint approx = new VectorOfPoint();
                    double peri = CvInvoke.ArcLength(digitContours[i], true);
                    CvInvoke.ApproxPolyDP(digitContours[i], approx, 0.04 * peri, true);

                    if (approx.Size >= 4 && approx.Size <= 9)
                        return true;
                }

                return false;
            }
        }

        private bool HasPedestrianPattern(Mat image, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                // Вырезаем ROI
                CvInvoke.CopyMakeBorder(image, cropped, roi.Y, image.Rows - roi.Bottom, roi.X, image.Cols - roi.Right, BorderType.Constant, new MCvScalar(0));

                // Переводим в оттенки серого
                Mat gray = new Mat();
                CvInvoke.CvtColor(cropped, gray, ColorConversion.Bgr2Gray);

                // Бинаризация для упрощения формы
                Mat binary = new Mat();
                CvInvoke.Threshold(gray, binary, 200, 255, ThresholdType.BinaryInv);

                // Поиск краёв
                Mat edges = new Mat();
                CvInvoke.Canny(binary, edges, 50, 150);

                // Найдём горизонтальные линии (полосы пешеходного перехода)
                LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 30, 10, 10);

                int horizontalLines = 0;
                foreach (var line in lines)
                {
                    double angle = Math.Abs(line.P1.Y - line.P2.Y);
                    if (angle < 10) // Горизонтальная линия
                        horizontalLines++;
                }

                // Если больше 3 параллельных линий → это пешеходный переход
                return horizontalLines >= 3;
            }
        }

        private string MatchWithTemplates(Mat cropped)
        {
            var templates = new Dictionary<string, string>
            {
                {"Ограничение скорости1", "templates/speed_limit1.png"},
                {"Ограничение скорости2", "templates/speed_limit2.png"},
                {"Пешеходный переход1", "templates/pedestrian1.png"},
                {"Пешеходный переход2", "templates/pedestrian2.png"},
                {"Пешеходный переход3", "templates/pedestrian3.png"},
                {"Стоп", "templates/stop.png"},
                {"Уступите дорогу", "templates/yield.png"}
            };

            double bestMatch = 0.2;
            string matchedSign = "Неизвестный";

            foreach (var pair in templates)
            {
                string signName = pair.Key;
                string path = pair.Value;

                if (!System.IO.File.Exists(path))
                {
                    System.Diagnostics.Debug.WriteLine($"Файл не найден: {path}");
                    continue;
                }

                using (Mat template = CvInvoke.Imread(path, ImreadModes.Color))
                {
                    if (template.IsEmpty)
                    {
                        System.Diagnostics.Debug.WriteLine($"{signName}: Шаблон пустой");
                        continue;
                    }

                    double matchValue = TemplateMatch(cropped, template);
                    System.Diagnostics.Debug.WriteLine($"{signName}: {matchValue:F2}");

                    if (matchValue > bestMatch)
                    {
                        bestMatch = matchValue;
                        matchedSign = signName;
                    }
                }
            }

            return matchedSign;
        }

        private bool AnalyzeInterior(Mat image, Rectangle roi, string shape)
        {
            using (Mat cropped = new Mat())
            {
                CvInvoke.CopyMakeBorder(image, cropped, roi.Y, image.Rows - roi.Y - roi.Height,
                    roi.X, image.Cols - roi.X - roi.Width, BorderType.Constant, new MCvScalar());

                Mat grayCropped = new Mat();
                CvInvoke.CvtColor(cropped, grayCropped, ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayCropped, grayCropped, 128, 255, ThresholdType.Binary);

                // Поиск внутренних контуров
                VectorOfVectorOfPoint innerContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(grayCropped, innerContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                int innerCornerCount = 0;
                for (int j = 0; j < innerContours.Size; j++)
                {
                    VectorOfPoint approx = new VectorOfPoint();
                    double peri = CvInvoke.ArcLength(innerContours[j], true);
                    CvInvoke.ApproxPolyDP(innerContours[j], approx, 0.04 * peri, true);

                    if (approx.Size > 2)
                        innerCornerCount += approx.Size;
                }

                Console.WriteLine($"Внутренние углы: {innerCornerCount}");

                // Если много внутренних контуров — это может быть цифра или полосы
                return innerCornerCount > 10;
            }
        }
        private double TemplateMatch(Mat source, Mat template)
        {
            if (source.IsEmpty || template.IsEmpty)
                return 0;

            // Переводим в серый и нормализуем
            Mat sourceGray = new Mat();
            CvInvoke.CvtColor(source, sourceGray, ColorConversion.Bgr2Gray);
            Mat templateGray = new Mat();
            CvInvoke.CvtColor(template, templateGray, ColorConversion.Bgr2Gray);

            // Масштабируем под одинаковый размер
            Mat sourceResized = new Mat();
            CvInvoke.Resize(sourceGray, sourceResized, new Size(100, 100));

            Mat templateResized = new Mat();
            CvInvoke.Resize(templateGray, templateResized, new Size(100, 100));

            Mat result = new Mat();
            CvInvoke.MatchTemplate(sourceResized, templateResized, result, TemplateMatchingType.CcoeffNormed);

            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            return maxVal;
        }

        private string RecognizeSpeedLimitNumber(Mat cropped)
        {
            Mat processed = PreprocessForOCR(cropped); // метод ниже
            string tempPath = Path.Combine(Path.GetTempPath(), "speed_limit_temp.png");
            processed.ToBitmap().Save(tempPath, ImageFormat.Png);

            using (var engine = new TesseractEngine("tessdata", "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(tempPath))
                {
                    using (var page = engine.Process(img))
                    {
                        string text = page.GetText().Trim();
                        var match = Regex.Match(text, @"\d+");
                        return match.Success ? match.Value : null;
                    }
                }
            }
        }

        private Mat PreprocessForOCR(Mat input)
        {
            Mat gray = new Mat();
            CvInvoke.CvtColor(input, gray, ColorConversion.Bgr2Gray);

            Mat binary = new Mat();
            CvInvoke.Threshold(gray, binary, 200, 255, ThresholdType.BinaryInv);

            CvInvoke.GaussianBlur(binary, binary, new Size(3, 3), 0);
            CvInvoke.Erode(binary, binary, null,  new Point(-1, -1), 1, BorderType.Constant, new MCvScalar());
            CvInvoke.Dilate(binary, binary, null, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar());

            return binary;
        }

        private string DetectTrafficSignWithTemplates(Mat image, Rectangle roi)
        {
            using (Mat cropped = new Mat())
            {
                int top = Math.Max(0, roi.Y);
                int bottom = Math.Max(0, image.Rows - roi.Y - roi.Height);
                int left = Math.Max(0, roi.X);
                int right = Math.Max(0, image.Cols - roi.X - roi.Width);

                CvInvoke.CopyMakeBorder(image, cropped, top, bottom, left, right, BorderType.Constant, new MCvScalar(0));

                // Шаблоны
                var templates = new Dictionary<string, string>
        {
            {"Стоп", "templates/stop.png"},
            {"Ограничение скорости", "templates/speed_limit.png"},
            {"Уступите дорогу", "templates/yield.png"},
            {"Пешеходный переход", "templates/pedestrian.png"}
        };

                double bestMatch = 0.5; // Минимум совпадения
                string matchedSign = "Неизвестный знак";

                foreach (var pair in templates)
                {
                    string signName = pair.Key;
                    string path = pair.Value;

                    if (!System.IO.File.Exists(path))
                        continue;

                    using (Mat template = CvInvoke.Imread(path, ImreadModes.Color))
                    {
                        if (template.IsEmpty)
                            continue;

                        double matchValue = TemplateMatch(cropped, template);
                        System.Diagnostics.Debug.WriteLine($"{signName}: {matchValue:F2}");

                        if (matchValue > bestMatch)
                        {
                            bestMatch = matchValue;
                            matchedSign = signName;
                        }
                    }
                }

                return matchedSign;
            }
        }
    }
}
