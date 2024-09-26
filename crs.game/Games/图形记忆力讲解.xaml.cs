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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 图形记忆力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 图形记忆力讲解 : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
         {

new string[]
{
    "BILD1/2/电话机.jpg",
    "BILD1/2/菠萝.jpg",
    "BILD1/2/电视机.jpg",
    "BILD1/2/番茄.jpg",
    "BILD1/2/吹风机.jpg",
    "BILD1/2/锤子.jpg",
    "BILD1/2/大象.jpg",
    "BILD1/2/电脑.jpg"
},
new string[]
{
    "BILD1/3/蝴蝶.jpg",
    "BILD1/3/橘子.jpg",
    "BILD1/3/狗.jpg",
    "BILD1/3/螺丝钉.jpg",
    "BILD1/3/苹果.jpg",
    "BILD1/3/梨子.jpg",
    "BILD1/3/闹钟.jpg",
    "BILD1/3/剪刀.jpg",
    "BILD1/3/猕猴桃.jpg",
    "BILD1/3/猫.jpg"
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
        private int hardness = 1;
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

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 图形记忆力讲解()
        {
            InitializeComponent();

            hard_set();
            sharedTimer = new DispatcherTimer();
            sharedTimer.Interval = TimeSpan.FromSeconds(1);
            sharedTimer.Tick += OnTick;
            // 随机选择指定数量的不重复图片

            List<string> allImages = imagePaths.SelectMany(x => x).ToList();

            Random random = new Random();
            selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
            // 将 selectedImagePaths 内容拼接成一个字符串
            string selectedImagesMessage = string.Join("\n", selectedImagePaths);

            // 使用 MessageBox 显示 selectedImagePaths 的内容
            //MessageBox.Show(selectedImagesMessage, "Selected Image Paths");

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
            this.Loaded += 图形记忆力讲解_Loaded;

        
        
        }

        private void 图形记忆力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
this.Focus();  
        }


        private void OnTick(object sender, EventArgs e)
        {
            if (max_time > 0)
            {
                //TimeStatisticsAction.Invoke(10, 10);
                max_time--;
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


            }
            else
            {
                sharedTimer.Stop();

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
            try
            {
                // 检查 imageContainer 是否已正确初始化
                if (imageContainer == null)
                {
                    //MessageBox.Show("imageContainer is null. Please ensure it is initialized properly.");
                    return;
                }

                // 清除之前的图片
                imageContainer.Children.Clear();

                // 检查 selectedImagePaths 是否包含图像路径
                if (selectedImagePaths == null || selectedImagePaths.Count == 0)
                {
                    //MessageBox.Show("selectedImagePaths is null or empty. Please ensure it is initialized properly.");
                    return;
                }

                // 动态添加图片到 UniformGrid
                foreach (var imagePath in selectedImagePaths)
                {
                    try
                    {
                        // 检查图片路径是否有效
                        if (string.IsNullOrWhiteSpace(imagePath))
                        {
                            //MessageBox.Show($"Image path is null or empty: {imagePath}");
                            continue;
                        }

                        // 尝试加载图片
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(imagePath, UriKind.Relative);
                        bitmapImage.EndInit();

                        System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image()
                        {
                            Source = bitmapImage,
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(5)
                        };

                        imageContainer.Children.Add(imageControl);

                        // 如果加载成功，显示成功加载的信息
                        //MessageBox.Show($"Successfully loaded image: {imagePath}");
                    }
                    catch (Exception ex)
                    {
                        // 如果加载失败，捕获异常并显示错误信息
                        //MessageBox.Show($"Failed to load image: {imagePath}\nException: {ex.Message}");
                    }
                }

                // 检查 imageContainer.Children 中的内容
                StringBuilder childrenInfo = new StringBuilder();
                childrenInfo.AppendLine("imageContainer.Children contains the following items:");

                foreach (UIElement child in imageContainer.Children)
                {
                    if (child is System.Windows.Controls.Image image)
                    {
                        childrenInfo.AppendLine($"Image with Source: {image.Source?.ToString() ?? "No Source"}");
                    }
                    else
                    {
                        childrenInfo.AppendLine($"Unknown UIElement of type: {child.GetType().Name}");
                    }
                }

                //MessageBox.Show(childrenInfo.ToString(), "Children Information");
            }
            catch (Exception ex)
            {
                // 捕获外层异常并显示错误信息
                //MessageBox.Show($"Error in SetImagesVisible: {ex.Message}\n{ex.StackTrace}");
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
                    Text = System.IO.Path.GetFileNameWithoutExtension(imagePath),
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

            if (errorDecisions > 1)
            {
                result_text.Text = "很遗憾答错了！";
                result_text.Foreground = new SolidColorBrush(Colors.Red);

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // 设置3秒的间隔
                timer.Tick += (s, args) =>
                {
                    // 3秒后执行的操作
                    // 清除 imageContainer2 中的所有动画并移除图片
                    result_text.Text = "";
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

                    // 调整难度

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

                    // 停止计时器，防止重复触发
                    timer.Stop();
                };

                timer.Start(); // 启动计时器
            }
            else
            {
                result_text.Text = "恭喜你答对了！";
                result_text.Foreground = new SolidColorBrush(Colors.Green);
                end.Visibility = Visibility.Visible;
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
                System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image()
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Margin = new Thickness(5)
                };

                stackPanel.Children.Add(imageControl);
            }

            this.Content = stackPanel;
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
                        Grid1.Visibility = Visibility.Collapsed;
                        Grid2.Visibility = Visibility.Collapsed;
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
                        Grid1.Visibility = Visibility.Collapsed;
                        Grid2.Visibility = Visibility.Collapsed;
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
                        Grid1.Visibility = Visibility.Visible;
                       

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

