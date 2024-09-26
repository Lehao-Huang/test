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
    /// 反应行为讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 平面识别能力讲解 : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
            {
                "Img3/1/1.jpg",
                "Img3/1/2.jpg",
                "Img3/1/3.jpg",
                "Img3/1/4.jpg",
                "Img3/1/5.jpg",
                "Img3/1/6.jpg",
                "Img3/1/7.jpg",
                "Img3/1/8.jpg",
                "Img3/1/9.jpg",
            },
            new string[]
            {
                "Img3/2/1.jpg",
                "Img3/2/2.jpg",
                "Img3/2/3.jpg",
                "Img3/2/4.jpg",
                "Img3/2/5.jpg",
                "Img3/2/6.jpg",
                "Img3/2/7.jpg",
                "Img3/2/8.jpg",
                "Img3/2/9.jpg",
            },
            new string[]
            {
                "Img3/3/1.jpg",
                "Img3/3/2.jpg",
                "Img3/3/3.jpg",
                "Img3/3/4.jpg",
                "Img3/3/5.jpg",
                "Img3/3/6.jpg",
                "Img3/3/7.jpg",
                "Img3/3/8.jpg",
                "Img3/3/9.jpg",
            },
            new string[]
            {
                "Img3/4/1.jpg",
                "Img3/4/2.jpg",
                "Img3/4/3.jpg",
                "Img3/4/4.jpg",
                "Img3/4/5.jpg",
                "Img3/4/6.jpg",
                "Img3/4/7.jpg",
                "Img3/4/8.jpg",
                "Img3/4/9.jpg",
            },
            new string[]
            {
                "Img3/5/1.jpg",
                "Img3/5/2.jpg",
                "Img3/5/3.jpg",
                "Img3/5/4.jpg",
                "Img3/5/5.jpg",
                "Img3/5/6.jpg",
                "Img3/5/7.jpg",
                "Img3/5/8.jpg",
                "Img3/5/9.jpg",
            },
            new string[]
            {
                "Img3/6/1.jpg",
                "Img3/6/2.jpg",
                "Img3/6/3.jpg",
                "Img3/6/4.jpg",
                "Img3/6/5.jpg",
                "Img3/6/6.jpg",
                "Img3/6/7.jpg",
                "Img3/6/8.jpg",
                "Img3/6/9.jpg",
            },
            new string[]
            {
                "Img3/7/1.jpg",
                "Img3/7/2.jpg",
                "Img3/7/3.jpg",
                "Img3/7/4.jpg",
                "Img3/7/5.jpg",
                "Img3/7/6.jpg",
                "Img3/7/7.jpg",
                "Img3/7/8.jpg",
                "Img3/7/9.jpg",
            },
            new string[]
            {
                "Img3/8/1.jpg",
                "Img3/8/2.jpg",
                "Img3/8/3.jpg",
                "Img3/8/4.jpg",
                "Img3/8/5.jpg",
                "Img3/8/6.jpg",
                "Img3/8/7.jpg",
                "Img3/8/8.jpg",
                "Img3/8/9.jpg",
            },
        };

        private int imageCount;
        private int max_time = 10;
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 40;
        private int INCREASE = 5; // 提高难度的阈值
        private int DECREASE = 5;  // 降低难度的阈值
        private int TRAIN_TIME = 60; // 训练持续时间（单位：秒）
        private bool IS_RESTRICT_TIME = true; // 限制练习时间是否启用
        private bool IS_BEEP = true;
        private int train_time;
        private int counter;
        private int randomIndex;
        private Random random;
        private Random randomforrotate;
        private const int moveAmount = 2;
        private int left;
        private int top;
        private int hardness;
        private DispatcherTimer timer;
        private int remainingTime;
        private DispatcherTimer trainingTimer; // 新的计时器用于训练时间

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 平面识别能力讲解()
        {
            InitializeComponent();
            max_time = 60;
            INCREASE = 2; // 提高难度的阈值
            DECREASE = 2;  // 降低难度的阈值
            TRAIN_TIME = 60; // 训练持续时间（单位：秒）
            IS_RESTRICT_TIME = true; // 限制练习时间是否启用
            IS_BEEP = true;
            hardness = 1;
            remainingTime = max_time;
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            random = new Random();
            counter = 0;
            train_time = TRAIN_TIME;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // 启动训练计时器

            LoadImages(imageCount);
            ShowRandomImage();
            left = 1;
            top = 1;

            this.Loaded += 平面识别能力讲解_Loaded;
            // 捕获全局按键事件, 使用 PreviewKeyDown 更早捕获事件
            this.PreviewKeyDown += Window_PreviewKeyDown;

            // 确保焦点保持在窗口
            this.Focusable = true;  // 确保窗口能够获得焦点
            this.Focus();  // 强制将焦点放到当前窗口

        }

        private void 平面识别能力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            FocusWindow();
        }

        private void FocusWindow()
        {
            // 强制将焦点保持在 ImageGrid 或 SelectionBox 上
            Keyboard.Focus(SelectionBox);  // 或者 ImageGrid
        }

        private void ShowRandomImage()
        {
            randomIndex = random.Next(imageCount);
            RandomImage.Source = new BitmapImage(new Uri(imagePaths[(hardness - 1) / 3][randomIndex], UriKind.Relative));

            if (IS_RESTRICT_TIME)
            {
                remainingTime = max_time;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime <= 0)
            {
                timer.Stop();
                LoadImages(imageCount);
                ShowRandomImage();
                remainingTime = max_time;
                timer.Start();
            }
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // 训练时间倒计时

            if (train_time <= 0)
            {
                timer.Stop(); // 停止主计时器
                trainingTimer.Stop(); // 停止训练计时器
                this.Close(); // 关闭当前窗口
            }
        }
        private void Close()
        {
            throw new NotImplementedException();
        }


        private void LoadImages(int imageCount)
        {
            // 清空之前的图片
            for (int i = ImageGrid.Children.Count - 1; i >= 0; i--)
            {
                if (ImageGrid.Children[i] is Image)
                {
                    ImageGrid.Children.RemoveAt(i);
                }
            }
            // 加载新图片
            for (int i = 0; i < imageCount; i++)
            {
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[(hardness - 1) / 3][i], UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                randomforrotate = new Random();
                int rotationDegrees;
                if (hardness >= 1 && hardness <= 3)
                {
                    rotationDegrees = randomforrotate.Next(4) * 5;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                int row = 1 + (i / 3) * 2;
                int column = 1 + (i % 3) * 2;
                Grid.SetRow(image, row);
                Grid.SetColumn(image, column);
                ImageGrid.Children.Add(image);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool isCorrect = (top - 1) * 3 / 2 + (left - 1) / 2 == randomIndex;
                if (isCorrect)
                {
                    ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#00ff00"));
                    textblock.Text = "恭喜你答对了！";
                    textblock.Foreground = new SolidColorBrush(Colors.Green);
                    OkButton.Visibility = Visibility.Visible;
                }
                else
                {
                    if (IS_BEEP)
                        Console.Beep();
                    ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#ff0000"));
                    textblock.Text = "很遗憾答错了！";
                    textblock.Foreground = new SolidColorBrush(Colors.Red);
                    OkButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (top > 1 && e.Key == Key.Up)
                    top -= moveAmount;
                if (top < (imageCount / 3) * 2 - 1 && e.Key == Key.Down)
                    top += moveAmount;
                if (left > 1 && e.Key == Key.Left)
                    left -= moveAmount;
                if (left < 5 && e.Key == Key.Right)
                    left += moveAmount;
            }

            Grid.SetColumn(SelectionBox, left);
            Grid.SetRow(SelectionBox, top);

            // 确保焦点保持在 SelectionBox 上
            FocusSelectionBoxWithDelay();

            // 防止事件继续传播
            e.Handled = true;
        }

        private void FocusSelectionBoxWithDelay()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Keyboard.Focus(SelectionBox);
            }), DispatcherPriority.Background);
        }

        private async void ChangeSelectionBoxColor(Color color)
        {
            SelectionBox.Stroke = new SolidColorBrush(color);

            // 等待指定的时间
            await Task.Delay(TimeSpan.FromSeconds(WAIT_DELAY));

            // 恢复原来的颜色
            SelectionBox.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a98d1"));

            left = 1;
            top = 1;
            Grid.SetColumn(SelectionBox, left);
            Grid.SetRow(SelectionBox, top);

            // 调整难度
            LoadImages(imageCount);
            ShowRandomImage();
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
                        Text_2.Visibility = Visibility.Collapsed; 
                        Image_2.Visibility = Visibility.Collapsed; 
                        Image_3.Visibility = Visibility.Collapsed;
                        ImageGrid.Visibility = Visibility.Collapsed;
                        image.Visibility = Visibility.Collapsed;
                        SelectionBox.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        textblock.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = false;
                        Button_2.Content = "下一步";

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;
                        ImageGrid.Visibility = Visibility.Collapsed;
                        ImageGrid.Visibility = Visibility.Collapsed;
                        image.Visibility = Visibility.Collapsed;
                        SelectionBox.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        textblock.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        ImageGrid.Visibility = Visibility.Visible;
                        image.Visibility = Visibility.Visible;
                        SelectionBox.Visibility = Visibility.Visible;
                        RandomImage.Visibility = Visibility.Visible;
                        textblock.Visibility = Visibility.Visible;

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