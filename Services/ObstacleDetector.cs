using NAMI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAMI.Services
{
    public class ObstacleDetector
    {
        private const int SafeDistance = 300;
        private const int SideSafeDistance = 150;

        public List<DetectedObject> DetectObstacles(List<DetectedObject> lidarObjects)
        {
            var detectedObjects = new List<DetectedObject>();

            foreach (var point in lidarObjects)
            {
                if (point.Position.Y < 0 || point.Position.Y > 600)
                    continue; // За пределами области

                ObjectType type;

                // Определяем тип объекта на основе Position.X
                if (point.Position.X < 300)
                    type = ObjectType.Car;
                else if (point.Position.X > 500)
                    type = ObjectType.OncomingCar;
                else
                    type = ObjectType.Pedestrian;

                // Добавляем объект
                detectedObjects.Add(new DetectedObject
                {
                    Type = type,
                    Position = point.Position,
                    Speed = type == ObjectType.Pedestrian ? 5 : 10
                });
            }

            return detectedObjects;
        }
    }
}