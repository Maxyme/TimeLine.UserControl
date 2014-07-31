using System;
using System.Collections.Generic;
using Timeline.Model;
using System.Windows.Forms;
using System.Drawing;

namespace Timeline
{
    public static class Global
    {
        public const int BarStartRight = 25;
        public const int BarStartLeft = 50;
        public const int HeaderTimeStartTop = 30;
        public const int BarStartTop = 50;
        public const int BarHeight = 40;
        public const int BarSpace = 8;
        public const int ToolTipTitleHeight = 14;
        public const int ToolTipfontHeight = 12;
        public static Font RowFont = new Font("Arial", 10, FontStyle.Regular, GraphicsUnit.Point);
        public static Font TitleFont = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Point);
        public static Pen GridColor = Pens.LightBlue;
        public const int MinimumIntervalInMinutes = 5;
        public static ToolTip ToolTip { get; set; }
        public static VScrollBar VScrollBar1 { get; set; }
        public static int AvailableWidth { get; set; }
        public static Point OldMousePosition { get; set; }
        public static int ScrollPosition { get; set; }
        public static List<string> ToolTipTextList { get; set; }
        public static string ToolTipTextTitle { get; set; }
        public static List<BarModel> BarList { get; set; }
        public static int WidthPerHeader { get; set; }
        public static DateTime StartDate { get; set; }
        public static int ItemCount { get; set; }
        public static DateTime EndDate { get; set; }
    }
}
