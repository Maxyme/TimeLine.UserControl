/// <summary>
/// Adds a timeline or Gantt user control.
/// See Timeline Test for usage.
/// updated by Maxime Jacques - 2014
/// Parts of code from v0.55 by Adrian "Adagio" Grau http://www.codeproject.com/Articles/20731/Gantt-Chart
/// </summary> 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Timeline.Model;
using Timeline.Processor;

namespace Timeline
{
    public partial class Timeline : UserControl
    {
        #region constants

        private const int barStartRight = 25;
        private const int barStartLeft = 50;
        private const int headerTimeStartTop = 30;
        private const int barStartTop = 50;
        private const int barHeight = 40;
        private const int barSpace = 8;
        private const int toolTipTitleHeight = 14;
        private const int toolTipfontHeight = 12;

        #endregion

        #region variables

        private readonly ToolTip toolTip = new ToolTip();
        private readonly VScrollBar vScrollBar1 = new VScrollBar();
        private int availableWidth;
        private List<BarModel> barList = new List<BarModel>();
        private DateTime endDate;
        private int itemCount;
        private int numberOfBarsInControl;
        private Point oldMousePosition;
        private Font rowFont;
        private int scrollPosition;
        private DateTime startDate;
        private Font titleFont;
        private List<string> toolTipTextlist;
        private int widthPerHeader;
        private string ToolTipTextTitle { get; set; }

        #endregion

        #region public properties

        public Pen GridColor { get; set; }
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
            //reset ItemList
            barList.Clear();
            startDate = chartStartDate;
            endDate = chartEndDate;
            barList = BarChartProcessor.GetBarList(items, out itemCount);
        }

        private void ChartMouseMove(Object sender, MouseEventArgs e)
        {
            var localMousePosition = new Point(e.X, e.Y);
            if (localMousePosition == oldMousePosition)
            {
                return;
            }

            var mouseOverObject = false;
            var tempText = new List<string>();
            var tempTitle = "";

            foreach (var bar in barList)
            {
                if (BarChartProcessor.MouseInsideBar(localMousePosition, bar) && bar.Visible)
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
            }

            ToolTipTextList = tempText;
            ToolTipTextTitle = tempTitle;

            if (mouseOverObject)
            {
                PaintChart();
            }

            oldMousePosition = localMousePosition;
        }

        private void ChartMouseClick(Object sender, MouseEventArgs e)
        {
            var localMousePosition = new Point(e.X, e.Y);
            if (localMousePosition == oldMousePosition)
            {
                return;
            }

            foreach (var bar in barList)
            {
                if (BarChartProcessor.MouseInsideBar(localMousePosition, bar) && bar.Visible)
                {
                    bar.IsClicked = true;
                }
                else
                {
                    bar.IsClicked = false;
                }
            }
        }

        private void Timeline_Load(object sender, EventArgs e)
        {
            //initialize variables
            GridColor = Pens.LightBlue;
            HoverClickColor = Color.LightBlue;
            RowFont = DateFont = TimeFont = new Font("Arial", 10, FontStyle.Regular, GraphicsUnit.Point);
            Font = new Font("Arial", 10, FontStyle.Regular, GraphicsUnit.Point);
            titleFont = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Point);
            rowFont = new Font("Arial", 10, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.White;

            //initialize mouse controls
            MouseMove += ChartMouseMove;
            MouseWheel += ChartMouseWheel;
            MouseClick += ChartMouseClick;

            //initialize Tooltip
            toolTip.OwnerDraw = true;
            toolTip.Draw += ToolTipText_Draw;
            toolTip.Popup += ToolTipText_Popup;

            //Flicker free drawing
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            numberOfBarsInControl = (Height - barStartTop)/(barHeight + barSpace);

            //ScrollBar
            vScrollBar1.Dock = DockStyle.Right;
            vScrollBar1.Visible = false;
            Controls.Add(vScrollBar1);
            vScrollBar1.Scroll += vScrollBar1_Scroll;
        }

        private void ChartMouseWheel(object sender, MouseEventArgs e)
        {
            vScrollBar1.Focus();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            scrollPosition = vScrollBar1.Value;
            PaintChart();
        }

        #region ToolTip

        private List<string> ToolTipTextList
        {
            get { return toolTipTextlist; }
            set
            {
                toolTipTextlist = value;
                toolTip.SetToolTip(this, toolTipTextlist.Count > 0 ? value.ToString() : "");
            }
        }

        private void ToolTipText_Draw(Object sender, DrawToolTipEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, e.Bounds);
            e.Graphics.DrawString(ToolTipTextTitle, titleFont, Brushes.Black, 5, 0);

