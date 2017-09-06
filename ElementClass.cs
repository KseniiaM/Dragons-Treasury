using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TheDragonsTreasury
{
    enum Stone
    {
        Ruby,
        Amber,
        Citrine,
        Emerald,
        Sapphire,
        Amethyst,
        none,
    }

    struct Point
    {
        private int x;
        private int y;

        public int X
        {
            get { return x; }
            set { if ((value >= 0) && (value < 13)) x = value;
                else x = 0; }
        }
        public int Y
        {
            get { return y; }
            set { if ((value >= 0) && (value < 6)) y = value;
                else y = 0; }
        }

        public Point(int xi, int yi):this()
        {
            X = xi;
            Y = yi;
        }
    }
    class ElementClass
    {
        public Stone elType { get; set; }
        public Point Loc { get; set; }
        public bool toBeDeleted { get; set; }

        public ElementClass() { }
        public ElementClass(Stone el, Point l):this()
        {
            elType = el;
            Loc = l;
            toBeDeleted = false;
        }
    }
}
