using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAMI.Models;

namespace NAMI.Services
{
    public class LiDARSimulator
    {
        private Random _random = new Random();
        public const int RoadWidth = 400;
        public const int LaneOffset = 100;

        // Метод принимает roadX и pictureBoxHeight как параметры
        public List<DetectedObject> GenerateTestPoints(int roadX, int pictureBoxHeight)
        {
            var objects = new List<DetectedObject>();

            // 🚗 Своя машина — теперь появляется справа
            if (_random.Next(0, 100) < 10) // 15% шанс
            {
                float y = _random.Next(0, 50); // сверху

                SizeF size = ObjectType.Car switch
                {
                    _ => new SizeF(70, 110), // стандартный размер
                };

                objects.Add(new DetectedObject
                {
                    Type = ObjectType.Car,
                    Position = new PointF(roadX + RoadWidth - LaneOffset, y), // справа
                    Speed = 10f, // реалистичная скорость
                    InitialSpeed = 10f
                });
            }

            //  Встречная машина — теперь появляется слева
            if (_random.Next(0, 100) < 15) // 10% шанс
            {
                float y = _random.Next(0, 50); // сверху

                SizeF size = ObjectType.Car switch
                {
                    _ => new SizeF(70, 110), // стандартный размер
                };

                objects.Add(new DetectedObject
                {
                    Type = ObjectType.OncomingCar,
                    Position = new PointF(roadX + LaneOffset, y), // слева
                    Speed = 20f,// чуть быстрее
                    InitialSpeed = 20f
                });

            }

            // 👤 Пешеход — появляется слева и переходит дорогу
            if (_random.Next(0, 100) < 5) // 5% шанс
            {
                float x = roadX - 20; // слева от дороги
                float y = _random.Next(pictureBoxHeight / 2 - 50, pictureBoxHeight / 2 + 50); // центр дороги

                objects.Add(new DetectedObject
                {
                    Type = ObjectType.Pedestrian,
                    Position = new PointF(x, y),
                    Speed = 50f // медленнее, чем автомобили
                });
            }

            return objects;
        }
    }
}
