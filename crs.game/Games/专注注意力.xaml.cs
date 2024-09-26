using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Automation;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using crs.core;
using crs.core.DbModels;
using System.IO;
using System.Collections.Generic;
using Microsoft.Identity.Client.NativeInterop;

namespace crs.game.Games
{
    public partial class 专注注意力 : BaseUserControl
    {

        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
{
    "专注注意力/1/1.png",
    "专注注意力/1/2.png",
    "专注注意力/1/3.png",
    "专注注意力/1/4.png",
    "专注注意力/1/5.png",
    "专注注意力/1/6.png",
    "专注注意力/1/7.png",
    "专注注意力/1/8.png",
    "专注注意力/1/9.png",
},
new string[]
{
    "专注注意力/2/1.png",
    "专注注意力/2/2.png",
    "专注注意力/2/3.png",
    "专注注意力/2/4.png",
    "专注注意力/2/5.png",
    "专注注意力/2/6.png",
    "专注注意力/2/7.png",
    "专注注意力/2/8.png",
    "专注注意力/2/9.png",
},
new string[]
{
    "专注注意力/3/1.png",
    "专注注意力/3/2.png",
    "专注注意力/3/3.png",
    "专注注意力/3/4.png",
    "专注注意力/3/5.png",
    "专注注意力/3/6.png",
    "专注注意力/3/7.png",
    "专注注意力/3/8.png",
    "专注注意力/3/9.png",
},
new string[]
{
    "专注注意力/4/1.png",
    "专注注意力/4/2.png",
    "专注注意力/4/3.png",
    "专注注意力/4/4.png",
    "专注注意力/4/5.png",
    "专注注意力/4/6.png",
    "专注注意力/4/7.png",
    "专注注意力/4/8.png",
    "专注注意力/4/9.png",
},
new string[]
{
    "专注注意力/5/1.png",
    "专注注意力/5/2.png",
    "专注注意力/5/3.png",
    "专注注意力/5/4.png",
    "专注注意力/5/5.png",
    "专注注意力/5/6.png",
    "专注注意力/5/7.png",
    "专注注意力/5/8.png",
    "专注注意力/5/9.png",
},
new string[]
{
    "专注注意力/6/1.png",
    "专注注意力/6/2.png",
    "专注注意力/6/3.png",
    "专注注意力/6/4.png",
    "专注注意力/6/5.png",
    "专注注意力/6/6.png",
    "专注注意力/6/7.png",
    "专注注意力/6/8.png",
    "专注注意力/6/9.png",
},
new string[]
{
    "专注注意力/7/1.png",
    "专注注意力/7/2.png",
    "专注注意力/7/3.png",
    "专注注意力/7/4.png",
    "专注注意力/7/5.png",
    "专注注意力/7/6.png",
    "专注注意力/7/7.png",
    "专注注意力/7/8.png",
    "专注注意力/7/9.png",
},
new string[]
{
    "专注注意力/8/1.png",
    "专注注意力/8/2.png",
    "专注注意力/8/3.png",
    "专注注意力/8/4.png",
    "专注注意力/8/5.png",
    "专注注意力/8/6.png",
    "专注注意力/8/7.png",
    "专注注意力/8/8.png",
    "专注注意力/8/9.png",
},
        };

        private int imageCount;
        private int max_time = 10;
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 9;
        private int INCREASE; // 提高难度的阈值
        private int DECREASE;  // 降低难度的阈值
        private int TRAIN_TIME = 60; // 训练持续时间（单位：秒）
        private bool IS_RESTRICT_TIME = true; // 限制练习时间是否启用
        private bool IS_BEEP = true;
        private int train_time;
        private int counter;
        private int randomIndex;
        private Random random;
        private const int moveAmount = 2;
        private int left;
        private int top;
        private int hardness;
        private int[] correctAnswers; // 存储每个难度的正确答案数量
        private int[] wrongAnswers; // 存储每个难度的错误答案数量
        private int[] igonreAnswer;
        private int[] leftAnswer;
        private int[] rightAnswer;
        private int[] centerAnswer;
        private int[] lefttime;
        private int[] righttime;
        private int[] centertime;
        private DispatcherTimer timer;
        private int remainingTime;
        private DispatcherTimer trainingTimer; // 新的计时器用于训练时间
        private Queue<bool> recentResults = new Queue<bool>(5);

        public 专注注意力()
        {
            InitializeComponent();
        }

        private void ShowRandomImage()
        {
            randomIndex = random.Next(imageCount);
            RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[(hardness - 1) / 3][randomIndex]}", UriKind.Relative));

