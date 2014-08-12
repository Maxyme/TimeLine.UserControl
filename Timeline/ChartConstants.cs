using System;
using System.Collections.Generic;
using Timeline.Model;
using System.Windows.Forms;
using System.Drawing;

namespace Timeline
{
    public static class ChartConstants
    {
        public const int BarStartRight = 25;
        public const int BarStartLeft = 50;
        public const int HeaderTimeStartTop = 30;
        public const int BarStartTop = 50;
        public const int BarHeight = 40;
        public const int BarSpace = 8;
        public const int ToolTipTitleHeight = 14;
        public const int ToolTipfontHeight = 12;
        public const int MinimumIntervalInMinutes = 5;
        public static Font RowFont = new Font("Arial", 10, FontStyle.Regular, GraphicsUnit.Point);
        public static Font TitleFont = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Point);
        public static Pen GridColor = Pens.LightBlue;
    }
}
