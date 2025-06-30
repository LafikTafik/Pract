using NAMI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAMI.Services
{
    class DecisionMaker
    {
        public string MakeDecision(List<DetectedObject> obstacles, TrafficSign trafficSign = null)
        {
            foreach (var obj in obstacles)
            {
                // Если есть пешеход перед нами — останавливаемся
                if (obj.Type == ObjectType.Pedestrian && obj.Position.Y < 200)
                {
                    return "Остановиться — пешеход на проезжей части!";
                }

                // Если встречная машина приближается — объехать справа
                if (obj.Type == ObjectType.OncomingCar && obj.Position.Y < 300)
                {
                    return "Объехать справа — встречный автомобиль";
                }

                // Если объект на нашей полосе близко — останавливаемся
                if (obj.Type == ObjectType.Car && obj.Position.Y < 100)
                {
                    return "Остановиться — автомобиль впереди";
                }
            }

            return "Движение разрешено";
        }
    }
}
