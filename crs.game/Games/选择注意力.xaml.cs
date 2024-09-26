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
    /// 选择注意力.xaml 的交互逻辑
    /// </summary>
    public partial class 选择注意力 : BaseUserControl
    {
        private string imagePath; // 存储找到的文件夹路径
        private string[] imagePaths;
        private const int DELAY = 5000; // 等待5000ms显示下一张图片
        private const int MAX_GAME = 20;
        private BitmapImage targetImage;
        private BitmapImage leftImage;
        private int attemptCount = 0;
        private Stopwatch stopwatch;
        private double total_time = 0;
        private int wrong = 0;
        private int skip = 0;
        private CancellationTokenSource cancellationTokenSource; // 用于取消加载任务

        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒// 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }
        public 选择注意力()
        {
            InitializeComponent();
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
                    string targetDirectory = Path.Combine(targetParentDirectory, "选择注意力");
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

        private void LoadTargetImage()
        {
            Random rand = new Random();
            int index = rand.Next(imagePaths.Length);
            targetImage = new BitmapImage(new Uri(imagePaths[index]));
            TargetImage.Source = targetImage;
        }

        private async void LoadNewLeftImage()
        {
            // 如果当前有加载任务正在运行，则取消它
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            attemptCount++;
            if (attemptCount >= MAX_GAME)
            {
                //MessageBox.Show($"测试结束！\n" + $"总反应时间: {total_time}秒\n" + $"总错误次数：{wrong}\n" + $"总遗漏次数：{skip}", "测试结果");
                //选择注意力报告 nwd=new 选择注意力报告(total_time/(attemptCount - wrong - skip),attemptCount-wrong-skip,wrong,skip);
                //nwd.Show();
                OnGameEnd();
                return;
            }

            Random rand = new Random();
            int index = rand.Next(imagePaths.Length);
            leftImage = new BitmapImage(new Uri(imagePaths[index]));
            LeftImage.Source = leftImage;

            // 随机设置 LeftImage 的位置
            double maxX = LeftImageCanvas.ActualWidth - LeftImage.Width;
            double maxY = LeftImageCanvas.ActualHeight - LeftImage.Height;

            double randomX = rand.NextDouble() * maxX;
            double randomY = rand.NextDouble() * maxY;

            Canvas.SetLeft(LeftImage, randomX);
            Canvas.SetTop(LeftImage, randomY);

            stopwatch = Stopwatch.StartNew();

            try
            {
                await Task.Delay(DELAY, token); // 等待DELAY时间
            }
            catch (TaskCanceledException)
            {
                // 任务被取消，直接返回
                return;
            }

            if (leftImage.UriSource == targetImage.UriSource)
            {
                skip++;
                total_time += DELAY;
            }
            RightStatisticsAction?.Invoke(attemptCount - wrong - skip, 10);
            WrongStatisticsAction?.Invoke(wrong + skip, 10);

            if (attemptCount < MAX_GAME)
                LoadNewLeftImage(); // 递归调用加载新图片
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Stop();
            if (leftImage.UriSource == targetImage.UriSource)
            {
                total_time += stopwatch.Elapsed.TotalSeconds;
            }
            else
            {
                wrong++;
            }
            RightStatisticsAction?.Invoke(attemptCount - wrong - skip, 10);
            WrongStatisticsAction?.Invoke(wrong + skip, 10);
            LoadNewLeftImage(); // 按钮点击后立即加载新图片
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 停止计时器
            stopwatch?.Stop();
            // 关闭窗口
            gameTimer?.Stop();
            OnGameEnd();

        }
    }
    public partial class 选择注意力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            imagePath = FindImagePath(); // 查找目标文件夹路径

            if (imagePath != null)
            {
                imagePaths = Directory.GetFiles(imagePath); // 获取文件夹中的所有文件
                LoadTargetImage();
            }
            else
            {
                MessageBox.Show("未找到目标文件夹，请检查路径。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);
            //
            totalGameTime = TimeSpan.Zero; // 重置总时间
            gameTimer = new DispatcherTimer();
            gameTimer.Tick += GameTimer_Tick; // 绑定计时器事件
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
        }

        protected override async Task OnStartAsync()
        {    
            gameTimer.Start(); // 启动计时器

            LoadNewLeftImage();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            stopwatch?.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
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
            return new 选择注意力讲解();
        }

        private int GetCorrectNum()
        {
            return attemptCount - wrong - skip;
        }
        private int GetWrongNum()
        {
            return wrong;
        }
        private int GetIgnoreNum()
        {
            return skip;
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
                        double totalMilliseconds = totalGameTime.TotalMilliseconds;  // 转换为double类型的毫秒数
                        double time = (double)totalMilliseconds / attemptCount;
                        // 计算准确率
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "逻辑推理能力评估报告",
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
                                    ValueName = "正确",
                                    Value = correctCount,
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
                                    ValueName = "遗漏",
                                    Value = ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率",
                                    Value = accuracy * 100, // 以百分比形式存储
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
