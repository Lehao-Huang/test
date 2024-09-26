using crs.core;
using crs.core.DbModels;
using crs.game.Games;
using Microsoft.Identity.Client.NativeInterop;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// VIG.xaml 的交互逻辑
    /// </summary>
    public partial class 警惕训练2 : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
             new string[]
{
    "VIG/1/1.jpg",
    "VIG/1/2.jpg",
    "VIG/1/3.jpg",
    "VIG/1/4.jpg",
    "VIG/1/5.jpg",
    "VIG/1/6.jpg",
    "VIG/1/7.jpg",
    "VIG/1/8.jpg",
    "VIG/1/9.jpg",
    "VIG/1/11.jpg",
    "VIG/1/12.jpg",
    "VIG/1/13.jpg",
    "VIG/1/14.jpg",
    "VIG/1/15.jpg",
    "VIG/1/16.jpg",
    "VIG/1/17.jpg",
    "VIG/1/18.jpg",
    "VIG/1/19.jpg",
},
new string[]
{
    "VIG/2/1.jpg",
    "VIG/2/2.jpg",
    "VIG/2/3.jpg",
    "VIG/2/4.jpg",
    "VIG/2/5.jpg",
    "VIG/2/6.jpg",
    "VIG/2/7.jpg",
    "VIG/2/8.jpg",
    "VIG/2/9.jpg",
    "VIG/2/11.jpg",
    "VIG/2/12.jpg",
    "VIG/2/13.jpg",
    "VIG/2/14.jpg",
    "VIG/2/15.jpg",
    "VIG/2/16.jpg",
    "VIG/2/17.jpg",
    "VIG/2/18.jpg",
    "VIG/2/19.jpg",
},
new string[]
{
    "VIG/3/1.jpg",
    "VIG/3/2.jpg",
    "VIG/3/3.jpg",
    "VIG/3/4.jpg",
    "VIG/3/5.jpg",
    "VIG/3/6.jpg",
    "VIG/3/7.jpg",
    "VIG/3/8.jpg",
    "VIG/3/9.jpg",
    "VIG/3/11.jpg",
    "VIG/3/12.jpg",
    "VIG/3/13.jpg",
    "VIG/3/14.jpg",
    "VIG/3/15.jpg",
    "VIG/3/16.jpg",
    "VIG/3/17.jpg",
    "VIG/3/18.jpg",
    "VIG/3/19.jpg",
},
new string[]
{
    "VIG/4/1.jpg",
    "VIG/4/2.jpg",
    "VIG/4/3.jpg",
    "VIG/4/4.jpg",
    "VIG/4/5.jpg",
    "VIG/4/6.jpg",
    "VIG/4/7.jpg",
    "VIG/4/8.jpg",
    "VIG/4/9.jpg",
    "VIG/4/11.jpg",
    "VIG/4/12.jpg",
    "VIG/4/13.jpg",
    "VIG/4/14.jpg",
    "VIG/4/15.jpg",
    "VIG/4/16.jpg",
    "VIG/4/17.jpg",
    "VIG/4/18.jpg",
    "VIG/4/19.jpg",
},

        };

        private int max_time = 20; // 程序持续时间（分钟）
        
        //------------------------  报告所需参数---------------------------------
        private int LEVEL_UP_THRESHOLD = 85; // 提高难度的正确率阈值（百分比）
        private int LEVEL_DOWN_THRESHOLD = 70; // 降低难度的正确率阈值（百分比）
        private bool IS_VISUAL_FEEDBACK = true; // 是否有视觉回馈
        private bool IS_AUDITORY_FEEDBACK = true; // 是否有声学回馈
        private bool IS_REALISTIC = true; // 图片是否显示为真实物体（默认显示真实图片）
        private double SPEED_FACTOR = 1.0; // 传送带的速度因素（默认值）
        private bool IS_FIXED_INTERVAL = false; // 项目间隔是否固定（默认不固定）
        private int LEVEL_DURATION = 10; // 单次游戏的持续时间（分钟）
        private double reactionTime;

        private int TYPE_OF_INPUT = 0; // 选用哪种输入方式
        private int CHOOSE_IMAGE_COUNT = 10; // imageBorder内图片显示数量
        private Queue<bool> recentResults = new Queue<bool>(5); // 记录最近5次游戏结果的队列
        private bool IS_IMAGE_DETAIL = true; // 跟难度相关，决定选择图片类型是否细节
        private bool IS_IMAGE_HARD = true; // 跟难度相关，决定图片类型是难还是简单
        private double DISPLAY_TIME = 3; // 图片滑行的总展示时间
        double RATE_OF_ERRORIMAGE = 0.5; // 展示错误（即非image2,3,4）的概率
        double Correct_decision_rate = 0;
        private int totalDecisions;
        private int correctDecisions;
        private int errorDecisions;
        private int missDecisions;
        private int hardness = 1; // 难度级别（初始值）
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 9; // 最大难度等级
        private const int MaxGames = 10;
        private int[] correctAnswers = new int[MAX_HARDNESS+1];
        private int[] wrongAnswers = new int[MAX_HARDNESS+1];
        private int[] ignoreAnswers = new int[MAX_HARDNESS+1];
        private int gameIndex = 0;
        private int imageGenerationCounter = 0;
        private int imageGenerationInterval = 5; // 控制每隔多少个 Tick 生成一次图片
        private int remainingTime;
        private int GameremainingTime;
        private DispatcherTimer trainingTimer; // 程序持续时间计时器
        private DispatcherTimer gameTimer; // 单次游戏计时器
        private Random random = new Random();
        private int continueButtonPressCount = 0;// 按钮按下的次数
        private bool isGameRunning = false; // 标志游戏是否正在进行


        public 警惕训练2()
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
        private void AdjustDifficulty()
        {
            // 最近五次游戏的总正确决定数
            // 首先检查最近5次结果是否达到了5次
            if (recentResults.Count < 5)
            {
                return; // 如果结果少于5次，不更新难度
            }

            // 计算最近5次中的正确和错误次数
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);
            int correctRate = correctCount / recentResults.Count;

            // 根据正确率调整难度
            if (correctRate >= LEVEL_UP_THRESHOLD && hardness < MAX_HARDNESS)
            {
                hardness++;
                recentResults.Clear();
            }
            else if (correctRate < LEVEL_DOWN_THRESHOLD && hardness > 1)
            {
                hardness--;
                recentResults.Clear();
            }


            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            // 根据新的难度调整游戏参数
            hard_set();
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
        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            if (remainingTime > 0)
            {
                remainingTime--;

                TimeStatisticsAction.Invoke(remainingTime, GameremainingTime);
                //UpdateTimeTextBlock();
            }
            else
            {
                trainingTimer.Stop();
                gameTimer?.Stop();
                isGameRunning = false;
                //VIG_Report reportWindow = new VIG_Report(LEVEL_UP_THRESHOLD, LEVEL_DOWN_THRESHOLD, max_time, LEVEL_DURATION, true, IS_REALISTIC, correctAnswers, wrongAnswers, ignoreAnswers);
                //reportWindow.Show(); // 打开报告窗口
                OnGameEnd();
            }
        }
        private void UpdateTimeTextBlock()
        {
            int minutes = remainingTime / 60;
            int seconds = remainingTime % 60;
            TimeTextBlock.Text = $"{minutes:D2}:{seconds:D2}";
            SelectionBox.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6D6E71"));
        }


        private void UpdateGameTimeTextBlock()
        {
            int minutes = GameremainingTime / 60;
            int seconds = GameremainingTime % 60;
            GameTimeTextBlock.Text = $"{minutes:D2}:{seconds:D2}";
        }


        private void ShowRandomImagesInBorder()
        {
            // 清空之前的图片
            foreach (UIElement element in imageContainer.Children)
            {
                if (element is System.Windows.Controls.Image image)
                {
                    image.Source = null;
                }
            }
            imageContainer.Children.Clear(); // 移除所有图片元素


            int imageType = 0;
            if (IS_IMAGE_DETAIL && !IS_IMAGE_HARD) imageType = 1;
            else if (!IS_IMAGE_DETAIL && IS_IMAGE_HARD) imageType = 2;
            else if (!IS_IMAGE_DETAIL && !IS_IMAGE_HARD) imageType = 3;
            else if (IS_IMAGE_DETAIL && IS_IMAGE_HARD) imageType = 0;

            // 从硬度对应的图片集合中选择不同的图片
            int[] randomIndexes = new int[CHOOSE_IMAGE_COUNT];
            for (int i = 0; i < CHOOSE_IMAGE_COUNT; i++)
            {
                int randomIndex;
                do
                {
                    randomIndex = random.Next(0, imagePaths[imageType].Length);
                } while (Array.Exists(randomIndexes, index => index == randomIndex));
                randomIndexes[i] = randomIndex;
            }

            for (int i = 0; i < CHOOSE_IMAGE_COUNT; i++)
            {
                string imagePath = imagePaths[imageType][randomIndexes[i]];
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                System.Windows.Controls.Image image = new System.Windows.Controls.Image
                {
                    Source = bitmap,
                    Width = 100,
                    Height = 100,
                    Margin = new Thickness(5)
                };
                imageContainer.Children.Add(image);
            }
        }



        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isGameRunning)
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            GameremainingTime = LEVEL_DURATION * 60;
            isGameRunning = true;
            // 启动游戏计时器
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void EndGame()
        {
            // 计算反应时间
            correctAnswers[hardness] += correctDecisions;
            wrongAnswers[hardness] += errorDecisions;
            ignoreAnswers[hardness] += missDecisions;
            // 获取游戏开始到结束的总时间 (单位：ms)
            double totalTime = (LEVEL_DURATION * 60 * 1000); // 这里假设游戏时间 LEVEL_DURATION 为分钟，乘以60转换成秒，再乘以1000转换成毫秒
            string correctAnswersString = string.Join(", ", correctAnswers);
            string wrongAnswersString = string.Join(", ", wrongAnswers);
            string ignoreAnswersString = string.Join(", ", ignoreAnswers);

            // 创建要显示的消息
            string message = $"Correct Answers: {correctAnswersString}\n" +
                             $"Wrong Answers: {wrongAnswersString}\n" +
                             $"Ignore Answers: {ignoreAnswersString}";

            // 使用 MessageBox 显示消息
            //MessageBox.Show(message, "Answers Information");              // 将当前游戏结果添加到对应的数组中
            int totalAnswers = 0;
            for (int i = 0; i <= MAX_HARDNESS; i++)
            {
                totalAnswers += correctAnswers[i] + wrongAnswers[i] + ignoreAnswers[i];
            }
            // 确保总回答数不为0，防止除以0的异常
            if (totalAnswers > 0)
            {
                reactionTime = totalTime / totalAnswers;
            }
            else
            {
                reactionTime = 0; // 如果没有回答，反应时间设置为0
            }


            gameIndex++;



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

            // 清除 imageContainer 中的所有图片
            foreach (UIElement element in imageContainer.Children)
            {
                if (element is System.Windows.Controls.Image image)
                {
                    image.Source = null;
                }
            }
            imageContainer.Children.Clear(); // 移除所有图片元素

            // 调整难度
            AdjustDifficulty();
            // 显示新的随机图片
            ShowRandomImagesInBorder();
            totalDecisions = 0;
            correctDecisions = 0;
            errorDecisions = 0;
            missDecisions = 0;
            // 停止游戏计时器
            UpdateDecisionTextBlocks();
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer = null; // 清除计时器
            }

            isGameRunning = false;
            // 重置 SelectionBox 的描边颜色
            SelectionBox.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6D6E71"));
            gameIndex++;
        }




        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (GameremainingTime > 0)
            {
                GameremainingTime--;
                TimeStatisticsAction.Invoke(remainingTime, GameremainingTime);
                UpdateGameTimeTextBlock();
                imageGenerationCounter++;
                if (imageGenerationCounter >= imageGenerationInterval)
                {
                    ShowRandomImage1();
                    imageGenerationCounter = 0;  // 重置计数器
                }
            }
            else
            {
                EndGame();
            }
        }

        private void UpdateDecisionTextBlocks()
        {
            right_text.Text = correctDecisions.ToString();
            error_text.Text = errorDecisions.ToString();
            miss_text.Text = missDecisions.ToString();
        }


        private void ShowRandomImage1()
        {
            if (hardness > MAX_HARDNESS) hardness = MAX_HARDNESS;
            if (hardness < 1) hardness = 1;

            int imageType = 0;
            if (IS_IMAGE_DETAIL && !IS_IMAGE_HARD) imageType = 1;
            else if (!IS_IMAGE_DETAIL && IS_IMAGE_HARD) imageType = 2;
            else if (!IS_IMAGE_DETAIL && !IS_IMAGE_HARD) imageType = 3;

            // 生成随机数以确定是否显示错误图片
            double randomValue = random.NextDouble();

            string imagePath;
            if (randomValue < RATE_OF_ERRORIMAGE)
            {
                // 显示错误图片
                List<string> errorImages = imagePaths[imageType].Where(path =>
                    !imageContainer.Children.OfType<System.Windows.Controls.Image>()
                    .Any(img => IsImageSourceEqual(new BitmapImage(new Uri(path, UriKind.Relative)), img.Source))).ToList();
                totalDecisions++;
                if (errorImages.Count > 0)
                {
                    int errorImageIndex = random.Next(0, errorImages.Count);
                    imagePath = errorImages[errorImageIndex];
                }
                else
                {
                    imagePath = errorImages[0];
                    // throw new Exception("没有符合条件的错误图片可显示");
                }
            }
            else
            {
                // 显示正确图片
                totalDecisions++;
                List<string> correctImages = imagePaths[imageType].Where(path =>
                    imageContainer.Children.OfType<System.Windows.Controls.Image>()
                    .Any(img => IsImageSourceEqual(new BitmapImage(new Uri(path, UriKind.Relative)), img.Source))).ToList();

                if (correctImages.Count > 0)
                {
                    int randomIndex = random.Next(0, correctImages.Count);
                    imagePath = correctImages[randomIndex];
                }
                else
                {
                    imagePath = correctImages[0];
                    //  throw new Exception("没有符合条件的正确图片可显示");
                }
            }

            BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            System.Windows.Controls.Image newImage = new System.Windows.Controls.Image
            {
                Source = bitmap,
                Width = 250,
                Height = 250,
                Margin = new Thickness(5)
            };

            // 将新图片添加到 imageContainer2
            imageContainer2.Children.Add(newImage);

            // 动画移动图片
            AnimateImage(newImage);
        }
        private void AnimateImage(System.Windows.Controls.Image img)
        {
            img.Tag = false; // 标记图片未被手动处理
            double fromValue = 0;
            double windowWidth = 1280; // 固定窗口宽度
            double toValue = windowWidth - img.ActualWidth; // 窗口的宽度减去图片的宽度

            TranslateTransform translateTransform = new TranslateTransform();
            img.RenderTransform = translateTransform;

            double REAL_TIME = DISPLAY_TIME * SPEED_FACTOR; // 合成真正时间

            imageGenerationInterval = (int)(REAL_TIME / 3);  // 比如每2秒增加一个 interval，你可以调整这个倍数

            DoubleAnimation animation = new DoubleAnimation
            {
                From = fromValue,
                To = toValue,
                Duration = new Duration(TimeSpan.FromSeconds(REAL_TIME))
            };
            animation.Completed += (s, e) =>
            {
                if (img.Tag != null && (bool)img.Tag == true)
                {
                    // 图片已被处理，直接返回
                    return;
                }

                // 动画完成后隐藏图片

                // 从 imageContainer2 中移除图片
                imageContainer2.Children.Remove(img);
                // 检查消失的图片是否是正确的图片
                bool isCorrectImage = false;
                foreach (UIElement element in imageContainer.Children)
                {
                    if (element is System.Windows.Controls.Image correctImage)
                    {
                        if (IsImageSourceEqual(img.Source, correctImage.Source))
                        {
                            //System.Windows.MessageBox.Show("11111");
                            isCorrectImage = true;
                            break;
                        }
                    }
                }
                img.Source = null;
                if (isCorrectImage)
                {
                    // 只有当消失的图片是正确图片时，增加对应难度的 ignoreAnswers
                    SelectionBox.Stroke = new SolidColorBrush(Colors.Orange);
                    missDecisions++;
                    UpdateDecisionTextBlocks();
                }
            };

            translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            PerformAction();
        }

        private void PerformAction()
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
            if (isOverlapFound)
            {

                // 清除动画而不触发 Completed 事件
                if (overlappedImage != null)
                {
                    overlappedImage.Tag = true;
                    overlappedImage.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);

                    // 检查动画是否在进行
                    var storyboard = (Storyboard)overlappedImage.Resources["YourStoryboardKey"];
                    if (storyboard != null && storyboard.GetCurrentState() != ClockState.Stopped)
                    {
                        // 如果动画仍然在进行，先暂停动画
                        storyboard.Pause();

                        // 等待一段时间，确保动画已经完全停止
                        storyboard.Completed += (s, e) =>
                        {
                            // 确保完成后移除图片
                            imageContainer2.Children.Remove(overlappedImage);
                        };
                    }
                    else
                    {
                        // 动画已经停止，可以直接移除图片
                        imageContainer2.Children.Remove(overlappedImage);
                    }
                }



                // 检查是否与 imageContainer 中的一个图像相同
                bool isMatchFound = false;
                foreach (UIElement element in imageContainer.Children)
                {
                    if (element is System.Windows.Controls.Image image)
                    {
                        if (IsImageSourceEqual(overlappedImage.Source, image.Source))
                        {
                            isMatchFound = true;
                            break;
                        }
                    }
                }

                // 根据匹配结果更新 correctDecisions 和 SelectionBox 的描边颜色
                if (isMatchFound)
                {
                    if (recentResults.Count >= 5)
                    {
                        recentResults.Dequeue(); // 移除最早的结果
                    }
                    recentResults.Enqueue(true); // 添加当前结果
                    correctDecisions++;
                    UpdateDecisionTextBlocks();

                    int correctCount = 0;
                    foreach (bool result in recentResults)
                    {
                        if (result)
                        {
                            correctCount++;
                        }
                    }
                    RightStatisticsAction?.Invoke(correctCount, 5);
                    SelectionBox.Stroke = new SolidColorBrush(Colors.Green); // 正确
                }
                else
                {
                    if (recentResults.Count >= 5)
                    {
                        recentResults.Dequeue(); // 移除最早的结果
                    }
                    recentResults.Enqueue(false); // 添加当前结果
                    errorDecisions++;
                    UpdateDecisionTextBlocks();
                    int incorrectCount = 0;
                    foreach (bool result in recentResults)
                    {
                        if (result)
                        {
                        }
                        else
                        {
                            incorrectCount++;
                        }
                    }
                    WrongStatisticsAction?.Invoke(incorrectCount, 5);
                    SelectionBox.Stroke = new SolidColorBrush(Colors.Red); // 错误
                }
            }
            else
            {
            }
        }

        private bool IsImageSourceEqual(ImageSource source1, ImageSource source2)
        {
            if (source1 is BitmapImage bmp1 && source2 is BitmapImage bmp2)
            {
                return bmp1.UriSource == bmp2.UriSource;
            }
            return false;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OkButton_Click(OK, null);
            }
            else if (TYPE_OF_INPUT == 1 && e.Key == Key.Space)
            {
                PerformAction();
            }
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TYPE_OF_INPUT == 0)
            {
                PerformAction();
            }
        }

        private void MainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            if (TYPE_OF_INPUT == 2)
            {
                PerformAction();
            }
        }



    }
    public partial class 警惕训练2 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            int mt; int lit; int ldt; int ld; int h; bool ir; bool ifi; double sf; bool ivf; bool iaf; int toi;


            // 此处应该由客户端传入参数为准，目前先用测试数据
            {
                int max_time = 20; // 治疗时间（分钟）
                int LEVEL_UP_THRESHOLD = 85; // 提高难度的正确率阈值（百分比）
                int LEVEL_DOWN_THRESHOLD = 70; // 降低难度的正确率阈值（百分比）
                int LEVEL_DURATION = 10; // 单个任务的持续时间（分钟）
                int hardness = 1; // 难度级别（初始值）
                bool IS_REALISTIC = true; // 图片是否显示为真实物体（默认显示真实图片）
                bool IS_FIXED_INTERVAL = false; // 项目间隔是否固定（默认不固定）
                double SPEED_FACTOR = 1.0; // 传送带的速度因素（默认值）
                bool IS_VISUAL_FEEDBACK = true; // 是否有视觉回馈
                bool IS_AUDITORY_FEEDBACK = true; // 是否有声学回馈
                int TYPE_OF_INPUY = 0; //选用哪种输入方式（这里是不是input打错了）

                mt = max_time;
                lit = LEVEL_UP_THRESHOLD;
                ldt = LEVEL_DOWN_THRESHOLD;
                ld = LEVEL_DURATION;
                h = hardness;
                ir = IS_REALISTIC;
                ifi = IS_FIXED_INTERVAL;
                sf = SPEED_FACTOR;
                ivf = IS_VISUAL_FEEDBACK;
                iaf = IS_AUDITORY_FEEDBACK;
                toi = TYPE_OF_INPUY;

            }
            max_time = mt;
            LEVEL_UP_THRESHOLD = lit;
            LEVEL_DOWN_THRESHOLD = ldt;
            LEVEL_DURATION = 1;
            hardness = h;
            IS_REALISTIC = ir;
            IS_FIXED_INTERVAL = ifi;
            SPEED_FACTOR = sf;
            IS_VISUAL_FEEDBACK = ivf;
            IS_AUDITORY_FEEDBACK = iaf;
            TYPE_OF_INPUT = toi; // 选用哪种输入方式 0为鼠标，1为键盘，2为触摸屏
            remainingTime = max_time;
            // 启动程序计时器
            // 初始化剩余时间（以秒为单位）
            remainingTime = remainingTime * 60;

            GameremainingTime = LEVEL_DURATION * 60;

            {
                // 参数（包含模块参数信息）
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
                            switch (par.ModuleParId)
                            {
                                case 166: // 等级
                                    hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"HARDNESS: {hardness}");
                                    break;
                                case 113: // 治疗时间
                                    max_time = par.Value.HasValue ? (int)par.Value.Value : 20;
                                    Debug.WriteLine($"max_time={max_time}");
                                    break;
                                case 114: // 等级提高
                                    LEVEL_UP_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 85;
                                    Debug.WriteLine($"LEVEL_UP_THRESHOLD={LEVEL_UP_THRESHOLD}");
                                    break;
                                case 115: // 等级降低
                                    LEVEL_DOWN_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 70;
                                    Debug.WriteLine($"LEVEL_DOWN_THRESHOLD ={LEVEL_DOWN_THRESHOLD}");
                                    break;
                                case 116: // 水平持续
                                    LEVEL_DURATION = par.Value.HasValue ? (int)par.Value.Value : 10;
                                    Debug.WriteLine($"是否水平持续 ={LEVEL_DURATION}");
                                    break;
                                case 117: // 项目间隔是否固定
                                    IS_FIXED_INTERVAL = par.Value == 1;
                                    Debug.WriteLine($"是否真实 ={IS_FIXED_INTERVAL}");
                                    break;
                                case 118: // 速度因素
                                    SPEED_FACTOR = par.Value.HasValue ? (double)par.Value.Value : 1;
                                    Debug.WriteLine($"速度因素 ={SPEED_FACTOR}");
                                    break;
                                case 119: // 是否真实
                                    IS_REALISTIC = par.Value == 1;
                                    Debug.WriteLine($"是否真实 ={IS_REALISTIC}");
                                    break;

                                case 122: //声音
                                    IS_AUDITORY_FEEDBACK = par.Value == 1;
                                    Debug.WriteLine($"是否声音反馈 ={IS_AUDITORY_FEEDBACK}");
                                    break;
                                case 123: //视觉
                                    IS_VISUAL_FEEDBACK = par.Value == 1;
                                    Debug.WriteLine($"是否视觉反馈 ={IS_VISUAL_FEEDBACK}");
                                    break;

                                case 135: // 输入方式
                                    TYPE_OF_INPUT = par.Value.HasValue ? (int)par.Value.Value : 0;
                                    Debug.WriteLine($"TYPE_OF_INPUT={TYPE_OF_INPUT}");
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


            hard_set();

            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start();

            ShowRandomImagesInBorder();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            gameTimer?.Stop();
            trainingTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            gameTimer?.Stop();
            trainingTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {
            // 调整难度
            AdjustDifficulty();

            ShowRandomImagesInBorder();

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
            return null;
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
                        for (int lv = 0; lv < correctAnswers.Length; lv++)
                        {
                            // 获取当前难度级别的数据
                            int correctCount = GetCorrectNum(lv);
                            int wrongCount = GetWrongNum(lv);
                            int ignoreCount = GetIgnoreNum(lv);
                            int allcount = correctCount + wrongCount + ignoreCount;
                            if (correctCount == 0 && wrongCount == 0 && ignoreCount == 0)
                            {
                                // 如果所有数据都为0，跳过此难度级别
                                Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
                                continue;
                            }
                            // 计算准确率
                            double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                            int time = (int)reactionTime;
                            int correct = CHOOSE_IMAGE_COUNT;

                            int imagecount = (int)(CHOOSE_IMAGE_COUNT * RATE_OF_ERRORIMAGE + CHOOSE_IMAGE_COUNT);
                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "警惕训练2",
                                Eval = false,
                                Lv = lv, // 当前的难度级别
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
                                    ValueName = "等级",
                                    Value = lv,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "项目全部",
                                    Value = allcount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "项目相关",
                                    Value = correct,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "水平（%）",
                                    Value = accuracy * 100,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                  new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "省略",
                                    Value = ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "中间值反应时间（ms）",
                                    Value = time,
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };
                            // 插入 ResultDetail 数据
                             db.ResultDetails.AddRange(resultDetails);
                             await db.SaveChangesAsync();
                            // 输出每个 ResultDetail 对象的数据
                            Debug.WriteLine($"难度级别 {lv}:");
                            foreach (var detail in resultDetails)
                            {
                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
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
