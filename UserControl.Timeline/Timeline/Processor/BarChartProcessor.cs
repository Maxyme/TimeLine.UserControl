using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Timeline.Model;

namespace Timeline.Processor
{
    public class BarChartProcessor
    {
        private static List<string> ItemNameList { get; set; }

        public static List<BarModel> GetBarList(List<ItemModel> itemList, out int itemCount)
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
            itemCount = ItemNameList.Count;
            return barList;
        }

        private static int GetRowIndex(string barName)
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

        public static List<HeaderModel> GetFullHeaderList(DateTime startDate, DateTime endDate, int availableWidth)
        {
            var headerList = new List<HeaderModel>();
            var newFromTime = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var interval = endDate - startDate;

            if (interval.Days > 1)
            {
                while (newFromTime <= endDate)
                {
                    var header = new HeaderModel
                    {
                        Time = new DateTime(newFromTime.Year, newFromTime.Month, newFromTime.Day, 0, 0, 0)
                    };
                    headerList.Add(header);
                    newFromTime = newFromTime.AddDays(1);
                }
            }
            else
            {
                newFromTime = newFromTime.AddHours(startDate.Hour);

                if (startDate.Minute < 59 & startDate.Minute > 29)
                {
                    newFromTime = newFromTime.AddMinutes(30);
                }
                else
                {
                    newFromTime = newFromTime.AddMinutes(0);
                }

                while (newFromTime <= endDate)
                {
                    var header = new HeaderModel
                    {
                        Text = newFromTime.Hour + ":"
                    };

                    if (newFromTime.Minute < 10)
                    {
                        header.Text += "0" + newFromTime.Minute;
                    }
                    else
                    {
                        header.Text += "" + newFromTime.Minute;
                    }

                    header.Time = new DateTime(newFromTime.Year, newFromTime.Month, newFromTime.Day, newFromTime.Hour,
                        newFromTime.Minute, 0);
                    headerList.Add(header);
                    newFromTime = newFromTime.AddMinutes(5);
                    // The minimum interval of time between the headers
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

        public static bool MouseInsideBar(Point mousePosition, BarModel bar)
        {
            return mousePosition.X >= bar.BarSquare.TopLeftCorner.X
                   && mousePosition.X <= bar.BarSquare.TopRightCorner.X
                   && mousePosition.Y >= bar.BarSquare.TopLeftCorner.Y
                   && mousePosition.Y <= bar.BarSquare.BottomLeftCorner.Y;
        }
    }
}