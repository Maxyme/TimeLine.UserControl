using System;
using System.Drawing;

namespace Timeline.Model
{
    public class BarModel
    {
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

}
