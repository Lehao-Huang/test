using crs.core.DbModels;
using crs.core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace crs.game.Games
{
    /// <summary>
    /// 记忆广度.xaml 的交互逻辑
    /// </summary>
    public partial class 记忆广度 : BaseUserControl
    {
        private const int GridSize = 5;
        private const int MAX_BLOCKS = 4; // 最大方块数量
        private const double DELAY = 1.0; // 两相邻方块展示时间间隔
        private const int TOTAL_ROUNDS_PER_BLOCK = 2; // 每种方块数量的总轮数
        private List<Button> buttons = new List<Button>();
        private List<int> sequence = new List<int>();
        private List<int> selectedIndices = new List<int>(); // 记录玩家选中的方块索引
        private int currentBlockCount; // 当前展示的方块数量
        private int currentRound = 1; // 当前轮次
        private Stopwatch stopwatch = new Stopwatch();
        private DispatcherTimer countdownTimer; // 倒计时定时器
        private List<DispatcherTimer> sequenceTimers = new List<DispatcherTimer>(); // 存储展示方块的定时器
        private Dictionary<int, int> errorCounts; // 记录每个方块数量的错误次数
        // private Dictionary<int, int> correctCounts; // 记录每个方块数量的正确次数
        private bool isShowingSequence; // 是否正在展示方块
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间


        public 记忆广度()
        {
            InitializeComponent();
        }

        private void InitializeGrid()
        {
            GameGrid.Children.Clear();
            buttons.Clear(); // 同时清空按钮列表
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    Button button = new Button
                    {
                        Background = Brushes.Gray,
                        Margin = new Thickness(2),
                        FontSize = 24, // 设置默认字体大小
                        Content = "", // 初始化按钮内容为空
                        Style = null
                    };
                    button.Click += Button_Click;
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    GameGrid.Children.Add(button);
                    buttons.Add(button);
                }
            }
        }
        //将所有方块设置为灰色,清除按钮内容
        private void ResetGridGray()
        {
            foreach (var item in buttons)
            {
                item.Background = Brushes.Gray;
                item.Content = "";
            }
        }

        private void StartGame()
        {
            sequence.Clear();
            ResetGridGray();
            isShowingSequence = true;
            StatusTextBlock.Text = "准备开始..."; // 设置准备开始的文本
            StartCountdown();
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒// 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        private void StartCountdown()
        {
            countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            int countdownTime = 5; // 5秒倒计时
            countdownTimer.Tick += (s, args) =>
            {
                if (countdownTime > 0)
                {
                    StatusTextBlock.Text = $"倒计时: {countdownTime}秒, 当前展示 {currentBlockCount} 个方块, 第{currentRound}轮";
                    countdownTime--;
                }
                else
                {
                    countdownTimer.Stop();
                    ShowNextRound();
                }
            };
            countdownTimer.Start();
        }

        private void ShowNextRound()
        {
            int numberToShow = currentBlockCount;
            sequence.Clear();
            Random rand = new Random();
            HashSet<int> shownIndices = new HashSet<int>();

            for (int i = 0; i < numberToShow; i++)
            {
                int index;
                do
                {
                    index = rand.Next(buttons.Count);
                } while (shownIndices.Contains(index));

                shownIndices.Add(index);
                sequence.Add(index);
            }
            stopwatch.Restart();
            ShowSequence(0);
        }

        private void ShowSequence(int index)
        {
            if (index < sequence.Count)
            {
                int buttonIndex = sequence[index];
                buttons[buttonIndex].Background = Brushes.Red;

                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(DELAY)
                };
                timer.Tick += (s, args) =>
                {
                    buttons[buttonIndex].Background = Brushes.Gray; // 隐藏方块
                    timer.Stop();
                    sequenceTimers.Remove(timer); // 移除定时器
                    ShowSequence(index + 1); // 展示下一个方块
                };
                timer.Start();
                sequenceTimers.Add(timer); // 添加到列表中
            }
            else
            {
                // 所有方块展示完毕，提示用户
                isShowingSequence = false;
                StatusTextBlock.Text = "现在请依次按下对应方块";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isShowingSequence)
            {
                return; // 如果正在展示方块，直接返回，不处理点击事件
            }

            Button clickedButton = sender as Button;
            int clickedIndex = buttons.IndexOf(clickedButton);

            // 如果按钮已经被选中，则取消选中
            if (selectedIndices.Contains(clickedIndex))
            {
                selectedIndices.Remove(clickedIndex);
                clickedButton.Background = Brushes.Gray; // 恢复为灰色
                clickedButton.Content = ""; // 清空内容
            }
            else
            {
                // 添加选中的方块
                selectedIndices.Add(clickedIndex);
                clickedButton.Background = Brushes.Red; // 变为红色
                clickedButton.Content = (selectedIndices.Count).ToString(); // 显示当前点击的方块数
                clickedButton.Foreground = Brushes.White; // 设置字体颜色为白色
                clickedButton.FontSize = 36; // 增加字体大小
            }

            // 判断选中的方块数量
            if (selectedIndices.Count == sequence.Count)
            {
                int errors = 0; // 计算错误数量
                isShowingSequence = true;
                // 清理选择的方块并提供反馈
                for (int i = 0; i < sequence.Count; i++)
                {
                    if (sequence[i] == selectedIndices[i])
                    {
                        buttons[selectedIndices[i]].Background = Brushes.Green; // 正确的选项
                        buttons[selectedIndices[i]].Content = "✔"; // 显示正确标记
                    }
                    else
                    {
                        buttons[selectedIndices[i]].Background = Brushes.Red; // 错误的选项
                        buttons[selectedIndices[i]].Content = "✖"; // 显示错误标记
                        errorCounts[currentBlockCount]++;
                        errors++;
                    }
                }
                // 显示反馈信息
                string feedbackText;
                SolidColorBrush feedbackColor;

                if (errors == 0)
                {
                    feedbackText = "正确！";
                    feedbackColor = Brushes.Green; // 正确为绿色
                }
                else
                {
                    feedbackText = "错误！";
                    feedbackColor = Brushes.Red; // 错误为红色
                }

                // 更新状态文本
                StatusTextBlock.Text = feedbackText;
                StatusTextBlock.Foreground = feedbackColor; // 设置字体颜色

                selectedIndices.Clear(); // 清空选中的索引

                // 创建计时器以恢复状态文本颜色
                DispatcherTimer resetTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3) // 3秒后恢复
                };
                resetTimer.Tick += (s, args) =>
                {
                    foreach (var button in buttons)
                    {
                        button.Background = Brushes.Gray; // 恢复为灰色
                        button.Content = ""; // 清空内容
                        button.Foreground = Brushes.Black; // 将字体颜色恢复为黑色
                    }
                    StatusTextBlock.Foreground = Brushes.Black; // 恢复字体颜色为黑色
                    resetTimer.Stop(); // 停止计时器
                    resetTimer = null; // 清空计时器引用
                    StatusTextBlock.Text = "";
                    // 继续游戏逻辑
                    currentRound++; // 增加当前轮次
                    isShowingSequence = true;
                    if (currentRound <= TOTAL_ROUNDS_PER_BLOCK)
                    {
                        StartCountdown(); // 进行下一轮
                    }
                    else
                    {
                        currentRound = 1; // 重置当前轮次
                        currentBlockCount++; // 增加方块数量
                        if (currentBlockCount <= MAX_BLOCKS)
                        {
                            StartCountdown(); // 进行下一轮的倒计时
                        }
                        else
                        {
                            // 游戏结束，显示错误统计
                            //记忆广度报告 nwd = new 记忆广度报告(errorCounts);
                            //nwd.Show();
                            StopAllTimers();
                            OnGameEnd();
                        }
                    }
                };
                resetTimer.Start(); // 启动计时器
            }
        }

        private void StopAllTimers()
        {
            // 停止倒计时定时器
            countdownTimer?.Stop();
            gameTimer?.Stop();
            // 停止所有展示方块的定时器
            foreach (var timer in sequenceTimers)
            {
                timer.Stop();
            }
            sequenceTimers.Clear(); // 清空定时器列表
        }
    }
    public partial class 记忆广度 : BaseUserControl
    {

        protected override async Task OnInitAsync()
        {
            currentBlockCount = 2; // 从2个方块开始
            InitializeGrid();
            errorCounts = new Dictionary<int, int>(); // 初始化错误次数字典

            for (int i = 2; i <= MAX_BLOCKS; i++)
            {
                errorCounts[i] = 0; // 初始化为 0
            }
            // 调用委托
            LevelStatisticsAction?.Invoke(currentBlockCount, MAX_BLOCKS);
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
            StartGame();
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

        private int GetCorrectNum()
        {
            // 18 ？？？
       
            return 2* currentBlockCount-errorCounts[currentBlockCount];
            //return correctCounts.Values.Sum();

        }
        private int GetWrongNum()
        {
            return errorCounts.Values.Sum();
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
                                    ValueName = "最小等级",
                                    Value = 2,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "记忆广度",
                                    Value = 2,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "最大等级",
                                    Value = 4,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确",
                                    Value = correctCount , // 以百分比形式存储
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
