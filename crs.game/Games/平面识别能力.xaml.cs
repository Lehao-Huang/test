using crs.game.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using crs.core;
using crs.core.DbModels;
using System.IO;


namespace crs.game.Games
{
    /// <summary>
    /// VOR.xaml 的交互逻辑
    /// </summary>
    public partial class 平面识别能力 : BaseUserControl
    {

        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
{
    "VOR/1/1.jpg",
    "VOR/1/2.jpg",
    "VOR/1/3.jpg",
    "VOR/1/4.jpg",
    "VOR/1/5.jpg",
    "VOR/1/6.jpg",
    "VOR/1/7.jpg",
    "VOR/1/8.jpg",
    "VOR/1/9.jpg",
},
new string[]
{
    "VOR/2/1.jpg",
    "VOR/2/2.jpg",
    "VOR/2/3.jpg",
    "VOR/2/4.jpg",
    "VOR/2/5.jpg",
    "VOR/2/6.jpg",
    "VOR/2/7.jpg",
    "VOR/2/8.jpg",
    "VOR/2/9.jpg",
},
new string[]
{
    "VOR/3/1.jpg",
    "VOR/3/2.jpg",
    "VOR/3/3.jpg",
    "VOR/3/4.jpg",
    "VOR/3/5.jpg",
    "VOR/3/6.jpg",
    "VOR/3/7.jpg",
    "VOR/3/8.jpg",
    "VOR/3/9.jpg",
},
new string[]
{
    "VOR/4/1.jpg",
    "VOR/4/2.jpg",
    "VOR/4/3.jpg",
    "VOR/4/4.jpg",
    "VOR/4/5.jpg",
    "VOR/4/6.jpg",
    "VOR/4/7.jpg",
    "VOR/4/8.jpg",
    "VOR/4/9.jpg",
},
new string[]
{
    "VOR/5/1.jpg",
    "VOR/5/2.jpg",
    "VOR/5/3.jpg",
    "VOR/5/4.jpg",
    "VOR/5/5.jpg",
    "VOR/5/6.jpg",
    "VOR/5/7.jpg",
    "VOR/5/8.jpg",
    "VOR/5/9.jpg",
},
new string[]
{
    "VOR/6/1.jpg",
    "VOR/6/2.jpg",
    "VOR/6/3.jpg",
    "VOR/6/4.jpg",
    "VOR/6/5.jpg",
    "VOR/6/6.jpg",
    "VOR/6/7.jpg",
    "VOR/6/8.jpg",
    "VOR/6/9.jpg",
},
new string[]
{
    "VOR/7/1.jpg",
    "VOR/7/2.jpg",
    "VOR/7/3.jpg",
    "VOR/7/4.jpg",
    "VOR/7/5.jpg",
    "VOR/7/6.jpg",
    "VOR/7/7.jpg",
    "VOR/7/8.jpg",
    "VOR/7/9.jpg",
},
new string[]
{
    "VOR/8/1.jpg",
    "VOR/8/2.jpg",
    "VOR/8/3.jpg",
    "VOR/8/4.jpg",
    "VOR/8/5.jpg",
    "VOR/8/6.jpg",
    "VOR/8/7.jpg",
    "VOR/8/8.jpg",
    "VOR/8/9.jpg",
},

        };

        private int imageCount;
        private int max_time = 15;
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 24;
        private int INCREASE = 5; // 提高难度的阈值
        private int DECREASE = 5;  // 降低难度的阈值
        private int TRAIN_TIME = 60; // 训练持续时间（单位：秒）
        private int cost_time;
        private bool IS_RESTRICT_TIME = true; // 限制练习时间是否启用
        private bool IS_BEEP = true;
        private int train_time;
        private int counter;
        private int randomIndex;
        private Random random;
        private Random randomforrotate;
        private const int moveAmount = 2;
        private int left;
        private int top;
        private int hardness;
        private int[] correctAnswers; // 存储每个难度的正确答案数量
        private int[] wrongAnswers; // 存储每个难度的错误答案数量
        private int[] igonreAnswer;
        private DispatcherTimer timer;
        private int remainingTime;
        private DispatcherTimer trainingTimer; // 新的计时器用于训练时间

        private int[] TotalCountByHardness;
        private double[] TotalAccuracyByHardness;
        private double[] AverageTimeByHardness;
        private List<int>[] CostTime;
        private int[] MinCostTime;
        private int[] MaxCostTime;

        public 平面识别能力()
        {
            InitializeComponent();
        }


