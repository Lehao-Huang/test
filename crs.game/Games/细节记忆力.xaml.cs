using crs.core;
using crs.core.DbModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// VERB.xaml 的交互逻辑
    /// </summary>
    public partial class 细节记忆力 : BaseUserControl
    {

        private Queue<bool> recentResults = new Queue<bool>(5); // 记录最近5次游戏结果的队列


        private DispatcherTimer countdownTimer; // 用于倒计时的定时器
        private int read;// 熟悉阶段最大时间期限
        private int countdownValue; // 初始倒计时值
        private DispatcherTimer totalTimer; // 用于总时间的定时器
        private int totalTimeRemaining; // 剩余总时间
        private int remainingTime;
        private string baseDirectory;
        private int hardness = 1;
        private Question selectedQuestion;
        private int[] correctAnswersByLevel;
        private int[] wrongAnswersByLevel;
        private int totalTime = 60; // 总时间，单位为秒
        private int repeat = 1; // 重复参数
        private Random random = new Random();
        private List<Question> questionList = new List<Question>();
        private const int MAX_HARDNESS = 7;

        public 细节记忆力()
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
        private void TotalTimer_Tick(object sender, EventArgs e)
        {
            totalTimeRemaining--; // 减少剩余时间
            TimeStatisticsAction.Invoke(totalTimeRemaining, remainingTime);

            if (totalTimeRemaining <= 0)
            {
                totalTimer.Stop(); // 停止总时间的计时器
                countdownTimer?.Stop();

                // 关闭所有的 Answer_Window
                var openWindows = Application.Current.Windows.OfType<VERB_Answerwindow>().ToList();
                foreach (var answerWin in openWindows)
                {
                    answerWin.Close(); // 关闭 Answer_Window
                }
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (countdownValue > 0)
            {
                countdownValue--; // 倒计时减一
                CountdownTextBlock.Text = countdownValue.ToString(); // 更新 TextBlock 的值
            }
            if (countdownValue <= 0)
            {
                countdownTimer.Stop(); // 停止倒计时
                var openWindows = Application.Current.Windows.OfType<VERB_Answerwindow>().ToList();
                foreach (var answerWin in openWindows)
                {
                    answerWin.StopTimer(); // 停止 Answer_Window 的计时器
                    answerWin.Close(); // 关闭 Answer_Window
                }
                var answerWindow = new VERB_Answerwindow(selectedQuestion);
                answerWindow.ResultReturned += OnResultReturned;
                answerWindow.Show();
            }
        }

        private string FindBaseDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            while (true)
            {
                string targetDirectory = Path.Combine(currentDirectory, "细节记忆力");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);
                if (parentDirectory == null)
                {
                    break;
                }
                currentDirectory = parentDirectory.FullName;
            }
            return null;
        }
        private void DisplayMaterial(Question question)
        {
            MaterialTitleTextBlock.Text = question.MaterialTitle;
            MaterialContentTextBlock.Text = question.MaterialContent;
        }
        //获取题目列表
        private void LoadQuestions(int level)
        {
            string levelDirectory = Path.Combine(baseDirectory, "Level" + level);
            if (!Directory.Exists(levelDirectory))
            {

            }
            questionList.Clear();
            string[] questionFiles = Directory.GetFiles(levelDirectory);
            foreach (string questionFile in questionFiles)
            {
                string[] lines = File.ReadAllLines(questionFile);
                Question question = new Question
                {
                    MaterialTitle = lines[0],
                    MaterialContent = lines[1],
                    repeat = this.repeat
                };
                for (int i = 2; i < lines.Length; i += 6)
                {
                    QuestionGroup group = new QuestionGroup
                    {
                        Content = lines[i],
                        Options = lines.Skip(i + 1).Take(4).ToList(),
                        CorrectOptionIndex = int.Parse(lines[i + 5]) // 解析正确选项索引
                    };
                    question.QuestionGroups.Add(group); // 将 QuestionGroup 添加到 QuestionGroups 列表
                }
                questionList.Add(question);
            }
        }

        private void GetRandomQuestion()
        {
            if (questionList == null || questionList.Count == 0)
            {

                return;
            }

            // 随机选择一个问题
            int index = random.Next(questionList.Count);
            selectedQuestion = questionList[index];

            // 检查 repeat 属性
            if (selectedQuestion.repeat < 0)
            {
                // 如果 repeat < 0，则从列表中移除该问题
                questionList.RemoveAt(index);
                GetRandomQuestion();
            }
            else
            {
                DisplayMaterial(selectedQuestion);
                selectedQuestion.repeat--;
                countdownValue = read;
                CountdownTextBlock.Text = countdownValue.ToString();
                countdownTimer.Start();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            countdownTimer?.Stop();
            var answerWindow = new VERB_Answerwindow(selectedQuestion);
            answerWindow.ResultReturned += OnResultReturned;
            answerWindow.ResultUpdated += UpdateResults;
            answerWindow.TimeUpdated += UpdateAnswerTime;
            answerWindow.Show();

        }
        private void UpdateResults(int correctCount, int wrongCount)
        {
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
        }
        private void UpdateAnswerTime(int remainingTime)
        {
            this.remainingTime = remainingTime;
            TimeStatisticsAction.Invoke(totalTimeRemaining, remainingTime);
        }
        private void OnResultReturned(int correctCount, int wrongCount)
        {
            correctAnswersByLevel[hardness] += correctCount;
            wrongAnswersByLevel[hardness] += wrongCount;

            if (repeat == 0)
            {
                // 如果 repeat 为 0，只有在所有问题都回答正确时才提升难度
                if (wrongCount == 0)
                {
                    IncreaseDifficulty();
                }
                else
                {
                    DecreaseDifficulty();
                }
            }
            else
            {
                // 如果 repeat 不为 0，根据前5次结果调整难度
                UpdateRecentResults(wrongCount == 0);

                if (recentResults.Count == 5)
                {
                    int correctCountInRecent = recentResults.Count(result => result);
                    int wrongCountInRecent = recentResults.Count(result => !result);

                    if (correctCountInRecent >= repeat)
                    {
                        IncreaseDifficulty();
                    }
                    else if (wrongCountInRecent >= repeat)
                    {
                        DecreaseDifficulty();
                    }
                }
            }

            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            // 获取下一个问题
            GetRandomQuestion();
        }
        private void UpdateRecentResults(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(isCorrect); // 添加当前结果
        }

        private void IncreaseDifficulty()
        {
            if (hardness < correctAnswersByLevel.Length - 1)
            {
                hardness++;
                LoadQuestions(hardness);

            }
        }

        private void DecreaseDifficulty()
        {
            if (hardness > 1)
            {
                hardness--;
                LoadQuestions(hardness);

            }
        }

    }
    public partial class 细节记忆力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            int train_time = 60, repeat = 0, read = 20;
            totalTimeRemaining = totalTime; // 初始化剩余总时间
            InitializeComponent();
            baseDirectory = FindBaseDirectory();
            this.read = read;
            countdownValue = this.read;
            this.repeat = repeat;
            this.totalTime = train_time;
            correctAnswersByLevel = new int[20];
            wrongAnswersByLevel = new int[20];
            // 初始化 totalTimer
            totalTimer = new DispatcherTimer();
            totalTimer.Interval = TimeSpan.FromSeconds(1);
            totalTimer.Tick += TotalTimer_Tick;
            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1);
            countdownTimer.Tick += CountdownTimer_Tick;


            {
                //Debug.WriteLine(moduleId);
                // 参数（包含模块参数信息）
                var baseParameter = BaseParameter;
                if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
                {
                    Debug.WriteLine("ProgramModulePars 已加载数据：");
                    // 遍历 ProgramModulePars 打印出每个参数
                    foreach (var par in baseParameter.ProgramModulePars)
                    {
                        //Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");
                        if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                        {
                            switch (par.ModuleParId)
                            {
                                case 103: // 治疗时间
                                    train_time = par.Value.HasValue ? (int)par.Value.Value : 60;
                                    Debug.WriteLine($"train_time={train_time}");
                                    break;
                                case 104: // 重复
                                    repeat = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"repeat={repeat}");
                                    break;
                                case 105: // 最大时间期限
                                    read = par.Value.HasValue ? (int)par.Value.Value : 99;
                                    Debug.WriteLine($"DECREASE ={read}");
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

            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {
            totalTimer.Start();
            LoadQuestions(hardness);
            GetRandomQuestion();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            totalTimer.Stop(); // 停止总时间的计时器
            countdownTimer?.Stop();

            // 关闭所有的 Answer_Window
            var openWindows = Application.Current.Windows.OfType<VERB_Answerwindow>().ToList();
            foreach (var answerWin in openWindows)
            {
                answerWin.Close(); // 关闭 Answer_Window
            }
        }

        protected override async Task OnPauseAsync()
        {
            totalTimer.Stop(); // 停止总时间的计时器
            countdownTimer?.Stop();

            // 关闭所有的 Answer_Window
            var openWindows = Application.Current.Windows.OfType<VERB_Answerwindow>().ToList();
            foreach (var answerWin in openWindows)
            {
                answerWin.Close(); // 关闭 Answer_Window
            }
        }

        protected override async Task OnNextAsync()
        {
            // 调整难度
            GetRandomQuestion();

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
            return new 专注注意力讲解();
        }


        // 插入写法
        private int GetCorrectNum(int difficultyLevel)
        {
            return correctAnswersByLevel[difficultyLevel];
        }
        private int GetWrongNum(int difficultyLevel)
        {
            return wrongAnswersByLevel[difficultyLevel];
        }
        /* private int GetIgnoreNum(int difficultyLevel)
         {
             return igonreAnswer[difficultyLevel];
         }*/
        private double CalculateAccuracy(int correctCount, int wrongCount)
        {
            int totalAnswers = correctCount + wrongCount;
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
                        for (int lv = 0; lv < correctAnswersByLevel.Length; lv++)
                        {
                            // 获取当前难度级别的数据
                            int correctCount = GetCorrectNum(lv);
                            int wrongCount = GetWrongNum(lv);
                            //int ignoreCount = GetIgnoreNum(lv);
                            if (correctCount == 0 && wrongCount == 0)
                            {
                                // 如果所有数据都为0，跳过此难度级别
                                Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
                                continue;
                            }
                            // 计算准确率
                            double accuracy = CalculateAccuracy(correctCount, wrongCount);
                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = baseParameter.ProgramId, // program_id
                                Report = "细节记忆力",
                                Eval = false,
                                Lv = lv, // 当前的难度级别
                                ScheduleId = (int)baseParameter.ScheduleId// 假设的 Schedule_id，可以替换为实际值
                            };
                            db.Results.Add(newResult);
                            await db.SaveChangesAsync(); //这里注释了
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
                                    ModuleId = baseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误次数",
                                    Value = wrongCount,
                                    ModuleId =  baseParameter.ModuleId
                                },
                                /*new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "遗漏次数",
                                    Value = ignoreCount,
                                    ModuleId =  baseParameter.ModuleId
                                },*/
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率",
                                    Value = accuracy * 100, // 以百分比形式存储
                                    ModuleId =  baseParameter.ModuleId
                                }
                            };
                            // 插入 ResultDetail 数据
                            /* db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();*/
                            // 输出每个 ResultDetail 对象的数据
                            Debug.WriteLine($"难度级别 {lv}:");
                            foreach (var detail in resultDetails)
                            {
                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
                            }
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
    public class QuestionGroup
    {
        public string Content { get; set; }
        public List<string> Options { get; set; }
        public int CorrectOptionIndex { get; set; }
    }

    public class Question
    {
        public string MaterialTitle { get; set; }
        public string MaterialContent { get; set; }
        public List<QuestionGroup> QuestionGroups { get; set; }
        public int repeat { get; set; }
        public Question()
        {
            QuestionGroups = new List<QuestionGroup>(); // 在这里初始化列表
        }
    }


}
