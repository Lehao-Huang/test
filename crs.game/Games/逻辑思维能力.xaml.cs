using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using crs.core;
using crs.core.DbModels;


namespace crs.game.Games
{
    /// <summary>
    /// LODE.xaml 的交互逻辑
    /// </summary>
    public partial class 逻辑思维能力 : BaseUserControl
    {
        private int countdownTime;
        private const int MAX_DELAY = 5000; // 5秒
        private double INCREASE; // 正确增加难度的比例
        private double DECREASE; // 错误降低难度的比例
        private const int AMOUNT = 1; // 阈值的基数
        private string imagePath;
        private string[] imagePaths;
        private string[] directoryPaths;
        private string[] questionPaths;
        private string[] answerPaths;
        private int hardness;
        private int index;
        private Image lastClickedImage;
        private int[] correctAnswers; // 存储每个难度的正确答案数量
        private int[] wrongAnswers; // 存储每个难度的错误答案数量
        private int[] ignoreAnswers;
        private int imageCount;
        private int max_time = 10;
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 23;
        private int TRAIN_TIME; // 训练持续时间（单位：秒）
        private int cost_time;
        private bool IS_RESTRICT_TIME = false; // 限制练习时间是否启用
        private bool IS_BEEP = true;
        private int train_time;
        private int counter;
        private int randomIndex;
        private Random random;
        private const int moveAmount = 2;
        private int left;
        private int top;
        private DispatcherTimer timer;
        private DispatcherTimer trainingTimer; // 新的计时器用于训练时间
        private List<bool> boolList = new List<bool>(5);

        private int[] TotalCountByHardness;
        private double[] TotalAccuracyByHardness;
        private double[] AverageTimeByHardness;
        private List<int>[] CostTime;
        private double[] AverageCostTime;

        public 逻辑思维能力()
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
        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show(IS_RESTRICT_TIME.ToString()); //我已经把他搞成false了 但是时间还是正常走

            if (IS_RESTRICT_TIME)
            {
                train_time--; // 训练时间倒计时

            }
            TimeStatisticsAction.Invoke(train_time, countdownTime);

            if (train_time <= 0)
            {
                timer.Stop(); // 停止主计时器
                trainingTimer.Stop(); // 停止训练计时器
                OnGameEnd();
               
            }
        }
        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            while (true)
            {
                string targetParentDirectory = System.IO.Path.Combine(currentDirectory, "crs.game");
                if (Directory.Exists(targetParentDirectory))
                {
                    string targetDirectory = System.IO.Path.Combine(targetParentDirectory, "逻辑思维能力");
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


        private void AddImages()
        {
            Random rand = new Random();
            directoryPaths = Directory.GetDirectories(imagePath);
            directoryPaths = directoryPaths.OrderBy(path => int.Parse(path.Split('\\').Last())).ToArray();
            imagePaths = Directory.GetDirectories(directoryPaths[hardness - 1]);
            index = rand.Next(imagePaths.Length);
            string newFolderPath = System.IO.Path.Combine(imagePaths[index], "Q");

            questionPaths = Directory.GetFiles(newFolderPath);
            questionPaths = questionPaths.OrderBy(f =>
            {
                var match = Regex.Match(Path.GetFileNameWithoutExtension(f), @"\d+");
                return match.Success ? int.Parse(match.Value) : 0;
            }).ToArray();
            /*string message = string.Join(Environment.NewLine, questionPaths);
            MessageBox.Show(message);*/
            for (int i = 0; i < questionPaths.Length; i++)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(questionPaths[i])),
                    Width = 150,
                    Height = 150
                };
                ImagePanel.Children.Add(img);
            }

            Button additionalButton = new Button
            {
                Width = 150,
                Height = 150,
                Margin = new Thickness(5),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")), // 设置背景颜色
                BorderBrush = Brushes.Transparent // 去掉边框颜色
            };

            additionalButton.Click += AdditionalButton_Click;

            Image buttonImg = new Image
            {
                Source = null,
                Width = 150,
                Height = 150
            };

