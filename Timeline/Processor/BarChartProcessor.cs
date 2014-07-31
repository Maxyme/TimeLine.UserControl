using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Timeline.Model;
using Timeline;

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

        public List<HeaderModel> GetFullHeaderList(DateTime startDate, DateTime endDate, int availableWidth)
        {
            var headerList = new List<HeaderModel>();

            if ((endDate - startDate).TotalDays > 4)
            {
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var header = new HeaderModel
                    {
                        Time = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0)
                    };
                    headerList.Add(header);
                }
            }
            else
            {
                for (var date = startDate; date <= endDate; date = date.AddMinutes(Global.MinimumIntervalInMinutes))
                {
                    var header = new HeaderModel
                    {
                        Text = date.ToShortTimeString(),
                        Time = date
                    };

                    headerList.Add(header);
                }
            }

            // Clean the header list by removing half until it fits
            while (availableWidth/headerList.Count < 50)
            {
                headerList = headerList.Select((value, index) => new {value, index})
                    .Where(z => z.index%2 == 0)
                    .Select(z => z.value).ToList();
            }
            return headerList;
        }

        public bool MouseInsideBar(Point mousePosition, BarModel bar)
        {
            return mousePosition.X >= bar.BarSquare.TopLeftCorner.X && mousePosition.X <= bar.BarSquare.TopRightCorner.X
                   && mousePosition.Y >= bar.BarSquare.TopLeftCorner.Y && mousePosition.Y <= bar.BarSquare.BottomLeftCorner.Y;
        }
    }
}