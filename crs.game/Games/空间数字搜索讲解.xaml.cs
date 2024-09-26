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
using static System.Net.Mime.MediaTypeNames;

namespace crs.game.Games
{
    /// <summary>
    /// 空间数字搜索讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 空间数字搜索讲解 : BaseUserControl
    {
        private List<int> numbers;
        private int lastClickedNumber;
        private int maxConsecutiveNumber; // 记录最长连续数字串的最大值
        private Brush defaultButtonBackground; // 存储按钮的默认背景颜色
        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }
        public 空间数字搜索讲解()
        {
            InitializeComponent();

            TipBlock.Text = "请按1-4的顺序依次点击上方数字。";
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            lastClickedNumber = 0; // 初始化为0，表示未点击
            maxConsecutiveNumber = 0; // 初始化最大连续数字串为0
            defaultButtonBackground = Brushes.White; // 设置默认背景颜色
            InitializeNumberGrid();

            this.Loaded += 空间数字搜索讲解_Loaded;


        }

        private void 空间数字搜索讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void InitializeNumberGrid()
        {
            numbers = Enumerable.Range(1, 4).ToList(); // 修改为1到4
            Random rand = new Random();
            numbers = numbers.OrderBy(x => rand.Next()).ToList(); // 打乱顺序

            foreach (var number in numbers)
            {
                Button button = new Button
                {
                    Content = number,
                    FontSize = 32,
                    Margin = new Thickness(40),
                    Style = null,
                    Background = defaultButtonBackground // 设置按钮的初始背景颜色
                };
                button.Click += NumberButton_Click;
                NumberGrid.Children.Add(button);
            }
        }

        private async void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int clickedNumber = Convert.ToInt32(clickedButton.Content);

            if (maxConsecutiveNumber == 0 && clickedNumber == maxConsecutiveNumber + 1)
            {
                maxConsecutiveNumber++;
                clickedButton.IsEnabled = false; // 禁用按钮
            }
            else
            {
                if (clickedNumber == maxConsecutiveNumber + 1)
                {
                    maxConsecutiveNumber++;
                    clickedButton.IsEnabled = false; // 禁用按钮

                    // 检查是否所有数字都已被点击
                    if (maxConsecutiveNumber == 4) // 修改为4
                    {
                        TipBlock.FontSize = 40;
                        TipBlock.Text = "恭喜你答对了！";
                        TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                        OkButton.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    clickedButton.Background = Brushes.Black; // 设置按钮背景为黑色
                    TipBlock.Text = "很遗憾答错了！";
                    TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                    // 等待0.5秒后恢复颜色
                    await Task.Delay(500); // 等待500毫秒
                    clickedButton.Background = defaultButtonBackground; // 恢复按钮背景为默认颜色
                    await Task.Delay(500); // 等待500毫秒
                    TipBlock.Text = "请按1-4的顺序依次点击上方数字。";
                    TipBlock.Foreground = new SolidColorBrush(Colors.Black);
                    clickedButton.IsEnabled = true; // 重新启用按钮
                }
            }
            lastClickedNumber = clickedNumber;
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
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        NumberGrid.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = false;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // 显示讲解的第二个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;

                        NumberGrid.Visibility = Visibility.Visible;
                        TipBlock.Visibility = Visibility.Visible;


                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                       
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