            if (IS_RESTRICT_TIME)
            {
                remainingTime = max_time;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            // 调用委托
            TimeStatisticsAction?.Invoke(train_time, remainingTime);

            if (remainingTime <= 0)
            {
                timer.Stop();
                igonreAnswer[hardness]++;
                recentResults.Enqueue(false); // 使用新的逻辑更新最近的结果
                AdjustDifficulty();
                ShowRandomImage();
                remainingTime = max_time;
                timer.Start();
            }
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // 训练时间倒计时

            // 调用委托
            TimeStatisticsAction?.Invoke(train_time, remainingTime);

            if (train_time <= 0)
            {
                timer.Stop(); // 停止主计时器
                trainingTimer.Stop(); // 停止训练计时器
                //专注注意力报告 reportWindow = new 专注注意力报告(_INCREASE, _DECREASE, max_time, TRAIN_TIME, IS_RESTRICT_TIME, IS_BEEP, correctAnswers, wrongAnswers, igonreAnswer);
                //reportWindow.ShowDialog(); // 打开报告窗口
                //this.Close(); // 关闭当前窗口
                //StopAction?.Invoke();

                OnGameEnd();
            }
        }

        private void LoadImages(int imageCount)
        {
            // 清空之前的图片
            for (int i = ImageGrid.Children.Count - 1; i >= 0; i--)
            {
                if (ImageGrid.Children[i] is Image)
                {
                    ImageGrid.Children.RemoveAt(i);
                }
            }
            // 加载新图片
            for (int i = 0; i < imageCount; i++)
            {
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri($@"../{imagePaths[(hardness - 1) / 3][i]}", UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                int row = 1 + (i / 3) * 2;
                int column = 1 + (i % 3) * 2;
                Grid.SetRow(image, row);
                Grid.SetColumn(image, column);
                ImageGrid.Children.Add(image);
            }
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool isCorrect = (top - 1) * 3 / 2 + (left - 1) / 2 == randomIndex;

                if (isCorrect)
                {
                    correctAnswers[hardness]++; // 更新对应难度的正确答案计数
                    ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#00ff00"));

                }
                else
                {
                    wrongAnswers[hardness]++; // 更新对应难度的错误答案计数
                    if (IS_BEEP)
                        Console.Beep(300, 200);
                    ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#ff0000"));
                }
                recentResults.Enqueue(isCorrect); // 使用新的逻辑更新最近的结果
                AdjustDifficulty();
            }
            else
            {
                if (top > 1 && e.Key == Key.Up)
                    top -= moveAmount;
                if (top < (imageCount / 3) * 2 - 1 && e.Key == Key.Down)
                    top += moveAmount;
                if (left > 1 && e.Key == Key.Left)
                    left -= moveAmount;
                if (left < 5 && e.Key == Key.Right)
                    left += moveAmount;
            }
            Grid.SetColumn(SelectionBox, left);
            Grid.SetRow(SelectionBox, top);
        }

        private void AdjustDifficulty()
        {
            timer?.Stop();
            switch (randomIndex % 3)
            {
                case 0:
                    leftAnswer[hardness]++;
                    lefttime[hardness] += max_time - remainingTime;
                    break;
                case 1:
                    centerAnswer[hardness]++;
                    centertime[hardness] += max_time - remainingTime;
                    break;
                case 2:
                    rightAnswer[hardness]++;
                    righttime[hardness] += max_time - remainingTime;
                    break;
                default:
                    break;
            }
            int totalCorrect;
            int totalWrong;
            if (recentResults.Count > 5)
            {
                recentResults.Dequeue();
                totalCorrect = recentResults.Count(result => result); // 统计 true 的个数，即正确答案数
                totalWrong = recentResults.Count(result => !result);
                // 提高难度
                if (totalCorrect >= INCREASE && hardness < 9)
                {
                    hardness++;
                    imageCount = (hardness % 3) * 3;
                    if (imageCount == 0)
                        imageCount = 9;
                    while (recentResults.Any())
                        recentResults.Dequeue();
                    LoadImages(imageCount);
                }

                // 降低难度
                else if (totalWrong >= DECREASE && hardness > 1)
                {
                    hardness--;
                    imageCount = (hardness % 3) * 3;
                    if (imageCount == 0)
                        imageCount = 9;
                    while (recentResults.Any())
                        recentResults.Dequeue();
                    LoadImages(imageCount);

                }
            }
            totalCorrect = recentResults.Count(result => result); // 统计 true 的个数，即正确答案数
            totalWrong = recentResults.Count(result => !result);
            RightStatisticsAction?.Invoke(totalCorrect, 5);
            WrongStatisticsAction?.Invoke(totalWrong, 5);
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
        }

