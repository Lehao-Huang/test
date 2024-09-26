using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;
using crs.core;
using System.Diagnostics;
using crs.core.DbModels;
using System.Windows.Media.Animation;
namespace crs.game.Games
{
    /// <summary>
    /// BILD.xaml 的交互逻辑
    /// </summary>
    public partial class 图形记忆力 : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
{
    "BILD/1/手表.jpg",
    "BILD/1/香蕉.jpg",
    "BILD/1/西瓜.jpg",
    "BILD/1/油桃.jpg",
    "BILD/1/葡萄.jpg",
    "BILD/1/鱼.jpg",
    "BILD/1/兔子.jpg",
    "BILD/1/鞋子.jpg",
    "BILD/1/熊猫.jpg",
    "BILD/1/乌龟.jpg",
    "BILD/1/羊.jpg",
    "BILD/1/猪.jpg",
},
new string[]
{
    "BILD/2/电话机.jpg",
    "BILD/2/菠萝.jpg",
    "BILD/2/电视机.jpg",
    "BILD/2/番茄.jpg",
    "BILD/2/吹风机.jpg",
    "BILD/2/锤子.jpg",
    "BILD/2/大象.jpg",
    "BILD/2/电脑.jpg"
},
new string[]
{
    "BILD/3/蝴蝶.jpg",
    "BILD/3/橘子.jpg",
    "BILD/3/狗.jpg",
    "BILD/3/螺丝钉.jpg",
    "BILD/3/苹果.jpg",
    "BILD/3/梨子.jpg",
    "BILD/3/闹钟.jpg",
    "BILD/3/剪刀.jpg",
    "BILD/3/猕猴桃.jpg",
    "BILD/3/猫.jpg"
}

        };

        // 用于保存选中的图片路径
        private List<string> selectedImagePaths = new List<string>();
        private int LEVEL_DURATION = 1; 
        private int total_picture_number_para = 7; // 该参数乘以正确的图片数量为总的图片数量
        private int right_picture_number = 3; // 显示的正确图片数量
        private int train_mode = 1;
        private int LEVEL_UP_THRESHOLD = 85; // 提高难度的正确率阈值（百分比）
        private int LEVEL_DOWN_THRESHOLD = 70; // 降低难度的正确率阈值（百分比）
        private int max_time = 30;
        private bool IS_REALISTIC = true; // 图片是否显示为真实物体（默认显示真实图片）
        private int[] correctAnswers = new int[10];
        private int[] wrongAnswers = new int[10];
        private int[] ignoreAnswers = new int[10];
        private const int MaxGames = 10;
        private int hardness =1;
        private const int MAX_HARDNESS = 9; // 最大难度等级
        private DispatcherTimer sharedTimer;
        private Queue<bool> recentResults = new Queue<bool>(5); // 记录最近5次选择结果的队列



        //--------------window2--------------------

        private List<string> rightImagePaths; // 正确图片的路径
        private List<string> totalImagePaths; // 总的题库图片路径
        private int totalPictureMultiplier = 7; // 参数：该参数乘以正确的图片数量为总的图片数量
    
        private bool IS_FIXED_INTERVAL = false; // 项目间隔是否固定（默认不固定）
        private double SPEED_FACTOR = 1.0; // 传送带的速度因素（默认值）
        private bool IS_VISUAL_FEEDBACK = true; // 是否有视觉回馈
        private bool IS_AUDITORY_FEEDBACK = true; // 是否有声学回馈
        private int TYPE_OF_INPUT = 0; // 选用哪种输入方式
        private int CHOOSE_IMAGE_COUNT = 10; // imageBorder内图片显示数量

        private bool IS_IMAGE_DETAIL = true; // 跟难度相关，决定选择图片类型是否细节
        private bool IS_IMAGE_HARD = true; // 跟难度相关，决定图片类型是难还是简单
        private double DISPLAY_TIME = 3; // 图片滑行的总展示时间
        double RATE_OF_ERRORIMAGE = 0.5; // 展示错误（即非image2,3,4）的概率
        double Correct_decision_rate = 0;
        private int totalDecisions;
        private int correctDecisions;
        private int errorDecisions;
        private int missDecisions;
        private const int WAIT_DELAY = 1;

        private int gameIndex = 0;

        //private int remainingTime = 30;
        private int GameremainingTime = 10;
        private DispatcherTimer trainingTimer; // 程序持续时间计时器
        private DispatcherTimer gameTimer; // 单次游戏计时器
        private Random random = new Random();
        private int continueButtonPressCount = 0;// 按钮按下的次数
        private bool isGameRunning = false; // 标志游戏是否正在进行
        public event Action<int> GameremainingTimeUpdated;
        public event Action<int, int[], int[]> GameStatsUpdated;




        public 图形记忆力(int hardness_, int training_mode, int[] correctAnswers1, int[] wrongAnswers1, int[] ignoreAnswers1)
        {
            InitializeComponent();
            correctAnswers = correctAnswers1;
            wrongAnswers = wrongAnswers1;
            ignoreAnswers = ignoreAnswers1;
            hardness = hardness_;
            hard_set();
            sharedTimer = new DispatcherTimer();
            sharedTimer.Interval = TimeSpan.FromSeconds(1);
            sharedTimer.Tick += OnTick;
            // 随机选择指定数量的不重复图片
            train_mode = training_mode;
            List<string> allImages = imagePaths.SelectMany(x => x).ToList();
            Random random = new Random();
            selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();

            // 根据 training_mode 设置图片或文本的可见性
            if (training_mode == 1 || training_mode == 2)
            {
                // 显示图片，隐藏文本
                SetImagesVisible();
            }
            else if (training_mode == 3)
            {
                // 显示文本，隐藏图片
                SetTextsVisible();
            }
        }
        public 图形记忆力()
        {
            InitializeComponent();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (max_time > 0)
            {
                //TimeStatisticsAction.Invoke(10, 10);
                max_time--;
                TimeStatisticsAction.Invoke(max_time, GameremainingTime);
                LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
                int correctCount = 0;
                int incorrectCount = 0;
                foreach (bool result in recentResults)
                {
                    if (result)
                    {
                        correctCount++;
                    }
                    else
                    {
                        incorrectCount++;
                    }
                }
                RightStatisticsAction?.Invoke(correctCount, 5);
                WrongStatisticsAction?.Invoke(incorrectCount, 5);

            }
            else
            {
                sharedTimer.Stop();
                OnGameEnd();
            }
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

        



        private void SetImagesVisible()
        {
            // 清除之前的图片
            imageContainer.Children.Clear();

            // 动态添加图片到 UniformGrid
            foreach (var imagePath in selectedImagePaths)
            {
                Image imageControl = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(5)
                };

                imageContainer.Children.Add(imageControl);
            }
        }

        private void SetTextsVisible()
        {
            // 清除之前的文本
            imageContainer.Children.Clear();

            // 动态添加文本到 UniformGrid
            foreach (var imagePath in selectedImagePaths)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = Path.GetFileNameWithoutExtension(imagePath),
                    FontSize = 24,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                imageContainer.Children.Add(textBlock);
            }

        }

        // ContinueButton_Click 事件处理程序
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedImagePaths.Count > 0)
            {
                // 创建并打开新窗口，同时传递选中的图片路径列表
                //BILD_Answerwindow window1 = new BILD_Answerwindow(selectedImagePaths, train_mode, hardness);
                //window1.GameStatsUpdated += OnGameStatsUpdated;
                //window1.GameremainingTimeUpdated += OnGameremainingTimeUpdated;
                //window1.Show();
                Grid1.Visibility = Visibility.Collapsed;
                Grid2.Visibility = Visibility.Visible;

                // 启动程序计时器
                // 初始化剩余时间（以秒为单位）
                //remainingTime = remainingTime * 60;

                train_mode = 2;
                GameremainingTime = LEVEL_DURATION * 60;
                //GameremainingTime = 30;
                //GameremainingTime = 30;
                rightImagePaths = selectedImagePaths;

                GenerateTotalImagePaths();
                // 启动程序计时器
                trainingTimer = new DispatcherTimer();
                trainingTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
                trainingTimer.Tick += TrainingTimer_Tick;
                trainingTimer.Start();
                StartGame();
            }
            else
            {
                MessageBox.Show("未选择任何图片路径。");
            }
        }


        private void OnGameremainingTimeUpdated(int remainingTime)
        {// 调用操作的示例
            TimeStatisticsAction.Invoke(GameremainingTime, max_time);

        }

        private void StartGame()
        {
            if (!isGameRunning)
            {
                isGameRunning = true;
                // 启动游戏计时器
                // 检查是否已经存在 displayTimer 实例
                if (gameTimer != null)
                {
                    // 如果计时器正在运行，先停止它
                    if (gameTimer.IsEnabled)
                    {
                        gameTimer.Stop();
                    }

                    // 取消之前注册的事件（防止重复注册事件）
                    gameTimer.Tick -= GameTimer_Tick;

                    // 将 displayTimer 置为 null，表示它已被清理
                    gameTimer = null;
                }

                gameTimer = new DispatcherTimer();
                gameTimer.Interval = TimeSpan.FromSeconds(1);
                gameTimer.Tick += GameTimer_Tick;
                gameTimer.Start();
            }

        }

        private void NotifyGameStatsUpdated()
        {
            GameStatsUpdated?.Invoke(hardness, correctAnswers, wrongAnswers);
        }

        //protected override void OnClosed(EventArgs e)
        //{
        //    base.OnClosed(e);
        //    TimerManager.TimerElapsed -= OnTimerElapsed;
        //}
        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            if (true)
            {
                //remainingTime--;
                GameremainingTimeUpdated?.Invoke(GameremainingTime);
            }
            else
            {
                //trainingTimer.Stop();
                //gameTimer?.Stop();
                //isGameRunning = false;
                //AUFM_Report reportWindow = new AUFM_Report(LEVEL_UP_THRESHOLD, LEVEL_DOWN_THRESHOLD, max_time, LEVEL_DURATION, true, IS_REALISTIC, correctAnswers, wrongAnswers, ignoreAnswers);
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (GameremainingTime > 0)
            {
                GameremainingTime--;
                try
                {
                    TimeStatisticsAction.Invoke(max_time, GameremainingTime); 
                }
                catch (Exception ex)
                {
                    // 可以记录异常日志或其他处理方式
                   // Console.WriteLine($"Exception occurred: {ex.Message}");
                }
                ShowRandomImage1();
            }
            else
            {
                EndGame();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            PerformAction();
        }

        private void PerformAction()
        {
            try
            {
                // 获取 SelectionBox 的位置和尺寸
                GeneralTransform selectionTransform = SelectionBox.TransformToVisual(ImageGrid);
                Rect selectionRect = selectionTransform.TransformBounds(new Rect(0, 0, SelectionBox.ActualWidth, SelectionBox.ActualHeight));

                // 标记是否找到重叠的图片
                bool isOverlapFound = false;
                System.Windows.Controls.Image overlappedImage = null;

                // 遍历 imageContainer2 中的每一个图片，检查是否与 SelectionBox 重叠
                foreach (UIElement element in imageContainer2.Children)
                {
                    if (element is System.Windows.Controls.Image image)
                    {
                        GeneralTransform imageTransform = image.TransformToVisual(ImageGrid);
                        Rect imageRect = imageTransform.TransformBounds(new Rect(0, 0, image.ActualWidth, image.ActualHeight));

                        // 检查是否重叠
                        if (selectionRect.IntersectsWith(imageRect))
                        {
                            isOverlapFound = true;
                            overlappedImage = image;
                            break;
                        }
                    }
                }

                // 根据重叠结果进行处理
                if (isOverlapFound && overlappedImage != null)
                {
                    // 清除动画而不触发 Completed 事件
                    overlappedImage.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);

                    // 获取重叠图片的路径，直接从Tag属性获取
                    string imagePath = overlappedImage.Tag.ToString();
                    string imageName = System.IO.Path.GetFileNameWithoutExtension(imagePath); // 获取图片的名称（不含扩展名）

                    // 检查该图片路径是否在正确的题库中
                    bool isCorrect = rightImagePaths.Any(path => imagePath.EndsWith(path, StringComparison.OrdinalIgnoreCase));

                    // 根据检查结果更新 correctDecisions 和 SelectionBox 的描边颜色，并更新 Border 和 TextBlock
                    if (isCorrect)
                    {
                        correctDecisions++;
                        textBlock.Background = new SolidColorBrush(Colors.Green);
                        textBlock1.Text = imageName + " 正确！";
                        if (recentResults.Count >= 5)
                        {
                            recentResults.Dequeue(); // 移除最早的结果
                        }
                        recentResults.Enqueue(true); // 添加当前结果
                    }
                    else
                    {
                        errorDecisions++;
                        textBlock.Background = new SolidColorBrush(Colors.Red);
                        textBlock1.Text = imageName + " 错误！";
                        if (recentResults.Count >= 5)
                        {
                            recentResults.Dequeue(); // 移除最早的结果
                        }
                        recentResults.Enqueue(false); // 添加当前结果;
                    }
                    NotifyGameStatsUpdated();
                    // 从 imageContainer2 中移除图片
                    imageContainer2.Children.Remove(overlappedImage);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in PerformAction: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ShowRandomImage1()
        {
            try
            {
                // 检查 totalImagePaths 是否为 null 或为空
                if (totalImagePaths == null || totalImagePaths.Count == 0)
                {
                    throw new Exception("totalImagePaths is null or empty. Please ensure it is initialized properly.");
                }

                // 从总题库中随机选择一张图片
                string imagePath = totalImagePaths[random.Next(totalImagePaths.Count)];

                // 检查 imageContainer2 是否为 null
                if (imageContainer2 == null)
                {
                    throw new Exception("imageContainer2 is null. Please ensure it is initialized properly.");
                }

                System.Windows.Controls.Image newImage;

                if (train_mode == 2)
                {
                    // 提取文件名并去掉扩展名
                    string imageName = System.IO.Path.GetFileNameWithoutExtension(imagePath);

                    // 创建包含文件名的文本图像
                    RenderTargetBitmap renderBitmap = CreateTextImage(imageName);

                    newImage = new System.Windows.Controls.Image
                    {
                        Source = renderBitmap,
                        Width = 100,
                        Height = 100,
                        Margin = new Thickness(5),
                        Tag = imagePath // 将原始图片路径作为Tag存储
                    };
                }
                else
                {
                    // 正常显示图片
                    BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.Relative));

                    newImage = new System.Windows.Controls.Image
                    {
                        Source = bitmap,
                        Width = 100,
                        Height = 100,
                        Margin = new Thickness(5),
                        Tag = imagePath // 将原始图片路径作为Tag存储
                    };
                }

                // 确保图片显示在最上层
                Panel.SetZIndex(newImage, int.MaxValue); // 将 ZIndex 设置为最大值

                // 将新图片添加到 imageContainer2
                imageContainer2.Children.Add(newImage);

                // 动画移动图片
                AnimateImage(newImage);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in ShowRandomImage1: {ex.Message}\n{ex.StackTrace}");
            }
        }
        private RenderTargetBitmap CreateTextImage(string text)
        {
            // 创建文本块来显示文本
            TextBlock textBlock = new TextBlock
            {
                Text = text, // 使用文件名而不是全路径
                FontSize = 24,
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Colors.White),
                Width = 100,
                Height = 100,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // 渲染文本块为位图
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
            textBlock.Measure(new Size(100, 100));
            textBlock.Arrange(new Rect(new Size(100, 100)));
            renderBitmap.Render(textBlock);

            return renderBitmap;
        }
        private void AnimateImage(System.Windows.Controls.Image img)
        {
            try
            {
                double fromValue = 0;
                double windowWidth = 1280; // 固定窗口宽度
                double toValue = windowWidth - img.ActualWidth; // 窗口的宽度减去图片的宽度

                TranslateTransform translateTransform = new TranslateTransform();
                img.RenderTransform = translateTransform;

                double REAL_TIME = DISPLAY_TIME * SPEED_FACTOR; // 合成真正时间

                DoubleAnimation animation = new DoubleAnimation
                {
                    From = fromValue,
                    To = toValue,
                    Duration = new Duration(TimeSpan.FromSeconds(REAL_TIME))
                };
                animation.Completed += (s, e) =>
                {
                    try
                    {
                        // 动画完成后隐藏图片
                        img.Source = null;
                        // 从 imageContainer2 中移除图片
                        imageContainer2.Children.Remove(img);
                        missDecisions++;
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show($"Error during animation completion: {ex.Message}\n{ex.StackTrace}");
                    }
                };

                translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in AnimateImage: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void EndGame()
        {

            // 将当前游戏结果添加到对应的数组中

            if (hardness < 8)
            {
                if (errorDecisions > 0)
                {
                    wrongAnswers[hardness]++;
                }
                else
                {
                    correctAnswers[hardness]++;
                }
            }
            else
            {
                if(errorDecisions > 1)
                {
                    wrongAnswers[hardness]++;
                }
                else
                {
                    correctAnswers[hardness]++;
                }
            }

            

            // 重置这些变量

            // 清除 imageContainer2 中的所有动画并移除图片
            foreach (UIElement element in imageContainer2.Children)
            {
                if (element is System.Windows.Controls.Image image)
                {
                    // 停止动画
                    image.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);
                    // 清除图片资源
                    image.Source = null;
                }
            }
            imageContainer2.Children.Clear(); // 移除所有图片元素
            imageContainer2.Children.Clear(); // 移除所有图片元素




            // 调整难度
            AdjustDifficulty();
            // 显示新的随机图片
            totalDecisions = 0;
            correctDecisions = 0;
            errorDecisions = 0;
            missDecisions = 0;
            // 停止游戏计时器

            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer = null; // 清除计时器
            }

            isGameRunning = false;
            // 重置 SelectionBox 的描边颜色
            gameIndex++;

            // 打开 AUFM_Report 窗口，并关闭当前窗口
            //MainWindow reportWindow = new MainWindow(hardness, train_mode, correctAnswers, wrongAnswers, ignoreAnswers);
            Grid1.Visibility = Visibility.Visible; 
            Grid2.Visibility = Visibility.Collapsed;

            hard_set();

            // 随机选择指定数量的不重复图片

            List<string> allImages = imagePaths.SelectMany(x => x).ToList();
            Random random = new Random();
            selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
            // 根据 training_mode 设置图片或文本的可见性
            if (train_mode == 1 || train_mode == 2)
            {
                // 显示图片，隐藏文本
                SetImagesVisible();
            }
            else if (train_mode == 3)
            {
                // 显示文本，隐藏图片
                SetTextsVisible();
            }
        }

        private void AdjustDifficulty()
        {
            NotifyGameStatsUpdated();
            switch (hardness)
            {
                case 1:
                    if (errorDecisions >= 0)
                    {
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 2:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 3:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 4:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 5:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 6:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 7:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 8:
                    if (errorDecisions >= 1)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 9:
                    if (errorDecisions >= 1)
                    {
                        hardness--;
                    }
                    else
                    {
                    }

                    break;
                default:
                    throw new Exception("未知的难度级别");
            }
        }

        public void hard_set()
        {
            switch (hardness)
            {
                case 1:
                    IS_IMAGE_DETAIL = false;
                    IS_IMAGE_HARD = false;
                    CHOOSE_IMAGE_COUNT = 2;
                    RATE_OF_ERRORIMAGE = 0.33;
                    DISPLAY_TIME = 8;
                    break;
                case 2:
                    IS_IMAGE_DETAIL = false;
                    IS_IMAGE_HARD = false;
                    CHOOSE_IMAGE_COUNT = 3;
                    RATE_OF_ERRORIMAGE = 0.30;
                    DISPLAY_TIME = 8;
                    break;
                case 3:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = false;
                    CHOOSE_IMAGE_COUNT = 4;
                    RATE_OF_ERRORIMAGE = 0.28;
                    DISPLAY_TIME = 7;
                    break;
                case 4:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = false;
                    CHOOSE_IMAGE_COUNT = 6;
                    RATE_OF_ERRORIMAGE = 0.26;
                    DISPLAY_TIME = 7;
                    break;
                case 5:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 4;
                    RATE_OF_ERRORIMAGE = 0.24;
                    DISPLAY_TIME = 7;
                    break;
                case 6:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 6;
                    RATE_OF_ERRORIMAGE = 0.22;
                    DISPLAY_TIME = 7;
                    break;
                case 7:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 6;
                    RATE_OF_ERRORIMAGE = 0.20;
                    DISPLAY_TIME = 6;
                    break;
                case 8:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 9;
                    RATE_OF_ERRORIMAGE = 0.15;
                    DISPLAY_TIME = 6;
                    break;
                case 9:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 9;
                    RATE_OF_ERRORIMAGE = 0.10;
                    DISPLAY_TIME = 6;
                    break;
                default:
                    throw new Exception("未知的难度级别");
            }
        }

        private void GenerateTotalImagePaths()
        {
            try
            {
                // 初始化 totalImagePaths 并添加 rightImagePaths 中的元素
                if (rightImagePaths == null || rightImagePaths.Count == 0)
                {
                    throw new Exception("rightImagePaths is null or empty.");
                }

                totalImagePaths = new List<string>(rightImagePaths);

                int totalNumberOfPictures = rightImagePaths.Count * totalPictureMultiplier;
                Console.WriteLine($"Total number of pictures needed: {totalNumberOfPictures}");

                // 获取所有可能的图片路径
                List<string> allImages = imagePaths.SelectMany(x => x).ToList();
                if (allImages == null || allImages.Count == 0)
                {
                    throw new Exception("allImages is null or empty.");
                }
                Console.WriteLine($"Total number of all images: {allImages.Count}");

                // 从所有图片路径中删除已经选择的正确图片路径，确保不会重复选择
                var remainingImages = allImages.Except(rightImagePaths).ToList();
                Console.WriteLine($"Remaining images after excluding right images: {remainingImages.Count}");

                Random random = new Random();

                // 填充 totalImagePaths，直到达到所需的图片数量
                while (totalImagePaths.Count < totalNumberOfPictures && remainingImages.Count > 0)
                {
                    int randomIndex = random.Next(remainingImages.Count);
                    totalImagePaths.Add(remainingImages[randomIndex]);
                    remainingImages.RemoveAt(randomIndex);

                    Console.WriteLine($"Added image to totalImagePaths, remaining images: {remainingImages.Count}");
                }

                // 如果剩余的图片不足以填充到总数，发出警告
                if (totalImagePaths.Count < totalNumberOfPictures)
                {
                    //MessageBox.Show("总图片数量不足，请调整 totalPictureMultiplier 参数。");
                }
                else
                {
                    Console.WriteLine($"Successfully filled totalImagePaths with {totalImagePaths.Count} images.");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in GenerateTotalImagePaths: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DisplayImagePaths()
        {
            StackPanel stackPanel = new StackPanel();

            foreach (var imagePath in totalImagePaths)
            {
                Image imageControl = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Margin = new Thickness(5)
                };

                stackPanel.Children.Add(imageControl);
            }

            this.Content = stackPanel;
        }

    }
    public partial class 图形记忆力 : BaseUserControl
    {
        private bool is_pause = false;

        protected override async Task OnInitAsync()
        {
            train_mode = 1;
            hardness = 1;
                 sharedTimer = new DispatcherTimer();
                 sharedTimer.Interval = TimeSpan.FromSeconds(1);
                 sharedTimer.Tick += OnTick;
            if (train_mode == 1 || train_mode == 2)
            {
                // 显示图片，隐藏文本
                SetImagesVisible();
            }
            else if (train_mode == 3)
            {
                // 显示文本，隐藏图片
                SetTextsVisible();
            }

            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars 已加载数据：");

                // 遍历 ProgramModulePars 打印出每个参数
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    /*Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");*/
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // 完成赋值
                        {

                            case 156: //游戏等级
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                            case 21: // 治疗时间 
                                max_time = par.Value.HasValue ? (int)par.Value.Value : 30;
                                Debug.WriteLine($"训练时间={max_time}");
                                break;
                            case 22: // 要素词汇
                                total_picture_number_para = par.Value.HasValue ? (int)par.Value.Value : 7;
                                Debug.WriteLine($"要素词汇={total_picture_number_para}");
                                break;
                            case 26: // 等级提高
                                LEVEL_UP_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 85;
                                Debug.WriteLine($"DECREASE ={LEVEL_UP_THRESHOLD}");
                                break;
                            case 27: // 等级降低
                                LEVEL_DOWN_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 50;
                                Debug.WriteLine($"DECREASE ={LEVEL_DOWN_THRESHOLD}");
                                break;
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

            max_time = max_time * 60;
            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {

            if (!isGameRunning)
            {
                if (!is_pause)
                {
                    // 随机选择指定数量的不重复图片
                    hard_set();
                    List<string> allImages = imagePaths.SelectMany(x => x).ToList();
                    Random random = new Random();
                    selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
                    // 根据 training_mode 设置图片或文本的可见性

                    if (train_mode == 1 || train_mode == 2)
                    {
                        // 显示图片，隐藏文本
                        SetImagesVisible();
                    }
                    else if (train_mode == 3)
                    {
                        // 显示文本，隐藏图片
                        SetTextsVisible();
                    }
                }
                sharedTimer.Start();
                is_pause = false; sharedTimer.Start();
            }
            else
            {
                gameTimer.Start();
                sharedTimer.Start();
            }

            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            sharedTimer.Stop();
            if (gameTimer != null)
            {
                gameTimer.Stop();
            }
        }

        protected override async Task OnPauseAsync()
        {
            if (isGameRunning)
            {
                gameTimer.Stop();
            }
            sharedTimer.Stop();
            is_pause = true;
        }

        protected override async Task OnNextAsync()
        {
            // 调整难度

            // 随机选择指定数量的不重复图片
            if (!isGameRunning)
            {
                List<string> allImages = imagePaths.SelectMany(x => x).ToList();
                Random random = new Random();
                selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();

                // 根据 training_mode 设置图片或文本的可见性
                if (train_mode == 1 || train_mode == 2)
                {
                    // 显示图片，隐藏文本
                    SetImagesVisible();
                }
                else if (train_mode == 3)
                {
                    // 显示文本，隐藏图片
                    SetTextsVisible();
                }

                // 调用委托
                VoiceTipAction?.Invoke("测试返回语音指令信息");
                SynopsisAction?.Invoke("测试题目说明信息");
            }
            else
            {
                EndGame();
            }
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 图形记忆力讲解();
        }

        private int GetCorrectNum()
        {
            return correctAnswers.Sum();
        }
        private int GetWrongNum()
        {
            return wrongAnswers.Sum();
        }
        private int GetIgnoreNum()
        {
            return ignoreAnswers.Sum();
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
                        // 获取当前难度级别的数据
                        int correctCount = GetCorrectNum();
                        int wrongCount = GetWrongNum();
                        int ignoreCount = GetIgnoreNum();
                        // 计算准确率
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "图形记忆力",
                            Eval = false,
                            Lv = hardness, // 当前的难度级别
                            ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际值
                        };

                        db.Results.Add(newResult);
                        await db.SaveChangesAsync();

                        // 获得 result_id
                        int result_id = newResult.ResultId;

                        // 创建 ResultDetail 对象列表
                        var resultDetails = new List<ResultDetail>
                    {
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "正确次数",
                            Value = correctCount,
                            ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "错误次数",
                            Value = wrongCount,
                            ModuleId = BaseParameter.ModuleId
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "遗漏次数",
                            Value = ignoreCount,
                            ModuleId = BaseParameter.ModuleId
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "正确率",
                            Value = accuracy * 100, // 以百分比形式存储
                            ModuleId = BaseParameter.ModuleId
                        }
                    };

                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别 {hardness}:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                        }
                        // 提交事务
                        //await transaction.CommitAsync();
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

