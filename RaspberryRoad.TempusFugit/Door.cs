using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaspberryRoad.TempusFugit
{
    public class Door
    {
        public Position Position { get; set; }
        public bool IsOpen { get; set; }

        public Door()
        {
            Position = new Position();
        }

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }
    }
}
