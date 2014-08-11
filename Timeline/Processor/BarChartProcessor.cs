using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Timeline.Model;
using Timeline;
using System.Threading.Tasks;

namespace Timeline.Processor
{
    public class BarChartProcessor
    {
        private List<string> ItemNameList { get; set; }
        public List<BarModel> GetBarList(List<ItemModel> itemList)
        {
            var barList = new List<BarModel>();
            itemList.ForEach(x =>
            {
                var bar = new BarModel
                {
                    Name = x.ItemName,
                    StartValue = x.StartDate,
                    EndValue = x.StartDate + x.Duration,
                    Duration = x.Duration,
                    Color = x.ItemColor,
                    RowIndex = GetRowIndex(x.ItemName),
                    BarSquare = new Square()
                };
                barList.Add(bar);
            });
            return barList;
        }

        private int GetRowIndex(string barName)
        {
            if (ItemNameList == null)
            {
                ItemNameList = new List<string>();
            }
            if (!ItemNameList.Contains(barName))
            {
                ItemNameList.Add(barName);
            }
            return ItemNameList.FindIndex(x => x == barName);
        }

        public List<HeaderModel> GetFullHeaderList(DateTime startDate, DateTime endDate, int availableWidth, Font timeFont)
        {
            var headerList = new List<HeaderModel>();

            var timeInterval = endDate - startDate;

            var headerSpace = System.Windows.Forms.TextRenderer.MeasureText("12-12-12", timeFont);

            var numberOfHeaders = availableWidth / headerSpace.Width;

            var timeIncrement = new TimeSpan(timeInterval.Ticks/numberOfHeaders);

            var index = 0;

            for (var date = startDate;  date <= endDate; date = date.Add(timeIncrement), index ++)
            {
                var header = new HeaderModel
                {
                    HeaderDateTime = date,
                    StartLocation = ChartConstants.BarStartLeft + (index * headerSpace.Width)
                };
                
                headerList.Add(header);
            }

            return headerList;
        }

        public bool MouseInsideBar(Point mousePosition, BarModel bar)
        {
            return mousePosition.X >= bar.BarSquare.TopLeftCorner.X 
                && mousePosition.X <= bar.BarSquare.TopRightCorner.X
                && mousePosition.Y >= bar.BarSquare.TopLeftCorner.Y 
                && mousePosition.Y <= bar.BarSquare.BottomLeftCorner.Y;
        }
        internal List<BarModel> MouseClickHandler(List<BarModel> list, Point localMousePosition)
        {
            Parallel.ForEach(list, bar =>
            {
                if (MouseInsideBar(localMousePosition, bar) && bar.Visible)
                {
                    bar.IsClicked = true;
                }
                else
                {
                    bar.IsClicked = false;
                }
            });

            return list;
        }
        internal double GetPixelsPerSecond(List<HeaderModel> headerList)
        {
            var timeBetweenHeaders = headerList[1].HeaderDateTime - headerList[0].HeaderDateTime;
            var widthBetween = headerList[1].StartLocation - headerList[0].StartLocation;
            var pixelsPerSecond = widthBetween / timeBetweenHeaders.TotalSeconds;
            return pixelsPerSecond;
        }
    }
}