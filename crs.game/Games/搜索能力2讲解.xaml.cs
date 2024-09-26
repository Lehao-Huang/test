using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
namespace crs.game.Games
{
    /// <summary>
    /// 搜索能力2讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 搜索能力2讲解 : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
          new string[]
{
    "Img1/1/1.jpg", "Img1/1/2.jpg", "Img1/1/3.jpg", "Img1/1/4.jpg",
    "Img1/1/5.jpg", "Img1/1/6.jpg", "Img1/1/7.jpg", "Img1/1/8.jpg",
    "Img1/1/9.jpg", "Img1/1/10.jpg", "Img1/1/11.jpg", "Img1/1/12.jpg",
    "Img1/1/13.jpg", "Img1/1/14.jpg", "Img1/1/15.jpg", "Img1/1/16.jpg",
    "Img1/1/17.jpg", "Img1/1/18.jpg", "Img1/1/19.jpg", "Img1/1/20.jpg",
    "Img1/1/21.jpg", "Img1/1/22.jpg", "Img1/1/23.jpg", "Img1/1/24.jpg",
    "Img1/1/25.jpg", "Img1/1/26.jpg", "Img1/1/27.jpg", "Img1/1/28.jpg",
    "Img1/1/29.jpg", "Img1/1/30.jpg", "Img1/1/31.jpg", "Img1/1/32.jpg"
},
new string[]
{
    "Img1/2/Rocket1.jpg", "Img1/2/Rocket2.jpg", "Img1/2/Star.jpg", "Img1/2/Planet1.jpg",
    "Img1/2/Planet3.jpg", "Img1/2/Sun.jpg", "Img1/2/Planet2.jpg", "Img1/2/Meteor.jpg",
    "Img1/2/Planet4.jpg", "Img1/2/Spaceship.jpg", "Img1/2/Background.jpg"
}
        };

        private int max_time = 1; // 窗口总的持续时间，单位分钟
        private int train_mode = 1; // 游戏模式，1，2，3，4
        private bool is_gaming = false;
        private int success_time = 0;
        private int fail_time = 0;
        private int level = 1; // 当前游戏难度等级
        private List<int> missingNumbers;
        private List<int> userInputNumbers;
        private string userInput; // 存储用户输入的数字

        private int right_picture_number = 4; // 显示的正确图片数量
        private int chose_picture_number = 6; // 显示的可选择图片数量

        private int max_right_display = 2; // 最多显示的正确图片数量
        private int mini_right_display = 1; // 最少显示的正确图片数量
        private int mislead_picture_display_number = 4; // 干扰图片中总的显示数量

        private List<System.Windows.Controls.Image> correctImages; // 正确图片的列表
        private List<System.Windows.Controls.Image> selectedImages; // 用户选择的图片

        private Queue<bool> recentResults = new Queue<bool>(5); // 记录最近5次游戏结果的队列
        private int level_updata_threshold = 3; // 难度更新的正确或错误阈值
        private int maxnumber = 5; // 显示的最大数字
        private int miss_number = 2; // 消失的数字数量
        private int mode1_display_size = 4; // 显示框的大小：1=小，2=中，3=大，4=全屏

        private const int MaxGames = 10;
        private int[] correctAnswers = new int[MaxGames];
        private int[] wrongAnswers = new int[MaxGames];
        private int[] ignoreAnswers = new int[MaxGames];

        private DispatcherTimer gameTimer; // 全局计时器
        private TimeSpan timeRemaining; // 剩余时间

        private Canvas selectionCanvas; // 在类中声明 selectionCanvas 作为全局变量

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 搜索能力2讲解()
        {
            InitializeComponent();
            InitializeGame();

            // 初始化计时器
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
            gameTimer.Tick += GameTimer_Tick;
            timeRemaining = TimeSpan.FromMinutes(max_time); // 设定整个窗口存在的时间
            gameTimer.Start(); // 开始计时

            this.Loaded += 搜索能力2讲解_Loaded;

           
        }

        private void 搜索能力2讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // 每秒减少剩余时间
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                UpdateTimerDisplay(); // 更新计时器显示
            }
            else
            {
                gameTimer.Stop(); // 停止计时器
                CloseApplication(); // 关闭整个应用窗口
            }
        }

        private void UpdateTimerDisplay()
        {
            // 这里可以选择是否显示时间，但根据你的要求，单次游戏没有时间显示逻辑。
            // 如果你不希望显示时间，可以省略此方法的实现
        }

        private void CloseApplication()
        {
            // 关闭整个应用窗口;

        }

        private void InitializeGame()
        {
            ResetGameState(); // 在开始新游戏前重置状态
            switch (train_mode)
            {
                case 1:
                    modeTextBlock.Text = "找出数字范围内的缺失数字，并将它们从小到大逐个输入";
                    break;
                case 2:
                    modeTextBlock.Text = "识别出叠加在一起的不同形状，并将它们从屏幕下部选择出来";
                    break;
                case 3:
                    modeTextBlock.Text = "需要寻找的目标对象出现屏幕的下部，从上部的图片中将这些对象寻找出来";
                    break;
                case 4:
                    modeTextBlock.Text = "数出并输入每个正确对象在图片中出现的次数";
                    break;
                default:
                    modeTextBlock.Text = "未知模式";
                    break;
            }

            // 隐藏组件，确保它们不会在非模式1下显示
            confirm1.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Collapsed;
            myCanvas.Visibility = Visibility.Collapsed;
            confirm.Visibility = Visibility.Collapsed;

            //AdjustDifficulty(level); // 根据当前level调整游戏难度

            // 初始化游戏模式
            if (train_mode == 1)
            {
                SetupGameMode1();
            }
            else if (train_mode == 2)
            {

            }
            else if (train_mode == 3)
            {
                SetupGameMode3();
            }
            else if (train_mode == 4)
            {
                SetupGameMode4();
            }
        }

        private void SetupGameMode4()
        {
            confirm1.Visibility = Visibility.Visible;
            textBlock.Visibility = Visibility.Visible;
            myCanvas.Visibility = Visibility.Visible;

            // 清除MainGrid中的所有内容
            MainGrid.Children.Clear();

            // 创建一个Grid来放置背景图片和选择图片
            Grid gameGrid = new Grid();

            // 创建背景图片
            System.Windows.Controls.Image backgroundImage = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("Img/2/Background.png", UriKind.Relative)),
                Stretch = Stretch.Uniform,
                Width = 500,  // 调整背景图片的宽度
                Height = 300,  // 调整背景图片的高度
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 将背景图片添加到Grid的Background层
            gameGrid.Children.Add(backgroundImage);

            // 初始化正确图片列表
            correctImages = new List<System.Windows.Controls.Image>();

            // 随机选择正确的图片，并将其添加到correctImages列表中
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != "Img/2/Background.png" && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    // 将正确图片添加到correctImages列表中
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };

                    correctImages.Add(img);
                }
            }

            // 创建一个用于显示选择图片的Canvas
            selectionCanvas = new Canvas
            {
                Width = 500,  // 与背景图片的宽度保持一致
                Height = 300  // 与背景图片的高度保持一致
            };

            // 在Canvas加载完成后再显示可选择的图片
            selectionCanvas.Loaded += (s, e) =>
            {
                DisplaySelectableImagesMode4(selectionCanvas, rand);
            };

            // 创建一个带白色边框的Border，表示随机生成范围
            Border selectionBorder = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = 500,  // 边框的宽度，与Canvas保持一致
                Height = 300,  // 边框的高度，与Canvas保持一致
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 创建一个StackPanel来包含选择Canvas和正确图片Panel
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // 将Canvas添加到Grid的前景层（背景图片之上）
            gameGrid.Children.Add(selectionCanvas);

            // 将带边框的Border添加到Canvas之上，表示随机生成范围
            gameGrid.Children.Add(selectionBorder);

            // 在StackPanel的下方显示需要选择的正确图片
            DisplayCorrectImagesMode4(mainPanel);

            // 将StackPanel添加到Grid的下方
            gameGrid.Children.Add(mainPanel);

            // 将Grid添加到MainGrid中
            MainGrid.Children.Add(gameGrid);
        }

        private void DisplaySelectableImagesMode4(Canvas selectionCanvas, Random rand)
        {
            // 边界信息
            double leftBound = -3.2;
            double rightBound = 496.8;
            double topBound = 1.98;
            double bottomBound = 282.52;

            // 初始化图片显示计数
            List<System.Windows.Controls.Image> imagesToDisplay = new List<System.Windows.Controls.Image>();

            // 添加正确图片到图片显示列表
            foreach (var correctImage in correctImages)
            {
                // 为每个正确图片随机生成显示次数
                int displayCount = rand.Next(mini_right_display, max_right_display + 1);
                for (int i = 0; i < displayCount; i++)
                {
                    System.Windows.Controls.Image imgCopy = new System.Windows.Controls.Image
                    {
                        Source = correctImage.Source,
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };
                    imagesToDisplay.Add(imgCopy);
                }
            }

            // 添加干扰图片到图片显示列表
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[1].Length)
                .Where(i => !correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString().EndsWith(imagePaths[1][i]))
                             && imagePaths[1][i] != "Img/2/Background.png")
                .OrderBy(x => rand.Next())
                .Take(mislead_picture_display_number)
                .ToList();

            foreach (var index in remainingIndices)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                imagesToDisplay.Add(img);
            }

            // 随机化图片的位置，并将图片显示在Canvas中
            foreach (var img in imagesToDisplay)
            {
                // 随机生成X和Y坐标，确保图片不会超出背景图片边界
                double maxLeft = rightBound - img.Width;
                double maxTop = bottomBound - img.Height;

                double left = rand.NextDouble() * (maxLeft - leftBound) + leftBound;
                double top = rand.NextDouble() * (maxTop - topBound) + topBound;

                // 创建边框并设置图片位置
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // 初始时无边框
                    Child = img
                };

                // 设置 Border 的位置
                Canvas.SetLeft(border, left);
                Canvas.SetTop(border, top);

                selectionCanvas.Children.Add(border);
            }
        }

        private void confirmButton_Click4(object sender, RoutedEventArgs e)
        {
            bool isCorrect = true;
            int index = 0;

            foreach (var correctImage in correctImages)
            {
                string imageUri = ((BitmapImage)correctImage.Source).UriSource.ToString().Replace("pack://application:,,,", "");

                // 计算玩家输入的数量与实际数量是否匹配
                int correctImageCount = selectionCanvas.Children.OfType<Border>()
                    .Count(border => ((BitmapImage)((System.Windows.Controls.Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == imageUri);

                if (userInputNumbers[index] != correctImageCount)
                {
                    isCorrect = false;
                    break;
                }

                index++;
            }

            if (isCorrect)
            {
                success_time++;
            }
            else
            {
                fail_time++;
            }

            EndGame(); // 触发结束游戏逻辑
        }

        private void DisplayCorrectImagesMode4(StackPanel mainPanel)
        {
            StackPanel correctPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0) // 调整正确图片的显示位置
            };

            // 创建一个HashSet用于去重，确保每个正确图片只显示一次
            HashSet<string> displayedImages = new HashSet<string>();

            foreach (var img in correctImages)
            {
                // 获取图片的URI
                string imageUri = ((BitmapImage)img.Source).UriSource.ToString();

                // 如果图片已经显示过，则跳过
                if (displayedImages.Contains(imageUri))
                    continue;

                // 添加到HashSet中，防止重复显示
                displayedImages.Add(imageUri);

                // 在下方面板中显示一次该图片
                System.Windows.Controls.Image correctImg = new System.Windows.Controls.Image
                {
                    Source = img.Source,
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                correctPanel.Children.Add(correctImg);
            }

            mainPanel.Children.Add(correctPanel);
        }

        private void SetupGameMode3()
        {
            // 使确认按钮可见
            confirm.Visibility = Visibility.Visible;

            // 清除MainGrid中的所有内容
            MainGrid.Children.Clear();

            // 创建一个Grid来放置背景图片和选择图片
            Grid gameGrid = new Grid();

            // 创建背景图片
            System.Windows.Controls.Image backgroundImage = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("Img/2/Background.png", UriKind.Relative)),
                Stretch = Stretch.Uniform,
                Width = 500,  // 调整背景图片的宽度
                Height = 300,  // 调整背景图片的高度
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 将背景图片添加到Grid的Background层
            gameGrid.Children.Add(backgroundImage);

            // 初始化正确图片列表
            correctImages = new List<System.Windows.Controls.Image>();
            selectedImages = new List<System.Windows.Controls.Image>();

            // 随机选择正确的图片，并将其添加到correctImages列表中
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != "Img/2/Background.png" && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    System.Windows.Controls.Image img = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };

                    correctImages.Add(img);
                }
            }

            // 创建一个用于显示选择图片的Canvas
            Canvas selectionCanvas = new Canvas
            {
                Width = 500,  // 与背景图片的宽度保持一致
                Height = 300  // 与背景图片的高度保持一致
            };

            // 在Canvas加载完成后再显示可选择的图片
            selectionCanvas.Loaded += (s, e) =>
            {
                DisplaySelectableImages(selectionCanvas, rand);
            };

            // 创建一个带白色边框的Border，表示随机生成范围
            Border selectionBorder = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = 500,  // 边框的宽度，与Canvas保持一致
                Height = 300,  // 边框的高度，与Canvas保持一致
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 创建一个StackPanel来包含选择Canvas和正确图片Panel
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // 将Canvas添加到Grid的前景层（背景图片之上）
            gameGrid.Children.Add(selectionCanvas);

            // 将带边框的Border添加到Canvas之上，表示随机生成范围
            gameGrid.Children.Add(selectionBorder);

            // 在StackPanel的下方显示需要选择的正确图片
            //DisplayCorrectImages(mainPanel);

            // 将StackPanel添加到Grid的下方
            gameGrid.Children.Add(mainPanel);

            // 将Grid添加到MainGrid中
            MainGrid.Children.Add(gameGrid);
        }

        private void DisplaySelectableImages(Canvas selectionCanvas, Random rand)
        {
            // 边界信息
            double leftBound = -3.2;
            double rightBound = 496.8;
            double topBound = 1.98;
            double bottomBound = 282.52;

            // 创建一个可选择图片的索引列表，并首先添加正确图片的索引
            List<int> selectableIndices = correctImages
                .Select(img => Array.IndexOf(imagePaths[1], ((BitmapImage)img.Source).UriSource.ToString().Replace("pack://application:,,,", "")))
                .ToList();

            // 从剩余的图片中随机选择，直到达到chose_picture_number
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[1].Length)
                .Where(i => !selectableIndices.Contains(i) && imagePaths[1][i] != "Img/2/Background.png")
                .OrderBy(x => rand.Next())
                .Take(chose_picture_number - selectableIndices.Count)
                .ToList();

            selectableIndices.AddRange(remainingIndices);

            // 随机化选择图片的顺序
            selectableIndices = selectableIndices.OrderBy(x => rand.Next()).ToList();

            foreach (int index in selectableIndices)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                // 随机生成X和Y坐标，确保图片不会超出背景图片边界
                double maxLeft = rightBound - img.Width;
                double maxTop = bottomBound - img.Height;

                double left = rand.NextDouble() * (maxLeft - leftBound) + leftBound;
                double top = rand.NextDouble() * (maxTop - topBound) + topBound;

                // 创建边框并设置图片位置
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // 初始时无边框
                    Child = img
                };

                // 设置 Border 的位置
                Canvas.SetLeft(border, left);
                Canvas.SetTop(border, top);

                border.MouseLeftButtonDown += (sender, e) =>
                {
                    if (correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString() == ((BitmapImage)img.Source).UriSource.ToString()))
                    {
                        border.BorderBrush = Brushes.Green;
                        selectedImages.Add(img);
                    }
                    else
                    {
                        border.BorderBrush = Brushes.Red;
                    }
                };

                selectionCanvas.Children.Add(border);
            }
        }

        private void confirmButton_Click2(object sender, RoutedEventArgs e)
        {
            bool isCorrect = false;

            if (train_mode == 2)
            {
                // 模式2的确认逻辑
                isCorrect = selectedImages.Count == correctImages.Count &&
                            selectedImages.All(si => correctImages.Any(ci => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));

                if (isCorrect)
                {
                    success_time++;
                }
                else
                {
                    fail_time++;
                }

                // 更新最近的游戏结果队列（模式2）
                UpdateRecentResultsMode2(isCorrect);
            }
            else if (train_mode == 3)
            {
                // 模式3的确认逻辑
                isCorrect = selectedImages.Count == correctImages.Count &&
                            selectedImages.All(si => correctImages.Any(ci => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));

                if (isCorrect)
                {
                    success_time++;
                }
                else
                {
                    fail_time++;
                }
            }

            EndGame(); // 触发结束游戏逻辑
        }

        private void UpdateRecentResultsMode2(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(isCorrect); // 添加当前结果

            if (recentResults.Count == 5)
            {
                AdjustDifficultyBasedOnResultsMode2();
            }
        }

        private void AdjustDifficultyBasedOnResultsMode2()
        {
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                IncreaseDifficultyMode2();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                DecreaseDifficultyMode2();
            }
        }

        private void IncreaseDifficultyMode2()
        {
            if (level < 18) // 假设最大难度是18级
            {
                level++;
                AdjustDifficultyMode2(level);
            }
        }

        // 降低难度（模式2）
        private void DecreaseDifficultyMode2()
        {
            if (level > 1) // 假设最低难度是1级
            {
                level--;
                AdjustDifficultyMode2(level);
            }
        }

        private void ResetGameState()
        {
            // 重置所有游戏状态
            missingNumbers = new List<int>();
            userInputNumbers = new List<int>();
            userInput = string.Empty;
            UpdateTextBlock();
        }

        private void SetupGameMode1()
        {
            confirm1.Visibility = Visibility.Visible;
            textBlock.Visibility = Visibility.Visible;
            myCanvas.Visibility = Visibility.Visible;

            // 清除上一次游戏的内容
            MainGrid.Children.Clear();

            // 检查并移除 `confirm1` 的父容器
            if (confirm1.Parent != null)
            {
                ((Grid)confirm1.Parent).Children.Remove(confirm1);
            }

            MainGrid2.Children.Add(confirm1);

            // 根据 mode1_display_size 设置显示框的大小
            double width = 300;
            double height = 150;

            switch (mode1_display_size)
            {
                case 1:
                    width = 300;
                    height = 150;
                    break;
                case 2:
                    width = 450;
                    height = 225;
                    break;
                case 3:
                    width = 600;
                    height = 300;
                    break;
                case 4:
                    width = 1000;  // 手动设置大小
                    height = 650; // 手动设置大小
                    break;
            }

            // 创建一个带有白色边框的透明长方形
            Border gameAreaBorder = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            MainGrid.Children.Add(gameAreaBorder);

            // 随机生成数字列表并移除几个数字
            List<int> numbers = Enumerable.Range(1, maxnumber).ToList();
            missingNumbers = RemoveRandomNumbers(numbers);

            // 显示剩余的数字，并随机分布在显示区域中
            Canvas numbersCanvas = new Canvas();
            Random rand = new Random();

            foreach (int number in numbers)
            {
                TextBlock numberText = new TextBlock
                {
                    Text = number.ToString(),
                    FontSize = 40,
                    Foreground = Brushes.Black
                };

                // 随机设置数字的位置
                double left = rand.Next(0, Math.Max(1, (int)(width - 50)));
                double top = rand.Next(0, Math.Max(1, (int)(height - 50)));

                Canvas.SetLeft(numberText, left);
                Canvas.SetTop(numberText, top);

                numbersCanvas.Children.Add(numberText);
            }

            gameAreaBorder.Child = numbersCanvas;
        }

        private List<int> RemoveRandomNumbers(List<int> numbers)
        {
            Random rand = new Random();
            List<int> removedNumbers = new List<int>();

            for (int i = 0; i < miss_number; i++)
            {
                if (numbers.Count == 0)
                    break;

                int index = rand.Next(numbers.Count);
                removedNumbers.Add(numbers[index]);
                numbers.RemoveAt(index);
            }

            return removedNumbers.OrderBy(n => n).ToList(); // 返回已移除的数字（排序后的）
        }

        // 数字按钮按下事件处理函数
        private void OnNumberButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                int number = int.Parse(button.Content.ToString());
                userInputNumbers.Add(number);
                userInput += button.Content.ToString();
                UpdateTextBlock();
            }
        }

        // "✔" 按钮按下事件处理函数
        private void OnSubmitButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                userInput = string.Empty;
                UpdateTextBlock();
            }
        }
        // "确认" 按钮按下事件处理函数，原OnBackButtonClick功能
        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (train_mode == 4) // 确保这是模式4的逻辑
            {
                // 获取用户输入的图片数量列表
                List<int> userInputCounts = new List<int>(userInputNumbers);

                // 初始化正确图片数量计数
                Dictionary<string, int> correctImageCounts = new Dictionary<string, int>();

                foreach (var correctImage in correctImages)
                {
                    string imageUri = ((BitmapImage)correctImage.Source).UriSource.ToString().Replace("pack://application:,,,", "");

                    if (!correctImageCounts.ContainsKey(imageUri))
                    {
                        correctImageCounts[imageUri] = 0;
                    }

                    correctImageCounts[imageUri]++;
                }

                bool isCorrect = true;

                foreach (var correctImageUri in correctImageCounts.Keys)
                {
                    // 获取背景图片中实际出现的正确图片数量
                    int actualCount = selectionCanvas.Children.OfType<Border>()
                        .Count(border => ((BitmapImage)((System.Windows.Controls.Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == correctImageUri);

                    // 检查玩家输入的数量是否匹配实际数量
                    if (!userInputCounts.Contains(actualCount))
                    {
                        isCorrect = false;
                        break;
                    }

                    // 移除已匹配的数量，避免重复匹配
                    userInputCounts.Remove(actualCount);
                }

                if (isCorrect)
                {
                    success_time++;
                }
                else
                {
                    fail_time++;
                }

                // 更新最近的游戏结果
                UpdateRecentResultsMode4(isCorrect);

                // 调整难度
                AdjustDifficultyBasedOnResultsMode4();

                EndGame(); // 触发结束游戏逻辑
            }
            else
            {
                SubmitInput();
            }
        }

        private void UpdateRecentResultsMode4(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(isCorrect); // 添加当前结果
        }

        private void AdjustDifficultyBasedOnResultsMode4()
        {
            // 首先检查最近5次结果是否达到了5次
            if (recentResults.Count < 5)
            {
                return; // 如果结果少于5次，不更新难度
            }

            // 计算最近5次中的正确和错误次数
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                IncreaseDifficultyMode4();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                DecreaseDifficultyMode4();
            }
        }

        private void IncreaseDifficultyMode4()
        {
            if (level < 18) // 假设最大难度是18级
            {
                level++;
                AdjustDifficultyMode4(level);
            }
        }

        private void DecreaseDifficultyMode4()
        {
            if (level > 1) // 假设最低难度是1级
            {
                level--;
                AdjustDifficultyMode4(level);
            }
        }

        // "←" 按钮按下事件处理函数，新功能：删除上一个输入的数字
        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                // 删除最后一个数字
                userInputNumbers.RemoveAt(userInputNumbers.Count - 1);
                userInput = userInput.Substring(0, userInput.Length - 1);
                UpdateTextBlock();
            }
        }

        private void UpdateTextBlock()
        {
            displayTextBlock.Text = userInput;
        }

        private void SubmitInput()
        {
            // 将用户输入的数字与缺失的数字列表进行比较


            // 确保缺失的数字列表也是排序的
            missingNumbers.Sort();

            // 比较两个已排序的列表
            bool isCorrect = userInputNumbers.SequenceEqual(missingNumbers);

            if (isCorrect)
            {
                modeTextBlock.Text = "恭喜你答对了！";
                modeTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                end.Visibility = Visibility.Visible;
                confirm1.Visibility = Visibility.Collapsed;

            }
            else
            {
                // 重新初始化游戏内容
                modeTextBlock.Text = "很遗憾答错了！";
                modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // 设置3秒的间隔
                timer.Tick += (s, args) =>
                {
                    // 3秒后执行的操作
                    InitializeGame(); // 再次重新开始游戏
                                      // 重置用户输入
                    userInputNumbers.Clear();
                    userInput = string.Empty;
                    UpdateTextBlock();
                    modeTextBlock.Text = "找出数字范围内的缺失数字，并将它们从小到大逐个输入";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                    // 停止计时器，防止重复触发
                    timer.Stop();
                };

                timer.Start(); // 启动计时器

            }

            // 更新最近的游戏结果队列
            //UpdateRecentResults(isCorrect);

            // 重新初始化游戏内容

        }

        private void UpdateRecentResults(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(isCorrect); // 添加当前结果

            if (recentResults.Count == 5)
            {
                AdjustDifficultyBasedOnResults();
            }
        }

        private void AdjustDifficultyBasedOnResults()
        {
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                IncreaseDifficulty();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                DecreaseDifficulty();
            }
        }

        private void IncreaseDifficulty()
        {
            if (level < 18) // 假设最大难度是18级
            {
                level++;
                AdjustDifficulty(level);
            }
        }

        private void DecreaseDifficulty()
        {
            if (level > 1) // 假设最低难度是1级
            {
                level--;
                AdjustDifficulty(level);
            }
        }

        private void EndGame()
        {
            ResetGameState(); // 重置游戏状态
            InitializeGame(); // 开始新游戏
        }

        private void AdjustDifficulty(int level)
        {
            switch (level)
            {
                case 1:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1; // 小
                    break;
                case 2:
                    maxnumber = 7;
                    miss_number = 2;
                    mode1_display_size = 1; // 小
                    break;
                case 3:
                    maxnumber = 8;
                    miss_number = 2;
                    mode1_display_size = 1; // 小
                    break;
                case 4:
                    maxnumber = 9;
                    miss_number = 3;
                    mode1_display_size = 1; // 小
                    break;
                case 5:
                    maxnumber = 9;
                    miss_number = 3;
                    mode1_display_size = 1; // 小
                    break;
                case 6:
                    maxnumber = 12;
                    miss_number = 3;
                    mode1_display_size = 2; // 中
                    break;
                case 7:
                    maxnumber = 14;
                    miss_number = 3;
                    mode1_display_size = 2; // 中
                    break;
                case 8:
                    maxnumber = 16;
                    miss_number = 4;
                    mode1_display_size = 2; // 中
                    break;
                case 9:
                    maxnumber = 18;
                    miss_number = 4;
                    mode1_display_size = 2; // 中
                    break;
                case 10:
                    maxnumber = 20;
                    miss_number = 4;
                    mode1_display_size = 3; // 大
                    break;
                case 11:
                    maxnumber = 24;
                    miss_number = 5;
                    mode1_display_size = 3; // 大
                    break;
                case 12:
                    maxnumber = 28;
                    miss_number = 5;
                    mode1_display_size = 3; // 大
                    break;
                case 13:
                    maxnumber = 30;
                    miss_number = 5;
                    mode1_display_size = 4; // 全屏
                    break;
                case 14:
                    maxnumber = 35;
                    miss_number = 5;
                    mode1_display_size = 4; // 全屏
                    break;
                case 15:
                    maxnumber = 38;
                    miss_number = 6;
                    mode1_display_size = 4; // 全屏
                    break;
                case 16:
                    maxnumber = 40;
                    miss_number = 6;
                    mode1_display_size = 4; // 全屏
                    break;
                case 17:
                    maxnumber = 45;
                    miss_number = 7;
                    mode1_display_size = 4; // 全屏
                    break;
                case 18:
                    maxnumber = 50;
                    miss_number = 8;
                    mode1_display_size = 4; // 全屏
                    break;
                default:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1; // 小
                    break;
            }
        }

        private void AdjustDifficultyMode2(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    break;
                case 3:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 4:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 5:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 6:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    break;
                case 7:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 8:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    break;
                case 9:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 10:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 11:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 12:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 13:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 14:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 15:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 16:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 17:
                    right_picture_number = 3;
                    chose_picture_number = 12; // 4x3
                    break;
                case 18:
                    right_picture_number = 3;
                    chose_picture_number = 18; // 6x3
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
            }
        }

        private void AdjustDifficultyMode3(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 3; // 2个正确图片 + 1个干扰图片
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 4; // 2个正确图片 + 2个干扰图片
                    break;
                case 3:
                    right_picture_number = 3;
                    chose_picture_number = 5; // 3个正确图片 + 2个干扰图片
                    break;
                case 4:
                    right_picture_number = 3;
                    chose_picture_number = 6; // 3个正确图片 + 3个干扰图片
                    break;
                case 5:
                    right_picture_number = 3;
                    chose_picture_number = 7; // 3个正确图片 + 4个干扰图片
                    break;
                case 6:
                    right_picture_number = 4;
                    chose_picture_number = 8; // 4个正确图片 + 4个干扰图片
                    break;
                case 7:
                    right_picture_number = 4;
                    chose_picture_number = 9; // 4个正确图片 + 5个干扰图片
                    break;
                case 8:
                    right_picture_number = 4;
                    chose_picture_number = 10; // 4个正确图片 + 6个干扰图片
                    break;
                case 9:
                    right_picture_number = 5;
                    chose_picture_number = 11; // 5个正确图片 + 6个干扰图片
                    break;
                case 10:
                    right_picture_number = 5;
                    chose_picture_number = 12; // 5个正确图片 + 7个干扰图片
                    break;
                case 11:
                    right_picture_number = 6;
                    chose_picture_number = 13; // 6个正确图片 + 7个干扰图片
                    break;
                case 12:
                    right_picture_number = 6;
                    chose_picture_number = 14; // 6个正确图片 + 8个干扰图片
                    break;
                case 13:
                    right_picture_number = 7;
                    chose_picture_number = 15; // 7个正确图片 + 8个干扰图片
                    break;
                case 14:
                    right_picture_number = 7;
                    chose_picture_number = 16; // 7个正确图片 + 9个干扰图片
                    break;
                case 15:
                    right_picture_number = 8;
                    chose_picture_number = 17; // 8个正确图片 + 9个干扰图片
                    break;
                case 16:
                    right_picture_number = 8;
                    chose_picture_number = 18; // 8个正确图片 + 10个干扰图片
                    break;
                case 17:
                    right_picture_number = 9;
                    chose_picture_number = 19; // 9个正确图片 + 10个干扰图片
                    break;
                case 18:
                    right_picture_number = 10;
                    chose_picture_number = 20; // 10个正确图片 + 10个干扰图片
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
            }
        }

        private void AdjustDifficultyMode4(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 1; // 要计数对象的种类
                    max_right_display = 2;
                    mini_right_display = 2;
                    mislead_picture_display_number = 4; // 不相关物品种类
                    break;
                case 2:
                    right_picture_number = 1;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 5;
                    break;
                case 3:
                    right_picture_number = 1;
                    max_right_display = 4;
                    mini_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 4:
                    right_picture_number = 1;
                    max_right_display = 5;
                    mini_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 5:
                    right_picture_number = 1;
                    max_right_display = 6;
                    mini_right_display = 6;
                    mislead_picture_display_number = 8;
                    break;
                case 6:
                    right_picture_number = 2;
                    max_right_display = 4;
                    mini_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 7:
                    right_picture_number = 2;
                    max_right_display = 5;
                    mini_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 8:
                    right_picture_number = 2;
                    max_right_display = 6;
                    mini_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 9:
                    right_picture_number = 2;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 10:
                    right_picture_number = 2;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 11:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 7;
                    break;
                case 12:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 13:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 14:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 15:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 11;
                    break;
                case 16:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 15;
                    break;
                case 17:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 20;
                    break;
                case 18:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 25;
                    break;
                default:
                    right_picture_number = 1;
                    max_right_display = 2;
                    mini_right_display = 2;
                    mislead_picture_display_number = 4;
                    break;
            }
        }


private void end_Click(object sender, RoutedEventArgs e)
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

                        MainGrid.Visibility = Visibility.Collapsed;
                        MainGrid2.Visibility = Visibility.Collapsed;

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
                        MainGrid.Visibility = Visibility.Collapsed;
                        MainGrid2.Visibility = Visibility.Collapsed;

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
                        MainGrid2.Visibility = Visibility.Visible;
                        MainGrid.Visibility = Visibility.Visible;
                        // 强制焦点保持在窗口
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
