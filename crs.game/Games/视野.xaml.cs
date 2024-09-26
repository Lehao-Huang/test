using crs.core;
using crs.core.DbModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 视野.xaml 的交互逻辑
    /// </summary>
    public partial class 视野 : BaseUserControl
    {
        private Random random = new Random();
        private const int DELAY = 5000; // 两相邻刺激间延时等待
        private const double prob = 0.4; // 生成的dot符合要求的概率
        private const int GAMETIME = 20;
        private Stopwatch stopwatch;
        private double total_time = 0;
        private int attemptCount = 0;
        private int wrong = 0;
        private List<Point> reactionPoints = new List<Point>();
        private List<DateTime> reactionTimes = new List<DateTime>();
        private int forget = 0; // 记录忘记点击的次数
        private CancellationTokenSource cts; // 用于取消超时任务
        private bool is_target;
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒// 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        public 视野()
        {
            InitializeComponent();
        }

        private void CenterFixedDot()
        {
            int radius = 30;
            double centerX = trainingCanvas.ActualWidth / 2;
            double centerY = trainingCanvas.ActualHeight / 2;
            Ellipse fixedDot = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(Colors.Black), // 黑色圆点
                Stroke = Brushes.Transparent
            };
            Canvas.SetZIndex(fixedDot, 1);
            Canvas.SetLeft(fixedDot, trainingCanvas.ActualWidth / 2 - radius);
            Canvas.SetTop(fixedDot, trainingCanvas.ActualHeight / 2 - radius);

            trainingCanvas.Children.Add(fixedDot);
        }

        private async Task Show_Target()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            trainingCanvas.Children.Clear(); // 清除画布上内容
            CenterFixedDot();
            if (attemptCount >= GAMETIME)
            {
                //MessageBox.Show($"测试结束！", "测试结果");
                //视野报告 nwd = new 视野报告(total_time/(attemptCount-wrong-forget),attemptCount-wrong-forget,wrong,forget);
                //nwd.Show();
                OnGameEnd();
                //this.Dispose();
                return;
            }
            // 随机决定生成线条还是圆点
            var token = cts.Token;
            int decision = random.Next(3);
            if (decision == 0)
            {
                AddRandomLine();
                is_target = false;
            }
            else if (decision == 1)
            {
                AddRandomDot();
            }
            else
            {
                AddRandomDot_line();
            }
            attemptCount++;
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

            if (is_target)
            {
                forget++;
                total_time += DELAY;
            }
            RightStatisticsAction?.Invoke(attemptCount - forget - wrong, 10);
            WrongStatisticsAction?.Invoke(forget + wrong, 10);
            if (attemptCount <= 20)
                await Show_Target(); // 递归调用加载新图片
        }

        private void AddRandomLine()
        {
            double centerX = trainingCanvas.ActualWidth / 2;
            double centerY = trainingCanvas.ActualHeight / 2;
            Point startPoint = new Point(centerX, centerY);
            Point endPoint = new Point(random.Next(0, (int)trainingCanvas.ActualWidth), random.Next(0, (int)trainingCanvas.ActualHeight));

            // 创建线条并设置属性
            Line line = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 25
            };

            // 将线条添加到Canvas
            trainingCanvas.Children.Add(line);
            is_target = false;
        }

        private void AddRandomDot()
        {
            int radius = 20;
            double centerX = trainingCanvas.ActualWidth / 2;
            double centerY = trainingCanvas.ActualHeight / 2;
            Point blackDotPosition = new Point(centerX, centerY);

            if (random.NextDouble() < prob) // 以prob的概率生成与中心圆同心的圆点
            {
                // 创建白色圆点并设置属性
                Ellipse whiteDot = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Fill = new SolidColorBrush(Colors.White), // 白色圆点
                    Stroke = Brushes.Transparent
                };
                Canvas.SetZIndex(whiteDot, 1);
                // 将白色圆点放置到黑色圆点的上方
                Canvas.SetLeft(whiteDot, blackDotPosition.X - radius); // 确保白色圆点中心在blackDotPosition位置
                Canvas.SetTop(whiteDot, blackDotPosition.Y - radius); // 确保白色圆点中心在blackDotPosition位置

                trainingCanvas.Children.Add(whiteDot);
                is_target = true; // 设置为目标
            }
            else
            {
                Point point = new Point(random.Next(0, (int)trainingCanvas.ActualWidth), random.Next(0, (int)trainingCanvas.ActualHeight));
                is_target = false; // 设置为非目标

                Ellipse dot = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Fill = new SolidColorBrush(Colors.White),
                    Stroke = Brushes.Transparent
                };

                // 将圆点放置到Canvas上
                Canvas.SetLeft(dot, point.X - radius); // 确保圆点中心在point位置
                Canvas.SetTop(dot, point.Y - radius); // 确保圆点中心在point位置

                // 将圆点添加到Canvas
                trainingCanvas.Children.Add(dot);
            }
        }

        private void AddRandomDot_line()
        {
            double centerX = trainingCanvas.ActualWidth / 2;
            double centerY = trainingCanvas.ActualHeight / 2;
            double radius = 25; // 圆点半径
            Point startPoint = new Point(centerX, centerY);
            Point endPoint = new Point(random.Next(0, (int)trainingCanvas.ActualWidth), random.Next(0, (int)trainingCanvas.ActualHeight));

            // 创建线条并设置属性
            Line line = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 25
            };

            // 将线条添加到Canvas
            trainingCanvas.Children.Add(line);

            // 创建圆点并设置属性
            Ellipse dot = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(Colors.White),
                Stroke = Brushes.Transparent
            };

            // 将圆点放置到Canvas上
            Canvas.SetLeft(dot, endPoint.X - radius);
            Canvas.SetTop(dot, endPoint.Y - radius);

            // 将圆点添加到Canvas
            trainingCanvas.Children.Add(dot);
            is_target = true;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // 记录反应位置
            reactionPoints.Add(new Point(/* 受试者点击的位置 */));
            stopwatch.Stop();
            // MessageBox.Show(is_target.ToString());
            if (is_target)
            {
                total_time += stopwatch.Elapsed.TotalSeconds;
            }
            else
            {
                wrong++;
            }
            Show_Target();
            RightStatisticsAction?.Invoke(attemptCount - forget - wrong, 10);
            WrongStatisticsAction?.Invoke(forget + wrong, 10);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Dispose();
        }

        public void Dispose()
        {
            stopwatch.Stop();
            cts?.Cancel();
            cts?.Dispose();
            OnGameEnd();

        }
    }
    public partial class 视野 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
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
            totalGameTime = TimeSpan.Zero; // 重置总时间
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            gameTimer.Tick += GameTimer_Tick; // 绑定计时器事件
            gameTimer.Start(); // 启动计时器
            Show_Target();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            Dispose();
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            Dispose();
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
            return new 视野讲解();
        }

        private int GetCorrectNum()
        {
            return attemptCount - wrong - forget;
        }
        private int GetWrongNum()
        {
            return wrong;
        }
        private int GetIgnoreNum()
        {
            return forget;
        }
        private double CalculateAccuracy(int correctCount, int wrongCount, int ignoreCount)
        {
            //int totalAnswers = correctCount + wrongCount + ignoreCount;
            int totalAnswers = attemptCount;
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
                        double time = Math.Round((double)totalMilliseconds / attemptCount,0);
                        // 计算准确率
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "视野能力评估报告",
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
                                    Value = time, 
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