        private async void ChangeSelectionBoxColor(Color color)
        {
            SelectionBox.Stroke = new SolidColorBrush(color);

            // 等待指定的时间
            await Task.Delay(TimeSpan.FromSeconds(WAIT_DELAY));

            // 恢复原来的颜色
            SelectionBox.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a98d1"));

            left = 1;
            top = 1;
            Grid.SetColumn(SelectionBox, left);
            Grid.SetRow(SelectionBox, top);

            // 调整难度
            AdjustDifficulty();

            ShowRandomImage();
        }
    }

    public partial class 专注注意力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            max_time = 10;
            IS_RESTRICT_TIME = true; // 限制练习时间是否启用
            remainingTime = max_time;
            random = new Random();
            counter = 0;
            train_time = TRAIN_TIME;
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
                            case 1: // 治疗时间 
                                TRAIN_TIME = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TRAIN_TIME={TRAIN_TIME}");
                                break;
                            case 2: // 等级提高
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"INCREASE={INCREASE}");
                                break;
                            case 3: // 等级降低
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 7: // 听觉反馈
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"是否听觉反馈 ={IS_BEEP}");
                                break;
                            case 151:
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS ={hardness}");
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


            // 初始化正确和错误答案的计数数组
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            train_time = 60 * TRAIN_TIME;
            correctAnswers = new int[MAX_HARDNESS];
            wrongAnswers = new int[MAX_HARDNESS];
            igonreAnswer = new int[MAX_HARDNESS];
            leftAnswer = new int[MAX_HARDNESS];
            rightAnswer = new int[MAX_HARDNESS];
            centerAnswer = new int[MAX_HARDNESS];
            lefttime = new int[MAX_HARDNESS];
            righttime = new int[MAX_HARDNESS];
            centertime = new int[MAX_HARDNESS];
            for (int i = 0; i < correctAnswers.Length; i++)
            {
                correctAnswers[i] = 0;
                wrongAnswers[i] = 0;
                igonreAnswer[i] = 0;
                leftAnswer[i] = 0;
                rightAnswer[i] = 0;
                centerAnswer[i] = 0;
                lefttime[i] = 0;
                righttime[i] = 0;
                centertime[i] = 0;
            }

            // 调用委托
            LevelStatisticsAction?.Invoke(0, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {
            timer?.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            trainingTimer?.Stop();
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // 启动训练计时器

            LoadImages(imageCount);
            ShowRandomImage();
            left = 1;
            top = 1;

            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            timer?.Stop();
            trainingTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            timer?.Stop();
            trainingTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {
            LoadImages(imageCount);
            ShowRandomImage();
            left = 1;
            top = 1;
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
            return igonreAnswer[difficultyLevel];
        }
        private int GetleftAnswer(int difficultyLevel)
        {
            return leftAnswer[difficultyLevel];
        }
        private int GetcenterAnswer(int difficultyLevel)
        {
            return centerAnswer[difficultyLevel];
        }
        private int GetrightAnswer(int difficultyLevel)
        {
            return rightAnswer[difficultyLevel];
        }
        private int Getrighttime(int difficultyLevel)
        {
            return righttime[difficultyLevel];
        }
        private int Getcentertime(int difficultyLevel)
        {
            return centertime[difficultyLevel];
        }
        private int Getlefttime(int difficultyLevel)
        {
            return lefttime[difficultyLevel];
        }
        private double CalculateAccuracy(int lv)
        {
            int total = correctAnswers[lv] + wrongAnswers[lv] + igonreAnswer[lv];
            int a = GetCorrectNum(lv);
            Debug.WriteLine($"计算时，总数为{total}，正确数为{a}");
            double accuracy = (double)GetCorrectNum(lv) / total;  // 转换为 double 类型
            // ----------------------------问题在于，在C# 整数相除会去除小数部分，这里得强制加double才不会转类型
            return total > 0 ? Math.Round(accuracy, 2) : 0;
        }
        private int Total(int lv)
        {
            int total = correctAnswers[lv] + wrongAnswers[lv] + igonreAnswer[lv];
            return total;
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
                            int correctCount = GetCorrectNum(lv);

                            int wrongCount = GetWrongNum(lv);

                            int ignoreCount = GetIgnoreNum(lv);

                            int totalcount = Total(lv);

                            double accuracy = CalculateAccuracy(lv);

                            int left_answer = GetleftAnswer(lv);

                            int center_answer = GetcenterAnswer(lv);

                            int right_answer = GetrightAnswer(lv);

                            int lefttime = Getlefttime(lv);

                            int centertime = Getcentertime(lv);

                            int righttime = Getrighttime(lv);

                            double left_time = 0;
                            if (left_answer != 0)
                            {
                                left_time = lefttime / left_answer;
                            }
                            double center_time = 0;
                            if (center_answer != 0)
                            {
                                center_time = centertime / center_answer;
                            }
                            double right_time = 0;
                            if (right_answer != 0)
                            {
                                right_time = righttime / right_answer;
                            }
                            Debug.WriteLine($"正确14 {right_time}");
                            if (totalcount == 0)
                            {
                                // 如果所有数据都为0，跳过此难度级别
                                Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
                                continue;
                            }
                            double acctime = (double)(lefttime + centertime + righttime) / totalcount;
                            Debug.WriteLine($"正确15 {acctime}");


                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "专注注意力",
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
                                    ValueName = "抉择",
                                    Value = totalcount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确数",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率",
                                    Value = accuracy * 100,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误数",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "忽略",
                                    Value = ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "中间值反应时间(ms)",
                                    Value = acctime * 1000,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "中央反映时间左侧(s)",
                                    Value = left_time,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "中央反映时间中间(s)",
                                    Value = center_time,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "中央反映时间右侧(s)",
                                    Value = right_time,
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
