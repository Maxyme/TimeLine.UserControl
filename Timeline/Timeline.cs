﻿/// <summary>
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
        public SolidBrush HoverClickSolidBrush { get; set; }
        public Font RowFont { get; set; }
        public Font DateFont { get; set; }
        public Font TimeFont { get; set; }
        #endregion

        #region private properties
        private ToolTip ToolTip { get; set; }
        private VScrollBar VScrollBar1 { get; set; }
        private int AvailableWidth { get; set; }
        private Point OldMousePosition { get; set; }
        private int ScrollPosition { get; set; }
        private List<string> ToolTipTextList { get; set; }
        private string ToolTipTextTitle { get; set; }
        private List<BarModel> BarList { get; set; }
        private DateTime StartDate { get; set; }
        private int DistinctItemCount { get; set; }
        private DateTime EndDate { get; set; }
        #endregion
        public Timeline()
        {
            InitializeComponent();
        }
        public void ShowBarChart(DateTime chartStartDate, DateTime chartEndDate, List<ItemModel> items)
        {
            this.StartDate = chartStartDate;
            this.EndDate = chartEndDate;
            var proc = new BarChartProcessor();
            this.BarList = proc.GetBarList(items);
            this.DistinctItemCount = items.Select(i => i.ItemName).Distinct().Count();

            this.Refresh();
        }
        private void ChartMouseMove(Object sender, MouseEventArgs e)
        {
            var localMousePosition = new Point(e.X, e.Y);

            if (BarList == null || BarList.Count == 0)
            {
                return;
            }

            if (localMousePosition == this.OldMousePosition)
            {
                return;
            }

            var proc = new BarChartProcessor();

            var mouseOverObject = false;
            var tempText = new List<string>();
            var tempTitle = "";

            Parallel.ForEach(this.BarList, bar =>
            {
                if (proc.MouseInsideBar(localMousePosition, bar) && bar.Visible)
                {
                    bar.IsMouseOver = true;
                    tempTitle = bar.Name;
                    tempText.Add("Event Start:  " + bar.StartValue.ToUniversalTime());
                    tempText.Add("Event End:   " + bar.EndValue.ToUniversalTime());
                    mouseOverObject = true;
                }
                else
                {
                    bar.IsMouseOver = false;
                }
            });

            this.ToolTipTextList = tempText;
            this.ToolTipTextTitle = tempTitle;
            this.ToolTip.SetToolTip(this, this.ToolTipTextList.Count > 0 ? this.ToolTipTextList.ToString() : "");

            if (mouseOverObject)
            {
                this.Refresh();
            }

            this.OldMousePosition = localMousePosition;
        }
        private void ChartMouseClick(Object sender, MouseEventArgs e)
        {
            if (BarList == null || BarList.Count == 0)
            {
                return;
            }

            var localMousePosition = new Point(e.X, e.Y);

            var proc = new BarChartProcessor();

            this.BarList = proc.MouseClickHandler(this.BarList, localMousePosition);
        }
        private void ChartMouseWheel(object sender, MouseEventArgs e)
        {
            this.VScrollBar1.Focus();
        }
        private void Timeline_Load(object sender, EventArgs e)
        {
            //initialize public properties
            RowFont = TimeFont = DateFont = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point);
            HoverClickSolidBrush = new SolidBrush(Color.LightBlue);
            BackColor = Color.White;

            //initialize mouse controls
            MouseMove += ChartMouseMove;
            MouseWheel += ChartMouseWheel;
            MouseClick += ChartMouseClick;

            //initialize Tooltip
            this.ToolTip = new ToolTip
            {
                OwnerDraw = true
            };
            this.ToolTip.Draw += ToolTipText_Draw;
            this.ToolTip.Popup += ToolTipText_Popup;

            //Flicker free drawing
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            //ScrollBar
            this.VScrollBar1 = new VScrollBar 
            {   Dock = DockStyle.Right, 
                Visible = false
            };
            Controls.Add(this.VScrollBar1);
            this.VScrollBar1.Scroll += vScrollBar1_Scroll;
        }
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            this.ScrollPosition = this.VScrollBar1.Value;
            this.Refresh();
        }
        private void ToolTipText_Draw(Object sender, DrawToolTipEventArgs e)
        {
            if (this.ToolTipTextList.Count == 0)
            {
                return;
            }
            e.Graphics.FillRectangle(Brushes.White, e.Bounds);
            e.Graphics.DrawString(this.ToolTipTextTitle, ChartConstants.TitleFont, Brushes.Black, 5, 0);

            // Draws the lines in the text box
            foreach (var item in this.ToolTipTextList)
            {
                var stringY = (ChartConstants.ToolTipTitleHeight - ChartConstants.ToolTipfontHeight - e.Graphics.MeasureString(item, ChartConstants.RowFont).Height) / 2 +
                              10 + ((this.ToolTipTextList.IndexOf(item) + 1) * 14);
                e.Graphics.DrawString(item, ChartConstants.RowFont, Brushes.Black, 5, stringY);
            }
        }
        private void ToolTipText_Popup(Object sender, PopupEventArgs e)
        {
            var toolTipHeight = (ChartConstants.ToolTipTitleHeight + 4) + (this.ToolTipTextList.Count * (ChartConstants.ToolTipfontHeight + 3));
            e.ToolTipSize = new Size(230, toolTipHeight);
        }
        private void PaintChart(Graphics graphics)
        {
            if (BarList == null || BarList.Count == 0)
            {
                return;
            }
            var proc = new BarChartProcessor();
            var headerList = proc.GetFullHeaderList(this.StartDate, this.EndDate, this.Width, this.TimeFont);
            if (headerList.Count == 0 || this.DistinctItemCount == 0)
            {
                return;
            }

            var pixelsPerSecond = proc.GetPixelsPerSecond(headerList);

            this.AvailableWidth = Width - ChartConstants.BarStartLeft - ChartConstants.BarStartRight;

            if (this.DistinctItemCount * (ChartConstants.BarHeight + ChartConstants.BarSpace) > Height)
            {
                this.VScrollBar1.Visible = true;
                this.VScrollBar1.Maximum = this.DistinctItemCount - 3;
            }

            graphics.Clear(BackColor);
            DrawChartHeadersAndNet(graphics, headerList);

            DrawBars(graphics, this.BarList, pixelsPerSecond);
        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            PaintChart(pe.Graphics);
        }
        private void DrawBars(Graphics graphics, IEnumerable<BarModel> barList, double pixelsPerSecond)
        {
            //list of machineNames to add to the left of each row
            var rowTitleList = new List<string>();

            var proc = new BarChartProcessor();

            // Draws each bar
            foreach (var bar in barList)
            {
                var numberOfBarsInControl = (Height - ChartConstants.BarStartTop)/(ChartConstants.BarHeight + ChartConstants.BarSpace);

                if ((bar.RowIndex >= this.ScrollPosition &&
                     bar.RowIndex < numberOfBarsInControl + this.ScrollPosition))
                {
                    var newBar = proc.GetBar(bar, this.StartDate, pixelsPerSecond, ScrollPosition, this.Width);
                    DrawBarAndRowText(newBar, rowTitleList, graphics);
                }
                else
                {
                    bar.Visible = false;
                }
            }
        }
        private void DrawBarAndRowText(BarModel newBar, ICollection<string> rowTitleList, Graphics graphics)
        {
            var barBrush = new SolidBrush(newBar.Color);
            if (newBar.IsMouseOver || newBar.IsClicked)
            {
                barBrush = HoverClickSolidBrush;
            }

            graphics.FillRectangle(barBrush, newBar.BarRectangle);
            graphics.DrawRectangle(Pens.Black, newBar.BarRectangle);

            // Draws the rowtext, only once for each machine
            if (!rowTitleList.Contains(newBar.Name))
            {
                graphics.DrawString(newBar.Name,
                    RowFont,
                    Brushes.Black,
                    0,
                    ChartConstants.BarStartTop + (ChartConstants.BarHeight * (newBar.RowIndex - this.ScrollPosition)) +
                    (ChartConstants.BarSpace * (newBar.RowIndex - this.ScrollPosition)));

                rowTitleList.Add(newBar.Name);
            }
        }
        private void DrawChartHeadersAndNet(Graphics graphics, IList<HeaderModel> headerList)
        {
            var verticalLineLastY = ChartConstants.BarStartTop + (this.DistinctItemCount - this.ScrollPosition) * (ChartConstants.BarHeight + ChartConstants.BarSpace);

            //draw headers
            foreach (var header in headerList)
            {
                //draw the date when there is a change of day
                var index = headerList.IndexOf(header);

                if (headerList.IndexOf(header) == 0 
                    || header.HeaderDateTime.Day != headerList[index - 1].HeaderDateTime.Day)
                {
                    graphics.DrawString(
                        header.HeaderDateTime.ToShortDateString(),
                        DateFont,
                        Brushes.Black,
                        header.StartLocation, 
                        0);
                }

                graphics.DrawString(
                    header.HeaderDateTime.ToShortTimeString(),
                    TimeFont,
                    Brushes.Black,
                    header.StartLocation,
                    ChartConstants.HeaderTimeStartTop);

                //draw vertical line under header
                graphics.DrawLine(
                    ChartConstants.GridColor,
                    header.StartLocation,
                    ChartConstants.HeaderTimeStartTop,
                    header.StartLocation,
                    verticalLineLastY);

            }

            //draw last vertical line
            graphics.DrawLine(
                ChartConstants.GridColor,
                ChartConstants.BarStartLeft + this.AvailableWidth,
                ChartConstants.HeaderTimeStartTop,
                ChartConstants.BarStartLeft + this.AvailableWidth,
                verticalLineLastY);

            //draw horizontal net
            for (var index = 0; index < this.DistinctItemCount; index++)
            {
                var y = ChartConstants.BarStartTop + index * (ChartConstants.BarHeight + ChartConstants.BarSpace);
                graphics.DrawLine(
                    ChartConstants.GridColor,
                    ChartConstants.BarStartLeft,
                    y,
                    ChartConstants.BarStartLeft + this.AvailableWidth,
                    y
                    );
            }
        }
    }
}