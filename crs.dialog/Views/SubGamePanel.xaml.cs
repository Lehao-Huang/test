using crs.core.Services;
using crs.extension;
using crs.extension.Models;
using Microsoft.VisualBasic.Devices;
using System;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using static crs.core.Services.Crs_GptService;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using crs.theme.Extensions;
using System.Windows.Interop;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using crs.game;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Web.WebView2.Wpf;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for SubGamePanel
    /// </summary>
    public partial class SubGamePanel : UserControl
    {
        bool isReady = false;
        bool isPorcess = false;
        DispatcherTimer timer;
        CancellationTokenSource tokenSource;
        string lastText = null;

        //WebView2 webView;

        public SubGamePanel()
        {
            InitializeComponent();
            this.Loaded += SubGamePanel_Loaded;
            this.Unloaded += SubGamePanel_Unloaded;

            SetBinding(PatientItemProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath("PatientItem"),
                Source = this.DataContext
            });

            SetBinding(ModeTypeProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath("ModeType"),
                Source = this.DataContext
            });

            MicrophoneButton.IsEnabled = false;

            //webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.clearBrowserC0.ache", "{}");
            webView.NavigationCompleted += WebView_NavigationCompleted;
        }

        private async void SubGamePanel_Loaded(object sender, RoutedEventArgs e)
        {
            var (status, msg) = await Crs_SpeechToTextService.Instance.InitAsync();
            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                return;
            }

            MicrophoneButton.IsEnabled = true;

            Crs_SpeechToTextService.Instance.OnMessageReceivedEvent -= OnMessageReceivedEvent;
            Crs_SpeechToTextService.Instance.OnMessageReceivedEvent += OnMessageReceivedEvent;
            await Crs_SpeechToTextService.Instance.StartAsync();

            timer?.Stop();
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Crs_SpeechToTextService.Instance.AwakenTime) };
            timer.Tick += Timer_Tick;
        }

        private async void SubGamePanel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (webView != null)
            {
                string script = "document.getElementById('stop').click();";
                await this.webView.CoreWebView2.ExecuteScriptAsync(script);
                webView.Dispose();
            }

            var (status, msg) = await Crs_SpeechToTextService.Instance.StopAsync();
            if (status)
            {
                Crs_SpeechToTextService.Instance.OnMessageReceivedEvent -= OnMessageReceivedEvent;
            }

            timer?.Stop();
        }

        private async void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                string script = "document.body.style.overflow = 'hidden';";
                await this.webView.CoreWebView2.ExecuteScriptAsync(script);

                script = "document.getElementById('media').style.width='auto';";
                await this.webView.CoreWebView2.ExecuteScriptAsync(script);

                script = "document.getElementById('media').style.height='auto';";
                await this.webView.CoreWebView2.ExecuteScriptAsync(script);

                //script = "document.getElementById('media').style.backgroundColor='white';";
                //await this.webView.CoreWebView2.ExecuteScriptAsync(script); 

                var window = Window.GetWindow(this);

                var zoomX = webViewGravatar.ActualWidth / 1210d;
                var actualWidth = window.ActualWidth * zoomX;

                script = $"document.getElementById('video').style.height='{actualWidth}px';";
                await this.webView.CoreWebView2.ExecuteScriptAsync(script);

                script = "document.getElementById('video').style.width='auto';";
                await this.webView.CoreWebView2.ExecuteScriptAsync(script);

                script = "document.getElementById('start').click();";
                await this.webView.CoreWebView2.ExecuteScriptAsync(script);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer)?.Stop();

            MicrophoneButton.IsEnabled = true;
            MicrophoneNotify.Visibility = Visibility.Collapsed;
            MicrophoneTextBlock.Text = null;
            isReady = false;
            lastText = null;
        }

        private void OnMessageReceivedEvent(string text, bool isEndpoint)
        {
            App.Current.Dispatcher.Invoke(() => _OnMessageReceivedEvent(text, isEndpoint));
        }

        private async void _OnMessageReceivedEvent(string text, bool isEndpoint)
        {
            if (isPorcess)
            {
                return;
            }

            if (isReady)
            {
                timer.Stop();
                timer.Start();
                tokenSource?.Cancel();

                if (lastText == null)
                {
                    MicrophoneTextBlock.Text = text;
                }
                else
                {
                    MicrophoneTextBlock.Text = lastText + "," + text;
                }
                text = MicrophoneTextBlock.Text;
            }

            if (!isEndpoint)
            {
                return;
            }

            if (!isReady && text.Contains(Crs_SpeechToTextService.Instance.AwakenWord))
            {
                await Crs_SpeechToTextService.Instance.StopAsync();
                await Crs_SpeechToTextService.Instance.StartAsync();

                MicrophoneButton.IsEnabled = false;
                MicrophoneNotify.Visibility = Visibility.Visible;
                MicrophoneTextBlock.Text = "您请说";

                isReady = true;
                timer.Start();
                return;
            }

            if (!isReady)
            {
                return;
            }

            lastText = text;

            try
            {
                tokenSource?.Cancel();
                tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;

                await Task.Delay(Crs_SpeechToTextService.Instance.DelayTime, token);
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                isPorcess = true;
                timer.Stop();
                lastText = null;

                MicrophoneTextBlock.Text = "正在查询...";

                await Crs_SpeechToTextService.Instance.StopAsync();

                var patientItem = PatientItem;
                var modeType = ModeType;

                var parameter = new GuideParameter
                {
                    ModuleName = modeType?.ToString(),
                    PatientName = patientItem?.Name,
                    Question = text
                };

                var (status, msg, data) = await Crs_GptService.Instance.Guide(parameter);
                if (!status)
                {
                    await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(msg);
                    return;
                }

                var output = data.Value<string>("output");

                output = output.Replace("\r", "").Replace("\n", "");

                MicrophoneTextBlock.Text = output;

                string script = $"document.getElementById('message').value='{output}';";
                await this.webView.CoreWebView2.ExecuteScriptAsync(script);

                script = "document.getElementById('send').click();";
                await this.webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                await Crs_DialogEx.MessageBoxShow(Crs_DialogToken.SubTopMessageBox).GetMessageBoxResultAsync(ex.Message);
                return;
            }
            finally
            {
                await Crs_SpeechToTextService.Instance.StartAsync();

                MicrophoneTextBlock.Text = "您请说";
                isPorcess = false;
                timer.Start();
            }
        }

        private void MicrophoneButton_Click(object sender, RoutedEventArgs e)
        {
            isReady = false;
            string message = Crs_SpeechToTextService.Instance.AwakenWord;
            _OnMessageReceivedEvent(message, true);
        }

        public PatientItem PatientItem
        {
            get { return (PatientItem)GetValue(PatientItemProperty); }
            set { SetValue(PatientItemProperty, value); }
        }

        public static readonly DependencyProperty PatientItemProperty =
            DependencyProperty.Register("PatientItem", typeof(PatientItem), typeof(SubGamePanel), new PropertyMetadata(null));


        public Enum ModeType
        {
            get { return (Enum)GetValue(ModeTypeProperty); }
            set { SetValue(ModeTypeProperty, value); }
        }

        public static readonly DependencyProperty ModeTypeProperty =
            DependencyProperty.Register("ModeType", typeof(Enum), typeof(SubGamePanel), new PropertyMetadata(null));
    }
}
