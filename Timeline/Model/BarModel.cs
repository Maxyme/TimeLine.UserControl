using System;
using System.Drawing;

namespace Timeline.Model
{
    public class BarModel
    {
        public Square BarSquare { get; set; }
        public DateTime StartValue { get; set; }
        public DateTime EndValue { get; set; }
        public Color Color { get; set; }
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public int RowIndex { get; set; }
        public bool IsClicked { get; set; }
        public bool IsMouseOver { get; set; }
        public bool Visible { get; set; }
        public Rectangle BarRectangle { get; set; }
    }

    public class Square
    {
        public Point BottomRightCorner { get; set; }
        public Point BottomLeftCorner { get; set; }
        public Point TopRightCorner { get; set; }
        public Point TopLeftCorner { get; set; }
    }
}
