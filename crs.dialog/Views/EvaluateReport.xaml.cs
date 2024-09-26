using crs.extension;
using System.Windows.Controls;
using crs.theme.Extensions;
using System.Windows.Input;
using System.Windows;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for EvaluateReport
    /// </summary>
    public partial class EvaluateReport : UserControl
    {
        public EvaluateReport()
        {
            InitializeComponent();
        }

        private async void SimplePanel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("非交互区域，请尝试点击返回按钮");
        }

        private void DataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;

                // 激发一个鼠标滚轮事件
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;

                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
