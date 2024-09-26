using CloneExtensions;
using crs.core;
using crs.core.DbModels;
using crs.dialog;
using crs.extension;
using crs.theme.Extensions;
using crs.window.ViewModels;
using crs.window.Views;
using LiveChartsCore;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace crs.window
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        Window mainWindow;
        Window subWindow;

        public App()
        {
            this.Startup += App_Startup;
            this.Exit += App_Exit;

            // UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            // 非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Debug.WriteLine("UI线程未捕获异常处理事件", e.Exception.ToString());

#if DEBUG
#else
            Crs_MessageBox.Show($"UI线程未捕获异常处理事件,{e.Exception}");
#endif
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("非UI线程未捕获异常处理事件", (e.ExceptionObject as Exception).ToString());

#if DEBUG
#else
            Crs_MessageBox.Show($"非UI线程未捕获异常处理事件,{(e.ExceptionObject as Exception)}");
#endif
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Debug.WriteLine("Task线程内未捕获异常处理事件", e.Exception.ToString());

#if DEBUG
#else
            Crs_MessageBox.Show($"Task线程内未捕获异常处理事件,{e.Exception}");
#endif
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            // 绑定Dialog的日志委托
            DialogEx.WriteDialogLog = (message, ex) =>
            {
                Debug.WriteLine($"{message}.{ex}");

#if DEBUG
#else
                Crs_MessageBox.Show($"{message},{ex}");
#endif
            };
            Crs_DialogEx.ContainerProvider = this.Container;

            subWindow = Container.Resolve<SubWindow>();
            RegionManager.SetRegionManager(subWindow, Container.Resolve<IRegionManager>());
            RegionManager.UpdateRegions();

            mainWindow = Container.Resolve<MainWindow>();
            return mainWindow;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            subWindow?.Show();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册Db2Context为单例
            containerRegistry.RegisterInstance(new Crs_Db2Context());

            // 注册视图
            containerRegistry.RegisterForNavigation<Login>(Crs_View.Login); // 登录
            containerRegistry.RegisterForNavigation<Check>(Crs_View.Check); // 检测
            containerRegistry.RegisterForNavigation<Menu>(Crs_View.Menu); // 主菜单
            containerRegistry.RegisterForNavigation<UserManagement>(Crs_View.UserManagement); // 用户管理           
            containerRegistry.RegisterForNavigation<EvaluateTestEx>(Crs_View.EvaluateTest); // 评估测试
            containerRegistry.RegisterForNavigation<Train>(Crs_View.Train); // 康复训练
            containerRegistry.RegisterForNavigation<ScheduleEx>(Crs_View.Schedule); // 排班查询
            containerRegistry.RegisterForNavigation<Report>(Crs_View.Report); // 数据报告
            containerRegistry.RegisterForNavigation<DigitalHuman>(Crs_View.DigitalHuman); // 数字人管理
            containerRegistry.RegisterForNavigation<Null>(Crs_View.Null); // 空白页面
            containerRegistry.RegisterForNavigation<SubNull>(Crs_View.SubNull); // 空白页面

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            // 加载模块
            moduleCatalog.AddModule<dialogModule>(); // 弹窗
        }
    }
}
