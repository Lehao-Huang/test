using System;
using System.Collections.Generic;
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

namespace crs.game.Games
{
    /// <summary>
    /// 专注注意力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 专注注意力讲解 : BaseUserControl
    {
        private int left = 1;

        public 专注注意力讲解()
        {
            InitializeComponent();
            TipBlock.Text = "请从左侧三张图片当中寻找与右侧相同的图片，通过←→按键控制选中框移动，在选中框移至目标图片上后按下enter键。";
            Image1.Source = new BitmapImage(new Uri("专注注意力/1/1.png", UriKind.Relative));
            Image2.Source = new BitmapImage(new Uri("专注注意力/1/2.png", UriKind.Relative));
            Image3.Source = new BitmapImage(new Uri("专注注意力/1/3.png", UriKind.Relative));
            TargetImage.Source = new BitmapImage(new Uri("专注注意力/1/3.png", UriKind.Relative));

            this.Loaded += 专注注意力讲解_Loaded;
        }

        private void 专注注意力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            nextButton_Click(null, null);
        }

        protected override void OnHostWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool isCorrect = (left == 2);//最右边是正确答案

                if (isCorrect)
                {
                    TipBlock.FontSize = 40;
                    TipBlock.Text = "恭喜你答对了！";
                    TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                    OkButton.Visibility = Visibility.Visible;
                }
                else
                {
                    TipBlock.FontSize = 40;
                    TipBlock.Text = "很遗憾答错了！";
                    TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                    OkButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (left > 1 && e.Key == Key.Left)
                    left -= 1;
                if (left < 3 && e.Key == Key.Right)
                    left += 1;
            }
            switch (left)
            {
                case 1:
                    Border1.BorderThickness = new Thickness(2); // 设置 Border1 的边框厚度为 -2
                    Border2.BorderThickness = new Thickness(0);   // 设置 Border2 的边框厚度为 0
                    Border3.BorderThickness = new Thickness(0);   // 设置 Border3 的边框厚度为 0
                    break;
                case 2:
                    Border1.BorderThickness = new Thickness(0);   // 设置 Border1 的边框厚度为 0
                    Border2.BorderThickness = new Thickness(2);  // 设置 Border2 的边框厚度为 -2
                    Border3.BorderThickness = new Thickness(0);   // 设置 Border3 的边框厚度为 0
                    break;
                case 3:
                    Border1.BorderThickness = new Thickness(0);   // 设置 Border1 的边框厚度为 0
                    Border2.BorderThickness = new Thickness(0);   // 设置 Border2 的边框厚度为 0
                    Border3.BorderThickness = new Thickness(2);  // 设置 Border3 的边框厚度为 -2
                    break;
            }


        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        }

        int currentPage = -1;

        private void lastButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage--;
            PageSwitch();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage++;
            PageSwitch();
        }

        private void ignoreButton_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        }

        async void PageSwitch()
        {
            switch (currentPage)
            {
                case 0:
                    {
                        page_panel.Visibility = Visibility.Visible;

                        page_0.Visibility = Visibility.Visible;
                        page_1.Visibility = Visibility.Collapsed;

                        lastButton.IsEnabled = false;
                        nextButton.Content = "下一步";

                        await OnVoicePlayAsync(page_0_message.Text);
                    }
                    break;
                case 1:
                    {
                        page_panel.Visibility = Visibility.Visible;

                        page_0.Visibility = Visibility.Collapsed;
                        page_1.Visibility = Visibility.Visible;

                        lastButton.IsEnabled = true;
                        nextButton.Content = "试玩";

                        await OnVoicePlayAsync(page_1_message.Text);
                    }
                    break;
                case 2:
                    {
                        page_panel.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }
    }
}
