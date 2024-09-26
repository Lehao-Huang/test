using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace crs.theme
{
    public class CustomTickBar : TickBar
    {
        public int TickCount
        {
            get { return (int)GetValue(TickCountProperty); }
            set { SetValue(TickCountProperty, value); }
        }

        public static readonly DependencyProperty TickCountProperty =
            DependencyProperty.Register("TickCount", typeof(int), typeof(CustomTickBar), new PropertyMetadata(6));

        protected override void OnRender(DrawingContext drawingContext)
        {
            var size = new Size(base.ActualWidth, base.ActualHeight);

            int tickCount = TickCount - 1;

            // Calculate tick's setting

            var tickFrequencySize = size.Width / tickCount;
            var tickFrequency = (this.Maximum - this.Minimum) / tickCount;

            // Draw each tick text

            for (var count = 0; count <= tickCount; count++)
            {
                var minimum = this.Minimum;
                var text = string.Format("{0:0.00}", Math.Round(minimum + (tickFrequency * count), 2).ToString());

                var formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 13, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);

                var offset = (formattedText.Width / 2) - 4;

                var x = tickFrequencySize * count;
                var y = 10;

                drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(x + 4, 1), new Point(x + 4, 6));

                drawingContext.DrawText(formattedText, new Point(x - offset, y));
            }
        }

        //protected override void OnRender(DrawingContext dc)
        //{
        //    var size = new Size(base.ActualWidth, base.ActualHeight);

        //    int tickCount = (int)((this.Maximum - this.Minimum) / this.TickFrequency) + 1;

        //    if ((this.Maximum - this.Minimum) % this.TickFrequency == 0)
        //        tickCount -= 1;

        //    // Calculate tick's setting

        //    var tickFrequencySize = (size.Width * this.TickFrequency / (this.Maximum - this.Minimum));

        //    // Draw each tick text

        //    for (var i = 0; i <= tickCount; i++)
        //    {
        //        var text = Convert.ToString(Convert.ToInt32(this.Minimum + this.TickFrequency * i), 10);

        //        var formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 13, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);

        //        dc.DrawText(formattedText, new Point((tickFrequencySize * i), 5));
        //    }

        //}
    }
}
