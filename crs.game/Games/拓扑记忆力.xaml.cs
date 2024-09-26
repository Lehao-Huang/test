using crs.core;
using crs.core.DbModels;
using crs.game.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
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
    /// MEMO.xaml 的交互逻辑
    /// </summary>
    // 难度数据结构
    public class DifficultyLevel
    {
        public int Level { get; set; } // 等级
        public int ImageCount { get; set; } // 图片数量
        public int ImageContrast { get; set; } //图片对比度
        public int MaxErrors { get; set; } // 最大误差
        public int MaxMemoryTime { get; set; } // 最大记忆时间

        // 新增属性
        public double AvgMemoryTime { get; set; } = 0; // 平均记忆时间
        public double LongestMemoryTime { get; set; } = 0; // 最长记忆时间
        public double LongestAnswerTime { get; set; } = 0; // 最长作答时间

        // 做题统计
        public int TotalQuestions { get; set; } = 0; // 做题数量
        public int CorrectAnswers { get; set; } = 0; // 正确数量
        public int WrongAnswers { get; set; } = 0; // 错误数量
    }

    // 训练配置类,默认开启秒表与限制记忆时间
    public class TrainingConfig
    {
        public int TreatmentTime { get; set; } // 治疗时间
        public int RepetitionCount { get; set; } // 重复次数
        public bool ShowClock { get; set; } = true;// 是否显示秒表
        public bool LimitMemoryTime { get; set; } = true; // 是否限制记忆时间

        public bool is_beep { get; set; } = true;
        public ImageMaterial SelectedMaterial { get; set; } // 选择的图片材料

        public TrainingConfig(int T, int R, bool S = true, bool L = true)
        {
            this.TreatmentTime = T;
            this.RepetitionCount = R;
            if (S) { this.ShowClock = L; }
            if (L)
            {
                this.LimitMemoryTime = L;
            }
            this.SelectedMaterial = new ImageMaterial();
        }
    }

    // 图片材料类
    public class ImageMaterial
    {
        public string Name { get; set; } // 图片集名称
        public List<string> ImagePaths { get; set; } // 图片列表
        public bool IsCustom { get; set; } // 是否为用户自定义

        public ImageMaterial() // 默认general
        {
            Name = "一般";
            IsCustom = false;

            // 获取当前执行目录并指向上级目录，返回到项目主目录
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var projectPath = Path.Combine(basePath, "..", "..", "..", "crs.game", "Games", "MEMO", "general");
            projectPath = Path.GetFullPath(projectPath); // 获取绝对路径
            ImagePaths = LoadImagesFromFolder(projectPath); // 加载图片
        }

        public ImageMaterial(string name, string folderPath, bool isCustom = false)
        {
            Name = name;
            IsCustom = isCustom;

            // 获取当前执行目录并指向上级目录，返回到项目主目录
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var projectPath = Path.Combine(basePath, "..", "..", "..", folderPath);
            projectPath = Path.GetFullPath(projectPath); // 获取绝对路径
            ImagePaths = LoadImagesFromFolder(projectPath); // 加载图片
        }


        private List<string> LoadImagesFromFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The directory {folderPath} does not exist.");
            }

            // 支持的图片格式
            string[] supportedExtensions = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" };

            List<string> imagePaths = new List<string>();

            // 从文件夹中加载所有支持的图片文件
            foreach (var extension in supportedExtensions)
            {
                var files = Directory.GetFiles(folderPath, extension);
                imagePaths.AddRange(files);
            }

            return imagePaths;
        }
    }

    // 难度设置管理
    public class DifficultyManager
    {
        public List<DifficultyLevel> DifficultyLevels { get; private set; }

        public DifficultyManager()
        {
            DifficultyLevels = new List<DifficultyLevel>
        {
                new DifficultyLevel { Level = 1, ImageCount = 3, ImageContrast = 1, MaxErrors = 0, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 2, ImageCount = 4, ImageContrast = 1, MaxErrors = 2, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 3, ImageCount = 5, ImageContrast = 1, MaxErrors = 2, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 4, ImageCount = 6, ImageContrast = 1, MaxErrors = 2, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 5, ImageCount = 7, ImageContrast = 2, MaxErrors = 3, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 6, ImageCount = 8, ImageContrast = 2, MaxErrors = 4, MaxMemoryTime = 75 },
                new DifficultyLevel { Level = 7, ImageCount = 8, ImageContrast = 3, MaxErrors = 4, MaxMemoryTime = 75 },
                new DifficultyLevel { Level = 8, ImageCount = 9, ImageContrast = 2, MaxErrors = 4, MaxMemoryTime = 75 },
                new DifficultyLevel { Level = 9, ImageCount = 9, ImageContrast = 3, MaxErrors = 4, MaxMemoryTime = 75 },
                new DifficultyLevel { Level = 10, ImageCount = 10, ImageContrast = 2, MaxErrors = 4, MaxMemoryTime = 75 },
                new DifficultyLevel { Level = 11, ImageCount = 10, ImageContrast = 3, MaxErrors = 4, MaxMemoryTime = 75 },
                new DifficultyLevel { Level = 12, ImageCount = 12, ImageContrast = 2, MaxErrors = 5, MaxMemoryTime = 90 },
                new DifficultyLevel { Level = 13, ImageCount = 12, ImageContrast = 3, MaxErrors = 5, MaxMemoryTime = 90 },
                new DifficultyLevel { Level = 14, ImageCount = 12, ImageContrast = 4, MaxErrors = 5, MaxMemoryTime = 90 },
                new DifficultyLevel { Level = 15, ImageCount = 14, ImageContrast = 2, MaxErrors = 6, MaxMemoryTime = 105 },
                new DifficultyLevel { Level = 16, ImageCount = 14, ImageContrast = 3, MaxErrors = 6, MaxMemoryTime = 105 },
                new DifficultyLevel { Level = 17, ImageCount = 14, ImageContrast = 4, MaxErrors = 6, MaxMemoryTime = 105 },
                new DifficultyLevel { Level = 18, ImageCount = 16, ImageContrast = 2, MaxErrors = 6, MaxMemoryTime = 120 },
                new DifficultyLevel { Level = 19, ImageCount = 16, ImageContrast = 3, MaxErrors = 6, MaxMemoryTime = 120 },
                new DifficultyLevel { Level = 20, ImageCount = 16, ImageContrast = 4, MaxErrors = 6, MaxMemoryTime = 120 }
        };
        }

        public DifficultyLevel GetDifficultyLevel(int level) // 访问函数
        {
            return DifficultyLevels.FirstOrDefault(d => d.Level == level);
        }
    }

    // 主要界面
    public partial class 拓扑记忆力 : BaseUserControl
    {
        private TimeSpan _trainTime; // 训练总用时
        private TimeSpan _remainingTime; // 当前题目用时
        private DispatcherTimer _updateTimer; // 新增的计时器
        private DispatcherTimer _windowTimer; // 总计时器
        private TrainingConfig _trainingConfig; // 训练参数
        private DifficultyManager _difficultyManager; // 难度参数
        private DifficultyLevel _currentDifficulty; // 当前难度及其参数
        private int _level;
        private int _currentRound; // 训练轮数
        private DispatcherTimer _memoryTimer; // 记忆阶段计时器
        private string _targetImagePath; // 目标图像路径
        private Queue<bool> _recentResults = new Queue<bool>(5); // 记录最近5次游戏结果
        private int _totalCountdown; //总倒计时
        private bool is_beep = true; // Beep
        private DateTime _startAnswerTime;

        // 输出指标
        private int _totalQuestions;
        private int _correctAnswers;
        private int _wrongAnswers;
        private int _timeoutCount;
        private DateTime _startTime;
        private int _treatmentTimeInSeconds;
        private int _repeat = 5;


        public 拓扑记忆力()
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
        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            _totalCountdown--;
            // 更新训练总用时
            _trainTime = _trainTime.Add(TimeSpan.FromSeconds(1));
            // 更新当前题目用时
            if (_remainingTime.TotalSeconds > 0)
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                // 判断当前是在记忆阶段还是作答阶段，根据不同阶段显示不同提示内容
                if (_memoryTimer.IsEnabled && _trainingConfig.LimitMemoryTime)
                {
                    // 记忆阶段
                    TipsTextBlock.Text = $"剩余记忆时间：{_remainingTime.TotalSeconds}秒";
                }
            }

            // 触发统计更新事件
            //TimeStatisticsAction?.Invoke((int?)_trainTime.TotalSeconds, (int?)_remainingTime.TotalSeconds);
            TimeStatisticsAction?.Invoke(_totalCountdown, (int?)_remainingTime.TotalSeconds);
        }
        private void InitializeUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            _updateTimer.Tick += OnUpdateTimerTick;
        }
        // 构造Timers
        private void InitializeWindowTimer(TimeSpan totalDuration)
        {
            _windowTimer = new DispatcherTimer();
            _windowTimer.Interval = totalDuration;
            _windowTimer.Tick += OnWindowTimerTick;
            _windowTimer.Start(); // 启动总计时器
        }

        private void InitializeTimers()
        {
            _memoryTimer = new DispatcherTimer();
            _memoryTimer.Interval = TimeSpan.FromSeconds(_trainingConfig?.LimitMemoryTime == true ? _currentDifficulty.MaxMemoryTime : 60);
            _memoryTimer.Tick += OnMemoryTimerTick;
        }


        private void OnWindowTimerTick(object sender, EventArgs e)
        {
            // 停止所有计时器
            _updateTimer?.Stop();
            _memoryTimer?.Stop();
            _windowTimer?.Stop();
            OnGameEnd();
        }
        // 等级设置函数
        public void SetDifficulty(int level)
        {
            _currentDifficulty = _difficultyManager.GetDifficultyLevel(level);
        }
        // 开始测试
        private void OnStartButtonClick()
        {
            _startTime = DateTime.Now;
            _currentRound = 0;
            _remainingTime = TimeSpan.FromSeconds(_currentDifficulty.MaxMemoryTime); // 重置剩余记忆时间
            _updateTimer.Start(); // 启动更新计时器
            StartNextRound();
        }

        private void StartNextRound()
        {

            int correctCount = _recentResults.Count(result => result);
            int wrongCount = _recentResults.Count(result => !result);
            // 判断是否需要调整难度
            if (_recentResults.Count == 5)
            {

                if (correctCount >= _trainingConfig.RepetitionCount)
                {
                    IncreaseDifficulty();
                }
                else if (wrongCount >= _trainingConfig.RepetitionCount)
                {
                    DecreaseDifficulty();
                }
                _recentResults.Clear();
                correctCount = 0;
                wrongCount = 0;
            }

            if ((DateTime.Now - _startTime).TotalSeconds < _treatmentTimeInSeconds)
            {
                StartMemoryPhase();
            }
            else
            {
                OutputResultsGrid();
            }

            _remainingTime = TimeSpan.FromSeconds(_currentDifficulty.MaxMemoryTime); // 重置剩余记忆时间

            LevelStatisticsAction?.Invoke(_currentDifficulty.Level, _difficultyManager.DifficultyLevels.Count);
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
        }

        private void IncreaseDifficulty()
        {
            int currentLevel = _currentDifficulty.Level;
            if (currentLevel < _difficultyManager.DifficultyLevels.Count)
            {
                SetDifficulty(currentLevel + 1);

            }
        }

        private void DecreaseDifficulty()
        {
            int currentLevel = _currentDifficulty.Level;
            if (currentLevel > 1)
            {
                SetDifficulty(currentLevel - 1);

            }
        }
        // 记忆阶段
        private void StartMemoryPhase()
        {
            _totalQuestions++;
            if (is_beep) System.Media.SystemSounds.Beep.Play();

            // 更新提示信息，提示用户记忆图案及其位置
            TipsTextBlock.Text = "请记忆尽可能多的图案及其位置";
            // 开始记忆时间

            // 加载图片路径，并确保随机选择的图片不重复
            var selectedImages = _trainingConfig.SelectedMaterial.ImagePaths.ToList();
            var random = new Random();
            var selectedForButtons = new List<string>();

            PatternGrid.Children.Clear();

            for (int i = 0; i < _currentDifficulty.ImageCount; i++)
            {
                int randomIndex = random.Next(selectedImages.Count);
                var imagePath = selectedImages[randomIndex];

                // 将选中的图片从列表中移除，防止重复
                selectedForButtons.Add(imagePath);
                selectedImages.RemoveAt(randomIndex);

                var image = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePath)),
                    Stretch = Stretch.Uniform // 保持图片的宽高比
                };

                var viewbox = new Viewbox
                {
                    Child = image,
                    Stretch = Stretch.Uniform // Viewbox 保持宽高比
                };

                var button = new Button
                {
                    Tag = imagePath,
                    Width = 100,
                    Height = 100, // 保持按钮的宽高一致
                    Margin = new Thickness(5),
                    Content = viewbox, // 使用 Viewbox 包裹 Image
                    IsEnabled = false
                };

                // 为按钮添加点击事件处理程序
                //button.Click += OnImageButtonClick;

                PatternGrid.Children.Add(button);
            }

            // 随机从已选择的按钮中选取一个图案作为回忆目标
            _targetImagePath = selectedForButtons[random.Next(selectedForButtons.Count)];
            //记忆阶段不显示回忆目标
            RecallImage.Source = null;

            _memoryTimer.Start();
        }

        // 
        private void OnMemoryTimerTick(object sender, EventArgs e)
        {
            _memoryTimer.Stop(); // 停止记忆阶段的计时器
            if (_remainingTime.TotalSeconds <= 0)
            {
                // 记忆时间用完，自动判定为错误
                _wrongAnswers++; // 增加错误数
                _currentDifficulty.WrongAnswers++; // 更新当前等级的错误数量

                // 更新提示信息，显示错误
                IncorrectTextBox.Background = Brushes.LightCoral;
                CorrectTextBox.Background = Brushes.White;

                // 自动进入下一题
                StartNextRound();
            }
            StartRecallPhase();  // 自动进入回忆阶段
        }

        private void OnReadyButtonClick(object sender, RoutedEventArgs e)
        {
            _memoryTimer.Stop(); // 停止记忆阶段的计时器
            double currentMemoryTime = _currentDifficulty.MaxMemoryTime - _remainingTime.TotalSeconds;
            _currentDifficulty.AvgMemoryTime = (_currentDifficulty.AvgMemoryTime * _currentDifficulty.TotalQuestions + currentMemoryTime) / (_currentDifficulty.TotalQuestions + 1);

            if (currentMemoryTime > _currentDifficulty.LongestMemoryTime)
            {
                _currentDifficulty.LongestMemoryTime = currentMemoryTime;
            }

            // 记录作答开始时间
            _startAnswerTime = DateTime.Now;
            StartRecallPhase();  // 直接进入回忆阶段
        }

        // 回忆阶段
        private void StartRecallPhase()
        {
            // 记录当前的记忆时间
            double currentMemoryTime = _currentDifficulty.MaxMemoryTime - _remainingTime.TotalSeconds;

            // 更新平均记忆时间和最长记忆时间
            _currentDifficulty.AvgMemoryTime = (_currentDifficulty.AvgMemoryTime * _currentDifficulty.TotalQuestions + currentMemoryTime) / (_currentDifficulty.TotalQuestions + 1);
            if (currentMemoryTime > _currentDifficulty.LongestMemoryTime)
            {
                _currentDifficulty.LongestMemoryTime = currentMemoryTime;
            }


            if (is_beep) System.Media.SystemSounds.Beep.Play(); // Beep音
            // 将 PatternGrid 中所有按钮的内容盖住（隐藏原始图案）
            foreach (Button button in PatternGrid.Children)
            {
                // 方式1：清空按钮内容
                // button.Content = null;
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var wenhao = Path.Combine(basePath, "..", "..", "..", "crs.game", "Games", "MEMO", "问号.jpg");

                // 方式2：显示一个统一的覆盖图案，例如一个问号或其他符号
                button.Content = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(wenhao)), // 使用覆盖图案路径
                    Stretch = Stretch.Uniform
                };
                button.IsEnabled = true;
                button.Click += OnImageButtonClick;
            }

            // 显示目标图案在右侧的 RecallImage 中
            RecallImage.Source = new BitmapImage(new Uri(_targetImagePath));

            // 更新提示信息
            TipsTextBlock.Text = "请回忆图案位置并点击目标图案";
        }



        private void OnImageButtonClick(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;
            var clickedImagePath = clickedButton.Tag.ToString();
            bool isCorrect = clickedImagePath == _targetImagePath;
            if (is_beep) System.Media.SystemSounds.Beep.Play();
            // 记录当前作答时间
            double currentAnswerTime = (DateTime.Now - _startAnswerTime).TotalSeconds;

            // 更新最长作答时间
            _currentDifficulty.LongestAnswerTime = (_currentDifficulty.LongestAnswerTime * _currentDifficulty.TotalQuestions + currentAnswerTime) / (_currentDifficulty.TotalQuestions + 1);

            // 记录做题数量
            _currentDifficulty.TotalQuestions++;

            if (isCorrect)
            {
                /*_correctAnswers++; // 增加正确数*/
                _currentDifficulty.CorrectAnswers++; // 更新当前等级的正确数量
                CorrectTextBox.Background = Brushes.LightGreen;
                IncorrectTextBox.Background = Brushes.White;
            }
            else
            {
                /*_wrongAnswers++; // 增加错误数*/
                _currentDifficulty.WrongAnswers++; // 更新当前等级的错误数量
                IncorrectTextBox.Background = Brushes.LightCoral;
                CorrectTextBox.Background = Brushes.White;
            }

            // 更新最近的游戏结果队列
            if (_recentResults.Count >= 5)
                _recentResults.Dequeue(); // 移除所有的结果
            _recentResults.Enqueue(isCorrect); // 添加当前结果

            // 进入下一轮
            StartNextRound();
        }
        private void OutputResultsGrid()
        {
            double accuracy = (_correctAnswers / (double)_totalQuestions) * 100;

            var results = new List<KeyValuePair<string, string>>
            {
/*        new KeyValuePair<string, string>("总题数", _totalQuestions.ToString()),
        new KeyValuePair<string, string>("正确数", _correctAnswers.ToString()),
        new KeyValuePair<string, string>("错误数", _wrongAnswers.ToString()),
        new KeyValuePair<string, string>("超时数", _timeoutCount.ToString()),
        new KeyValuePair<string, string>("正确率", $"{accuracy:F2}%"),*/
        new KeyValuePair<string, string>("平均记忆时间", $"{_currentDifficulty.AvgMemoryTime:F2}秒"),
        new KeyValuePair<string, string>("最长记忆时间", $"{_currentDifficulty.LongestMemoryTime:F2}秒"),
        new KeyValuePair<string, string>("最长作答时间", $"{_currentDifficulty.LongestAnswerTime:F2}秒"),
        new KeyValuePair<string, string>("做题数量", $"{_currentDifficulty.TotalQuestions}"),
        new KeyValuePair<string, string>("错误数量", $"{_currentDifficulty.WrongAnswers}"),
        new KeyValuePair<string, string>("正确数量", $"{_currentDifficulty.CorrectAnswers}")
            };
            //MEMO_Report nwd = new MEMO_Report(5, _correctAnswers, _wrongAnswers, _timeoutCount);
        }


        // 重置按钮点击事件
        private void OnResetButtonClick(object sender, RoutedEventArgs e)
        {
            PatternGrid.Children.Clear();
            RecallImage.Source = null;
        }

    }
    public partial class 拓扑记忆力 : BaseUserControl
    {
        private void MEMO_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void MEMO_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer.Stop();
            _memoryTimer.Stop();
            _windowTimer.Stop();
        }


        protected override async Task OnInitAsync()
        {
            int level = 1; int trainTime = 1; int repeatNumber = 5; bool isClockVisible = false; bool memTimeLimit = true;
            _difficultyManager = new DifficultyManager();
            _trainingConfig = new TrainingConfig(trainTime, repeatNumber, isClockVisible, memTimeLimit);

            //SetDifficulty(level);
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
                            case 42: // 治疗时间 
                                _trainingConfig.TreatmentTime = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TRAIN_TIME={_trainingConfig.TreatmentTime}");
                                break;
                            case 44: // 重复次数
                                _trainingConfig.RepetitionCount = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"INCREASE={_trainingConfig.RepetitionCount}");
                                break;
                            case 45: // 限制时间
                                _trainingConfig.LimitMemoryTime = par.Value == 1;
                                Debug.WriteLine($"DECREASE ={_trainingConfig.LimitMemoryTime}");
                                break;
                            case 46: // 听觉反馈
                                _trainingConfig.is_beep = par.Value == 1;
                                Debug.WriteLine($"是否出声={_trainingConfig.is_beep}");
                                break;
                            // 添加其他需要处理的 ModuleParId
                            case 159:
                                level = par.Value.HasValue ? (int)par.Value.Value : 5;
                                //_currentDifficulty.Level = level;
                                Debug.WriteLine($"等级={level}");
                                break;
                            default:
                                Debug.WriteLine($"未处理的 ModuleParId: {par.ModuleParId}");
                                break;
                        }
                    }
                }
                SetDifficulty(level);

            }
            else
            {
                Debug.WriteLine("没有数据");
            }
            SetDifficulty(level);
            // 初始化统计变量
            _totalQuestions = 0;
            _correctAnswers = 0;
            _wrongAnswers = 0;
            _timeoutCount = 0;
            _treatmentTimeInSeconds = _trainingConfig.TreatmentTime * 60;


            //开始训练
            //OnStartButtonClick();

            _totalCountdown = _trainingConfig.TreatmentTime * 60;

            // 调用委托
            LevelStatisticsAction?.Invoke(_currentDifficulty.Level, _difficultyManager.DifficultyLevels.Count);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {

            InitializeTimers();
            InitializeWindowTimer(TimeSpan.FromMinutes(10));

            // 初始化训练时间和剩余时间
            _trainTime = TimeSpan.Zero;
            //_remainingTime = TimeSpan.FromSeconds(_trainingConfig.TreatmentTime * 60); // 根据训练时间初始化剩余时间
            InitializeUpdateTimer(); // 初始化更新计时器
            //StartNextRound();
            OnStartButtonClick();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            _updateTimer?.Stop();
            _memoryTimer?.Stop();
            _windowTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            _updateTimer.Stop();
            _memoryTimer.Stop();
            _windowTimer.Stop();
        }

        protected override async Task OnNextAsync()
        {
            // 调整难度
            StartNextRound();

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
                        // 遍历每个难度级别的数据
                        for (int i = 0; i < _difficultyManager.DifficultyLevels.Count; i++)
                        {
                            DifficultyLevel difficulty = _difficultyManager.DifficultyLevels[i];
                            if (difficulty.AvgMemoryTime == 0 &&
                                difficulty.LongestMemoryTime == 0 &&
                                difficulty.LongestAnswerTime == 0 &&
                                difficulty.TotalQuestions == 0) { continue; }
                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "拓扑记忆力", // 自定义内容
                                Eval = false,
                                Lv = difficulty.Level, // 当前的难度级别
                                ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际
                            };
                            db.Results.Add(newResult);
                            await db.SaveChangesAsync();

                            // 获取 result_id
                            int result_id = newResult.ResultId;

                            // 遍历 DifficultyLevel 中的属性，作为 ResultDetail 插入数据库
                            // --PS：以后有了Order，底下标注了Order的顺序，直接启用即可
                            var resultDetails = new List<ResultDetail>
                    {
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "等级",
                            Value = difficulty.Level,
                            ModuleId = BaseParameter.ModuleId
                            //Order = 1 位于等级后 
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "图片数",
                            Value = difficulty.ImageCount,
                            ModuleId = BaseParameter.ModuleId
                            //Order = 1 位于等级后 
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "平均学习时间",
                            Value = Math.Round(difficulty.AvgMemoryTime),
                            ModuleId = BaseParameter.ModuleId
                            //Order = 5
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "最长学习时间",
                            Value = difficulty.LongestMemoryTime,
                            ModuleId = BaseParameter.ModuleId
                            //Order = 6
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "平均求解时间",
                            Value = Math.Round(difficulty.LongestAnswerTime),
                            ModuleId = BaseParameter.ModuleId
                            //Order = 7
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "做题数量",
                            Value = difficulty.TotalQuestions,
                            ModuleId = BaseParameter.ModuleId
                            //Order = 2
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "正确数量",
                            Value = difficulty.CorrectAnswers,
                            ModuleId = BaseParameter.ModuleId
                            // Order=3
                        },
                        new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "错误数量",
                            Value = difficulty.WrongAnswers,
                            ModuleId = BaseParameter.ModuleId
                            //Order=4
                        }
                    };

                            // 插入 ResultDetail 数据
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();

                            // 输出每个难度级别的属性数据
                            Debug.WriteLine($"难度级别 {difficulty.Level}:");
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