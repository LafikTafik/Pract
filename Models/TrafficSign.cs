using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAMI.Models
{
    public enum TrafficSignType
    {
        SpeedLimit50,
        Stop,
        NoPassing,
        PedestrianCrossing,
        Yield,
        OneWay,
        Parking,
        None
    }

    public class TrafficSign
    {
        public TrafficSignType Type { get; set; }
        public Rectangle Bounds { get; set; } // Позиция и размер знака на экране
        public bool IsVisible { get; set; }

        public TrafficSign(TrafficSignType type, Rectangle bounds)
        {
            Type = type;
            Bounds = bounds;
            IsVisible = true;
        }
    }
}