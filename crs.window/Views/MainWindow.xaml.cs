using HandyControl.Data;
using System.Windows;
using crs.extension;
using System.Linq;
using System.Windows.Forms;
using System;
using HandyControl.Tools;
using System.Windows.Interop;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using static crs.extension.Crs_EventAggregator;

namespace crs.window.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        const int WM_NCHITTEST = 0x84;
        const int HTCLIENT = 1;

        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;

        HwndSource hwndSource;

        public MainWindow(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;

            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
            this.Closing += MainWindow_Closing;

            try
            {
                var screens = Screen.AllScreens;
                if (screens.Length >= 1)
                {
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    var screen = Screen.AllScreens[0];
                    this.Left = screen.Bounds.Left + (screen.WorkingArea.Width - this.Width) / 2;
                    this.Top = screen.Bounds.Top + (screen.WorkingArea.Height - this.Height) / 2;
                }
            }
            catch
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.eventAggregator.GetEvent<WindowStateChangedEvent>().Unsubscribe(WindowStateChanged);
            hwndSource?.RemoveHook(WndProc);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.eventAggregator.GetEvent<WindowStateChangedEvent>().Subscribe(WindowStateChanged);

            hwndSource ??= this.GetHwndSource();
            hwndSource?.AddHook(WndProc);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = Crs_MessageBox.Show("是否关闭程序？", button: MessageBoxButton.YesNo);
            e.Cancel = result == MessageBoxResult.No;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    // 在全屏模式下不允许拖动窗口
                    handled = true;
                    return new IntPtr(HTCLIENT);
                }
            }
            return IntPtr.Zero;
        }

        // 窗体变化
        void WindowStateChanged()
        {
            // 根据窗体句柄获取窗体所在的屏幕
            hwndSource ??= this.GetHwndSource();
            var screen = Screen.FromHandle(hwndSource.Handle);

            if (this.WindowState != WindowState.Maximized)
            {
                // 最大化
                this.Top = screen.Bounds.Top;
                this.Left = screen.Bounds.Left;
                this.Width = screen.Bounds.Width;
                this.Height = screen.Bounds.Height;

                this.ShowNonClientArea = false;
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                // 还原
                this.Width = 1280d;
                this.Height = 750d;
                this.Top = screen.WorkingArea.Top + ((screen.WorkingArea.Height - this.Height) / 2);
                this.Left = screen.WorkingArea.Left + ((screen.WorkingArea.Width - this.Width) / 2);

                this.ShowNonClientArea = true;
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }
    }
}
