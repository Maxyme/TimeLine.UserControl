using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timeline.Model;

namespace TimelineTest
{
    public partial class TimelineTest : Form
    {
        public TimelineTest()
        {
            InitializeComponent();
        }

        private void TimelineTest_Load(object sender, EventArgs e)
        {
            Random randomVar = new Random();

            List<ItemModel> testList = new List<ItemModel>();

            var startDate = new DateTime(2013, 12, 29, 0, 0, 0);
            var endDate = new DateTime(2013, 12, 30, 22, 30, 0);

            for (int i = 0; i < 20; i++)
            {
                for (DateTime d = startDate; d < endDate; d = d.AddSeconds(randomVar.Next(13000, 14000)))
                {
                    ItemModel item1 = new ItemModel
                    {
                        ItemName = "Item" + "" + i.ToString(),
                        Duration = TimeSpan.FromSeconds(randomVar.Next(8, 15000)),
                        StartDate = d,
                        ItemColor = Color.PowderBlue
                    };

                    testList.Add(item1);

                }
            }

            timeline1.ShowBarChart(startDate, endDate, testList);

        }

    }
}
