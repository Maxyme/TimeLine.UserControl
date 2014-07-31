/// <summary>
/// Adds a timeline or Gantt user control.
/// See Timeline Test for usage.
/// Created by Maxime Jacques - 2014
/// Parts of code and inspiration from VBGanttChart v0.55 
/// by Adrian "Adagio" Grau http://www.codeproject.com/Articles/20731/Gantt-Chart
/// </summary> 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timeline.Model;
using Timeline.Processor;

namespace Timeline
{
    public partial class Timeline : UserControl
    {
        #region public properties
        public Color HoverClickColor { get; set; }
        public Font RowFont { get; set; }
        public Font DateFont { get; set; }
        public Font TimeFont { get; set; }
        #endregion

        public Timeline()
        {
            InitializeComponent();
        }

        public void ShowBarChart(DateTime chartStartDate, DateTime chartEndDate, List<ItemModel> items)
        {
            Global.StartDate = chartStartDate;
            Global.EndDate = chartEndDate;
            var proc = new BarChartProcessor();
            Global.BarList = proc.GetBarList(items);
            Global.ItemCount = items.Select(i => i.ItemName).Distinct().Count();
        }
        private void ChartMouseMove(Object sender, MouseEventArgs e)
        {
            var localMousePosition = new Point(e.X, e.Y);
            var proc = new BarChartProcessor();
            if (localMousePosition == Global.OldMousePosition)
            {
                return;
            }

            var mouseOverObject = false;
            var tempText = new List<string>();
            var tempTitle = "";

            Parallel.ForEach(Global.BarList, bar =>
            {
                if (proc.MouseInsideBar(localMousePosition, bar) && bar.Visible)
                {
                    bar.IsMouseOver = true;
                    tempTitle = bar.Name;
                    tempText.Add("Event Start:  " + bar.StartValue.ToString("H:mm:ss dd-MM-yyyy"));
                    tempText.Add("Event End:   " + bar.EndValue.ToString("H:mm:ss dd-MM-yyyy"));
                    mouseOverObject = true;
                }
                else
                {
                    bar.IsMouseOver = false;
                }
            });

            Global.ToolTipTextList = tempText;
            Global.ToolTipTextTitle = tempTitle;
            Global.ToolTip.SetToolTip(this, Global.ToolTipTextList.Count > 0 ? Global.ToolTipTextList.ToString() : "");
            //if (Global.ToolTipTextList == null || Global.ToolTipTextTitle == null || Global.ToolTipTextTitle != tempTitle || !Global.ToolTipTextList.SequenceEqual(tempText))
            //{

            //}

            if (mouseOverObject)
            {
                PaintChart();
            }

            Global.OldMousePosition = localMousePosition;
        }
        private static void ChartMouseClick(Object sender, MouseEventArgs e)
        {
            var localMousePosition = new Point(e.X, e.Y);
            var proc = new BarChartProcessor();
            if (localMousePosition == Global.OldMousePosition)
            {
                return;
            }
            Parallel.ForEach(Global.BarList, bar =>
            {
                if (proc.MouseInsideBar(localMousePosition, bar) && bar.Visible)
                {
                    bar.IsClicked = true;
                }
                else
                {
                    bar.IsClicked = false;
                }
            });
        }
        private static void ChartMouseWheel(object sender, MouseEventArgs e)
        {
            Global.VScrollBar1.Focus();
        }
        private void Timeline_Load(object sender, EventArgs e)
        {
            //initialize public properties
            RowFont = TimeFont = DateFont = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point);
            HoverClickColor = Color.LightBlue;
            BackColor = Color.White;

            //initialize mouse controls
            MouseMove += ChartMouseMove;
            MouseWheel += ChartMouseWheel;
            MouseClick += ChartMouseClick;

            //initialize Tooltip
            Global.ToolTip = new ToolTip
            {
                OwnerDraw = true
            };
            Global.ToolTip.Draw += ToolTipText_Draw;
            Global.ToolTip.Popup += ToolTipText_Popup;

