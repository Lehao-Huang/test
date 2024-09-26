using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// VERB_Answerwindow.xaml 的交互逻辑
    /// </summary>
    public partial class VERB_Answerwindow : Window
    {
        // 定义事件
        public event Action<int> TimeUpdated;
        public event Action<int, int> ResultReturned;
        public event Action<int, int> ResultUpdated;

        private DispatcherTimer timer;
        private Stopwatch stopwatch;
        private int countdown;
        private Question currentQuestion;
        private int currentQuestionGroupIndex = 0; // 当前展示的 QuestionGroup 索引
        private int correctAnswersCount = 0;
        private int wrongAnswersCount = 0;

        public VERB_Answerwindow(Question question)
        {
            InitializeComponent();
            this.currentQuestion = question;
            DisplayQuestionGroup(currentQuestionGroupIndex);
            StartCountdown();
        }
        public void StopTimer()
        {
            timer?.Stop();
            stopwatch?.Stop();
        }
        private void DisplayQuestionGroup(int index)
        {
            if (index < currentQuestion.QuestionGroups.Count)
            {
                Dispatcher.Invoke(() =>
                {
                    TitleTextBlock.Text = currentQuestion.MaterialTitle;
                    QuestionTextBlock.Text = currentQuestion.QuestionGroups[index].Content;
                    Option1Button.Content = currentQuestion.QuestionGroups[index].Options[0];
                    Option2Button.Content = currentQuestion.QuestionGroups[index].Options[1];
                    Option3Button.Content = currentQuestion.QuestionGroups[index].Options[2];
                    Option4Button.Content = currentQuestion.QuestionGroups[index].Options[3];
                });
            }
            else
            {
                // 所有问题组展示完毕
                return;
            }
        }

        private void StartCountdown()
        {
            countdown = 10; // 设置倒计时时间
            Dispatcher.Invoke(() => CountdownTextBlock.Text = countdown.ToString());
            stopwatch = Stopwatch.StartNew();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100); // 每100毫秒更新一次
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            countdown = 10 - (int)stopwatch.Elapsed.TotalSeconds;
            CountdownTextBlock.Text = countdown.ToString();

            TimeUpdated?.Invoke(countdown);
            if (countdown <= 0)
            {
                timer.Stop();
                stopwatch.Stop();
                // 提交答案或处理超时
                wrongAnswersCount++;
                MoveToNextQuestionGroup();
            }
        }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int selectedIndex = int.Parse(clickedButton.Tag.ToString());
            bool isCorrect = selectedIndex == currentQuestion.QuestionGroups[currentQuestionGroupIndex].CorrectOptionIndex;

            if (isCorrect)
            {
                correctAnswersCount++;
                //MessageBox.Show("正确答案！");
            }
            else
            {
                wrongAnswersCount++;
                //MessageBox.Show("错误答案！");
            }

            ResultUpdated?.Invoke(correctAnswersCount, wrongAnswersCount);
            // 移动到下一个问题组
            MoveToNextQuestionGroup();
        }

        private void MoveToNextQuestionGroup()
        {
            // 停止计时器
            timer.Stop();
            stopwatch.Stop();

            // 更新索引并展示下一个问题组
            currentQuestionGroupIndex++;

            if (currentQuestionGroupIndex < currentQuestion.QuestionGroups.Count)
            {
                DisplayQuestionGroup(currentQuestionGroupIndex);
                StartCountdown(); // 重新开始计时
            }
            else
            {
                // 所有问题组展示完毕
                ResultReturned?.Invoke(correctAnswersCount, wrongAnswersCount); // 触发事件
                this.Close(); // 关闭窗口或执行其他操作
            }
        }

    }
}
