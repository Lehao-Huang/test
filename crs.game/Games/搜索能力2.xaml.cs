using crs.core.DbModels;
using crs.core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// EXO.xaml 的交互逻辑
    /// </summary>
    public partial class 搜索能力2 : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
{
    "EXO/1/1.png", "EXO/1/2.png", "EXO/1/3.png", "EXO/1/4.png",
    "EXO/1/5.png", "EXO/1/6.png", "EXO/1/7.png", "EXO/1/8.png",
    "EXO/1/9.png", "EXO/1/10.png", "EXO/1/11.png", "EXO/1/12.png",
    "EXO/1/13.png", "EXO/1/14.png", "EXO/1/15.png", "EXO/1/16.png",
    "EXO/1/17.png", "EXO/1/18.png", "EXO/1/19.png", "EXO/1/20.png",
    "EXO/1/21.png", "EXO/1/22.png", "EXO/1/23.png", "EXO/1/24.png",
    "EXO/1/25.png", "EXO/1/26.png", "EXO/1/27.png", "EXO/1/28.png",
    "EXO/1/29.png", "EXO/1/30.png", "EXO/1/31.png", "EXO/1/32.png"
},
new string[]
{
    "EXO/2/Rocket1.png", "EXO/2/Rocket2.png", "EXO/2/Star.png", "EXO/2/Planet1.png",
    "EXO/2/Planet3.png", "EXO/2/Sun.png", "EXO/2/Planet2.png", "EXO/2/Meteor.png",
    "EXO/2/Planet4.png", "EXO/2/Spaceship.png", "EXO/2/Background.png"
}

        };

        private int max_time = 1; // 窗口总的持续时间，单位分钟

        private bool is_gaming = false;
        private int success_time = 0;
        private int fail_time = 0;

        private List<int> missingNumbers;
        private List<int> userInputNumbers;
        private string userInput; // 存储用户输入的数字

        private int right_picture_number = 4; // 显示的正确图片数量
        private int chose_picture_number = 6; // 显示的可选择图片数量


        //------------------报告参数-------------------------------------
        private int train_mode = 3; // 游戏模式，1，2，3，4	1.	模式1：找出数字范围内的缺失数字，并将它们从小到大逐个输入。这种模式通常涉及用户识别和输入缺失的数字。
                                    //模式2：识别出叠加在一起的不同形状，并将它们从屏幕下部选择出来。这种模式涉及到用户需要从叠加的形状中找到正确的形状。
                                    //模式4：数出并输入每个正确对象在图片中出现的次数。
        private int level = 1; // 当前游戏难度等级
        private int level_updata_threshold = 3; // 难度更新的正确或错误阈值
        private bool is_finish = true;//是否完成

        private int repet_time = 1;

        //----------------------------------------------------------------
        private int max_right_display = 2; // 最多显示的正确图片数量
        private int mini_right_display = 1; // 最少显示的正确图片数量
        private int mislead_picture_display_number = 4; // 干扰图片中总的显示数量
        private int repet_count = 0;
        private List<Image> correctImages; // 正确图片的列表
        private List<Image> selectedImages; // 用户选择的图片

        private Queue<bool> recentResults = new Queue<bool>(5); // 记录最近5次游戏结果的队列
        private int maxnumber = 5; // 显示的最大数字
        private int miss_number = 2; // 消失的数字数量
        private int mode1_display_size = 4; // 显示框的大小：1=小，2=中，3=大，4=全屏

        private const int MaxGames = 10;
        private int[] correctAnswers = new int[20];
        private int[] wrongAnswers = new int[20];
        private int[] ignoreAnswers = new int[20];

        private DispatcherTimer gameTimer; // 全局计时器
        private TimeSpan timeRemaining; // 剩余时间

        private Canvas selectionCanvas; // 在类中声明 selectionCanvas 作为全局变量


        public 搜索能力2()
        {
            InitializeComponent();
        }

        public static Window GetTopLevelWindow(UserControl userControl)
        {
            DependencyObject current = userControl;
            while (current != null && !(current is Window))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            return current as Window;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // 每秒减少剩余时间
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                int remainingSeconds = (int)timeRemaining.TotalSeconds;

                TimeStatisticsAction.Invoke(remainingSeconds, 0);
            }
            else
            {
                gameTimer.Stop(); // 停止计时器

                OnGameEnd();
            }
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

            AdjustDifficulty(level); // 根据当前level调整游戏难度

            // 初始化游戏模式
            if (train_mode == 1)
            {
                SetupGameMode1();
            }
            else if (train_mode == 2)
            {
                SetupGameMode2();
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
        private void SetupGameMode2()
        {
            confirm.Visibility = Visibility.Visible;

            // 清除之前的内容
            MainGrid.Children.Clear();

            // 为 MainGrid 添加行定义
            MainGrid.RowDefinitions.Clear();
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) }); // 上方叠加图片区域
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // 下方选择图片区域

            // 显示叠加的正确图片
            correctImages = new List<Image>();
            selectedImages = new List<Image>();
            DisplayOverlayImages();

            // 显示可供选择的图片
            DisplayChoiceImages();
        }

        private void DisplayOverlayImages()
        {
            Canvas overlayCanvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 300,  // 宽度可以根据需求调整
                Height = 300  // 高度可以根据需求调整
            };

            Random rand = new Random();
            List<int> indices = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => rand.Next()).Take(right_picture_number).ToList();

            foreach (int index in indices)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[0][index], UriKind.Relative)),
                    Width = 100,
                    Height = 100,
                    Opacity = 0.5, // 设置透明度
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                // 随机调整图片的位置和旋转角度，使其产生重叠效果
                double left = rand.Next(50);
                double top = rand.Next(50);
                double angle = rand.Next(-15, 15);

                Canvas.SetLeft(img, left);
                Canvas.SetTop(img, top);

                RotateTransform rotateTransform = new RotateTransform(angle);
                img.RenderTransform = rotateTransform;

                correctImages.Add(img);  // 将正确图片添加到列表中
                overlayCanvas.Children.Add(img);
            }

            Grid.SetRow(overlayCanvas, 0);
            MainGrid.Children.Add(overlayCanvas);
        }

        private void DisplayChoiceImages()
        {
            WrapPanel choicePanel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            Random rand = new Random();
            List<int> indices = new List<int>();

            // 首先确保所有重叠显示的正确图片被添加到选择列表中
            foreach (var correctImg in correctImages)
            {
                // 这里直接获取正确图片的索引
                int correctIndex = Array.IndexOf(imagePaths[0], ((BitmapImage)correctImg.Source).UriSource.ToString().Replace("pack://application:,,,", ""));
                indices.Add(correctIndex);
            }

            // 填充剩余的选择图片，确保总数达到 chose_picture_number
            while (indices.Count < chose_picture_number)
            {
                int index = rand.Next(imagePaths[0].Length);
                if (!indices.Contains(index))
                {
                    indices.Add(index);
                }
            }

            // 随机化选择图片的顺序
            indices = indices.OrderBy(x => rand.Next()).ToList();

            foreach (int index in indices)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[0][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // 初始时无边框
                    Child = img
                };

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

                choicePanel.Children.Add(border);
            }

            Grid.SetRow(choicePanel, 1);
            MainGrid.Children.Add(choicePanel);
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
            Image backgroundImage = new Image
            {
                Source = new BitmapImage(new Uri("EXO/2/Background.png", UriKind.Relative)),
                Stretch = Stretch.Uniform,
                Width = 500,  // 调整背景图片的宽度
                Height = 300,  // 调整背景图片的高度
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 将背景图片添加到Grid的Background层
            gameGrid.Children.Add(backgroundImage);

            // 初始化正确图片列表
            correctImages = new List<Image>();

            // 随机选择正确的图片，并将其添加到correctImages列表中
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != "EXO/2/Background.png" && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    // 将正确图片添加到correctImages列表中
                    Image img = new Image
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
            List<Image> imagesToDisplay = new List<Image>();

            // 添加正确图片到图片显示列表
            foreach (var correctImage in correctImages)
            {
                // 为每个正确图片随机生成显示次数
                int displayCount = rand.Next(mini_right_display, max_right_display + 1);
                for (int i = 0; i < displayCount; i++)
                {
                    Image imgCopy = new Image
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
                             && imagePaths[1][i] != "EXO/2/Background.png")
                .OrderBy(x => rand.Next())
                .Take(mislead_picture_display_number)
                .ToList();

            foreach (var index in remainingIndices)
            {
                Image img = new Image
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
                    .Count(border => ((BitmapImage)((Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == imageUri);

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
                correctAnswers[level] += 1;
            }
            else
            {
                fail_time++;
                wrongAnswers[level] += 1;

                int ignoredCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                ignoreAnswers[level] += ignoredCount;
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
                Image correctImg = new Image
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
            Image backgroundImage = new Image
            {
                Source = new BitmapImage(new Uri("EXO/2/Background.png", UriKind.Relative)),
                Stretch = Stretch.Uniform,
                Width = 500,  // 调整背景图片的宽度
                Height = 300,  // 调整背景图片的高度
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 将背景图片添加到Grid的Background层
            gameGrid.Children.Add(backgroundImage);

            // 初始化正确图片列表
            correctImages = new List<Image>();
            selectedImages = new List<Image>();

            // 随机选择正确的图片，并将其添加到correctImages列表中
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != "EXO/2/Background.png" && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    Image img = new Image
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
            DisplayCorrectImages(mainPanel);

            // 将StackPanel添加到Grid的下方
            gameGrid.Children.Add(mainPanel);

            // 将Grid添加到MainGrid中
            MainGrid.Children.Add(gameGrid);
        }
        private void DisplayCorrectImages(StackPanel mainPanel)
        {
            StackPanel correctPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0) // 调整正确图片的显示位置
            };

            foreach (var img in correctImages)
            {
                Image correctImg = new Image
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
                .Where(i => !selectableIndices.Contains(i) && imagePaths[1][i] != "EXO/2/Background.png")
                .OrderBy(x => rand.Next())
                .Take(chose_picture_number - selectableIndices.Count)
                .ToList();

            selectableIndices.AddRange(remainingIndices);

            // 随机化选择图片的顺序
            selectableIndices = selectableIndices.OrderBy(x => rand.Next()).ToList();

            foreach (int index in selectableIndices)
            {
                Image img = new Image
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
                    correctAnswers[level] += 1;
                }
                else
                {
                    fail_time++;
                    wrongAnswers[level] += 1;
                    int ignoredCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                    ignoreAnswers[level] += ignoredCount;
                    is_finish = false;
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
                    correctAnswers[level] += 1;
                }
                else
                {
                    fail_time++;
                    wrongAnswers[level] += 1;
                    int ignoreCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                    ignoreAnswers[level] += ignoreCount;
                    is_finish = false;

                }

                UpdateRecentResultsMode3(isCorrect);
            }

            EndGame(); // 触发结束游戏逻辑
        }
        private void UpdateRecentResultsMode3(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(isCorrect); // 添加当前结果

            if (recentResults.Count == 5)
            {
                AdjustDifficultyBasedOnResultsMode3(); // 当结果达到5次时调整难度
            }

            LevelStatisticsAction?.Invoke(level, 18);

            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            // 更新正确和错误次数的统计
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
        }

        private void AdjustDifficultyBasedOnResultsMode3()
        {
            if (recentResults.Count < 5)
            {
                return; // 如果结果少于5次，不更新难度
            }

            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                level++;
                recentResults.Clear();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                if (repet_count > repet_time)
                {
                    level--;
                }
                else
                {
                    repet_count++;
                }
                recentResults.Clear();
            }
            LevelStatisticsAction?.Invoke(level, 18);

            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
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
            LevelStatisticsAction?.Invoke(level, 18);
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            // 使用 recentResults 中的对和错的数量
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
        }

        private void AdjustDifficultyBasedOnResultsMode2()
        {
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {

                IncreaseDifficultyMode2();
                recentResults.Clear();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                if (repet_count > repet_time)
                {
                    DecreaseDifficultyMode2();
                }
                else
                {
                    repet_count++;
                }
                recentResults.Clear();
            }
            LevelStatisticsAction?.Invoke(level, 18);

            // 使用 recentResults 中的对和错的数量
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
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
                        .Count(border => ((BitmapImage)((Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == correctImageUri);

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
                    correctAnswers[level] += 1;
                }
                else
                {
                    fail_time++;
                    wrongAnswers[level] += 1;
                    is_finish = false;
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

            LevelStatisticsAction?.Invoke(level, 18);
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            // 使用 recentResults 中的对和错的数量
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
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
                recentResults.Clear();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                if (repet_count > repet_time)
                {
                    DecreaseDifficultyMode4();
                }
                else
                {
                    repet_count++;
                }
                recentResults.Clear();
            }
            LevelStatisticsAction?.Invoke(level, 18);

            // 使用 recentResults 中的对和错的数量
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
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
            userInputNumbers.Sort(); // 确保输入的数字按顺序排列

            bool isCorrect = userInputNumbers.SequenceEqual(missingNumbers);

            if (isCorrect)
            {
                success_time++;
                correctAnswers[level] += 1;
            }
            else
            {
                fail_time++;
                wrongAnswers[level] += 1;
                is_finish = false;
            }

            int ignoreCount = missingNumbers.Count(number => !userInputNumbers.Contains(number));
            ignoreAnswers[level] += ignoreCount;
            // 更新最近的游戏结果队列
            UpdateRecentResults(isCorrect);

            // 重新初始化游戏内容
            InitializeGame(); // 再次重新开始游戏

            // 重置用户输入
            userInputNumbers.Clear();
            userInput = string.Empty;
            UpdateTextBlock();
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
            LevelStatisticsAction?.Invoke(level, 18);
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            // 使用 recentResults 中的对和错的数量
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
        }

        private void AdjustDifficultyBasedOnResults()
        {
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {

                IncreaseDifficulty();
                recentResults.Clear();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                if (repet_count > repet_time)
                {
                    DecreaseDifficulty();
                }
                else
                {
                    repet_count++;
                }
                recentResults.Clear();
            }
            LevelStatisticsAction?.Invoke(level, 18);

            // 使用 recentResults 中的对和错的数量
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
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

        /* protected override async Task OnStopAsync()
         {
             throw new NotImplementedException();
         }

         public List<GameBaseDemoInfo> GetDemoInfos()
         {
             throw new NotImplementedException();
         }*/
    }
    public partial class 搜索能力2 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            max_time = 1; // 窗口总的持续时间，单位分钟
            train_mode = 3; // 游戏模式，1、2或3
            level = 1; // 当前游戏难度等级


            {
                // 参数（包含模块参数信息）
                var baseParameter = BaseParameter;
                if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
                {
                    Debug.WriteLine("ProgramModulePars 已加载数据：");
                    // 遍历 ProgramModulePars 打印出每个参数
                    foreach (var par in baseParameter.ProgramModulePars)
                    {
                        //Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");
                        if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                        {
                            switch (par.ModuleParId)
                            {
                                case 139: // 治疗时间
                                    max_time = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"max_time={max_time}");
                                    break;

                                case 140: // 游戏模式
                                    train_mode = par.Value.HasValue ? (int)par.Value.Value : 3;
                                    Debug.WriteLine($"train_mode ={train_mode}");
                                    break;
                                case 177: //游戏等级
                                    level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"level ={level}");
                                    break;
                                case 142: // 难度更新阈值
                                    level_updata_threshold = par.Value.HasValue ? (int)par.Value.Value : 3;
                                    Debug.WriteLine($"level_updata_threshold={level_updata_threshold}");
                                    break;

                                case 143: // 正确图片数量
                                    right_picture_number = par.Value.HasValue ? (int)par.Value.Value : 4;
                                    Debug.WriteLine($"right_picture_number ={right_picture_number}");
                                    break;
                                case 144: // 可选图片数量
                                    chose_picture_number = par.Value.HasValue ? (int)par.Value.Value : 6;
                                    Debug.WriteLine($"chose_picture_number ={chose_picture_number}");
                                    break;
                                case 146: // 最大图片数量
                                    max_right_display = par.Value.HasValue ? (int)par.Value.Value : 4;
                                    Debug.WriteLine($"max_right_display ={max_right_display}");
                                    break;
                                case 147: // 最少图片数量
                                    mini_right_display = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"mini_right_display ={mini_right_display}");
                                    break;

                                case 148: //干扰图片数量
                                    mislead_picture_display_number = par.Value.HasValue ? (int)par.Value.Value : 4;
                                    Debug.WriteLine($"mislead_picture_display_number ={mislead_picture_display_number}");
                                    break;
                                case 149: //显示的最大数字
                                    maxnumber = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    Debug.WriteLine($"maxnumber ={maxnumber}");
                                    break;
                                case 150: //消失的数字数量
                                    miss_number = par.Value.HasValue ? (int)par.Value.Value : 2;
                                    Debug.WriteLine($"miss_number ={miss_number}");
                                    break;
                                // 添加其他需要处理的 ModuleParId
                                default:
                                    Debug.WriteLine($"未处理的 ModuleParId: {par.ModuleParId}");
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("没有数据");
                }


            }



            // 调用委托
            LevelStatisticsAction?.Invoke(level, 18);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
             gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
            gameTimer.Tick += GameTimer_Tick;
            timeRemaining = TimeSpan.FromMinutes(max_time); // 设定整个窗口存在的时间
        }

        protected override async Task OnStartAsync()
        {
            gameTimer.Start(); // 开始计时

            EndGame();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            gameTimer.Stop(); // 停止计时器
        }

        protected override async Task OnPauseAsync()
        {
            gameTimer.Stop(); // 停止计时器

        }

        protected override async Task OnNextAsync()
        {
            // 调整难度
            EndGame();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 搜索能力2讲解();
        }


        // 插入写法
        private int GetCorrectNum(int difficultyLevel)
        {
            return correctAnswers[difficultyLevel];
        }
        private int GetWrongNum(int difficultyLevel)
        {
            return wrongAnswers[difficultyLevel];
        }
        private int GetIgnoreNum(int difficultyLevel)
        {
            return ignoreAnswers[difficultyLevel];
        }
        private double CalculateAccuracy(int correctCount, int wrongCount, int ignoreCount)
        {
            int totalAnswers = correctCount + wrongCount + ignoreCount;
            return totalAnswers > 0 ? Math.Round((double)correctCount / totalAnswers, 2) : 0;
        }

        private async Task updateDataAsync()
        {
            var baseParameter = BaseParameter;

            var program_id = baseParameter.ProgramId;
            Crs_Db2Context db = baseParameter.Db;

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        for (int lv = 1; lv <= level; lv++)
                        {
                            // 获取当前难度级别的数据
                            int correctCount = GetCorrectNum(lv);
                            int wrongCount = GetWrongNum(lv);
                            int mode = train_mode;
                            int rep = repet_time;
                            int totalCount = wrongCount * (rep + 1);
                            int Count = totalCount + correctCount;
                            if (correctCount == 0 && wrongCount == 0)
                            {
                                // 如果所有数据都为0，跳过此难度级别
                                Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
                                continue;
                            }
                            // 计算准确率
                            double accuracy = Math.Round((double)correctCount / (double)Count, 2);
                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "搜索能力2",
                                Eval = false,
                                Lv = lv, // 当前的难度级别
                                ScheduleId = BaseParameter.ScheduleId ?? null// 假设的 Schedule_id，可以替换为实际值
                            };
                            db.Results.Add(newResult);
                            await db.SaveChangesAsync(); //这里注释了
                            // 获得 result_id
                            int result_id = newResult.ResultId;
                            // 创建 ResultDetail 对象列表
                            var resultDetails = new List<ResultDetail>
                            {
                               new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "等级",
                                    Value = lv,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "目录",
                                    Value = mode,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率",
                                    Value = accuracy * 100, // 以百分比形式存储
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                  new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "总机会数",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                   new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "已使用机会数",
                                    Value = Count,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确次数",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误次数",
                                    Value = wrongCount,
                                    ModuleId =  BaseParameter.ModuleId
                                }
                            };
                            // 插入 ResultDetail 数据
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();
                            // 输出每个 ResultDetail 对象的数据
                            Debug.WriteLine($"难度级别 {lv}:");
                            foreach (var detail in resultDetails)
                            {
                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
                            }
                        }
                        // 提交事务
                        await transaction.CommitAsync();
                        Debug.WriteLine("插入成功");
                    });
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

    }
}