            //Flicker free drawing
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            //ScrollBar
            Global.VScrollBar1 = new VScrollBar 
            {   Dock = DockStyle.Right, 
                Visible = false
            };
            Controls.Add(Global.VScrollBar1);
            Global.VScrollBar1.Scroll += vScrollBar1_Scroll;
        }
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Global.ScrollPosition = Global.VScrollBar1.Value;
            PaintChart();
        }

        private static void ToolTipText_Draw(Object sender, DrawToolTipEventArgs e)
        {
            if (Global.ToolTipTextList.Count == 0)
            {
                return;
            }
            e.Graphics.FillRectangle(Brushes.White, e.Bounds);
            e.Graphics.DrawString(Global.ToolTipTextTitle, Global.TitleFont, Brushes.Black, 5, 0);

            // Draws the lines in the text box
            foreach (var str in Global.ToolTipTextList)
            {
                var stringY = (Global.ToolTipTitleHeight - Global.ToolTipfontHeight - e.Graphics.MeasureString(str, Global.RowFont).Height) / 2 +
                              10 + ((Global.ToolTipTextList.IndexOf(str) + 1) * 14);
                e.Graphics.DrawString(str, Global.RowFont, Brushes.Black, 5, stringY);
            }
        }

        private static void ToolTipText_Popup(Object sender, PopupEventArgs e)
        {
            var height = (Global.ToolTipTitleHeight + 4) + (Global.ToolTipTextList.Count * (Global.ToolTipfontHeight + 3));
            e.ToolTipSize = new Size(230, height);
        }

        private void PaintChart()
        {
            Refresh();
        }

        private void PaintChart(Graphics graphics)
        {
            var proc = new BarChartProcessor();
            var headerList = proc.GetFullHeaderList(Global.StartDate, Global.EndDate, Width);
            if (headerList.Count == 0 || Global.ItemCount == 0)
            {
                return;
            }
            Global.AvailableWidth = Width - Global.BarStartLeft - Global.BarStartRight;
            Global.WidthPerHeader = Global.AvailableWidth / headerList.Count;
            if (Global.ItemCount * (Global.BarHeight + Global.BarSpace) > Height)
            {
                Global.VScrollBar1.Visible = true;
                Global.VScrollBar1.Maximum = Global.ItemCount - 3;
            }

            graphics.Clear(BackColor);
            DrawChart(graphics, headerList);
            DrawBars(graphics, headerList);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            PaintChart(pe.Graphics);
        }

        private void DrawBars(Graphics graphics, IList<HeaderModel> headerList)
        {
            var timeBetweenHeaders = headerList[1].Time - headerList[0].Time;
            var widthBetween = headerList[1].StartLocation - headerList[0].StartLocation;
            var pixelsPerSecond = widthBetween/timeBetweenHeaders.TotalSeconds;

            //list of machineNames to add to the left of each row
            var rowTitleList = new List<string>();

            // Draws each bar
            foreach (var bar in Global.BarList)
            {
                var startTimeSpan = bar.StartValue - Global.StartDate;
                var startLocation = (int) (pixelsPerSecond*startTimeSpan.TotalSeconds);
                var x = Global.BarStartLeft + startLocation;
                var y = Global.BarStartTop + (Global.BarHeight*(bar.RowIndex - Global.ScrollPosition)) +
                        (Global.BarSpace*(bar.RowIndex - Global.ScrollPosition)) + 4;
                var width = (int) (pixelsPerSecond*bar.Duration.TotalSeconds);

                //restrict the width if longer than the right size
                if (x + width > (Width - Global.BarStartRight))
                {
                    width = Global.AvailableWidth + Global.BarStartLeft - x;
                }
                var numberOfBarsInControl = (Height - Global.BarStartTop)/(Global.BarHeight + Global.BarSpace);

                if ((bar.RowIndex >= Global.ScrollPosition &&
                     bar.RowIndex < numberOfBarsInControl + Global.ScrollPosition))
                {
                    bar.Visible = true;

                    //bar location on chart for mouseover
                    bar.BarSquare = new Square
                    {
                        TopLeftCorner = new Point(x, y),
                        TopRightCorner = new Point(x + width, y),
                        BottomLeftCorner = new Point(x, y + Global.BarHeight),
                        BottomRightCorner = new Point(x + width, y + Global.BarHeight)
                    };

                    //sets the rectangle in the middle of the row
                    var barRect = new Rectangle(x, y, width, Global.BarHeight);

                    var barBrush = new SolidBrush(bar.Color);
                    if (bar.IsMouseOver || bar.IsClicked)
                    {
                        barBrush = new SolidBrush(HoverClickColor);
                    }

                    graphics.FillRectangle(barBrush, barRect);
                    graphics.DrawRectangle(Pens.Black, barRect);

                    // Draws the rowtext, only once for each machine
                    if (!rowTitleList.Contains(bar.Name))
                    {
                        graphics.DrawString(bar.Name,
                            RowFont,
                            Brushes.Black,
                            0,
                            Global.BarStartTop + (Global.BarHeight*(bar.RowIndex - Global.ScrollPosition)) +
                            (Global.BarSpace*(bar.RowIndex - Global.ScrollPosition)));

                        rowTitleList.Add(bar.Name);
                    }
                }
                else
                {
                    bar.Visible = false;
                }
            }
        }

        private void DrawChart(Graphics graphics, IList<HeaderModel> headerList)
        {
            var lastLineStop = Global.BarStartTop + (Global.ItemCount - Global.ScrollPosition) * (Global.BarHeight + Global.BarSpace);

            //draw headers
            foreach (var header in headerList)
            {
                header.StartLocation = Global.BarStartLeft + (headerList.IndexOf(header) * Global.WidthPerHeader);

                //draw the date when there is a change of day
                var index = headerList.IndexOf(header);

                if (index == 0 ||
                    header.Time.Day != headerList[index - 1].Time.Day)
                {
                    graphics.DrawString(
                        header.Time.ToString("%d-%M-%y"),
                        DateFont,
                        Brushes.Black,
                        header.StartLocation, 
                        0);
                }

                graphics.DrawString(
                    header.Text,
                    TimeFont,
                    Brushes.Black,
                    header.StartLocation,
                    Global.HeaderTimeStartTop);


                var x = Global.BarStartLeft + (index * Global.WidthPerHeader);

                //draw vertical net
                graphics.DrawLine(
                    Global.GridColor,
                    x,
                    Global.HeaderTimeStartTop,
                    x,
                    lastLineStop);

            }

            //draw last vertical line
            graphics.DrawLine(
                Global.GridColor,
                Global.BarStartLeft + Global.AvailableWidth,
                Global.HeaderTimeStartTop,
                Global.BarStartLeft + Global.AvailableWidth,
                lastLineStop);

            //draw horizontal net
            for (var index = 0; index < Global.ItemCount; index++)
            {
                var y = Global.BarStartTop + index * (Global.BarHeight + Global.BarSpace);
                graphics.DrawLine(
                    Global.GridColor,
                    Global.BarStartLeft,
                    y,
                    Global.BarStartLeft + Global.AvailableWidth,
                    y
                    );
            }
        }
    }
}