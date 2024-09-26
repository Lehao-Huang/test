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
    /// 词汇记忆能力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 词汇记忆能力讲解 : BaseUserControl
    {
        private DispatcherTimer gameTimer; // 用于游戏计时
        private DispatcherTimer waitTimer; // 用于等待计时
        private bool isgame;

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 词汇记忆能力讲解()
        {
            InitializeComponent();

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(5);
            gameTimer.Tick += GameTimer_Tick;

            waitTimer = new DispatcherTimer();
            waitTimer.Interval = TimeSpan.FromSeconds(5);
            waitTimer.Tick += WaitTimer_Tick;

            startgame();

            this.Loaded += 词汇记忆能力讲解_Loaded;

           
        }

        private void 词汇记忆能力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void startgame()
        {
            isgame = true;
            TipBlock.Text = "现在是记忆阶段，你将有五秒钟的时间记住上面的二字词语。";
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Foreground = new SolidColorBrush(Colors.Red);
            WordBlock.Text = "苹果";
            gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            gameTimer.Stop();
            isgame = false;
            TipBlock.Text = "记忆阶段结束，如果你看到屏幕上现在展示的词语与记忆阶段看到的词语相同，按下OK按钮，否则按下跳过按钮。";
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Text = "橙子";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!isgame)
            {
                TipBlock.Text = "很遗憾答错了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                OkButton.Visibility = Visibility.Collapsed;

                waitTimer.Start(); // 开始等待计时器
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!isgame)
            {
                TipBlock.Text = "恭喜你答对了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                OkButton.Visibility = Visibility.Visible;
            }
        }

        private void WaitTimer_Tick(object sender, EventArgs e)
        {
            waitTimer.Stop(); // 停止等待计时器
            startgame(); // 重新开始游戏
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

                        // 隐藏试玩部分内容
                        WordBlock.Visibility = Visibility.Collapsed;
                        anjian1.Visibility = Visibility.Collapsed;
                        anjian2.Visibility = Visibility.Collapsed;
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



                        // 隐藏试玩部分的控件
                        WordBlock.Visibility = Visibility.Collapsed;
                        anjian1.Visibility = Visibility.Collapsed;
                        anjian2.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        WordBlock.Visibility = Visibility.Visible;
                        anjian1.Visibility = Visibility.Visible;
                        anjian2.Visibility = Visibility.Visible;
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
