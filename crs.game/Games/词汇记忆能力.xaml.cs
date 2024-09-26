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
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 词语记忆能力.xaml 的交互逻辑
    /// </summary>
    public partial class 词汇记忆能力 : BaseUserControl
    {
        public Action StopAction { get; set; }
        public Action<object> ProgressAction { get; set; }
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间

        private List<string> wordsToMemorize = new List<string> { "苹果", "香蕉", "橙子", "西瓜", "草莓", "汽车", "自行车", "摩托车", "飞机", "火车", "铅笔", "钢笔", "橡皮", "尺子", "笔记本" };
        private List<string> testWords = new List<string>
        { 
            // 水果类
            "梨", "桃子", "葡萄", "菠萝", "柚子",
            "樱桃", "椰子", "蓝莓", "黑莓", "柠檬",
            "酸橙", "番茄", "红枣", "荔枝", "芒果",
            "甜瓜", "山竹", "无花果", "橙子", "榴莲",
    
            // 交通工具类
            "轮船", "电动车", "直升机", "小型飞机", "脚踏车",
            "滑板车", "公交车", "货车", "轿车", "旅游车",
            "三轮车", "赛摩托", "轨道交通", "轻轨", "高速列车",
            "观光巴士", "机动船", "皮划艇", "赛艇", "自驾车",
    
            // 文具类
            "书", "记号笔", "书写板", "纸张", "文件夹",
            "订书机", "便签纸", "笔筒", "涂改液", "画笔",
            "水彩笔", "画纸", "笔记本电脑", "画板", "计算器",
            "书签", "速写本", "夹子", "书法笔", "画册"
        };
        private HashSet<string> memorizedWords = new HashSet<string>();
        List<string> result = new List<string>();
        private int score = 0;
        private int totalTests = 72; // 总测试次数
        private int currentTestCount = 0; // 当前测试次数
        private int incorrectCount = 0; // 错误次数
        private int skippedCount = 0; // 跳过次数
        private DispatcherTimer timer;

        public 词汇记忆能力()
        {
            InitializeComponent();
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒
            // 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        private void StartMemorizationPhase()
        {
            Random random = new Random();

            // 随机选择一个单词进行记忆
            var selectedWords = wordsToMemorize.OrderBy(x => random.Next()).Take(5).ToList();
            string wordsToDisplay = string.Join(", ", selectedWords);
            WordTextBlock.Text = wordsToDisplay;
            WordTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            RememberText.Visibility = Visibility.Visible;
            foreach (var word in selectedWords)
            {
                memorizedWords.Add(word); // 将每个词存入集合
            }
            OKButton.IsEnabled = false;
            SkipButton.IsEnabled = false;

            // 每个词5次
            foreach (var word in selectedWords)
            {
                for (int i = 0; i < 5; i++)
                {
                    result.Add(word);
                }
            }

            // 从testWords中随机抽取词，直到达到72个词
            while (result.Count < 72)
            {
                var randomWord = testWords[random.Next(testWords.Count)];
                result.Add(randomWord);
            }

            // 打乱结果
            result = result.OrderBy(x => random.Next()).ToList();
            // 设置定时器，5秒后自动进入测试阶段
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                WordTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                RememberText.Visibility = Visibility.Collapsed;
                EnterTestingPhase();
            };
            timer.Start();
        }

        private void EnterTestingPhase()
        {
            if (currentTestCount >= totalTests)
            {
                ShowResults();
                return;
            }

            Random random = new Random();
            OKButton.IsEnabled = true;
            SkipButton.IsEnabled = true;
            WordTextBlock.Text = result[currentTestCount]; // 使用 currentTestCount 作为索引
            currentTestCount++;
            RightStatisticsAction?.Invoke(currentTestCount - incorrectCount - skippedCount, 10);
            WrongStatisticsAction?.Invoke((incorrectCount + skippedCount), 10);
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string currentWord = WordTextBlock.Text;
            if (memorizedWords.Contains(currentWord))
            {
                score++;
            }
            else
            {
                incorrectCount++; // 记录错误次数
            }

            RightStatisticsAction?.Invoke(currentTestCount - incorrectCount - skippedCount, 10);
            WrongStatisticsAction?.Invoke((incorrectCount + skippedCount), 10);
            EnterTestingPhase();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            string currentWord = WordTextBlock.Text;
            if (memorizedWords.Contains(currentWord))
            {
                skippedCount++;
            }
            RightStatisticsAction?.Invoke(currentTestCount - incorrectCount - skippedCount, 10);
            WrongStatisticsAction?.Invoke((incorrectCount + skippedCount), 10);
            EnterTestingPhase();
        }

        private async void ShowResults()
        {
            // 触发事件
            //词汇记忆能力报告 nwd=new 词语记忆能力报告(score,totalTests,incorrectCount,skippedCount);
            //nwd.Show();
            OnGameEnd();
            Button_Click(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            timer?.Stop();
            gameTimer?.Stop();
        }
    }
    public partial class 词汇记忆能力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            currentTestCount = 0; // 当前测试次数
            incorrectCount = 0; // 错误次数
            skippedCount = 0; // 跳过次数
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);

        }

        protected override async Task OnStartAsync()
        {
            totalGameTime = TimeSpan.Zero; // 重置总时间
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            gameTimer.Tick += GameTimer_Tick; // 绑定计时器事件
            gameTimer.Start(); // 启动计时器

            StartMemorizationPhase();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            timer?.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            timer?.Stop();
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
            return new 词汇记忆能力讲解();
        }

        private int GetCorrectNum()
        {
            return currentTestCount - GetWrongNum() - GetIgnoreNum();
        }
        private int GetWrongNum()
        {
            return incorrectCount;
        }
        private int GetIgnoreNum()
        {
            return skippedCount;    
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
                        double totalMilliseconds = totalGameTime.TotalMilliseconds;
                        double time = Math.Round((double)totalMilliseconds / currentTestCount, 0);
                        // 计算准确率
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "词汇记忆能力评估报告",
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
                                    ValueName = "中央反映时间(ms)",
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