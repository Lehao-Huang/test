using crs.core;
using crs.core.DbModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 警觉能力.xaml 的交互逻辑
    /// </summary>
    public partial class 警觉能力 : BaseUserControl
    {
        private const bool is_beep = true;
        private const int GAMETIME = 20;
        private const int wait_delay = 3000; // 等待用户3000ms作出反应
        private const int min_delay = 1000;   // 生成图片等待时间最小值
        private const int max_delay = 3001;   // 生成图片等待时间最大值
        private BitmapImage targetImage;
        private int attemptCount = 0;
        private Stopwatch stopwatch;
        private double total_time = 0;
        private int forget = 0; // 记录忘记点击的次数
        private CancellationTokenSource cts; // 用于取消超时任务
        string[] imagePaths; // 存储文件夹中照片

        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒// 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            while (true)
            {
                // 检查当前目录是否存在“crs.game”文件夹
                string targetParentDirectory = Path.Combine(currentDirectory, "crs.game");
                if (Directory.Exists(targetParentDirectory))
                {
                    string targetDirectory = Path.Combine(targetParentDirectory, "警觉能力");
                    if (Directory.Exists(targetDirectory))
                    {
                        return targetDirectory; // 找到文件夹，返回路径
                    }
                }

                // 向上移动到父目录
                DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);
                if (parentDirectory == null)
                {
                    break; // 到达根目录，停止查找
                }
                currentDirectory = parentDirectory.FullName; // 更新当前目录
            }

            return null; // 未找到文件夹
        }
        public 警觉能力()
        {
            InitializeComponent();

        }

        private void LoadTargetImage()
        {
            Random rand = new Random();
            int index = rand.Next(imagePaths.Length);
            targetImage = new BitmapImage(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), imagePaths[index])));
            TImage.Source = targetImage;
        }

        private async void LoadNewImage()
        {
            if (attemptCount >= GAMETIME)
            {
                //MessageBox.Show($"测试结束！\n" + $"总反应时间: {total_time}秒" + $"总遗漏次数: {forget}次", "测试结果");
                //警觉能力报告 nwd=new 警觉能力报告(total_time/GAMETIME,is_beep,forget);
                //nwd.Show();
                StopAllTasks();
                OnGameEnd();
                return;
            }

            // 取消之前的任务
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }
            //cts?.Cancel();
            cts = new CancellationTokenSource(); // 创建新的 CancellationTokenSource

            // 清空当前图片
            TargetImage.Source = null;

            var token = cts.Token;

            Random rand = new Random();
            int delay = rand.Next(min_delay, max_delay);
            try
            {
                await Task.Delay(delay, token); // 使用取消令牌
            }
            catch (TaskCanceledException)
            {

            }
            //await Task.Delay(delay, token); // 使用取消令牌

            // 随机设置 TargetImage 的位置
            TargetImage.Source = targetImage;
            double maxX = ImageCanvas.ActualWidth - TargetImage.Width;
            double maxY = ImageCanvas.ActualHeight - TargetImage.Height;

            double randomX = rand.NextDouble() * maxX;
            double randomY = rand.NextDouble() * maxY;

            Canvas.SetLeft(TargetImage, randomX);
            Canvas.SetTop(TargetImage, randomY);

            attemptCount++;
            RightStatisticsAction?.Invoke(attemptCount - forget, 10);
            WrongStatisticsAction?.Invoke((forget), 10);
            stopwatch = Stopwatch.StartNew();
            Console.Beep(800, 200); // 发出800Hz频率的声音，持续0.2秒
            try
            {
                await Task.Delay(wait_delay, token); // 等待DELAY时间
            }
            catch (TaskCanceledException)
            {
                return;
            }

            attemptCount++;
            RightStatisticsAction?.Invoke(attemptCount - forget, 10);
            forget++;
            total_time += wait_delay / 1000;
            TargetImage.Source = null; // 清空当前图片
            LoadNewImage(); // 重新加载新图片
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (TargetImage.Source == null)
            {
                MessageBox.Show("错误触碰按钮！");
                return;
            }

            stopwatch.Stop();
            total_time += stopwatch.Elapsed.TotalSeconds;
            LoadNewImage(); // 重新加载新图片
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EvaluateTest evaluateTestWindow = Application.Current.Windows.OfType<EvaluateTest>().FirstOrDefault();
            if (evaluateTestWindow != null)
            {
                evaluateTestWindow.SetContinueAssessment(false); // 停止后续窗口的展示
            }
            StopAllTasks(); // 在关闭窗口时停止所有任务
            OnGameEnd();
        }

        private void StopAllTasks()
        {
            // 取消当前的 CancellationTokenSource
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }
            //cts?.Cancel();
            cts?.Dispose(); // 释放资源
        }
    }
    public partial class 警觉能力 : BaseUserControl
    {

        protected override async Task OnInitAsync()
        {
            string imagePath = FindImagePath();

            if (imagePath != null)
            {
                // 如果找到了文件夹，获取该文件夹中的所有文件
                imagePaths = Directory.GetFiles(imagePath);
                LoadTargetImage();
            }
            else
            {
                MessageBox.Show("未找到名为“警觉能力”的文件夹。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);

            totalGameTime = TimeSpan.Zero; // 重置总时间
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            gameTimer.Tick += GameTimer_Tick; // 绑定计时器事件
        }

        protected override async Task OnStartAsync()
        {
            gameTimer.Start(); // 启动计时器
            LoadNewImage();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {

            StopAllTasks();
            stopwatch?.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTasks();
            stopwatch?.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {

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
            return new 警觉能力讲解();
        }

        private int GetCorrectNum()
        {
            return attemptCount - forget;
        }
        private int GetWrongNum()
        {
            return 0;
        }
        private int GetIgnoreNum()
        {
            return forget;
        }
        private double CalculateAccuracy(int correctCount, int wrongCount, int ignoreCount)
        {
            //int totalAnswers = correctCount + wrongCount + ignoreCount;
            int totalAnswers = attemptCount;//?
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

                    // 获取当前难度级别的数据
                    int correctCount = GetCorrectNum();
                    int wrongCount = GetWrongNum();
                    int ignoreCount = GetIgnoreNum();
                    double totalMilliseconds = totalGameTime.TotalMilliseconds;  // 转换为double类型的毫秒数
                    double time = (double)totalMilliseconds / attemptCount;
                    // 计算准确率
                    double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                    // 创建 Result 记录
                    var newResult = new Result
                    {
                        ProgramId = BaseParameter.ProgramId, // program_id
                        Report = "警觉能力评估报告",
                        Eval = true,
                        Lv = null, // 当前的难度级别
                        ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际
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
                                    ModuleId = BaseParameter.ModuleId
                                },
                                /*new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误次数",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },*/
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
                                    ValueName = "平均值反应时间",
                                    Value = time, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };
                    // 插入 ResultDetail 数据
                    db.ResultDetails.AddRange(resultDetails);
                    await db.SaveChangesAsync();
                    // 输出每个 ResultDetail 对象的数据
                    /*Debug.WriteLine($"难度级别 {lv}:");*/
                    foreach (var detail in resultDetails)
                    {
                        Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                    }

                    // 提交事务
                    await transaction.CommitAsync();
                    Debug.WriteLine("插入成功");
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
