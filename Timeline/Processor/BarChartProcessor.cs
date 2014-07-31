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

        private const int MinimumIntervalInMinutes = 5;

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

            if ((endDate - startDate).TotalDays > 3)
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
                newFromTime = newFromTime.AddHours(startDate.Hour);

                if (startDate.Minute < 59 & startDate.Minute > 29)
                {
                    newFromTime = newFromTime.AddMinutes(30);
                }
                else
                {
                    newFromTime = newFromTime.AddMinutes(0);
                }

                for (var date = newFromTime; date <= endDate; date = date.AddMinutes(MinimumIntervalInMinutes))
                {
                    var header = new HeaderModel
                    {
                        Text = date.Hour + ":"
                    };

                    if (date.Minute < 10)
                    {
                        header.Text += "0" + date.Minute;
                    }
                    else
                    {
                        header.Text += "" + date.Minute;
                    }

                    header.Time = new DateTime(date.Year, date.Month, date.Day, date.Hour,
                        date.Minute, 0);
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

        public static bool MouseInsideBar(Point mousePosition, BarModel bar)
        {
            return mousePosition.X >= bar.BarSquare.TopLeftCorner.X && mousePosition.X <= bar.BarSquare.TopRightCorner.X
                   && mousePosition.Y >= bar.BarSquare.TopLeftCorner.Y && mousePosition.Y <= bar.BarSquare.BottomLeftCorner.Y;
        }
    }
}