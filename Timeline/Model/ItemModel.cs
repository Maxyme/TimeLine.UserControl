using System;
using System.Drawing;

namespace Timeline.Model
{
    public class ItemModel
    {
        public string ItemName { get; set; }
        public DateTime StartDate { get; set; }
        public Color ItemColor { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