            // Draws the lines in the text box
            foreach (var str in ToolTipTextList)
            {
                var stringY = (int) (toolTipTitleHeight - toolTipfontHeight - e.Graphics.MeasureString(str, rowFont).Height)/2 +
                              10 + ((ToolTipTextList.IndexOf(str) + 1)*14);
                e.Graphics.DrawString(str, rowFont, Brushes.Black, 5, stringY);
            }
        }

        private void ToolTipText_Popup(Object sender, PopupEventArgs e)
        {
            var height = (toolTipTitleHeight + 4) + (ToolTipTextList.Count*(toolTipfontHeight + 3));
            e.ToolTipSize = new Size(230, height);
        }

        #endregion

        #region Draw

        private void PaintChart()
        {
            Refresh();
        }

        private void PaintChart(Graphics graphics)
        {
            var headerList = BarChartProcessor.GetFullHeaderList(startDate, endDate, Width);

            availableWidth = Width - barStartLeft - barStartRight;
            widthPerHeader = availableWidth/headerList.Count;

            if (headerList.Count == 0 || itemCount == 0)
            {
                return;
            }

            if (itemCount*(barHeight + barSpace) > Height)
            {
                vScrollBar1.Visible = true;
                vScrollBar1.Maximum = itemCount - 3;
            }

            graphics.Clear(BackColor);
            DrawChart(graphics, headerList);
            DrawBars(graphics, headerList);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            PaintChart(pe.Graphics);
        }

        private void DrawBars(Graphics graphics, List<HeaderModel> headerList)
        {
            var timeBetweenHeaders = headerList[1].Time - headerList[0].Time;
            var widthBetween = headerList[1].StartLocation - headerList[0].StartLocation;
            var pixelsPerSecond = widthBetween/timeBetweenHeaders.TotalSeconds;

            //list of machineNames to add to the left of each row
            var rowTitleList = new List<string>();

            // Draws each bar
            foreach (BarModel bar in barList)
            {
                var startTimeSpan = bar.StartValue - startDate;
                var startLocation = (int) (pixelsPerSecond*startTimeSpan.TotalSeconds);
                var x = barStartLeft + startLocation;
                var y = barStartTop + (barHeight*(bar.RowIndex - scrollPosition)) +
                        (barSpace*(bar.RowIndex - scrollPosition)) + 4;
                var width = (int) (pixelsPerSecond * bar.Duration.TotalSeconds);

                //restrict the width if longer than the right size
                if (x + width > (Width - barStartRight))
                {
                    width = availableWidth + barStartLeft - x;
                }

                if ((bar.RowIndex >= scrollPosition && bar.RowIndex < numberOfBarsInControl + scrollPosition))
                {
                    bar.Visible = true;
                    //bar location on chart for mouseover
                    bar.BarSquare = new Square
                    {
                        TopLeftCorner = new Point(x, y),
                        TopRightCorner = new Point(x + width, y),
                        BottomLeftCorner = new Point(x, y + barHeight),
                        BottomRightCorner = new Point(x + width, y + barHeight)
                    };

                    //sets the rectangle in the middle of the row
                    var barRect = new Rectangle(x, y, width, barHeight);

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
                        graphics.DrawString(bar.Name, RowFont, Brushes.Black, 0,
                            barStartTop + (barHeight*(bar.RowIndex - scrollPosition)) +
                            (barSpace*(bar.RowIndex - scrollPosition)));

                        rowTitleList.Add(bar.Name);
                    }
                }
                else
                {
                    bar.Visible = false;
                }
            }
        }

        private void DrawChart(Graphics graphics, List<HeaderModel> headerList)
        {
            var lastLineStop = barStartTop + (itemCount - scrollPosition)*(barHeight + barSpace);

            //draw headers
            foreach (HeaderModel header in headerList)
            {
                header.StartLocation = barStartLeft + (headerList.IndexOf(header)*widthPerHeader);
                //draw the date when there is a change of day
                if (headerList.IndexOf(header) == 0 ||
                    header.Time.Day != headerList[headerList.IndexOf(header) - 1].Time.Day)
                {
                    graphics.DrawString(
                        header.Time.ToString("%d-%M-%y"),
                        DateFont,
                        Brushes.Black,
                        header.StartLocation, 0);
                }

                graphics.DrawString(
                    header.Text,
                    TimeFont,
                    Brushes.Black,
                    header.StartLocation,
                    headerTimeStartTop);
            }

            //draw vertical net
            foreach (var header in headerList)
            {
                var index = headerList.IndexOf(header);
                var x = barStartLeft + (index * widthPerHeader);

                graphics.DrawLine(
                    GridColor,
                    x,
                    headerTimeStartTop,
                    x,
                    lastLineStop);
            }

            //draw last vertical line
            graphics.DrawLine(
                GridColor,
                barStartLeft + availableWidth,
                headerTimeStartTop,
                barStartLeft + availableWidth,
                lastLineStop);

            //draw horizontal net
            for (int index = 0; index < itemCount; index++)
            {
                int y = barStartTop + index*(barHeight + barSpace);
                graphics.DrawLine(
                    GridColor,
                    barStartLeft,
                    y,
                    barStartLeft + availableWidth,
                    y
                    );
            }
        }

        #endregion
    }
}