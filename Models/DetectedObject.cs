using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAMI.Models
{
    public enum ObjectType
    {
        Car,
        OncomingCar,
        Pedestrian
    }

    public class DetectedObject
    {
        public ObjectType Type { get; set; }
        public PointF Position { get; set; }
        public float Speed { get; set; } // в пикселях в секунду
        public string Decision { get; set; }      // В км/ч
        public float InitialSpeed { get; set; } // сохраняем начальную скорость
        public float Distance => Position.Y;
        public SizeF Size { get; set; }

        public float X => Position.X;
        public float Y => Position.Y;
    }
}