        private void ShowRandomImage()
        {
            randomIndex = random.Next(imageCount);
            RandomImage.Source = new BitmapImage(new Uri(imagePaths[(hardness - 1) / 3][randomIndex], UriKind.Relative));

            if (IS_RESTRICT_TIME)
            {
                remainingTime = max_time;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;
            cost_time += 1;
            TimeStatisticsAction.Invoke(train_time, remainingTime);

            if (remainingTime <= 0)
            {
                timer.Stop();
                igonreAnswer[hardness]++;
                //wrongAnswers[hardness]++;
                LoadImages(imageCount);
                ShowRandomImage();
                remainingTime = max_time;
                timer.Start();
            }
            TotalCountByHardness[hardness - 1] = correctAnswers[hardness - 1] + wrongAnswers[hardness - 1];
            if (TotalCountByHardness[hardness - 1] != 0)
            {
                TotalAccuracyByHardness[hardness - 1] = correctAnswers[hardness - 1] / TotalCountByHardness[hardness - 1];
            }
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // 训练时间倒计时
            TimeStatisticsAction.Invoke(train_time, remainingTime);

            if (train_time <= 0)
            {
                timer.Stop(); // 停止主计时器
                trainingTimer.Stop(); // 停止训练计时器
                //VOR_Report reportWindow = new VOR_Report(INCREASE, DECREASE, max_time, TRAIN_TIME, IS_RESTRICT_TIME, IS_BEEP, correctAnswers, wrongAnswers, igonreAnswer);
                //reportWindow.Show(); // 打开报告窗口
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
                    Source = new BitmapImage(new Uri(imagePaths[(hardness - 1) / 3][i], UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                randomforrotate = new Random();
                int rotationDegrees;
                if (hardness >= 1 && hardness <= 3)
                {
                    rotationDegrees = randomforrotate.Next(4) * 5;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 4 && hardness <= 6)
                {
                    rotationDegrees = randomforrotate.Next(4) * 15;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 7 && hardness <= 9)
                {
                    rotationDegrees = randomforrotate.Next(4) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 10 && hardness <= 12)
                {
                    rotationDegrees = randomforrotate.Next(36) * 10;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 13 && hardness <= 15)
                {
                    rotationDegrees = randomforrotate.Next(4) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 16 && hardness <= 18)
                {
                    rotationDegrees = randomforrotate.Next(36) * 10;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 19 && hardness <= 21)
                {
                    rotationDegrees = randomforrotate.Next(4) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 22 && hardness <= 24)
                {
                    rotationDegrees = randomforrotate.Next(36) * 10;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
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

                CostTime[hardness - 1].Add(cost_time);
                cost_time = 0;
                MinCostTime[hardness - 1] = CostTime[hardness - 1].Min();
                MaxCostTime[hardness - 1] = CostTime[hardness - 1].Max();


                bool isCorrect = (top - 1) * 3 / 2 + (left - 1) / 2 == randomIndex;
                if (isCorrect)
                {
                    correctAnswers[hardness]++; // 更新对应难度的正确答案计数
                    recentResults.Add(true);
                    ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#00ff00"));
                }
                else
                {
                    wrongAnswers[hardness]++; // 更新对应难度的错误答案计数
                    recentResults.Add(false);
                    if (IS_BEEP)
                        Console.Beep();
                    ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#ff0000"));
                }
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

        // 在类中添加一个列表来存储最近5道题的结果
        private List<bool> recentResults = new List<bool>();


        private void resetboollist()
        {
            recentResults.Clear();
        }
        private void AdjustDifficulty()
        {
            int correctCount = 0;
            int wrongCount = 0;
            // 添加当前题目结果到recentResults列表

            // 只保留最近5个结果
            if (recentResults.Count > 5)
            {
                recentResults.RemoveAt(0);
            }
            if (recentResults.Count == 5)
            {
                // 计算最近5道题中正确答案的数量
                correctCount = recentResults.Count(result => result);
                wrongCount = recentResults.Count(result => !result);
                // 提高难度
                if (correctCount >= INCREASE && hardness < 24)
                {
                    hardness++;
                    resetboollist();
                    imageCount = (hardness % 3) * 3;
                    if (imageCount == 0)
                        imageCount = 9;

                }

                // 降低难度
                else if (wrongCount >= DECREASE && hardness > 1)
                {
                    hardness--;
                    resetboollist();
                    imageCount = (hardness % 3) * 3;
                    if (imageCount == 0)
                        imageCount = 9;
                }
            }

            LoadImages(imageCount);
            correctCount = recentResults.Count(result => result == true);
            wrongCount = recentResults.Count(result => result == false);
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
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
            ShowRandomImage();
        }




    }
    public partial class 平面识别能力 : BaseUserControl
    {

        protected override async Task OnInitAsync()
        {
            //this.KeyDown += Window_KeyDown;
            //Debug.WriteLine("INIT");
            //var baseParameter = BaseParameter;
            int increase; int decrease; int mt; int tt; bool irt; bool ib; int hardness_;

            // 此处应该由客户端传入参数为准，目前先用测试数据
            {


                int max_time = 10;
                int INCREASE = 5; // 提高难度的阈值
                int DECREASE = 5;  // 降低难度的阈值
                int TRAIN_TIME = 60; // 训练持续时间（单位：秒）
                bool IS_RESTRICT_TIME = true; // 限制练习时间是否启用
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


            max_time = mt;
            INCREASE = increase; // 提高难度的阈值
            DECREASE = decrease;  // 降低难度的阈值
            TRAIN_TIME = tt; // 训练持续时间（单位：秒）
            IS_RESTRICT_TIME = irt; // 限制练习时间是否启用
            IS_BEEP = ib;
            hardness = hardness_;
            remainingTime = max_time;
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            random = new Random();
            counter = 0;



            {
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
                                case 47: // 治疗时间
                                    TRAIN_TIME = par.Value.HasValue ? (int)par.Value.Value * 60 : 25;
                                    Debug.WriteLine($"TRAIN_TIME={TRAIN_TIME}");
                                    break;
                                case 48: // 等级提高
                                    INCREASE = par.Value.HasValue ? (int)par.Value.Value : 3;
                                    Debug.WriteLine($"INCREASE={INCREASE}");
                                    break;
                                case 49: // 等级降低
                                    DECREASE = par.Value.HasValue ? (int)par.Value.Value : 3;
                                    Debug.WriteLine($"DECREASE ={DECREASE}");
                                    break;
                                case 53: // 听觉反馈
                                    IS_BEEP = par.Value == 1;
                                    Debug.WriteLine($"是否听觉反馈 ={IS_BEEP}");
                                    break;
                                case 51: // 解答时间限制
                                    IS_RESTRICT_TIME = par.Value == 1;
                                    Debug.WriteLine($"是否有解答时间限制 ={IS_RESTRICT_TIME}");
                                    break;
                                // 添加其他需要处理的 ModuleParId
                                case 170://等级
                                    hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"HARDNESS: {hardness}");
                                    break;
                                default:// 初始难度
                                    //hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    //Debug.WriteLine($"HARDNESS: {hardness}");
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


            train_time = TRAIN_TIME;
            // 初始化正确和错误答案的计数数组
            correctAnswers = new int[MAX_HARDNESS];
            wrongAnswers = new int[MAX_HARDNESS];
            igonreAnswer = new int[MAX_HARDNESS];
            TotalCountByHardness = new int[MAX_HARDNESS];
            TotalAccuracyByHardness = new double[MAX_HARDNESS];
            CostTime = new List<int>[MAX_HARDNESS];
            MinCostTime = new int[MAX_HARDNESS];
            MaxCostTime = new int[MAX_HARDNESS];
            for (int i = 0; i < correctAnswers.Length; i++)
            {
                correctAnswers[i] = 0;
                wrongAnswers[i] = 0;
                igonreAnswer[i] = 0;
                TotalAccuracyByHardness[i] = 0;
                TotalCountByHardness[i] = 0;
                CostTime[i] = new List<int>();
                MinCostTime[i] = 0;
                MaxCostTime[i] = 0;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;

            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
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
            left = 1;
            top = 1;
            ShowRandomImage();

            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");

        }

        protected override async Task OnStopAsync()//需要插入
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
            return new 平面识别能力讲解();
        }

        // 插入写法
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
        private int GetMinCostTime(int difficultyLevel)
        {
            return MinCostTime[difficultyLevel];
        }
        private int GetMaxCostTime(int difficultyLevel)
        {
            return MaxCostTime[difficultyLevel];
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
                        for (int lv = 1; lv <= hardness; lv++)
                        {
                            // 获取当前难度级别的数据
                            int correctCount = GetCorrectNum(lv);
                            int wrongCount = GetWrongNum(lv);
                            int ignoreCount = GetIgnoreNum(lv);
                            int totalCount = correctCount + wrongCount + ignoreCount;
                            int mincosttimeCount = GetMinCostTime(lv);
                            int maxcosttimeCount = GetMaxCostTime(lv);
                            if (totalCount == 0 && mincosttimeCount == 0 && maxcosttimeCount == 0)
                            {
                                continue;
                            }
                            // 计算准确率
                            double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "平面识别能力",
                                Eval = false,
                                Lv = lv, // 当前的难度级别
                                ScheduleId = BaseParameter.ScheduleId ?? null// 假设的 Schedule_id，可以替换为实际值
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
                                    ValueName = "等级",
                                    Value = lv,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "全部",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                     ResultId = result_id,
                                    ValueName = "错误",
                                    Value = wrongCount,
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "遗漏",
                                    Value = ignoreCount,
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                   ResultId = result_id,
                                    ValueName = "最小反应时间",
                                    Value = mincosttimeCount,
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "最大反应时间",
                                    Value = maxcosttimeCount,
                                    ModuleId =  BaseParameter.ModuleId
                                }
                            };
                            // 插入 ResultDetail 数据
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();
                            // 输出每个 ResultDetail 对象的数据
                            Debug.WriteLine($"难度级别 {lv}:");
                            foreach (var detail in resultDetails)
                            {
                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
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
