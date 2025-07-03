using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using NAMI.Models;

namespace NAMI.Services
{
    public class SignManager
    {
        private readonly PictureBox pictureBox;
        private readonly Label signLabel;
        private readonly DataGridView dataGridView;
        private readonly List<TrafficSign> trafficSigns = new List<TrafficSign>();
        private readonly SignSimulator signSimulator = new SignSimulator();
        private readonly SignDetector signDetector = new SignDetector();
        private int roadLineOffset = 0;

        public SignManager(PictureBox pictureBox, Label signLabel, DataGridView dataGridView)
        {
            this.pictureBox = pictureBox;
            this.signLabel = signLabel;
            this.dataGridView = dataGridView;
        }

        public void LoadAndDetectSigns(int roadX, int pictureBoxHeight)
        {
            // Генерируем новые знаки с небольшой вероятностью
            if (new Random().Next(0, 100) < 5) // 5% шанс появления знака
            {
                var signs = signSimulator.GenerateTestSigns(roadX, pictureBoxHeight);
                trafficSigns.AddRange(signs);
            }

            // Двигаем знаки вниз (как будто они приближаются к автомобилю)
            for (int i = 0; i < trafficSigns.Count; i++)
            {
                var sign = trafficSigns[i];
                sign.Bounds = new Rectangle(sign.Bounds.X, sign.Bounds.Y + 2, sign.Bounds.Width, sign.Bounds.Height);
            }

            // Удаляем знаки, которые вышли за пределы экрана
            trafficSigns = trafficSigns.Where(s => s.Bounds.Y < pictureBoxHeight).ToList();

            // Отрисовываем знаки на PictureBox
            DrawTrafficSigns();

            // Распознаём знаки
            var detectedSign = signDetector.DetectSign(trafficSigns, pictureBox.Height / 2);

            // Обновляем метку с названием знака
            if (detectedSign != null)
            {
                signLabel.Text = $"Обнаруженный знак: {detectedSign.Type}";
            }
            else
            {
                signLabel.Text = "Обнаруженный знак: Нет";
            }

            // Обновляем DataGridView с детальной информацией
            UpdateDataGridView(detectedSign);
        }

        private void DrawTrafficSigns()
        {
            // Создаём новый Bitmap для отрисовки знаков
            using (Bitmap bitmap = new Bitmap(pictureBox.Width, pictureBox.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Отрисовываем дорожные знаки
                    foreach (var sign in trafficSigns)
                    {
                        if (sign.IsVisible)
                        {
                            Image image = GetImageForSign(sign.Type);
                            if (image != null)
                            {
                                g.DrawImage(image, sign.Bounds);
                            }
                        }
                    }
                }

                // Устанавливаем изображение на PictureBox
                pictureBox.Image = bitmap;
            }
        }

        private Image GetImageForSign(TrafficSignType type)
        {
            switch (type)
            {
                case TrafficSignType.Stop:
                    return Properties.Resources.Stop;
                case TrafficSignType.SpeedLimit50:
                    return Properties.Resources.SpeedLimit50;
                case TrafficSignType.PedestrianCrossing:
                    return Properties.Resources.PedestrianCrossing;
                default:
                    return null;
            }
        }

        private void UpdateDataGridView(TrafficSign detectedSign)
        {
            dataGridView.Rows.Clear();

            if (detectedSign != null)
            {
                dataGridView.Rows.Add(detectedSign.Type.ToString(), "Расстояние", "Скорость");
            }
        }
    }
}
