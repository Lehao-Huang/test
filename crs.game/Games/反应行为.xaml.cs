using crs.core;
using crs.core.DbModels;
using Microsoft.Identity.Client.NativeInterop;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace crs.game.Games
{
    public partial class 反应行为 : BaseUserControl
    {
        private readonly string[] imagePaths =
        {
            "反应行为/forbid.png",
            "反应行为/left.png",
            "反应行为/right.png",
            "反应行为/down.jpeg",
            "反应行为/wildanimal.jpg"
        };
        private Random random;
        private int MAX_HARDNESS = 16;
        private int[] counterA;
        private int[] counterB;
        private int randomIndex;
        private int[] ignore;
        private int[] wrong;
        private int[] overtime;
        private DispatcherTimer displayTimer;
        private DispatcherTimer trainingTimer; // 新增计时器用于训练时间
        private DispatcherTimer imageDisplayTimer;//图片展示间隔
        private int STIMULI_INTERVAL = 2000; // 刺激间隔
        private int TRAIN_TIME = 60; // 训练时间60秒
        private int INCREASE = 5; // 增加难度的阈值
        private int DECREASE = 5; // 降低难度的阈值
        private int hardness = 1; // 设置初始难度
        private double remainingTime; // 当前题目剩余时间（秒）
        private int traintime; // 总剩余时间（秒）
        private bool IS_BEEP = true;
        private bool IS_SCREEN = true;
        private int delayTime;
        private int delaytime;
        private Queue<bool> recentResults = new Queue<bool>(5);
        public 反应行为()
        {
            InitializeComponent();

        }
        private async void ChangeBorderColor()
        {
            if (BorderElement.Background is SolidColorBrush brush && brush.Color == Colors.Red)
                return;
            // 保存原来的颜色
            var originalColor = BorderElement.Background;

            // 设置为红色
            BorderElement.Background = new SolidColorBrush(Colors.Red);

            // 等待 0.2 秒
            await Task.Delay(200);

            // 恢复原来的颜色
            BorderElement.Background = originalColor;
        }

        private void ShowRandomImage()
        {
            // 在显示新图像之前设置图像源为null
            RandomImage.Source = null;
            imageDisplayTimer?.Stop();
            // 生成一个随机的延迟时间（500ms 到 2000ms）
            if (hardness > 0 || hardness < 7)
                delayTime = random.Next((int)(0.5 * STIMULI_INTERVAL), (int)(1.5 * STIMULI_INTERVAL));
            imageDisplayTimer.Interval = TimeSpan.FromMilliseconds(delayTime);
            if (hardness < 7)
                imageDisplayTimer.Start(); // 启动延迟计时器
            if (hardness > 6)
            {
                DisplayRandomImage(); // 显示随机图像
            }
        }
        private void OnImageDisplayTimerElapsed(object sender, EventArgs e)
        {
            DisplayRandomImage(); // 显示随机图像
        }

        private void OnDisplayTimerElapsed(object sender, EventArgs e)
        {
            // 每秒减少剩余时间
            if (hardness > 0 && hardness < 7 && randomIndex != 4)
                remainingTime++;
            else
                remainingTime--;
            //MessageBox.Show(remainingTime.ToString());
            // 如果剩余时间达到 0，停止计时器并执行结算逻辑
            if (remainingTime < 0)
            {
                displayTimer?.Stop(); // 停止计时器
                if (hardness > 0 && hardness < 7)
                    remainingTime = 0;
                else if (hardness > 6 && hardness < 11)
                    remainingTime = STIMULI_INTERVAL / 1000;
                else if (hardness > 10 && hardness < MAX_HARDNESS + 1)
                    remainingTime = delaytime / 1000;
                if (randomIndex != 4)
                {
                    recentResults.Enqueue(false); // 使用新的逻辑更新最近的结果
                    ignore[hardness]++;
                }
                else if (randomIndex == 4)
                {
                    recentResults.Enqueue(true);
                }
                AdjustDifficulty();
                ShowRandomImage();
            }
            TimeStatisticsAction?.Invoke(traintime, (int)remainingTime);
        }
        private void OnTrainingTimerElapsed(object sender, EventArgs e)
        {
            traintime--;
            // 调用委托，传递训练用时和剩余时间
            TimeStatisticsAction?.Invoke(traintime, (int)remainingTime);
            // 如果达到 TRAIN_TIME，停止所有计时器并打开报告窗口
            if (traintime < 0)
            {
                StopAllTimers();
                OnGameEnd();
            }
        }

        private void StopAllTimers()
        {
            imageDisplayTimer?.Stop();
            displayTimer?.Stop();
            trainingTimer?.Stop();
        }
        private void DisplayRandomImage()
        {
            displayTimer.Start();
            double maxY;
            double maxX;
            double left;
            double top;

            switch (hardness)
            {
                case 1:
                    randomIndex = random.Next(1); //forbid
                    counterA[hardness]++;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 2:
                    randomIndex = random.Next(1); //forbid(全屏)
                    counterA[hardness]++;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 3:
                    randomIndex = 4;//wild

                    counterB[hardness]++;
                    forbid.Visibility = Visibility.Collapsed;
                    Forbid.Visibility = Visibility.Collapsed;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 4:
                    randomIndex = random.Next(2);//forbid+left

                    counterA[hardness]++;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 5:
                    randomIndex = random.Next(2);//forbid+wild
                    if (randomIndex == 1)
                    {
                        counterB[hardness]++;
                        randomIndex = 4;
                    }
                    else
                    {

                        counterA[hardness]++;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 6:
                    randomIndex = random.Next(2);//forbid+left(全屏)
                    counterA[hardness]++;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 7:
                    randomIndex = random.Next(4);//forbid+left+wild
                    if (randomIndex == 2 || randomIndex == 3)
                    {
                        counterB[hardness]++;
                        randomIndex = 4;
                    }
                    else
                    {
                        counterA[hardness]++;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 8:
                    randomIndex = random.Next(4);//forbid+left+wild(全屏)
                    if (randomIndex == 2 || randomIndex == 3)
                    {
                        counterB[hardness]++;
                        randomIndex = 4;
                    }
                    else
                    {
                        counterA[hardness]++;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 9:
                    randomIndex = random.Next(3);//forbid+left+right
                    counterA[hardness]++;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 10:
                    randomIndex = random.Next(3);//forbid+left+right(全屏)
                    counterA[hardness]++;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 11:
                    randomIndex = random.Next(6);//forbid+left+right+wild
                    if (randomIndex > 2)
                    {
                        counterB[hardness]++;
                        randomIndex = 4;
                    }
                    else
                    {
                        counterA[hardness]++;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 12:
                    randomIndex = random.Next(6);//forbid+left+right+wild(全屏)
                    if (randomIndex > 2)
                    {
                        counterB[hardness]++;
                        randomIndex = 4;
                    }
                    else
                    {
                        counterA[hardness]++;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 13:
                    randomIndex = random.Next(4);//forbid+left+right+forward
                    counterA[hardness]++;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 14:
                    randomIndex = random.Next(4);//forbid+left+right+forward(全屏)
                    counterA[hardness]++;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 15:
                    randomIndex = random.Next(8);//forbid+left+right+wild
                    if (randomIndex > 3)
                    {
                        counterB[hardness]++;
                        randomIndex = 4;
                    }
                    else
                    {
                        counterA[hardness]++;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 16:
                    randomIndex = random.Next(8);//forbid+left+right+wild(全屏)
                    if (randomIndex > 2)
                    {
                        counterB[hardness]++;
                        randomIndex = 4;
                    }
                    else
                    {
                        counterA[hardness]++;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;
                default:
                    // 默认情况，可以处理未定义的难度级别
                    break;
            }
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Down && e.Key != Key.Up && e.Key != Key.Left && e.Key != Key.Right)
                return;
            bool isCorrect = false;  // 声明并初始化 isCorrect 变量
            displayTimer?.Stop(); // 停止计时器
            if (RandomImage.Source == null)
            {
                recentResults.Enqueue(false);
                overtime[hardness]++;
                if (IS_BEEP)
                    Console.Beep(300, 200);
                if (IS_SCREEN)
                    ChangeBorderColor();
                AdjustDifficulty();
                ShowRandomImage();
                return;
            }
            else if (RandomImage.Source != null)
            {
                if (e.Key == Key.Down && randomIndex == 0)
                {
                    isCorrect = true;  // 如果按键正确，设置 isCorrect 为 true
                }
                else if (e.Key == Key.Left && randomIndex == 1)
                {
                    isCorrect = true;  // 如果按键正确，设置 isCorrect 为 true
                }
                else if (e.Key == Key.Right && randomIndex == 2)
                {
                    isCorrect = true;  // 如果按键正确，设置 isCorrect 为 true
                }
                else if (e.Key == Key.Up && randomIndex == 3)
                {
                    isCorrect = true;  // 如果按键正确，设置 isCorrect 为 true
                }
            }
            if (!isCorrect)
            {
                wrong[hardness]++;
                if (IS_BEEP)
                    Console.Beep(300, 200);
                if (IS_SCREEN)
                    ChangeBorderColor();
            }
            recentResults.Enqueue(isCorrect);
            if (hardness > 10)
            {
                if (isCorrect)
                    delaytime = (int)(delaytime * 0.95);
                else
                    delaytime = (int)(delaytime * 1.05);
            }
            if (hardness < 11)
            {
                delaytime = STIMULI_INTERVAL;
            }
            if (hardness > 0 && hardness < 7)
                remainingTime = 0;
            else if (hardness > 6 && hardness < 11)
                remainingTime = STIMULI_INTERVAL / 1000;
            else if (hardness > 10 && hardness < MAX_HARDNESS + 1)
                remainingTime = delaytime / 1000;
            // 调整难度
            AdjustDifficulty();
            ShowRandomImage();
        }


        private void AdjustDifficulty()
        {
            int totalCorrect;
            int totalWrong;
            if (recentResults.Count >= 5)
            {
                totalCorrect = recentResults.Count(result => result); // 统计 true 的个数，即正确答案数
                totalWrong = recentResults.Count(result => !result);
                recentResults.Dequeue();
                // 提高难度
                if (totalCorrect >= INCREASE && hardness < MAX_HARDNESS)
                {
                    hardness++;
                    while (recentResults.Any())
                        recentResults.Dequeue();
                }

                // 降低难度
                else if (totalWrong >= DECREASE && hardness > 1)
                {
                    hardness--;
                    while (recentResults.Any())
                        recentResults.Dequeue();

                }
            }
            totalCorrect = recentResults.Count(result => result); // 统计 true 的个数，即正确答案数
            totalWrong = recentResults.Count(result => !result);
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(totalCorrect, 5);
            WrongStatisticsAction?.Invoke(totalWrong, 5);
        }
    }

    public partial class 反应行为 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            random = new Random();
            //---------------------------------------------------------------------------------------------
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
                            case 152:
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS: {hardness}");
                                break;
                            case 11: // 治疗时间 
                                TRAIN_TIME = par.Value.HasValue ? (int)par.Value.Value * 60 : 60;
                                Debug.WriteLine($"TRAIN_TIME={TRAIN_TIME}");
                                break;
                            case 12: // 等级提高
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"INCREASE={INCREASE}");
                                break;
                            case 13: // 等级降低
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 14: // 刺激数量
                                break;
                            case 15: // 刺激间隔
                                STIMULI_INTERVAL = par.Value.HasValue ? (int)par.Value.Value : 2000;
                                Debug.WriteLine($"刺激间隔{STIMULI_INTERVAL}");
                                break;
                            case 16: // 刺激下限
                                break;
                            case 17: // 最长反应时间
                                break;
                            case 19: // 视觉反馈
                                IS_SCREEN = par.Value == 1;
                                Debug.WriteLine($"是否听觉反馈 ={IS_BEEP}");
                                break;
                            case 20: // 听觉反馈
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"是否视觉反馈 ={IS_SCREEN}");
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

            traintime = TRAIN_TIME;
            delaytime = STIMULI_INTERVAL;
            if (hardness > 0 && hardness < 7)
                remainingTime = 0;
            else
                remainingTime = delaytime / 1000;
            counterA = new int[MAX_HARDNESS + 1];
            counterB = new int[MAX_HARDNESS + 1];
            wrong = new int[MAX_HARDNESS + 1];
            overtime = new int[MAX_HARDNESS + 1];
            ignore = new int[MAX_HARDNESS + 1];
            for (int i = 0; i <= MAX_HARDNESS; i++)
            {
                counterA[i] = 0;
                counterB[i] = 0;
                wrong[i] = 0;
                overtime[i] = 0;
                ignore[i] = 0;
            }
            Left.Visibility = Visibility.Collapsed;
            left_arrow.Visibility = Visibility.Collapsed;
            Right.Visibility = Visibility.Collapsed;
            right_arrow.Visibility = Visibility.Collapsed;
            forward.Visibility = Visibility.Collapsed;
            Forward.Visibility = Visibility.Collapsed;

            imageDisplayTimer = new DispatcherTimer();
            imageDisplayTimer.Tick += OnImageDisplayTimerElapsed;
            imageDisplayTimer?.Stop();

            displayTimer = new DispatcherTimer(); // 初始化 wildAnimalTimer
            displayTimer.Interval = TimeSpan.FromSeconds(1);
            displayTimer.Tick += OnDisplayTimerElapsed; // 绑定 Tick 事件
            displayTimer?.Stop(); // 初始时停止计时器

            trainingTimer = new DispatcherTimer(); // 初始化 trainingTimer
            trainingTimer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
            trainingTimer.Tick += OnTrainingTimerElapsed; // 绑定 Tick 事件

            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {
            trainingTimer.Start(); // 启动训练计时器
            Forbid.Source = new BitmapImage(new Uri($@"../{imagePaths[0]}", UriKind.Relative));
            Left.Source = new BitmapImage(new Uri($@"../{imagePaths[1]}", UriKind.Relative));
            Right.Source = new BitmapImage(new Uri($@"../{imagePaths[2]}", UriKind.Relative));
            Forward.Source = new BitmapImage(new Uri($@"../{imagePaths[3]}", UriKind.Relative));
            ShowRandomImage();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnNextAsync()
        {
            ShowRandomImage();
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
            return new 反应行为讲解();
        }

        private int GetCorrectNum(int difficultyLevel)
        {   // Ca +CB - wrong - ignore - overtime我没找到overtime
            int correct = counterA[difficultyLevel] + counterB[difficultyLevel] - wrong[difficultyLevel] - ignore[difficultyLevel] - overtime[difficultyLevel];
            if (correct > 0)
            {
                return correct;
            }
            else
            {
                return 0;
            }
        }
        private int GetWrongNum(int difficultyLevel)
        {
            return wrong[difficultyLevel];
        }
        private int GetcounterA(int difficultyLevel)
        {
            return counterA[difficultyLevel];
        }
        private int GetcounterB(int difficultyLevel)
        {
            return counterB[difficultyLevel];
        }
        private int GetIgnoreNum(int difficultyLevel)
        {
            return ignore[difficultyLevel];
        }
        private int GetovertimeNum(int difficultyLevel)
        {
            return overtime[difficultyLevel];
        }
        private double CalculateAccuracy(int lv)
        {
            int total = counterA[lv] + counterB[lv];
            int a = GetCorrectNum(lv);
            Debug.WriteLine($"计算时，总数为{total}，正确数为{a}");
            double accuracy = (double)GetCorrectNum(lv) / total;  // 转换为 double 类型
            // ----------------------------问题在于，在C# 整数相除会去除小数部分，这里得强制加double才不会转类型
            return total > 0 ? Math.Round(accuracy, 2) : 0;
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
                        for (int lv = 1; lv <= hardness; lv++)
                        {
                            // 获取当前难度级别的数据
                            int wrongCount = GetWrongNum(lv);
                            int ignoreCount = GetIgnoreNum(lv);
                            int counterA = GetcounterA(lv);
                            int counterB = GetcounterB(lv);
                            int counterAB = counterA + counterB;
                            if (counterAB == 0)
                            {
                                // 如果所有数据都为0，跳过此难度级别
                                Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
                                continue;
                            }
                            int overtime = GetovertimeNum(lv);
                            int wrongall = wrongCount + overtime + ignoreCount;
                            int correctCount = GetCorrectNum(lv);
                            double accuracy = CalculateAccuracy(lv);

                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "反应行为",
                                Eval = false,
                                Lv = lv, // 当前的难度级别
                                ScheduleId = BaseParameter.ScheduleId ?? null// 假设的 Schedule_id，可以替换为实际值
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
                                    Value = lv ,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "刺激",
                                    Value = counterAB,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "刺激相关",
                                    Value = counterA, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "刺激非相关",
                                    Value = counterB, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率（%）",
                                    Value = accuracy * 100, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误 总数",
                                    Value = wrongall, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误 键盘",
                                    Value = wrongCount, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误 延迟",
                                    Value = overtime, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "遗漏",
                                    Value = ignoreCount, // 以百分比形式存储
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
