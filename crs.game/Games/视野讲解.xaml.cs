using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 视野讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 视野讲解 : BaseUserControl
    {
        private Random random = new Random();
        private bool is_target = false;
        private DispatcherTimer timer;
        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }
        public 视野讲解()
        {
            InitializeComponent();
            TipBlock.Text = "请在出现与黑色圆点通过线段相连或者相接触的白色圆圈时按下确认按钮。";
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            OkButton.Visibility = Visibility.Collapsed;
            this.Loaded += MainWindow_Loaded; // 订阅 Loaded 事件

            // 初始化定时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); // 设置为3秒
            timer.Tick += Timer_Tick; // 订阅 Tick 事件
            timer.Start();

    

            this.Loaded += 视野讲解_Loaded;


        }

        private void 视野讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CenterFixedDot();
        }

        private void CenterFixedDot()
        {
            int radius = 30;
            double centerX = trainingCanvas.ActualWidth / 2;
            double centerY = trainingCanvas.ActualHeight / 2;
            Ellipse fixedDot = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(Colors.Black), // 黑色圆点
                Stroke = Brushes.Transparent
            };
            Canvas.SetZIndex(fixedDot, 1);
            Canvas.SetLeft(fixedDot, trainingCanvas.ActualWidth / 2 - radius);
            Canvas.SetTop(fixedDot, trainingCanvas.ActualHeight / 2 - radius);

            trainingCanvas.Children.Add(fixedDot);
        }

        private void Show_Target()
        {
            TipBlock.Text = "请在出现与黑色圆点通过线段相连或者相接触的白色圆圈时按下确认按钮。";
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            OkButton.Visibility = Visibility.Collapsed;
            trainingCanvas.Children.Clear(); // 清除画布上内容
            is_target = false;
            AddRandomDot_line();
        }

        private void AddRandomDot_line()
        {
            CenterFixedDot();
            double centerX = trainingCanvas.ActualWidth / 2;
            double centerY = trainingCanvas.ActualHeight / 2;
            double radius = 25; // 圆点半径
            Point startPoint = new Point(centerX, centerY);
            Point endPoint = new Point(random.Next(0, (int)trainingCanvas.ActualWidth), random.Next(0, (int)trainingCanvas.ActualHeight));

            // 创建线条并设置属性
            Line line = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 25
            };
            // 将线条添加到Canvas
            trainingCanvas.Children.Add(line);
            // 创建圆点并设置属性
            Ellipse dot = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(Colors.White),
                Stroke = Brushes.Transparent
            };

            // 将圆点放置到Canvas上
            Canvas.SetLeft(dot, endPoint.X - radius);
            Canvas.SetTop(dot, endPoint.Y - radius);

            // 将圆点添加到Canvas
            trainingCanvas.Children.Add(dot);
            is_target = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop(); // 停止定时器
            Show_Target(); // 重新显示目标
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (is_target)
            {
                TipBlock.FontSize = 40;
                TipBlock.Text = "恭喜你答对了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                OkButton.Visibility = Visibility.Visible;
            }
            else
            {
                timer?.Stop();
                TipBlock.FontSize = 40;
                TipBlock.Text = "很遗憾答错了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                OkButton.Visibility = Visibility.Collapsed;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // 设置为3秒
                timer.Tick += Timer_Tick; // 订阅 Tick 事件
                timer.Start();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 开始答题的相关逻辑
            OnGameBegin();
        }

        int currentPage = -1;

        private void Button_1_Click(object sender, RoutedEventArgs e)
        {
            currentPage--;
            PageSwitch();
        }

        private void Button_2_Click(object sender, RoutedEventArgs e)
        {
            currentPage++;
            PageSwitch();
        }

        private void Button_3_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        }

        async void PageSwitch()
        {
            switch (currentPage)
            {
                case 0:
                    {
                        // 显示讲解的第一个界面
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 隐藏试玩部分内容
                        trainingCanvas.Visibility = Visibility.Collapsed;
                        confirmButton.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = false;
                        Button_2.Content = "下一步";

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // 显示讲解的第二个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 隐藏试玩部分内容
                        trainingCanvas.Visibility = Visibility.Collapsed;
                        confirmButton.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        
                        Button_1.IsEnabled = true;
                        Button_2.Content = "下一步";

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // 显示讲解的第三个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;


                        // 隐藏试玩部分的控件
                        trainingCanvas.Visibility = Visibility.Collapsed;
                        confirmButton.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_3.Text);
                    }
                    break;
                case 3:
                    {
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        trainingCanvas.Visibility = Visibility.Visible;
                        confirmButton.Visibility = Visibility.Visible;
                        TipBlock.Visibility = Visibility.Visible;
                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        // 强制焦点保持在窗口
                        this.Focus();


                    }
                    break;
            }
        }


        /// <summary>
        /// 讲解内容语音播放
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task VoicePlayer(string message)
        {
            var voicePlayFunc = VoicePlayFunc;
            if (voicePlayFunc == null)
            {
                return;
            }

            await voicePlayFunc.Invoke(message);
        }
    }
}
