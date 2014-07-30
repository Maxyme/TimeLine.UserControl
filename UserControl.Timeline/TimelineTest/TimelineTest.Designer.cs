namespace TimelineTest
{
    partial class TimelineTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimelineTest));
            this.timeline1 = new Timeline.Timeline();
            this.SuspendLayout();
            // 
            // timeline1
            // 
            this.timeline1.BackColor = System.Drawing.Color.White;
            this.timeline1.DateFont = new System.Drawing.Font("Arial", 10F);
            this.timeline1.Font = new System.Drawing.Font("Arial", 10F);
            this.timeline1.HoverClickColor = System.Drawing.Color.LightBlue;
            this.timeline1.Location = new System.Drawing.Point(13, 13);
            this.timeline1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.timeline1.MinimumSize = new System.Drawing.Size(117, 123);
            this.timeline1.Name = "timeline1";
            this.timeline1.RowFont = new System.Drawing.Font("Arial", 10F);
            this.timeline1.Size = new System.Drawing.Size(889, 637);
            this.timeline1.TabIndex = 0;
            this.timeline1.TimeFont = new System.Drawing.Font("Arial", 10F);
            // 
            // TimelineTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 651);
            this.Controls.Add(this.timeline1);
            this.Name = "TimelineTest";
            this.Text = "TimelineTestApp";
            this.Load += new System.EventHandler(this.TimelineTest_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Timeline.Timeline timeline1;

    }
}

