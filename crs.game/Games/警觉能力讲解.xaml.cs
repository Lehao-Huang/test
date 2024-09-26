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
    /// 警觉能力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 警觉能力讲解 : BaseUserControl
    {
        private DispatcherTimer waitTimer;
        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 警觉能力讲解()
        {
            InitializeComponent();

           

            this.Loaded += 警觉能力讲解_Loaded;


        }

        private void 警觉能力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void ShowImage()
        {
            RandomImage.Visibility = Visibility.Hidden;
            TipBlock.Text = "请在左侧出现右侧展示的图案之后尽快按下确认按钮。";
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            // 如果定时器已存在，则停止并重置
            if (waitTimer != null)
            {
                waitTimer.Stop();
            }

            // 创建一个新的定时器
            waitTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // 设置等待时间为 3 秒
            };

            // 定义定时器的 Tick 事件
            waitTimer.Tick += (s, args) =>
            {
                waitTimer.Stop(); // 停止定时器
                Console.Beep(800, 200); // 发出800Hz频率的声音，持续0.2秒
                RandomImage.Visibility = Visibility.Visible;
            };

            waitTimer.Start(); // 启动定时器
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (RandomImage.Visibility == Visibility.Visible)
            {
                TipBlock.FontSize = 40;
                TipBlock.Text = "恭喜你答对了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                OkButton.Visibility = Visibility.Visible;
            }
            else
            {
                waitTimer?.Stop(); // 停止定时器

                TipBlock.FontSize = 40;
                TipBlock.Text = "很遗憾答错了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                OkButton.Visibility = Visibility.Collapsed;

                // 等待三秒钟
                await Task.Delay(3000);

                // 调用 ShowImage 方法
                ShowImage();
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
                        Text_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 隐藏试玩部分内容
                        RandomImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        anjian.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
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
                        Text_3.Visibility = Visibility.Visible;
                        Text_4.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;



                        // 隐藏试玩部分的控件
                        RandomImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        anjian.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_2.Text);
                        await OnVoicePlayAsync(Text_3.Text);
                        await OnVoicePlayAsync(Text_4.Text);
                    }
                    break;
                case 2:
                    {
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        RandomImage.Visibility = Visibility.Visible;
                        TargetImage.Visibility = Visibility.Visible;
                        anjian.Visibility = Visibility.Visible;
                        TipBlock.Visibility = Visibility.Visible;
                        TipBlock1.Visibility = Visibility.Visible;
                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        ShowImage();
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
