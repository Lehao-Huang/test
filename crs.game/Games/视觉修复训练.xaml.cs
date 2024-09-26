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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 视觉修复训练.xaml 的交互逻辑
    /// </summary>
    public partial class 视觉修复训练 : BaseUserControl
    {   // 需要上传至报告的参数
        private int totalShapeChanges = 0; // 屏幕中心固定点出现变化的次数
        private int successfulFocusPresses = 0; // 患者成功响应屏幕中心固定点出现变化的次数
        private int successfulFocusPercentage = 0;// 患者成功响应屏幕中心固定点变化的百分比
        private int totalSquareFlashes = 0; // 白色方形刺激闪烁的次数
        private int successfulSquarePresses = 0; // 患者成功响应白色方形刺激的次数
        private int successfulSquarePercentage = 0;// 患者成功响应白色方形刺激的百分比



        private Random random = new Random();
        private double verticalRangeMin = -180;
        private double verticalRangeMax = 180;
        private double horizontalRangeMin = 0;
        private double horizontalRangeMax = 1800;
        private DispatcherTimer trainingTimer;
        private int train_time;

        private Stopwatch stopwatch;
        private DispatcherTimer checkTimer;
        private int restInterval = 120; // 每120秒休息10秒
        private int restDuration = 10; // 休息时长（秒）
        private bool showRestImage;
        private bool isResting = false;
        private Direction selectedVisionDirection; // 默认方向
        private double shapeChangeElapsedTime = 0; // 用于跟踪形状变化的时间
        private double currentShapeChangeInterval = 0; // 当前形状变化的间隔
        private bool isShapeChanged = false;
        private bool isFirstPhase = true; // 标识当前训练阶段
        private Rectangle blinkSquare; // 闪烁的方块
        private Polygon triangle;
        private double blinkElapsedTime = 0; // 用于跟踪时间的变量
        private bool isBlinkVisible = false; // 当前方块是否可见

        // 管理方块移动
        private int moveCount = 0; // 记录方块的水平移动次数
        private double initialTopMargin = 0; // 记录方块初始的上边距
        private bool canMoveSquare = true; // 用于控制第二阶段中方块的移动

        // 需要上传至报告的参数
        private Stopwatch reactionStopwatch = new Stopwatch(); // 用于测量用户反应时间


        private DispatcherTimer restTimer;
        private int remainingTime;
        private Queue<bool> recentResults = new Queue<bool>(5);
        public enum Direction
        {
            LeftUp,
            RightUp,
            Left,
            Right,
            LeftDown,
            RightDown
        }
        private void MoveBlinkSquareRandomly()
        {
            double newLeftMargin = random.NextDouble() * (horizontalRangeMax - horizontalRangeMin) + horizontalRangeMin;
            double newTopMargin = random.NextDouble() * (verticalRangeMax - verticalRangeMin) + verticalRangeMin;

            blinkSquare.Margin = new Thickness(newLeftMargin, newTopMargin, 0, 0);
        }        
        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            if (!isResting && stopwatch.Elapsed.TotalSeconds >= restInterval)
            {
                isResting = true;
                ShowRestWindow(); // 调用显示休息窗口的方法
            }
        }
        private async void ShowRestWindow()
        {
            stopwatch.Stop(); // 停止计时
            Rest(showRestImage);

            // 休息结束后继续训练
            await Task.Delay(TimeSpan.FromSeconds(restDuration)); // 模拟10秒的休息时间

            // 休息结束后恢复计时器和继续训练
            stopwatch.Restart(); // 重置并启动 Stopwatch
            isResting = false;
        }
        private void SetInitialPosition(Direction direction)
        {
            switch (direction)
            {
                case Direction.LeftUp:
                    blinkSquare.Margin = new Thickness(-60, -60, 0, 0);
                    break;
                case Direction.RightUp:
                    blinkSquare.Margin = new Thickness(60, -60, 0, 0);
                    break;
                case Direction.Left:
                    blinkSquare.Margin = new Thickness(-60, 0, 0, 0);
                    break;
                case Direction.Right:
                    blinkSquare.Margin = new Thickness(60, 0, 0, 0);
                    break;
                case Direction.LeftDown:
                    blinkSquare.Margin = new Thickness(-60, 60, 0, 0);
                    break;
                case Direction.RightDown:
                    blinkSquare.Margin = new Thickness(60, 60, 0, 0);
                    break;
            }
        }
        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // 训练时间倒计时

            // 调用委托
            TimeStatisticsAction?.Invoke(train_time, 0);
            if (train_time <= 0)
            {
                // 停止所有计时器
                trainingTimer.Stop();
                checkTimer.Stop();
                stopwatch.Stop();
                reactionStopwatch.Stop();
                currentShapeChangeInterval = 10000;
                //viewModel.UpdateResults(totalShapeChanges, successfulFocusPresses, totalSquareFlashes, successfulSquarePresses);
                // 创建报告窗口
                //var reportWindow = new reportWindow(viewModel);

                OnGameEnd();
            }

        }
        private void InitializeTraining()
        {
            InitializeComponent();

            currentShapeChangeInterval = GetRandomInterval(4, 7); // 第一阶段 4-6 秒


            // 在初始化时使用XAML中定义的FocusPoint

            // 初始化三角形对象
            triangle = new Polygon
            {
                Points = new PointCollection(new Point[]
                {
                    new Point(20, 0),  // 顶点
                    new Point(0, 34.64), // 左下角
                    new Point(40, 34.64) // 右下角
                }),
                Fill = new SolidColorBrush(Color.FromRgb(184, 134, 11)), // 深黄色
                Width = 40,
                Height = 34.64, // 等边三角形的高度
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Hidden  // 初始状态为隐藏
            };

            TrainGrid.Children.Add(triangle); // 添加到Grid中，但初始时隐藏

            // 初始化方块并设置其位置，默认不可见
            blinkSquare = new Rectangle
            {
                Width = 30,
                Height = 30,
                Fill = Brushes.White,
                Visibility = Visibility.Hidden // 初始状态为不可见
            };

            TrainGrid.Children.Add(blinkSquare);

            // 根据传递的方向参数设置初始位置
            SetInitialPosition(selectedVisionDirection);
            CompositionTarget.Rendering += OnRendering;
        }
        private double GetRandomInterval(int minSeconds, int maxSeconds)
        {
            Random rand = new Random();
            return rand.Next(minSeconds, maxSeconds);
        }
        private void OnRendering(object sender, EventArgs e)
        {
            shapeChangeElapsedTime += 1.0 / 60.0; // 假设每秒钟渲染60帧

            if (shapeChangeElapsedTime >= currentShapeChangeInterval)
            {
                if (!isShapeChanged)
                {
                    // 将圆形隐藏，将三角形显示
                    FocusPoint.Visibility = Visibility.Hidden;
                    triangle.Visibility = Visibility.Visible;
                    isShapeChanged = true;

                    // 增加形状变化的总次数
                    totalShapeChanges++;

                    // 开始计时用户反应时间
                    reactionStopwatch.Restart();

                    // 0.5秒后恢复为圆形
                    currentShapeChangeInterval = 0.5;
                }
                else
                {
                    // 将三角形隐藏，恢复为圆形
                    triangle.Visibility = Visibility.Hidden;
                    FocusPoint.Visibility = Visibility.Visible;

                    // 根据阶段调整下一个变化的间隔
                    currentShapeChangeInterval = isFirstPhase ? GetRandomInterval(4, 7) : GetRandomInterval(15, 20);
                }

                shapeChangeElapsedTime = 0.0; // 重置计时器
            }
            // 检查用户是否超时未按键
            if (isShapeChanged && reactionStopwatch.Elapsed.TotalSeconds > 1.5)
            {
                reactionStopwatch.Stop();
                HandleFailure(); // 用户未按键，处理为失败
            }

            // 控制方块闪烁（仅在第二阶段）
            if (!isFirstPhase)
            {
                blinkElapsedTime += 1.0 / 60.0;

                if (isBlinkVisible)
                {
                    if (blinkElapsedTime >= 0.2) // 显示0.2秒后隐藏
                    {
                        blinkSquare.Visibility = Visibility.Hidden;
                        isBlinkVisible = false;
                        blinkElapsedTime = 0.0;
                        canMoveSquare = true; // 方块闪烁完毕，允许下一次移动
                    }
                }
                else
                {
                    if (blinkElapsedTime >= 2.3) // 每2.5秒闪烁一次 (2.3 + 0.2 = 2.5)
                    {
                        blinkSquare.Visibility = Visibility.Visible;
                        isBlinkVisible = true;
                        blinkElapsedTime = 0.0;

                        // 增加方块闪烁次数
                        totalSquareFlashes++;
                    }
                }
            }
        }
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Debug.WriteLine("检测到 Enter 键按下");

            if (e.Key == Key.Enter && isShapeChanged)
            {
                // 停止计时，获取用户的反应时间
                reactionStopwatch.Stop();
                double reactionTime = reactionStopwatch.Elapsed.TotalSeconds;

                if (reactionTime >= 0.15 && reactionTime <= 1.5)
                {
                    // 用户在正确的时间窗口内按下了 Enter 键，判定成功
                    successfulFocusPresses++; // 增加成功次数
                    recentResults.Enqueue(true);
                    if (recentResults.Count > 5)
                    {
                        recentResults.Dequeue();
                    }
                    int totalCorrect = recentResults.Count(result => result); // 统计 true 的个数，即正确答案数
                    int totalWrong = recentResults.Count(result => !result);
                    RightStatisticsAction?.Invoke(totalCorrect, 5);
                    WrongStatisticsAction?.Invoke(totalWrong, 5);
                    isShapeChanged = false;

                    triangle.Visibility = Visibility.Hidden;  // 隐藏三角形
                    FocusPoint.Visibility = Visibility.Visible;  // 显示圆形

                    if (isFirstPhase)
                    {
                        // 进入第二阶段
                        isFirstPhase = false;
                        currentShapeChangeInterval = GetRandomInterval(15, 20); // 第二阶段间隔调整
                        shapeChangeElapsedTime = 0.0; // 重置计时器
                    }
                }
                else
                {
                    // 用户未在正确的时间窗口内按下 Enter 键，判定失败
                    HandleFailure();
                }
            }
            else if (e.Key == Key.Enter && !isFirstPhase && canMoveSquare)
            {
                // 成功按下了 Enter 键，移动方块
                MoveBlinkSquare();
                canMoveSquare = false; // 禁止后续的 Enter 判定，直到方块再次闪烁

                // 记录用户在白色方块闪烁后成功按下Enter的次数
                successfulSquarePresses++;
                recentResults.Enqueue(true);
                if (recentResults.Count > 5)
                {
                    recentResults.Dequeue();
                }
                int totalCorrect = recentResults.Count(result => result); // 统计 true 的个数，即正确答案数
                int totalWrong = recentResults.Count(result => !result);
                RightStatisticsAction?.Invoke(totalCorrect, 5);
                WrongStatisticsAction?.Invoke(totalWrong, 5);
            }
        }
        private void HandleFailure()
        {

            // 在失败情况下重置为圆形
            isShapeChanged = false;
            triangle.Visibility = Visibility.Hidden;  // 隐藏三角形
            FocusPoint.Visibility = Visibility.Visible;  // 显示圆形
            recentResults.Enqueue(false);
            if (recentResults.Count > 5)
            {
                recentResults.Dequeue();
            }
            int totalCorrect = recentResults.Count(result => result); // 统计 true 的个数，即正确答案数
            int totalWrong = recentResults.Count(result => !result);
            RightStatisticsAction?.Invoke(totalCorrect, 5);
            WrongStatisticsAction?.Invoke(totalWrong, 5);

            currentShapeChangeInterval = isFirstPhase ? GetRandomInterval(4, 7) : GetRandomInterval(15, 20);
            shapeChangeElapsedTime = 0.0; // 重置计时器            
        }
        private void MoveBlinkSquare()
        {
            if (selectedVisionDirection == Direction.Left || selectedVisionDirection == Direction.Right)
            {
                // 如果方向是左右，则随机移动方块
                MoveBlinkSquareRandomly();
            }
            else
            {
                moveCount++;

                double currentLeftMargin = blinkSquare.Margin.Left;
                double currentTopMargin = blinkSquare.Margin.Top;

                switch (selectedVisionDirection)
                {
                    case Direction.RightDown:
                        if (moveCount < 5)
                        {
                            blinkSquare.Margin = new Thickness(currentLeftMargin + 60, currentTopMargin, 0, 0);
                        }
                        else
                        {
                            moveCount = 0;
                            initialTopMargin += 60;
                            currentLeftMargin -= 180; // 每行起点向右下移动
                            blinkSquare.Margin = new Thickness(currentLeftMargin, initialTopMargin, 0, 0);
                        }
                        break;

                    case Direction.LeftDown:
                        if (moveCount < 5)
                        {
                            blinkSquare.Margin = new Thickness(currentLeftMargin - 60, currentTopMargin, 0, 0);
                        }
                        else
                        {
                            moveCount = 0;
                            initialTopMargin += 60;
                            currentLeftMargin += 180; // 每行起点向左下移动
                            blinkSquare.Margin = new Thickness(currentLeftMargin, initialTopMargin, 0, 0);
                        }
                        break;

                    case Direction.RightUp:
                        if (moveCount < 5)
                        {
                            blinkSquare.Margin = new Thickness(currentLeftMargin + 60, currentTopMargin, 0, 0);
                        }
                        else
                        {
                            moveCount = 0;
                            initialTopMargin -= 60;
                            currentLeftMargin -= 180; // 每行起点向右上移动
                            blinkSquare.Margin = new Thickness(currentLeftMargin, initialTopMargin, 0, 0);
                        }
                        break;

                    case Direction.LeftUp:
                        if (moveCount < 5)
                        {
                            blinkSquare.Margin = new Thickness(currentLeftMargin - 60, currentTopMargin, 0, 0);
                        }
                        else
                        {
                            moveCount = 0;
                            initialTopMargin -= 60;
                            currentLeftMargin += 180; // 每行起点向左上移动
                            blinkSquare.Margin = new Thickness(currentLeftMargin, initialTopMargin, 0, 0);
                        }
                        break;
                }
            }
        }
        private void Button_Click_Continue(object sender, RoutedEventArgs e)
        {
            TipGrid.Visibility = Visibility.Collapsed;
            RestGrid.Visibility = Visibility.Collapsed;
            TrainGrid.Visibility = Visibility.Visible;
        }
        private void Rest(bool showRestImage, int restDuration = 10)
        {
            TrainGrid.Visibility = Visibility.Collapsed;
            TipGrid.Visibility = Visibility.Collapsed;
            RestGrid.Visibility = Visibility.Visible;
            remainingTime = restDuration;

            // 根据 showRestImage 参数设置界面内容
            if (showRestImage)
            {
                RestLabel.Visibility = Visibility.Hidden;  // 隐藏文字
                RestImage.Visibility = Visibility.Visible; // 显示图片
                RestImage.Source = new BitmapImage(new Uri("视觉修复训练.jpg", UriKind.Relative));
            }
            else
            {
                RestLabel.Visibility = Visibility.Visible;  // 显示文字
                RestImage.Visibility = Visibility.Hidden;   // 隐藏图片
            }

            // 设置并启动计时器
            restTimer = new DispatcherTimer();
            restTimer.Interval = TimeSpan.FromSeconds(1);
            restTimer.Tick += RestTimer_Tick;
            restTimer.Start();
        }
        private void RestTimer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime <= 0)
            {
                restTimer.Stop();
                RestGrid.Visibility = Visibility.Collapsed;
            }

            RestLabel.Content = $"休息 {remainingTime} 秒"; // 更新显示的文本
        }
    }
    public partial class 视觉修复训练 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            InitializeTraining();
            TrainGrid.Visibility = Visibility.Collapsed;
            RestGrid.Visibility = Visibility.Collapsed;
            TipGrid.Visibility = Visibility.Visible;



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
                            case 174: // 治疗时间 
                                train_time = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TRAIN_TIME={train_time}");
                                break;
                            case 225: // 控制方向是否为左上
                                      // 假设 par.Value 是 double? 类型，根据值进行判断
                                bool isLeftUp = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isLeftUp)
                                {
                                    selectedVisionDirection = Direction.LeftUp;
                                }
                                Debug.WriteLine($"IS_LEFT_UP={isLeftUp}");
                                break;

                            case 226: // 控制方向是否为右上
                                bool isRightUp = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isRightUp)
                                {
                                    selectedVisionDirection = Direction.RightUp;
                                }
                                Debug.WriteLine($"IS_RIGHT_UP={isRightUp}");
                                break;
                            case 227: // 控制方向是否为左
                                bool isLeft = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isLeft)
                                {
                                    selectedVisionDirection = Direction.Left;
                                }
                                Debug.WriteLine($"IS_LEFT={isLeft}");
                                break;
                            case 228: // 控制方向是否为右
                                bool isRight = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isRight)
                                {
                                    selectedVisionDirection = Direction.Right;
                                }
                                Debug.WriteLine($"IS_RIGHT={isRight}");
                                break;
                            case 229: // 控制方向是否为左下
                                bool isLeftDown = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isLeftDown)
                                {
                                    selectedVisionDirection = Direction.LeftDown;
                                }
                                Debug.WriteLine($"IS_LEFT_DOWN={isLeftDown}");
                                break;
                            case 230: // 控制方向是否为右下
                                bool isRightDown = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isRightDown)
                                {
                                    selectedVisionDirection = Direction.RightDown;
                                }
                                Debug.WriteLine($"IS_RIGHT_DOWN={isRightDown}");
                                break;
                            case 243: // 控制休息时是否带有图片
                                      // 从数据库中读取的值，判断是否非零，非零代表显示图片
                                bool isShowRestImage = par.Value.HasValue ? (par.Value.Value != 0) : false;

                                // 根据数据库的值来设置 showRestImage 参数
                                showRestImage = isShowRestImage;

                                // 输出调试信息以确认读取的值
                                Debug.WriteLine($"showRestImage from database: {showRestImage}");
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

            // 调用委托
            LevelStatisticsAction?.Invoke(1,1);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {
            // 设置初始方块位置
            SetInitialPosition(selectedVisionDirection);
            // 设置训练时长的计时器
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromMinutes(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start();


            // 初始化 Stopwatch
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // 初始化定期检查时间的 DispatcherTimer
            checkTimer = new DispatcherTimer();
            checkTimer.Interval = TimeSpan.FromSeconds(10); // 每10秒检查一次时间
            checkTimer.Tick += CheckTimer_Tick;
            checkTimer.Start();

            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            trainingTimer.Stop();
            checkTimer.Stop();
            stopwatch.Stop();
            reactionStopwatch.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            trainingTimer.Stop();
            checkTimer.Stop();
            stopwatch.Stop();
            reactionStopwatch.Stop();
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

        /*
        private int totalShapeChanges = 0; // 屏幕中心固定点出现变化的次数
        private int successfulFocusPresses = 0; // 患者成功响应屏幕中心固定点出现变化的次数
        private int successfulFocusPercentage = 0 // 患者成功响应屏幕中心固定点变化的百分比
        private int totalSquareFlashes = 0; // 白色方形刺激闪烁的次数
        private int successfulSquarePresses = 0; // 患者成功响应白色方形刺激的次数
        private int successfulSquarePercentage = 0 // 患者成功响应白色方形刺激的百分比
        */
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
                            if (totalShapeChanges == 0 && successfulFocusPercentage == 0 && successfulFocusPresses == 0 && totalSquareFlashes ==0 
                                && successfulSquarePercentage ==0
                                && successfulSquarePresses==0)
                            {
                                // 如果所有数据都为0，跳过此难度级别
                                Debug.WriteLine($"没有数据，跳过.");
                                //continue;
                            }
                            Debug.WriteLine("这里进行Result插入1");
                            
                        // 创建 Result 记录

                        successfulFocusPercentage = (successfulFocusPresses / totalShapeChanges) * 100;
                        successfulSquarePercentage = (successfulSquarePresses / totalSquareFlashes) * 100;

                        var newResult = new Result
                            {
                                ProgramId = BaseParameter.ProgramId, // program_id
                                Report = "视觉修复训练",
                                Eval = false,
                                Lv = null, // 当前的难度级别
                                ScheduleId = BaseParameter.ScheduleId // 假设的 Schedule_id，可以替换为实际值
                            };
                            Debug.WriteLine($"截止");
                            db.Results.Add(newResult);
                            await db.SaveChangesAsync();

                            // 获得 result_id
                            int result_id = newResult.ResultId;

                            // 创建 ResultDetail 对象列表

                            // 把底下的改成你对应的报告参数-------------------------------------
                            var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "刺激",
                                    Value = totalSquareFlashes,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确 刺激",
                                    Value = successfulSquarePresses,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确 刺激%",
                                    Value = successfulSquarePercentage,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "固定检查",
                                    Value = totalShapeChanges,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确 固定检查",
                                    Value = successfulFocusPresses,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确 固定检查%",
                                    Value = successfulFocusPercentage,
                                    ModuleId = BaseParameter.ModuleId
                                },
                            };

                            // 插入 ResultDetail 数据
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();

                            // 输出每个 ResultDetail 对象的数据
                            //Debug.WriteLine($"难度级别 {lv}:");
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