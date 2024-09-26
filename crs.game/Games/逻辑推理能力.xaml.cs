using crs.core;
using crs.core.DbModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace crs.game.Games
{
    /// <summary>
    /// 逻辑推理能力.xaml 的交互逻辑
    /// </summary>
    public partial class 逻辑推理能力 : BaseUserControl
    {
        private string imagePath; // 存储找到的文件夹路径
        private const int MAX_GAME = 10;
        private string[] imagePaths;
        private string[] directoryPaths;
        private int display_index;
        private int questionCount; // 记录已展示的题目数量
        private int correctCount; // 记录正确回答的数量
        private int incorrectCount; // 记录错误回答的数量
        private DateTime startTime; // 开始时间
        private double total_time; // 总答题时间
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒// 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        public 逻辑推理能力()
        {
            InitializeComponent();
        }

        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            while (true)
            {
                // 检查当前目录是否存在“crs.game”文件夹
                string targetParentDirectory = Path.Combine(currentDirectory, "crs.game");
                if (Directory.Exists(targetParentDirectory))
                {
                    string targetDirectory = Path.Combine(targetParentDirectory, "逻辑推理能力");
                    if (Directory.Exists(targetDirectory))
                    {
                        return targetDirectory; // 找到文件夹，返回路径
                    }
                }

                // 向上移动到父目录
                DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);
                if (parentDirectory == null)
                {
                    break; // 到达根目录，停止查找
                }
                currentDirectory = parentDirectory.FullName; // 更新当前目录
            }

            return null; // 未找到文件夹
        }

        private void ShowImage()
        {
            directoryPaths = Directory.GetDirectories(imagePath); // 获取所有题目的路径
            Random rand = new Random();
            int index = rand.Next(directoryPaths.Length);
            imagePaths = Directory.GetFiles(directoryPaths[index]); // 某一道题目所在文件夹

            // 确保文件数量足够
            if (imagePaths.Length >= 8)
            {
                QImage1.Source = new BitmapImage(new Uri(imagePaths[0]));
                QImage2.Source = new BitmapImage(new Uri(imagePaths[1]));
                QImage3.Source = new BitmapImage(new Uri(imagePaths[2]));
                QImage4.Source = new BitmapImage(new Uri(imagePaths[3]));
                AImage1.Source = new BitmapImage(new Uri(imagePaths[4]));
                AImage2.Source = new BitmapImage(new Uri(imagePaths[5]));
                AImage3.Source = new BitmapImage(new Uri(imagePaths[6]));
                AImage4.Source = new BitmapImage(new Uri(imagePaths[7]));
            }
            else
            {
                MessageBox.Show("当前题目文件夹中的文件不足，请检查！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }

            Image.Source = null;
            startTime = DateTime.Now; // 记录开始时间
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (display_index != -1)
            {
                switch (display_index)
                {
                    case 0:
                        AImage1.Source = Image.Source;
                        break;
                    case 1:
                        AImage2.Source = Image.Source;
                        break;
                    case 2:
                        AImage3.Source = Image.Source;
                        break;
                    case 3:
                        AImage4.Source = Image.Source;
                        break;
                    default:
                        break;
                }
            }
            Image.Source = null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Image.Source = AImage1.Source;
            AImage1.Source = null;
            display_index = 0;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Image.Source = AImage2.Source;
            AImage2.Source = null;
            display_index = 1;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Image.Source = AImage3.Source;
            AImage3.Source = null;
            display_index = 2;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Image.Source = AImage4.Source;
            AImage4.Source = null;
            display_index = 3;
        }

        private void CheckQuestionCount()
        {
            questionCount++;
            if (questionCount >= MAX_GAME) // 展示两道题目后结束
            {
                //MessageBox.Show($"总答题时间: {total_time:F2} 秒\n正确回答: {correctCount}\n错误回答: {incorrectCount}");
                //逻辑推理能力报告 nwd=new 逻辑推理能力报告(total_time/MAX_GAME,correctCount,incorrectCount);
                //nwd.Show();
                OnGameEnd();
            }
            else
            {
                ShowImage();
            }
        }

        private void ConFirm_Click(object sender, RoutedEventArgs e)
        {
            DateTime endTime = DateTime.Now; // 记录结束时间
            TimeSpan duration = endTime - startTime; // 计算作答时间

            if (display_index == 0) // 假定第5张图片对应正确答案
            {
                correctCount++; // 正确计数加1
            }
            else
            {
                incorrectCount++; // 错误计数加1
            }

            total_time += duration.TotalSeconds; // 累加总时间
            RightStatisticsAction?.Invoke(correctCount, 10);
            WrongStatisticsAction?.Invoke(incorrectCount, 10);
            CheckQuestionCount();
        }

        private void Button_Click_(object sender, RoutedEventArgs e)
        {
            OnGameEnd();
        }
    }
    public partial class 逻辑推理能力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            questionCount = 0; // 初始化题目计数
            correctCount = 0; // 初始化正确计数
            incorrectCount = 0; // 初始化错误计数
            total_time = 0; // 初始化总时间
            display_index = -1;

            // 查找目标文件夹路径
            imagePath = FindImagePath();
            if (imagePath == null)
            {
                MessageBox.Show("未找到名为“逻辑推理能力”的文件夹。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
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
            ShowImage();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
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
            return null;
        }

        private int GetCorrectNum()
        {
            return correctCount;
        }
        private int GetWrongNum()
        {
            return incorrectCount;
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
                        int count = questionCount;
                        double totalMilliseconds = totalGameTime.TotalMilliseconds;  // 转换为double类型的毫秒数
                        double time = (double)totalMilliseconds / questionCount;
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
                                        ValueName = "任务",
                                        Value = count,
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
                                    ValueName = "错误",
                                    Value = wrongCount,
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
                                    ValueName = "中央解决方案时间",
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