            additionalButton.Content = buttonImg;
            ImagePanel.Children.Add(additionalButton);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (countdownTime > 0)
            {
                countdownTime--; // 每秒减少1
                cost_time += 1;
                TimeStatisticsAction.Invoke(train_time, countdownTime);
                // 更新UI（例如，显示剩余时间）
            }
            else
            {
                timer.Stop();
                ignoreAnswers[hardness - 1]++; // 记录忽略
                //wrongAnswers[hardness - 1]++; //不确定超时是否计入错误
                ClearAndLoadNewImages();
            }
            TotalCountByHardness[hardness - 1] = correctAnswers[hardness - 1] + wrongAnswers[hardness - 1];
            if (TotalCountByHardness[hardness - 1] != 0)
            {
                TotalAccuracyByHardness[hardness - 1] = correctAnswers[hardness - 1] / TotalCountByHardness[hardness - 1];
            }

        }
        private void ClearAndLoadNewImages()
        {

            ImagePanel.Children.Clear();
            ButtonPanel.Children.Clear();
            AddImages();
            AddButtons();
            countdownTime = max_time;
            timer.Start(); // 重新启动计时器
        }
        private void AdditionalButton_Click(object sender, RoutedEventArgs e)
        {
            Button additionalButton = sender as Button;
            Image buttonImg = additionalButton.Content as Image;

            if (buttonImg.Source != null && lastClickedImage != null)
            {
                lastClickedImage.Source = buttonImg.Source;
                buttonImg.Source = null;
                lastClickedImage = null;
            }
        }

        private void AddButtons()
        {
            string newFolderPath = System.IO.Path.Combine(imagePaths[index], "A");
            answerPaths = Directory.GetFiles(newFolderPath);
            Random rand = new Random();
            answerPaths = answerPaths.OrderBy(x => rand.Next()).ToArray();

            for (int i = 0; i < answerPaths.Length; i++)
            {
                Button btn = new Button
                {
                    Width = 150,
                    Height = 150,
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")), // 设置背景颜色
                    BorderBrush = Brushes.Transparent // 去掉边框颜色
                };

                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(answerPaths[i])),
                    Width = 150,
                    Height = 150
                };

                btn.Content = img;
                btn.Click += Button_Click;
                ButtonPanel.Children.Add(btn);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            Image clickedImage = clickedButton.Content as Image;

            Button additionalButton = ImagePanel.Children.OfType<Button>().LastOrDefault();
            Image additionalButtonImage = additionalButton?.Content as Image;

            if (additionalButtonImage.Source == null)
            {
                additionalButtonImage.Source = clickedImage.Source;
                clickedImage.Source = null;
                lastClickedImage = clickedImage;
            }
        }

        private async void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop(); // 停止计时器
            Button additionalButton = ImagePanel.Children.OfType<Button>().LastOrDefault();
            Image additionalButtonImage = additionalButton?.Content as Image;

            string firstImagePath = Directory.GetFiles(System.IO.Path.Combine(imagePaths[index], "A")).FirstOrDefault();
            bool isCorrect = additionalButtonImage.Source != null && additionalButtonImage.Source.ToString() == new BitmapImage(new Uri(firstImagePath)).ToString();
            additionalButton.BorderThickness = new Thickness(5);
            additionalButton.BorderBrush = Brushes.Transparent;
            CostTime[hardness - 1].Add(cost_time);
            cost_time = 0;
            AverageCostTime[hardness - 1] = CostTime[hardness - 1].Average();

            if (isCorrect)
            {

                additionalButton.BorderThickness = new Thickness(5);
                additionalButton.BorderBrush = Brushes.Green;

                correctAnswers[hardness - 1]++; // 记录正确
                RightStatisticsAction?.Invoke(correctAnswers[hardness - 1], 5);
                await Task.Delay(TimeSpan.FromSeconds(WAIT_DELAY));
                boolList.Add(true);

                // 检查列表长度并删除第一个元素以保持列表长度为5
                if (boolList.Count > 5)
                {
                    boolList.RemoveAt(0);
                }
            }
            else
            {
                additionalButton.BorderThickness = new Thickness(5);
                additionalButton.BorderBrush = Brushes.Red;
                wrongAnswers[hardness - 1]++; // 记录错误
                WrongStatisticsAction?.Invoke(wrongAnswers[hardness - 1], 5);
                if (IS_BEEP)
                    Console.Beep();
                await Task.Delay(TimeSpan.FromSeconds(WAIT_DELAY));
                boolList.Add(false);

                // 检查列表长度并删除第一个元素以保持列表长度为5
                if (boolList.Count > 5)
                {
                    boolList.RemoveAt(0);
                }
            }
            AdjustDifficulty();
            ClearAndLoadNewImages();
        }

        private void resetboollist()
        {
            boolList.Clear();
        }
        private void AdjustDifficulty()
        {
            int trueCount = 0;
            int falseCount = 0;

            // 遍历列表并计数
            foreach (bool value in boolList)
            {
                if (value)
                {
                    trueCount++;
                }
                else
                {
                    falseCount++;
                }
            }
            if (boolList.Count == 5)
            {
                if (trueCount >= INCREASE)
                {
                    if (hardness < MAX_HARDNESS)
                    {
                        hardness++;
                        resetboollist();
                    }
                }
                if (falseCount >= DECREASE)
                {
                    if (hardness > 1)
                    {
                        hardness--;
                        resetboollist();
                    }
                }
            }
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(trueCount, 5);
            WrongStatisticsAction?.Invoke(falseCount, 5);
            MessageBox.Show(ignoreAnswers[hardness - 1].ToString());
        }

        private void ResetCounts()
        {
            // 重置当前难度的正确和错误计数
            correctAnswers[hardness - 1] = 0;
            wrongAnswers[hardness - 1] = 0;
        }


    }
    public partial class 逻辑思维能力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            int increase; int decrease; int mt; int tt; bool irt; bool ib; int hardness_;

            // 此处应该由客户端传入参数为准，目前先用测试数据
            {
                int max_time = 10;
                int INCREASE = 3; // 提高难度的阈值
                int DECREASE = 3;  // 降低难度的阈值
                int TRAIN_TIME = 60; // 训练持续时间（单位：秒）
                bool IS_RESTRICT_TIME = false; // 限制练习时间是否启用
                bool IS_BEEP = true; // 是否发出声音
                int hardness = 1; // 难度级别

                increase = INCREASE;
                decrease = DECREASE;
                mt = max_time;
                tt = TRAIN_TIME;
                irt = IS_RESTRICT_TIME;
                ib = IS_BEEP;
                hardness_ = hardness;
            }


            correctAnswers = new int[23]; // 23个难度的正确计数
            wrongAnswers = new int[23]; // 23个难度的错误计数
            ignoreAnswers = new int[23];
            TotalCountByHardness = new int[23];
            TotalAccuracyByHardness = new double[23];
            CostTime = new List<int>[23];
            AverageCostTime = new double[23];
            max_time = mt;
            INCREASE = increase; // 提高难度的阈值
            DECREASE = decrease;  // 降低难度的阈值
            TRAIN_TIME = tt; // 训练持续时间（单位：秒）
            IS_RESTRICT_TIME = irt; // 限制练习时间是否启用
            IS_BEEP = ib;
            hardness = hardness_;
            countdownTime = max_time;
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            random = new Random();
            counter = 0;
            train_time = TRAIN_TIME;


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
                        switch (par.ModuleParId) // 完成赋值
                        {
                            case 178: // 等级
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS: {hardness}");
                                break;
                            case 35: // 治疗时间 
                                train_time = par.Value.HasValue ? (int)par.Value.Value * 60 : 60;
                                Debug.WriteLine($"TRAIN_TIME={train_time}");
                                break;
                            case 36: // 等级提高
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"INCREASE={INCREASE}");
                                break;
                            case 37: // 等级降低
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 38: // 项目每等级
                                /*                                IS_RESTRICT_TIME = par.Value == 1;
                                                                Debug.WriteLine($"是否限制时间 ={IS_BEEP}");*/
                                break;
                            case 39: // 限制解答时间
                                IS_RESTRICT_TIME = par.Value == 1;
                                Debug.WriteLine($"限制解答时间 ={IS_RESTRICT_TIME}");
                                break;
                            case 40: // 听觉反馈
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"是否听觉反馈 ={IS_BEEP}");
                                break;

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

            for (int i = 0; i < correctAnswers.Length; i++)
            {
                correctAnswers[i] = 0;
                wrongAnswers[i] = 0;
                ignoreAnswers[i] = 0;
                TotalAccuracyByHardness[i] = 0;
                TotalCountByHardness[i] = 0;
                CostTime[i] = new List<int>();
                AverageCostTime[i] = 0;

            };

            imagePath = FindImagePath();

            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {
            // 设置倒计时的初始值（例如，10秒）
            countdownTime = max_time; // 或者您可以设置为其他值

            timer?.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start(); // 启动计时器

            trainingTimer?.Stop();
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // 启动训练计时器

            AddImages();
            AddButtons();

            ClearAndLoadNewImages();
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
            // 调整难度
            AdjustDifficulty();
            ClearAndLoadNewImages();

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
            return new 逻辑思维能力讲解();
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
            return ignoreAnswers[difficultyLevel];
        }
        private double GetAverageCostTime(int difficultyLevel)
        {
            return AverageCostTime[difficultyLevel];
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
                        for (int lv = 0; lv < hardness; lv++)
                        {
                            // 获取当前难度级别的数据
                            int correctCount = GetCorrectNum(lv);
                            int wrongCount = GetWrongNum(lv);
                            int ignoreCount = GetIgnoreNum(lv);
                            int totalCount = correctCount + wrongCount + ignoreCount;
                            double averageTime = GetAverageCostTime(lv);
                            if (totalCount == 0 && averageTime == 0)
                            {
                                continue;
                            }
                            // 计算准确率
                            double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "逻辑思维能力",
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
                                    Value = lv+1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "系列图片数量",
                                    Value = totalCount,
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
                                    ValueName = "正确的（%）",
                                    Value = accuracy * 100, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误 全部的",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "遗漏",
                                    Value =ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "中间值 求解时间",
                                    Value = averageTime,
                                    ModuleId = BaseParameter.ModuleId
                                }
                             };

                            // 插入 ResultDetail 数据
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();

                            // 输出每个 ResultDetail 对象的数据
                            Debug.WriteLine($"难度级别 {lv + 1}:");
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