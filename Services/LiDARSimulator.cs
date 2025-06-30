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

            // 1. Машина на своей полосе
            if (_random.Next(0, 100) < 20)
            {
                float y = _random.Next(0, 50); // появляется сверху
                objects.Add(new DetectedObject
                {
                    Type = ObjectType.Car,
                    Position = new PointF(roadX + LaneOffset, y),
                    Speed = 25 // Скорость автомобиля
                });
            }

            // Встречная машина
            if (_random.Next(0, 100) < 25)
            {
                float y = _random.Next(0, 50); // появляется сверху
                objects.Add(new DetectedObject
                {
                    Type = ObjectType.OncomingCar,
                    Position = new PointF(roadX + RoadWidth - LaneOffset, y),
                    Speed = 50 // чуть быстрее
                });
            }

            // Пешеход
            if (_random.Next(0, 100) < 5) // 5% шанс
            {
                float x = roadX - 20; // Слева от дороги
                float y = _random.Next(pictureBoxHeight / 2 - 50, pictureBoxHeight / 2 + 50); // По центру высоты

                objects.Add(new DetectedObject
                {
                    Type = ObjectType.Pedestrian,
                    Position = new PointF(x, y),
                    Speed = 2f // Медленнее, чем автомобили
                });
            }

            return objects;
        }
    }
}
