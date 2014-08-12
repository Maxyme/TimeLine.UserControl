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

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var randomVar = new Random();

            var testList = new List<ItemModel>();

            var startDate = new DateTime(2013, 12, 30, 10, 9, 0);
            var endDate = new DateTime(2013, 12, 30, 22, 29, 0);

            for (var i = 0; i < 20; i++)
            {
                for (var d = startDate; d < endDate; d = d.AddSeconds(randomVar.Next(13000, 14000)))
                {
                    var item1 = new ItemModel
                    {
                        ItemName = "Item" + "" + i,
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
