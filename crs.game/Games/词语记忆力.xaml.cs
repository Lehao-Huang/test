using crs.core.DbModels;
using crs.core;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace crs.game.Games
{
    /// <summary>
    /// 词语记忆力.xaml 的交互逻辑
    /// </summary>
    public partial class 词语记忆力 : BaseUserControl
    {
        /*
         * 存放游戏参数，需要coder修改，或从数据库读写
         */
        public int MemoryWordNum = 10;//要素词汇，默认5个
        public int TrainingMode = 3;//词汇难度，0简单1中等2困难3混合
        public int RunDirection = 1;//词汇运动方向，1左0右
        public int RunSpeed = 10;//速度值，1-10，越大越快
        public int TreatDurations = 1;//训练时间，单位分钟
        public int IfSet = 0;//是否勾选了训练设置，1有0没有，只有不勾选训练设置，(词汇难度、允许错几个等)参数才能根据难度等级列表来确定
        public int IfVisionFeedBack = 1;//视觉反馈，1有0没有
        public int IfAudioFeedBack = 1;//声音反馈，1有0没有
        public int IfTextFeedBack = 1;//文字反馈，1有0没有
        public int RunMode = 1;//以什么模式开始，1正式训练0只是练习
        public int SelectedDifficulty = 1;//所选择的难度等级，默认为1


        /*
         游戏逻辑处理相关函数
         */
        private void CheckIfSet()
        {//检查一下是否选中了训练设置，如果没有勾选，
            //则那些什么单词数量，单词类型都是根据难度等级列表和选中的难度等级来确定
            if (IfSet == 0)
            {//需求变更，只需要修改速度值

                //MemoryWordNum = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][1];
                //TrainingMode = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][2];
                //AllowedError = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][3];

                RunSpeed = 5;//速度值默认为5
            }
        }

        private void ShowWordsToMemorize()
        {//把需要记忆的词语显示在WordsToMemorizeTextBLock中
            string RelativePath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Words", $"{GetWordLevel(TrainingMode)}.txt");
            WordsToMemorizeList = LoadPartWords(RelativePath, MemoryWordNum);
            WordsToMemorizeTextBLock.Text = string.Join(Environment.NewLine, WordsToMemorizeList);
            AllWordList = RemoveCommonElements(LoadAllWords(RelativePath), WordsToMemorizeList);

            // 初始化记忆阶段的计时器
            MemorizeSeconds = 0;
            MemorizeTimer = new DispatcherTimer();
            MemorizeTimer.Interval = TimeSpan.FromSeconds(1); // 以秒为单位
            MemorizeTimer.Tick += MemorizeTimer_Tick;
            MemorizeTimer.Start();
        }

        private void PositionRectangle(int direction)
        {//显示红色矩形(答题区的位置)
            if (direction == 1)
            {
                Canvas.SetLeft(TargetArea, 40); // 将Rectangle移到左侧
            }
            else
            {
                double canvasWidth = WordArea.ActualWidth;
                Canvas.SetLeft(TargetArea, canvasWidth - TargetArea.Width - 40); // 将Rectangle移到右侧
            }
            TargetArea.Visibility = Visibility.Visible;
        }

        private void CreateTextBlocksOffScreen()
        {//把几个TextBlock对象先创建出来并调整好参数，包括初始化
            double canvasHeight = WordArea.ActualHeight;
            double canvasWidth = WordArea.ActualWidth;

            // Fixed width and height for each TextBlock
            double textBlockWidth = 200;

            for (int i = 0; i < NumberOfTextBlocks; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = GetRandomWord(AllWordList, WordsToMemorizeList),
                    Background = Brushes.Transparent, // 设置背景透明
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Height = canvasHeight,
                    Width = textBlockWidth,
                    FontFamily = new FontFamily("Times New Roman"), // 设置字体
                    FontSize = 100 // 设置字体大小
                };

                AdjustTextBlockWidth(textBlock);
                // Calculate vertical center position for the TextBlock
                double textBlockHeight = textBlock.FontSize * textBlock.LineHeight;
                double verticalCenterPosition = (canvasHeight - textBlockHeight) / 2;

                // Set initial position off-screen 
                double initialLeftPosition = RunDirection == 1 ? canvasWidth : -textBlockWidth;
                Canvas.SetLeft(textBlock, initialLeftPosition);
                Canvas.SetTop(textBlock, verticalCenterPosition); // 设置垂直居中位置

                // Add the TextBlock to Canvas
                WordArea.Children.Add(textBlock);

                // 将 TextBlock 的检测状态初始化为 false
                TextBlockDetected[textBlock] = false;

                //添加到列表里
                CreatedTextBlocks.Add(textBlock);  // 添加到列表
            }
        }

        private void AnimateTextBlocks(int direction, double speed)
        {//根据速度值设置每个textblock的动画
            double canvasWidth = WordArea.ActualWidth;
            double textBlockWidth = 200;
            double durationInSeconds = 11 - speed; // Speed 1 (slowest) -> 10 seconds, Speed 10 (fastest) -> 1 second

            // Calculate delay per TextBlock to avoid them starting at the same time
            double delayInterval = durationInSeconds / NumberOfTextBlocks;

            for (int i = 0; i < WordArea.Children.Count; i++)
            {
                if (WordArea.Children[i] is TextBlock textBlock)
                {
                    double from = direction == 1 ? canvasWidth : -textBlockWidth;
                    double to = direction == 1 ? -textBlockWidth : canvasWidth;

                    StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.FromSeconds(i * delayInterval));
                }
            }
        }

        private void StartTextBlockAnimation(TextBlock textBlock, double from, double to, double durationInSeconds, TimeSpan beginTime)
        {//对于每个textblock，设置并开始他们的动画
            // Create and configure the animation
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(durationInSeconds)),
                BeginTime = beginTime
            };

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, textBlock);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));

            // 每次动画结束时都更新文本内容
            storyboard.Completed += (s, e) =>
            {
                WordsAll++; // 每次有动画结束则词汇数量++
                WordsAllTemp++;
                if (WordsToMemorizeList.Contains(textBlock.Text) && !TextBlockDetected[textBlock])
                { // 更新内容之前先做判断看看是否它是需要记住的词，而且没有按键按下
                    WordsIgnore++;
                    WordsIgnoreTemp++;
                }

                textBlock.Text = GetRandomWord(AllWordList, WordsToMemorizeList);

                AdjustTextBlockWidth(textBlock); // 动态调整 TextBlock 宽度
                StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.Zero); // 重启动画
            };

            // 在启动动画前重置检测状态
            TextBlockDetected[textBlock] = false;
            TextBlockAnimations.Add(storyboard); // 将动画添加到列表
            storyboard.Begin();
        }

        private void AdjustTextBlockWidth(TextBlock textBlock)
        {//动态调整TextBlock的宽度
            // 使用 FormattedText 来测量文本宽度
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            // 动态调整 TextBlock 的宽度
            textBlock.Width = formattedText.Width;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {//倒计时开始运作并更新
            if (CountdownSeconds > 0)
            {
                CountdownSeconds--;
                TimeStatisticsAction?.Invoke((int)CountdownSeconds, 0);
                CountdownDisplay.Text = CountdownSeconds.ToString(); // 更新界面上的倒计时显示
            }
            else
            {
                CountdownTimer.Stop();
                // 手动触发结束按钮点击事件
                RoutedEventArgs args = new RoutedEventArgs();
                EndClick(this, args);
            }
        }

        // Timer的tick事件处理程序
        private void MemorizeTimer_Tick(object sender, EventArgs e)
        {
            MemorizeSeconds++;
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {// 按键检测
            // 检查按下的键是否是你指定的键
            if (e.Key == System.Windows.Input.Key.Enter) // 假设你指定的键是回车键
            {
                CheckIntersection();//看看是否有交集
                CheckRecentResult();//每次检查完交集都要检查一下最近五道题的情况
                if ((WordsError - AllowedError) < 0)
                {//错误数量超过了，不能继续玩了
                    MessageBox.Show("你已超过最大错误数量！强制结束游戏！");
                    // 模拟点击EndClick按钮
                    RoutedEventArgs args = new RoutedEventArgs();
                    EndClick(this, args);
                }
            }
        }

        private async void CheckIntersection()
        {
            // 获取 Rectangle 的边界
            double rectLeft = Canvas.GetLeft(TargetArea);
            double rectTop = Canvas.GetTop(TargetArea);
            // 检查是否为 NaN 并给予默认值
            if (double.IsNaN(rectLeft)) rectLeft = 0;
            if (double.IsNaN(rectTop)) rectTop = 0;
            // 创建 Rectangle 边界
            Rect rectangleBounds = new Rect(rectLeft, rectTop, TargetArea.Width, TargetArea.Height);
            foreach (var child in WordArea.Children)
            {
                if (child is TextBlock textBlock)
                {
                    // 如果 TextBlock 尚未被检测到
                    if (!TextBlockDetected[textBlock])
                    {
                        // 获取 TextBlock 的边界
                        double textBlockLeft = Canvas.GetLeft(textBlock);
                        double textBlockTop = Canvas.GetTop(textBlock);
                        // 检查是否为 NaN 并给予默认值
                        if (double.IsNaN(textBlockLeft)) textBlockLeft = 0;
                        if (double.IsNaN(textBlockTop)) textBlockTop = 0;
                        // 创建 TextBlock 边界
                        Rect textBlockBounds = new Rect(textBlockLeft, textBlockTop, textBlock.Width, textBlock.ActualHeight);
                        // 检查是否有重叠
                        if (rectangleBounds.IntersectsWith(textBlockBounds))
                        {
                            bool isCorrect = WordsToMemorizeList.Contains(textBlock.Text);
                            // 进行判断并更新 _ViewModel 计数器
                            if (isCorrect)
                            {
                                WordsCorrect++; // 更新正确计数
                                WordsCorrectTemp++;
                                if (IfAudioFeedBack == 1) // 是否放声音
                                {
                                    PlayWav(CorrectSound);
                                }
                                if (IfVisionFeedBack == 1)
                                {
                                    ShowFeedbackImage(CorrectImage);
                                }
                                if (IfTextFeedBack == 1)
                                {
                                    ShowFeedbackTextBlock(CorrectTextBlock); // 显示正确文本反馈
                                }
                            }
                            else
                            {
                                WordsError++; // 更新错误计数
                                WordsErrorTemp++;
                                if (IfAudioFeedBack == 1) // 是否放声音
                                {
                                    PlayWav(ErrorSound);
                                }
                                if (IfVisionFeedBack == 1)
                                {
                                    ShowFeedbackImage(ErrorImage);
                                }
                                if (IfTextFeedBack == 1)
                                {
                                    ShowFeedbackTextBlock(ErrorTextBlock); // 显示正确文本反馈
                                }
                            }
                            // 更新最近的统计
                            UpdateRecentResults(isCorrect);
                            // MessageBox.Show(textBlock.Text);
                            TextBlockDetected[textBlock] = true; // 更新检测状态

                            // 停止所有动画
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Pause();
                            }
                            // 延迟 StopDurations 毫秒
                            await Task.Delay(StopDurations);
                            // 重新启动所有动画
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Resume();
                            }

                            break; // 找到一个重叠的文本后退出循环
                        }
                    }
                }
            }
        }

        /*这里是我更新难度的地方！*/
        private void CheckRecentResult()
        {//检查一下最近五道题的状态，并作出一些调整，同时保存或者叠加上这个难度等级的数据
            if (CorrectCount == WordsRecent)
            {//说明最近五道题都答对了，需要升级难度
                //升级难度之前，把这个难度等级的计数保存一下
                if (SelectedDifficulty > AllLevelResult.Count)
                {// 选的难度等级比列表的长度还长，说明这个等级之前没保存过
                    AllLevelResult.Add(
                        new Dictionary<string, int>
                        {
                            {"单词总数",WordsAllTemp },
                            {"正确单词",WordsCorrectTemp},
                            {"错误单词",WordsErrorTemp },
                            {"忽略单词",WordsIgnoreTemp}
                        }
                        );
                }
                else
                {//说明这个等级之前的保存过，和之前保存的内容相加
                    AllLevelResult[SelectedDifficulty - 1]["单词总数"] += WordsAllTemp;
                    AllLevelResult[SelectedDifficulty - 1]["正确单词"] += WordsCorrectTemp;
                    AllLevelResult[SelectedDifficulty - 1]["错误单词"] += WordsErrorTemp;
                    AllLevelResult[SelectedDifficulty - 1]["忽略单词"] += WordsIgnoreTemp;
                }
                if (SelectedDifficulty < MaxLevel)
                {//调整等级难度
                    SelectedDifficulty += 1;//提高难度等级
                    ParameterSet();
                    BackToInit();
                    InitRecentResults();//每次更新难度等级都要重新计算最近五道题的结果
                }
            }//WordsRecent是我定义的变量，用来控制在最近的多少道题里面去观察答题情况来升降难度
            else if (IncorrectCount == WordsRecent)
            {
                //降低难度之前，把这个难度等级的计数保存一下
                if (SelectedDifficulty > AllLevelResult.Count)
                {// 选的难度等级比列表的长度还长，说明这个等级之前没保存过
                    AllLevelResult.Add(
                        new Dictionary<string, int>
                        {
                            {"单词总数",WordsAllTemp },
                            {"正确单词",WordsCorrectTemp},
                            {"错误单词",WordsErrorTemp },
                            {"忽略单词",WordsIgnoreTemp}
                        }
                        );
                }
                else
                {//说明这个等级之前的保存过，和之前保存的内容相加
                    AllLevelResult[SelectedDifficulty - 1]["单词总数"] += WordsAllTemp;
                    AllLevelResult[SelectedDifficulty - 1]["正确单词"] += WordsCorrectTemp;
                    AllLevelResult[SelectedDifficulty - 1]["错误单词"] += WordsErrorTemp;
                    AllLevelResult[SelectedDifficulty - 1]["忽略单词"] += WordsIgnoreTemp;
                }
                if (SelectedDifficulty > MinLevel)
                {
                    SelectedDifficulty -= 1;//提高难度等级
                    ParameterSet();
                    BackToInit();
                    InitRecentResults();//每次更新难度等级都要重新计算最近五道题的结果
                }
            }
            /*
            关于难度等级更新，由于飞书文档上面的难度等级表中前面五个等级相差不大，但又只有最大等级5，
            所以一开始玩起来会觉得差别不大
            如果需要，后续可以调整难度等级一次更新的跨度，让它更具差异性
             */
        }

        private void BackToInit()
        {//写这个函数是为了在不改变参数的情况下，让它回到初始状态
            InitializeComponent();
            //最开始打开窗口看到的是记忆阶段的组件
            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//记忆阶段两个组件显示
            PlayGrid.Visibility = Visibility.Collapsed;
            ShowWordsToMemorize();//让需要记忆的词汇显示
            IfHaveStarted = false;
            // 移除创建的 TextBlock 对象
            foreach (var textBlock in CreatedTextBlocks)
            {
                WordArea.Children.Remove(textBlock);
            }
            CreatedTextBlocks.Clear();  // 清空列表
            // 停止并释放之前的计时器（如果有的话）
            if (CountdownTimer != null)
            {
                CountdownTimer.Stop();
                CountdownTimer.Tick -= CountdownTimer_Tick;
                CountdownTimer = null;
            }
            InitTempResults();//把一些计数量也重置
        }

        private void ParameterSet()
        {//这是在参数修改之后，手动同步到游戏设置上面，确保修改生效
            //主要设置词汇难度，需要记忆的词汇数量
            MemoryWordNum = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][1];
            TrainingMode = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][2];
            RunSpeed = 5;//速度值默认为5
        }

        /*
        按钮触发类函数
        */

        private void MemorizeOKButtonClick(object sender, RoutedEventArgs e)
        {//点击记忆完成后，记忆阶段的组件就应该隐藏掉
            WordsToMemorizeTextBLock.Visibility = Visibility.Collapsed;
            MemorizeOKButton.Visibility = Visibility.Collapsed;//记忆阶段两个组件消失
            PlayGrid.Visibility = Visibility.Visible;//训练阶段的组件都归到这个grid中，让他显示

            // 停止计时器
            if (MemorizeTimer != null && MemorizeTimer.IsEnabled)
            {
                MemorizeTimer.Stop();
                MemorizeTimer.Tick -= MemorizeTimer_Tick;
                MemorizeTimer = null;
                //此时MemorizeSeconds就是记忆的秒数
            }
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            if (IfHaveStarted == false)
            {
                CreateTextBlocksOffScreen();//创建出三个textblock
                PositionRectangle(RunDirection);
                AnimateTextBlocks(RunDirection, (double)RunSpeed);

                if (CountdownTimer != null)
                {// 停止并释放之前的计时器（如果有的话）
                    CountdownTimer.Stop();
                    CountdownTimer.Tick -= CountdownTimer_Tick;
                    CountdownTimer = null;
                }
                // 设置倒计时并启动计时器
                CountdownSeconds = TreatDurations * 60; // 重新设置倒计时时间
                CountdownTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1) // 计时器每秒触发一次
                };
                CountdownTimer.Tick += CountdownTimer_Tick;
                CountdownDisplay.Text = CountdownSeconds.ToString();
                CountdownTimer.Start();

                LoadDataFromXml();//再从本地把内容load进去

            }
            IfHaveStarted = true;
        }

        private void EndClick(object sender, RoutedEventArgs e)
        {
            if (SettingTable.Rows.Count > 0)
            {
                int lastSettingNumber = (int)SettingTable.Rows[SettingTable.Rows.Count - 1][0];
                TreatNum = lastSettingNumber + 1;//更新一下治疗的序号
            }
            AddNewRowAndSave(SettingTable, ResultTable);//把内存当中的两个结果表格加一行(这一次玩的一行)并保存到本地
            if (RunMode == 1)
            {//说明是正式训练模式，需要打开report窗口
                //ReportWindow reportWindow = new ReportWindow(SettingTable, ResultTable);//顺便把两个table传过去显示
                OnGameEnd();
            }
            else
            {//说明只是训练模式，回到最初的样子就好
                BackToInit();
            }
        }
        /*
         存放临时变量，不需要coder修改
         */
        private List<string> AllWordList = new List<string>();//从文件中读取到的除需要记忆的单词以外，所有的单词列表，用来动画显示
        private List<string> WordsToMemorizeList = new List<string>();//存放需要记住的单词列表
        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
        private string ResourcesPath = System.IO.Path.Combine("Resources", "词语记忆力");
        private bool IfHaveStarted = false;//是否已经点击过开始按钮
        private int NumberOfTextBlocks = 3;//要创建的TextBlock的数量，默认三个
        private Dictionary<TextBlock, bool> TextBlockDetected = new Dictionary<TextBlock, bool>(); // 初始化检测状态字典; // 存储每个 TextBlock 的检测状态
        private List<TextBlock> CreatedTextBlocks = new List<TextBlock>();//用来存放创建的textblock对象
        private Random RandomObject = new Random();//随机对象
        private List<Storyboard> TextBlockAnimations = new List<Storyboard>(); // 列表存储所有动画
        private double CountdownSeconds = 0;//计时器的计时数
        private DispatcherTimer CountdownTimer;//计时器对象
        private DispatcherTimer MemorizeTimer;//记忆阶段的计时器对像
        private int MemorizeSeconds;//记忆阶段的用时
        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSound;//错误的声音
        public string CorrectSound;//正确的声音
        private int StopDurations = 2000; // 停止时间，ms
        private DataTable LevelTableDifficultyLevel = new DataTable();//等级列表参数
        private DataTable SettingTable = new DataTable();//存储所设置的参数的表格
        private DataTable ResultTable = new DataTable();//存储所得到的结果的表格
        private int TreatNum = 0;//治疗编号，说明是从第几次开始的治疗训练
        private string DateNow = DateTime.Now.ToString("yyyy/M/d"); // 获取当前日期
        public int AllowedError = -1;//允许错误的单词数量，-1为不限制错误数量，需求好像没有体现，就不限制了

        private int WordsRecent = 5;//在最近五道题里面观察状态来升降难度
        private Queue<bool> RecentResults = new Queue<bool>();//定义一个队列，用来存放最近五道题的答对答错状态
        private int CorrectCount = 0;//最近五道题中正确的数目
        private int IncorrectCount = 0;//最近五道题中错误的数目

        private int MaxLevel = 5;//最高难度等级
        private int MinLevel = 1;//最低难度等级

        //由于存放的是列表，所以难度等级只能从1往上升，否则索引时顺序会不对应，后期再修改
        private List<Dictionary<string, int>> AllLevelResult = new List<Dictionary<string, int>>();//一个列表，存放各个难度等级下的游戏结果，游戏结果以字典形式储存，有单词总数，正确数等等。。。
        private int WordsAllTemp = 0;//单一个难度等级中飘过去的单词总数
        private int WordsIgnoreTemp = 0;//单一个难度等级中飘过去的单词中，忽略了的单词数
        private int WordsCorrectTemp = 0;//单一个难度等级中飘过去的单词中，正确的单词数
        private int WordsErrorTemp = 0;//单一个难度等级中飘过去的单词中，错误的单词数

        //声明存放结果的数组
        private int[] correctAnswers;//先不说长度多少
        private int[] wrongAnswers;
        private int[] igonreAnswer;

        //这一堆是整个游戏过程中的数据
        private int WordsAll = 0;//飘过去的单词总数
        private int WordsIgnore = 0;//飘过去的单词中，忽略了的单词数
        private int WordsCorrect = 0;//飘过去的单词中，正确的单词数
        private int WordsError = 0;//飘过去的单词中，错误的单词数

        /*
         存放功能函数，不需要coder修改
         */

        private List<string> LoadAllWords(string FileName)
        {//把本地所有单词都加载出来
            Encoding encoding = Encoding.UTF8;
            List<string> words = new List<string>();

            // 首先读取文件中的所有行
            using (StreamReader file = new StreamReader(FileName))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    words.Add(line.Trim());
                }
            }
            return words;
        }

        private List<string> LoadPartWords(string filename, int count)
        {//从本地读取部分单词出来
            Encoding encoding = Encoding.UTF8;
            List<string> words = new List<string>();

            // 首先读取文件中的所有行
            List<string> allLines = new List<string>();
            using (StreamReader file = new StreamReader(filename))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    allLines.Add(line.Trim());
                }
            }

            // 使用当前时间作为种子
            Random random = new Random();
            int linesToRead = Math.Min(count, allLines.Count);

            // 随机选择不重复的行
            for (int i = 0; i < linesToRead; i++)
            {
                int index = random.Next(allLines.Count);
                words.Add(allLines[index]);
                allLines.RemoveAt(index); // 确保不会重复选中同一行
            }
            return words;
        }

        public List<string> RemoveCommonElements(List<string> A, List<string> B)
        {//A-B
            // 使用 Except 方法获取 A 中不存在于 B 的元素
            List<string> result = A.Except(B).ToList();
            return result;
        }

        static private string GetWordLevel(int TrainingMode = 0)
        {//根据TrainingMode的选择返回词汇的难度字符串
            switch (TrainingMode)
            {
                case 0: return "Easy";
                case 1: return "Medium";
                case 2: return "Hard";
                case 3: return "Hybrid";
                default: return "Easy";
            }
        }

        private string GetRandomWord(List<string> AllWordList, List<string> WordsToMemorizeList)
        {//从两个列表中随机抽取出来，随机从AllWordList和WordsToMemorizeList两个list中随机挑
            // 确保两个列表都已经加载
            if ((AllWordList == null || AllWordList.Count == 0) && (WordsToMemorizeList == null || WordsToMemorizeList.Count == 0))
            {
                return "No Words Loaded";
            }
            // 随机选择列表，0 表示 WordList，1 表示 _ViewModel.WordsToMemorizeList
            int listSelector = RandomObject.Next(0, 2);
            List<string> selectedList;
            if (listSelector == 0 && AllWordList != null && AllWordList.Count > 0)
            {
                selectedList = AllWordList;
            }
            else if (WordsToMemorizeList != null && WordsToMemorizeList.Count > 0)
            {
                selectedList = WordsToMemorizeList;
            }
            else if (AllWordList != null && AllWordList.Count > 0)
            {
                // 如果上面都不符合条件，那就使用剩下的非空列表
                selectedList = AllWordList;
            }
            else
            {
                return "No Words Loaded";
            }

            // 从选择的列表中随机选取一个元素
            int index = RandomObject.Next(selectedList.Count);
            return selectedList[index];
        }

        private void PlayWav(string filePath)
        {//播放本地的wav文件
            if (soundPlayer != null)
            {
                soundPlayer.Stop();
                soundPlayer.Dispose();
            }

            soundPlayer = new SoundPlayer(filePath);
            soundPlayer.Load();
            soundPlayer.Play();
        }

        private async void ShowFeedbackImage(Image image)
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如2秒）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private async void ShowFeedbackTextBlock(TextBlock textBlock)
        {
            textBlock.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如2秒）
            await Task.Delay(StopDurations);

            textBlock.Visibility = Visibility.Collapsed;
        }

        public DataTable DifficultyLevelInit()
        {//把等级列表构造出来
            DataTable LevelTable = new DataTable();

            LevelTable.Columns.Add("等级", typeof(int));
            LevelTable.Columns.Add("单词数量", typeof(int));
            LevelTable.Columns.Add("单词类型", typeof(int));
            LevelTable.Columns.Add("允许错误数量", typeof(int));

            LevelTable.Rows.Add(1, 1, 0, 0);
            LevelTable.Rows.Add(2, 1, 1, 0);
            LevelTable.Rows.Add(3, 1, 2, 0);
            LevelTable.Rows.Add(4, 2, 0, 0);
            LevelTable.Rows.Add(5, 2, 1, 0);
            LevelTable.Rows.Add(6, 2, 2, 0);
            LevelTable.Rows.Add(7, 3, 0, 0);
            LevelTable.Rows.Add(8, 3, 1, 0);
            LevelTable.Rows.Add(9, 3, 2, 0);
            LevelTable.Rows.Add(10, 4, 0, 0);
            LevelTable.Rows.Add(11, 4, 1, 0);
            LevelTable.Rows.Add(12, 4, 2, 0);
            LevelTable.Rows.Add(13, 5, 0, 1);
            LevelTable.Rows.Add(14, 5, 1, 1);
            LevelTable.Rows.Add(15, 5, 2, 1);
            LevelTable.Rows.Add(16, 6, 0, 1);
            LevelTable.Rows.Add(17, 6, 1, 1);
            LevelTable.Rows.Add(18, 6, 2, 1);
            LevelTable.Rows.Add(19, 7, 2, 1);
            LevelTable.Rows.Add(20, 7, 1, 1);
            LevelTable.Rows.Add(21, 7, 2, 1);
            LevelTable.Rows.Add(22, 8, 0, 1);
            LevelTable.Rows.Add(23, 8, 1, 1);
            LevelTable.Rows.Add(24, 8, 2, 1);
            LevelTable.Rows.Add(25, 9, 0, 2);
            LevelTable.Rows.Add(26, 9, 1, 2);
            LevelTable.Rows.Add(27, 9, 2, 2);
            LevelTable.Rows.Add(28, 10, 0, 2);
            LevelTable.Rows.Add(29, 10, 1, 2);
            LevelTable.Rows.Add(30, 10, 2, 2);
            return LevelTable;
        }

        public DataTable SettingTableInit()
        {
            SettingTable = new DataTable();
            // 初始化 SettingTable 并定义列
            SettingTable = new DataTable("SettingsTable");
            SettingTable.Columns.Add("治疗编号", typeof(int));
            SettingTable.Columns.Add("日期", typeof(string));
            SettingTable.Columns.Add("速度", typeof(int));
            SettingTable.Columns.Add("反馈文字", typeof(string));
            SettingTable.Columns.Add("反馈听觉", typeof(string));
            SettingTable.Columns.Add("反馈视觉", typeof(string));
            SettingTable.Columns.Add("要素词汇", typeof(int));
            SettingTable.Columns.Add("练习模式", typeof(string));
            return SettingTable;
        }

        public DataTable ResultTableInit()
        {
            // 初始化 SettingTable 并定义列
            ResultTable = new DataTable("ResultTable");
            ResultTable.Columns.Add("治疗编号", typeof(int));
            ResultTable.Columns.Add("日期", typeof(string));
            ResultTable.Columns.Add("难度等级等级", typeof(int));
            ResultTable.Columns.Add("总数词汇", typeof(int));
            ResultTable.Columns.Add("错误词汇", typeof(int));
            ResultTable.Columns.Add("忽略词汇", typeof(int));
            return ResultTable;
        }

        public string TrainingModeToString(int TrainingMode)
        {//把TrainingMode转换成字符串输入，方便填到datatable里面
            switch (TrainingMode)
            {
                case 0: return "简单词汇";
                case 1: return "中等词汇";
                case 2: return "困难词汇";
                case 3: return "混合词汇";
                default: return "简单词汇";
            }
        }

        public void AddNewRowAndSave(DataTable SettingTable, DataTable ResultTable)
        {//给两个table根据目前的变量值，给他们加一行
            // 添加新行到 SettingTable
            SettingTable.Rows.Add(
                TreatNum,
                DateNow,
                RunSpeed,
                (IfTextFeedBack == 1) ? "开" : "关",
                (IfAudioFeedBack == 1) ? "开" : "关",
                (IfVisionFeedBack == 1) ? "开" : "关",
                MemoryWordNum,
                TrainingModeToString(TrainingMode)
            );

            // 添加新行到 ResultTable
            ResultTable.Rows.Add(
                TreatNum,
                DateNow,
                SelectedDifficulty,
                WordsAll,
                WordsError,
                WordsIgnore
            );

            // 保存回 XML 文件
            SaveDataToXml();
        }

        private string GetTablesFilePath(string fileName)
        {//根据文件名获取xml文件的路径
            string RelativePath = System.IO.Path.Combine(BaseDirectory, "Recourses", "ResultTables", $"{fileName}");
            return RelativePath;

        }

        public void LoadDataFromXml()
        {//把xml从本地load进来
            string settingFilePath = GetTablesFilePath("settings.xml");
            string resultFilePath = GetTablesFilePath("results.xml");

            if (File.Exists(settingFilePath))
            {
                SettingTable.ReadXml(settingFilePath);
            }
            if (File.Exists(resultFilePath))
            {
                ResultTable.ReadXml(resultFilePath);
            }
        }

        public void SaveDataToXml()
        {//把两个xml保存到本地
            string settingFilePath = GetTablesFilePath("settings.xml");
            string resultFilePath = GetTablesFilePath("results.xml");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(settingFilePath)); // 确保目录存在
            SettingTable.WriteXml(settingFilePath);
            ResultTable.WriteXml(resultFilePath);
        }

        private void UpdateRecentResults(bool isCorrect)
        {//更新最近五个单词的情况
            // 添加新记录到队列
            RecentResults.Enqueue(isCorrect);

            // 如果队列中的记录超过 5 个，则移除最早的记录
            if (RecentResults.Count > WordsRecent)
            {
                RecentResults.Dequeue();
            }
            // 统计最近 5 个单词的正确和错误数量
            CorrectCount = RecentResults.Count(item => item);  // 统计正确的数量
            IncorrectCount = RecentResults.Count - CorrectCount;    // 统计错误的数量
            RightStatisticsAction?.Invoke(CorrectCount, 5);
            WrongStatisticsAction?.Invoke(IncorrectCount, 5);
            LevelStatisticsAction?.Invoke(SelectedDifficulty, 30);
        }

        private void InitRecentResults()
        {//把最近的结果初始化一下
            RecentResults.Clear();
            CorrectCount = 0;
            IncorrectCount = 0;
        }

        private void InitTempResults()
        {//在切换难度等级后一些量要重置
            WordsAllTemp = 0;//单一个难度等级中飘过去的单词总数
            WordsIgnoreTemp = 0;//单一个难度等级中飘过去的单词中，忽略了的单词数
            WordsCorrectTemp = 0;//单一个难度等级中飘过去的单词中，正确的单词数
            WordsErrorTemp = 0;//单一个难度等级中飘过去的单词中，错误的单词数
        }

        private void AllLevelResultToArray()
        {
            int Length = AllLevelResult.Count;//这个是指一共玩了多少个等级
            correctAnswers = new int[Length];
            wrongAnswers = new int[Length];
            igonreAnswer = new int[Length];

            for (int i = 0;i< Length; i++)//遍历所有难度等级
            {//i实际上就是难度等级-1
                Dictionary<string, int> LevelResult = AllLevelResult[i];
                correctAnswers[i] = LevelResult["正确单词"];
                wrongAnswers[i] = LevelResult["错误单词"];
                igonreAnswer[i] = LevelResult["忽略单词"];
            }
        }
         
    }    
         
    public partial class 词语记忆力 : BaseUserControl
    {
        public 词语记忆力()
        {
            InitializeComponent();
        }

        protected override async Task OnInitAsync()
        {
            ////最开始打开窗口看到的是记忆阶段的组件
            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//记忆阶段两个组件显示
            PlayGrid.Visibility = Visibility.Collapsed;
            LevelTableDifficultyLevel = DifficultyLevelInit();
            SettingTable = SettingTableInit();
            ResultTable = ResultTableInit();//一来就要初始化好这些表格


            CorrectSound = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSound = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

            bool IfEasy = false;//是否是简单词汇，临时变量，用来和数据库交互，不用修改
            bool IfMedium = false;
            bool IfHard = false;
            bool IfHybrid = false;
            bool Left = false;//词汇向左运动，临时变量，用来和数据库交互，不用修改
            bool Right = false;

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
                            case 70: // 治疗时间 
                                TreatDurations = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TreatDurations={TreatDurations}");
                                break;
                            case 71: // 要素词汇
                                MemoryWordNum = par.Value.HasValue ? (int)par.Value.Value : 10;
                                Debug.WriteLine($"MemoryWordNum={MemoryWordNum}");
                                break;
                            case 72: // 简单词汇
                                IfEasy = par.Value.HasValue ? (par.Value.Value !=0) : false;
                                Debug.WriteLine($"IfEasy ={IfEasy}");
                                break;
                            case 73: // 中等难度词汇
                                IfMedium = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                Debug.WriteLine($"IfMedium ={IfMedium}");
                                break;
                            case 74: // 困难词汇
                                IfHard = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                Debug.WriteLine($"IfHard ={IfHard}");
                                break;
                            case 75: // 混合词汇
                                IfHybrid = par.Value.HasValue ? (par.Value.Value != 0) : true;
                                Debug.WriteLine($"IfHybrid ={IfHybrid}");
                                break;
                            case 76://听觉反馈
                                IfAudioFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"IfAudioFeedBack ={IfAudioFeedBack}");
                                break;
                            case 248://文字反馈
                                IfTextFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"IfTextFeedBack ={IfTextFeedBack}");
                                break;
                            case 77://视觉反馈
                                IfVisionFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"IfVisionFeedBack ={IfVisionFeedBack}");
                                break;
                            case 78://训练设置
                                IfSet = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"IfSet ={IfSet}");
                                break;
                            case 79://词汇向左运动
                                Left = par.Value.HasValue ? (par.Value.Value != 0) : true;
                                Debug.WriteLine($"Left ={Left}");
                                break;
                            case 80://词汇向右运动
                                Right = par.Value.HasValue ? (par.Value.Value != 0) : true;
                                Debug.WriteLine($"Right ={Right}");
                                break;
                            case 247://单词运动速度
                                RunSpeed = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"RunSpeed ={RunSpeed}");
                                break;
                            case 158://难度等级
                                SelectedDifficulty = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"SelectedDifficulty ={SelectedDifficulty}");
                                break;
                            // 添加其他需要处理的 ModuleParId
                            default:
                                Debug.WriteLine($"未处理的 ModuleParId: {par.ModuleParId}");
                                break;
                        }

                        //整理一下临时变量
                        if ((bool)Left == true && (bool)Right == false){RunDirection = 1; }//词汇向左运动
                        else if ((bool)Left == true && (bool)Right == false){RunDirection = 0;}
                        else{RunDirection = 1;}//强制向左运动，保证默认值存在

                        if ((bool)IfEasy == true && (bool)IfMedium == false && (bool)IfHard == false && (bool)IfHybrid == false){TrainingMode = 0;}
                        else if ((bool)IfEasy == false && (bool)IfMedium == true && (bool)IfHard == false && (bool)IfHybrid == false){TrainingMode = 1;}
                        else if ((bool)IfEasy == false && (bool)IfMedium == false && (bool)IfHard == true && (bool)IfHybrid == false){TrainingMode = 2;}
                        else if ((bool)IfEasy == false && (bool)IfMedium == false && (bool)IfHard == false && (bool)IfHybrid == true){TrainingMode = 3;}
                        else{TrainingMode = 0;}//强制为简单词汇，保证默认值存在
                    }
                }
            }
            else
                {
                    Debug.WriteLine("没有数据");
                }
        


            // 调用委托
            LevelStatisticsAction?.Invoke(1,30);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {
            // 重新启动所有动画
            foreach (var storyboard in TextBlockAnimations)
            {
                storyboard.Resume();
            }
            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//记忆阶段两个组件显示
            PlayGrid.Visibility = Visibility.Collapsed;
            CheckIfSet();//在显示需要记忆的单词之前就得检查一下参数设置
            ShowWordsToMemorize();//让需要记忆的词汇显示

            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnStopAsync()
        {
            CountdownTimer?.Stop();
            //我的函数
            AllLevelResultToArray();
        }

        protected override async Task OnPauseAsync()
        {
            CountdownTimer?.Stop(); 
            foreach (var storyboard in TextBlockAnimations)
            {
                storyboard.Pause();
            }
        }

        protected override async Task OnNextAsync()
        {
            // 重新启动所有动画
            foreach (var storyboard in TextBlockAnimations)
            {
                storyboard.Resume();
            }
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
                        for (int lv = 0; lv < AllLevelResult.Count; lv++)
                        {

                            int correctCount = GetCorrectNum(lv);
                            int wrongCount = GetWrongNum(lv);
                            int ignoreCount = GetIgnoreNum(lv);

                            if (correctCount == 0 && wrongCount == 0 && ignoreCount == 0)
                            {
                                // 如果所有数据都为0，跳过此难度级别
                                Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
                                continue;
                            }

                            // 计算准确率
                            double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                            Debug.WriteLine("这里进行Result插入1");

                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "词汇注意力",
                                Lv = lv, // 当前的难度级别
                                Eval = false,
                                ScheduleId = BaseParameter.ScheduleId ?? null, // 假设的 schedule_id，可以替换为实际值

                            };
                            Debug.WriteLine($"截止");
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
                                    ValueName = "难度等级",
                                    Value = SelectedDifficulty,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "总数词汇",
                                    Value = WordsAll,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误",
                                    Value = WordsError,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "忽略",
                                    Value = WordsIgnore, 
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "采集时间",
                                    Value = MemorizeSeconds,
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
