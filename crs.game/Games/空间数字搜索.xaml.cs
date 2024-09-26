using crs.core;
using crs.core.DbModels;
using Microsoft.EntityFrameworkCore;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 空间数字搜索.xaml 的交互逻辑
    /// </summary>
    public partial class 空间数字搜索 : BaseUserControl
    {
        private List<int> numbers;
        private int lastClickedNumber;
        private Stopwatch stopwatch;
        private DateTime startTime; // 记录表盘显示的开始时间
        private double[] timeIntervals; // 存储时间间隔
        private int a ;
        private int wrongAccount; // 记录错误计数
        private int maxConsecutiveNumber; // 记录最长连续数字串的最大值
        private Brush defaultButtonBackground; // 存储按钮的默认背景颜色
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        public 空间数字搜索()
        {
            InitializeComponent();
        }

        private void InitializeNumberGrid()
        {
            numbers = Enumerable.Range(1, 24).ToList();
            Random rand = new Random();
            numbers = numbers.OrderBy(x => rand.Next()).ToList(); // 打乱顺序

            foreach (var number in numbers)
            {
                Button button = new Button
                {
                    Content = number,
                    FontSize = 32,
                    Margin = new Thickness(10),
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
                TimeSpan timeSinceStart = DateTime.Now - startTime;
                timeIntervals[0] = timeSinceStart.TotalSeconds;
                a = 0;
                maxConsecutiveNumber++;
                clickedButton.IsEnabled = false; // 禁用按钮
            }
            else
            {
                TimeSpan timeInterval = stopwatch.Elapsed;
                if (clickedNumber == maxConsecutiveNumber + 1)
                {
                    // 存储时间间隔
                    timeIntervals[maxConsecutiveNumber - 1] = timeInterval.TotalSeconds; // 存储到对应索引
                    maxConsecutiveNumber++;
                    clickedButton.IsEnabled = false; // 禁用按钮
                    if (maxConsecutiveNumber > 1)
                    {
                        a = maxConsecutiveNumber - 1; // 更新 a 为时间间隔的数量
                    }

                    // 检查是否所有数字都已被点击
                    if (maxConsecutiveNumber == 24)
                    {
                        string timeIntervalsMessage = "时间间隔:\n";

                        // 遍历 timeIntervals 数组并构建消息字符串
                        for (int i = 0; i < timeIntervals.Length; i++)
                        {
                            timeIntervalsMessage += $"第 {i + 1} 个数字: {timeIntervals[i]} 毫秒\n";
                        }

                        // 显示 MessageBox
                        //MessageBox.Show(timeIntervalsMessage, "时间间隔统计");
                        //空间数字搜索报告 nwd = new 空间数字搜索报告(wrongAccount, timeIntervals);
                        //nwd.Show();
                        stopwatch.Stop();
                        OnGameEnd();
                    }
                }
                else
                {
                    // 增加错误计数
                    wrongAccount++;
                    clickedButton.Background = Brushes.Black; // 设置按钮背景为黑色

                    // 等待0.5秒后恢复颜色
                    await Task.Delay(500); // 等待500毫秒
                    clickedButton.Background = defaultButtonBackground; // 恢复按钮背景为默认颜色
                    clickedButton.IsEnabled = true; // 重新启用按钮
                }
            }

            RightStatisticsAction?.Invoke(maxConsecutiveNumber, 24);
            WrongStatisticsAction?.Invoke(wrongAccount, 24);
            // 更新状态
            lastClickedNumber = clickedNumber;
            stopwatch.Restart();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Stop();
            OnGameEnd();
        }
    }
    public partial class 空间数字搜索 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            lastClickedNumber = 0; // 初始化为0，表示未点击
            timeIntervals = new double[24];
            wrongAccount = 0;
            maxConsecutiveNumber = 0; // 初始化最大连续数字串为0
            defaultButtonBackground = Brushes.White; // 设置默认背景颜色
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
            InitializeNumberGrid();

            stopwatch = new Stopwatch();
            startTime = DateTime.Now; // 记录表盘显示的开始时间
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            stopwatch.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            stopwatch.Stop();
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
            return new 空间数字搜索讲解();
        }

        private int GetCorrectNum()
        {
            return 0;
        }
        private int GetWrongNum()
        {
            return wrongAccount;
        }
        private int GetIgnoreNum()
        {
            return 0;
        }
        private double CalculateAccuracy(int correctCount, int wrongCount, int ignoreCount)
        {
            int totalAnswers = correctCount + wrongCount + ignoreCount;
            return totalAnswers > 0 ? Math.Round((double)correctCount / totalAnswers, 2) : 0;
        }
        /*private async Task updateDataAsync(int program_id)
        {
            using (Crs_Db2Context db = new Crs_Db2Context())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
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
                        ProgramId = program_id,
                        Report = "空间数字搜索能力评估报告",
                        Eval = true,
                        Lv = null,
                        ScheduleId = BaseParameter.ScheduleId ?? null
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
                    ValueName = "错误次数",
                    Value = wrongCount,
                    ModuleId = BaseParameter.ModuleId
                }
            };

                    // 插入 ResultDetail 数据
                    db.ResultDetails.AddRange(resultDetails);
                    await db.SaveChangesAsync();

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
        }*/
        private async Task updateDataAsync()
        {
            var baseParameter = BaseParameter;

            var program_id = baseParameter.ProgramId;
            Crs_Db2Context db = baseParameter.Db;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    int correctCount = GetCorrectNum();
                    int wrongCount = GetWrongNum();
                    int ignoreCount = GetIgnoreNum();
                    double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                    db.Database.SetCommandTimeout(TimeSpan.FromMinutes(8)); // 根据需要调整
                    double sum = 0;
                    for (int ii = 0; ii < a; ii++)
                    {
                        sum += timeIntervals[ii]; // 累加有效的时间间隔
                    }
                    var newResult = new Result
                    {
                        ProgramId = program_id,
                        Report = "空间数字搜索能力评估报告",
                        Eval = true,
                        Lv = null,
                        ScheduleId = BaseParameter.ScheduleId ?? null
                    };
                    db.Results.Add(newResult);
                    db.SaveChanges();

                    int result_id = newResult.ResultId;

                    var resultDetails = new List<ResultDetail>
                    {
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
                            ValueName = "相邻两数字之间的点击间隔",
                            Value = a > 0 ? sum / a : 0,
                            ModuleId = BaseParameter.ModuleId
                        },
                    };

                    db.ResultDetails.AddRange(resultDetails);
                    db.SaveChanges();

                    foreach (var detail in resultDetails)
                    {
                        Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                    }

                    transaction.Commit();
                    Debug.WriteLine("插入成功");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

    }

}

