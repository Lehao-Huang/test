using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using crs.game;
using crs.game.Games;
using crs.theme.Extensions;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static crs.extension.Crs_Enum;
using static crs.extension.Crs_Interface;
using MessageBoxButton = crs.theme.Extensions.MessageBoxButton;

namespace crs.dialog.ViewModels
{
    public class EvaluateGamePanelViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<int, DigitalHumanItem>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        int? programId;

        public EvaluateGamePanelViewModel() { }
        public EvaluateGamePanelViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
        private IGameHost gameHost;
        public IGameHost GameHost
        {
            get { return gameHost; }
            set { SetProperty(ref gameHost, value); }
        }

        private IGameBase gameBase;
        public IGameBase GameBase
        {
            get { return gameBase; }
            set { SetProperty(ref gameBase, value); }
        }

        private string voiceTipContent;
        public string VoiceTipContent
        {
            get { return voiceTipContent; }
            set { SetProperty(ref voiceTipContent, value); }
        }

        private DateTime? totalCountdownTime;
        public DateTime? TotalCountdownTime
        {
            get { return totalCountdownTime; }
            set { SetProperty(ref totalCountdownTime, value); }
        }

        private DateTime? currentCountdownTime;
        public DateTime? CurrentCountdownTime
        {
            get { return currentCountdownTime; }
            set { SetProperty(ref currentCountdownTime, value); }
        }

        private ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>> programItems;
        public ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>> ProgramItems
        {
            get { return programItems; }
            set { SetProperty(ref programItems, value); }
        }

        private ProgramItem<EvaluateTestMode, EvaluateTestItem> programSelectedItem;
        public ProgramItem<EvaluateTestMode, EvaluateTestItem> ProgramSelectedItem
        {
            get { return programSelectedItem; }
            set { SetProperty(ref programSelectedItem, value); }
        }

        private DigitalHumanItem digitalHumanItem;
        public DigitalHumanItem DigitalHumanItem
        {
            get { return digitalHumanItem; }
            set { SetProperty(ref digitalHumanItem, value); }
        }

        private PatientItem patientItem;
        public PatientItem PatientItem
        {
            get { return patientItem; }
            set { SetProperty(ref patientItem, value); }
        }

        private bool gameStatus;
        public bool GameStatus
        {
            get { return gameStatus; }
            set { SetProperty(ref gameStatus, value); }
        }
        #endregion

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        async void ExecuteCancelCommand()
        {
            if (await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估还未完成，是否退出？", button: MessageBoxButton.OKOrCancel) == null)
            {
                return;
            }

            var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
            if (program != null)
            {
                program.ActEndTime = DateTime.Now;
                db.Programs.Update(program);
                db.SaveChanges();
            }

            var gameBase = GameBase;
            await (gameBase?.StopAsync() ?? Task.CompletedTask);

            var gameHost = GameHost;
            gameHost?.Remove(gameBase);
            (gameHost as IDialogResultable<object>)?.CloseAction();

            CloseAction?.Invoke();
            GameStatus = false;
        }

        private DelegateCommand exampleCommand;
        public DelegateCommand ExampleCommand =>
            exampleCommand ?? (exampleCommand = new DelegateCommand(ExecuteExampleCommand));

        async void ExecuteExampleCommand()
        {
            var gameBase = GameBase;
            var explanationExample = gameBase?.GetExplanationExample();

            if (explanationExample == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("讲解示例未实现");
                return;
            }

            explanationExample.GameBeginAction = ExecuteStartCommand;

            var selectedItem = ProgramSelectedItem;

            var gameHost = GameHost;
            gameHost?.ShowDemoInfo(explanationExample as FrameworkElement, selectedItem?.Mode.ToString());
        }

        private DelegateCommand exampleIgnoreCommand;
        public DelegateCommand ExampleIgnoreCommand =>
            exampleIgnoreCommand ?? (exampleIgnoreCommand = new DelegateCommand(ExecuteExampleIgnoreCommand));

        void ExecuteExampleIgnoreCommand()
        {
            ExecuteStartCommand();
        }

        private DelegateCommand startCommand;
        public DelegateCommand StartCommand =>
            startCommand ?? (startCommand = new DelegateCommand(ExecuteStartCommand));

        async void ExecuteStartCommand()
        {
            var gameHost = GameHost;
            gameHost?.ShowDemoInfo(null, null);

            var gameBase = GameBase;
            var control = gameBase as UserControl;
            if (control != null) control.IsEnabled = true;

            gameHost?.Show(gameBase);
            await (gameBase?.StartAsync() ?? Task.CompletedTask);
            GameStatus = true;
        }

        private DelegateCommand stopCommand;
        public DelegateCommand StopCommand =>
            stopCommand ?? (stopCommand = new DelegateCommand(ExecuteStopCommand));

        async void ExecuteStopCommand()
        {
            bool? result;
            if ((result = await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估还未完成，是否中断评估并生成报告？", button: MessageBoxButton.CustomReport)) == null)
            {
                return;
            }

            var gameBase = GameBase;
            var gameHost = GameHost;

            if (result.Value)
            {
                await (gameBase?.StopAsync() ?? Task.CompletedTask);
                await (gameBase?.ReportAsync() ?? Task.CompletedTask);
                gameHost?.Remove(gameBase);
                GameStatus = false;

                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("中断并生成报告成功");
            }
            else
            {
                await (gameBase?.StopAsync() ?? Task.CompletedTask);
                gameHost?.Remove(gameBase);
                GameStatus = false;
            }

            await TryProgramSelectedChanged();
        }

        private DelegateCommand pauseCommand;
        public DelegateCommand PauseCommand =>
            pauseCommand ?? (pauseCommand = new DelegateCommand(ExecutePauseCommand));

        async void ExecutePauseCommand()
        {
            var gameBase = GameBase;
            var control = gameBase as UserControl;
            if (control != null) control.IsEnabled = false;

            await (gameBase?.PauseAsync() ?? Task.CompletedTask);
            GameStatus = false;
        }

        private DelegateCommand resetCommand;
        public DelegateCommand ResetCommand =>
            resetCommand ?? (resetCommand = new DelegateCommand(ExecuteResetCommand));

        async void ExecuteResetCommand()
        {
            var gameBase = GameBase;
            await (gameBase?.StopAsync() ?? Task.CompletedTask);

            var gameHost = GameHost;
            gameHost?.Remove(gameBase);
            GameStatus = false;

            GameClear();
            await Task.Yield();

            await GameInit();
            ExecuteStartCommand();
        }

        private DelegateCommand nextCommand;
        public DelegateCommand NextCommand =>
            nextCommand ?? (nextCommand = new DelegateCommand(ExecuteNextCommand));

        async void ExecuteNextCommand()
        {
            var gameBase = GameBase;
            await (gameBase?.NextAsync() ?? Task.CompletedTask);
        }

        async Task TryProgramSelectedChanged()
        {
            do
            {
                GameClear();
                await Task.Yield();

                var selectedItem = await ProgramSelectedChanged();
                if (selectedItem == null)
                {
                    var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
                    if (program != null)
                    {
                        program.ActEndTime = DateTime.Now;
                        db.Programs.Update(program);
                        db.SaveChanges();
                    }

                    await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估结束");

                    var gameHost = GameHost;
                    (gameHost as IDialogResultable<object>)?.CloseAction();

                    CloseAction?.Invoke();
                    GameStatus = false;
                    return;
                }
            } while (!await GameInit());

            async Task<ProgramItem<EvaluateTestMode, EvaluateTestItem>> ProgramSelectedChanged()
            {
                var items = ProgramItems;
                if (items == null)
                {
                    return null;
                }

                var selectedItem = ProgramSelectedItem;
                ProgramSelectedItem = null;

                await Task.Yield();

                var _items = items.ToList();
                if (selectedItem == null)
                {
                    selectedItem = _items.FirstOrDefault();
                    ProgramSelectedItem = selectedItem;
                }
                else
                {
                    if (selectedItem == _items.LastOrDefault())
                    {
                        selectedItem = null;
                        ProgramSelectedItem = null;
                    }
                    else
                    {
                        for (int index = 0; index < _items.Count; index++)
                        {
                            var item = _items[index];
                            if (item == selectedItem)
                            {
                                selectedItem = _items[index + 1];
                                ProgramSelectedItem = selectedItem;
                                break;
                            }
                        }
                    }
                }

                return selectedItem;
            }
        }

        async Task<bool> GameInit()
        {
            var selectedItem = ProgramSelectedItem;
            if (selectedItem == null)
            {
                return false;
            }

            var assembly = typeof(IGameBase).Assembly;
            var type = assembly?.GetType($"crs.game.Games.{selectedItem.Mode}");
            if (type == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”未实现");
                return false;
            }

            if (type.BaseType != typeof(BaseUserControl) && type.BaseType != typeof(UserControl))
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”不是“BaseUserControl类型”或“UserControl类型”");
                return false;
            }

            var constructorInfo = type.GetConstructor(Type.EmptyTypes);
            if (constructorInfo == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”没有定义“无参构造函数”");
                return false;
            }

            var instance = Activator.CreateInstance(type) as UserControl;
            var gameBase = instance as IGameBase;
            if (gameBase == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”未实现“IGameBase接口”");
                return false;
            }

            gameBase.VoiceTipAction = content => VoiceTipContent = content;
            gameBase.SynopsisAction = content => { };
            gameBase.LevelStatisticsAction = (value, count) => { };
            gameBase.RightStatisticsAction = (value, count) => { };
            gameBase.WrongStatisticsAction = (value, count) => { };
            gameBase.TimeStatisticsAction = (totalCountdownTime, currentCountdownTime) =>
            {
                var gameHost = GameHost;
                gameHost?.ShowTime(totalCountdownTime, currentCountdownTime);

                TotalCountdownTime = DateTime.MinValue.AddSeconds(Math.Max(totalCountdownTime ?? 0, 0));
                CurrentCountdownTime = DateTime.MinValue.AddSeconds(Math.Max(currentCountdownTime ?? 0, 0));
            };
            gameBase.GameEndAction = () => App.Current.Dispatcher.InvokeAsync(async () =>
            {
                await (gameBase?.StopAsync() ?? Task.CompletedTask);
                await (gameBase?.ReportAsync() ?? Task.CompletedTask);

                var gameHost = GameHost;
                gameHost?.Remove(gameBase);
                GameStatus = false;

                GameClear();
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估已完成，报告已保存至数据中心");

                await TryProgramSelectedChanged();
            });

            var moduleId = selectedItem.Item.Data.ModuleId;
            await (gameBase?.InitAsync(programId.Value, moduleId, db) ?? Task.CompletedTask);
            GameBase = gameBase;

            var digitalHumanItem = DigitalHumanItem;
            var patientItem = PatientItem;

            var gameHost = GameHost;
            gameHost.Init(digitalHumanItem, patientItem, selectedItem.Mode);

            ExecuteExampleCommand();
            return true;
        }

        void GameClear()
        {
            var gameBase = GameBase;
            if (gameBase == null)
            {
                return;
            }

            gameBase.VoiceTipAction = null;
            gameBase.SynopsisAction = null;
            gameBase.LevelStatisticsAction = null;
            gameBase.RightStatisticsAction = null;
            gameBase.WrongStatisticsAction = null;
            gameBase.TimeStatisticsAction = null;
            gameBase.GameEndAction = null;

            GameBase = null;

            var gameHost = GameHost;
            gameHost?.ShowTime(null, null);

            VoiceTipContent = null;
            TotalCountdownTime = null;
            CurrentCountdownTime = null;
        }

        public async void Execute(int programId, DigitalHumanItem digitalHumanItem)
        {
            this.programId = programId;
            DigitalHumanItem = digitalHumanItem;

            var (status, msg, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<ProgramModule>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取方案信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var program = await db.Programs.Include(m => m.Patient).FirstOrDefaultAsync(m => m.ProgramId == programId);
                if (program != null)
                {
                    program.ActStartTime = DateTime.Now;
                    db.Programs.Update(program);
                    db.SaveChanges();
                }

                if (program.Patient != null)
                {
                    PatientItem = new PatientItem().Update(program.Patient);
                }

                var multiItems = await db.ProgramModules.AsNoTracking().Include(m => m.Module).Where(m => m.ProgramId == programId).ToListAsync();

                return (true, null, multiItems);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            var items = multiItems.Select(m =>
            {
                var module = m.Module;
                if (!Enum.TryParse<EvaluateTestMode>(module.Name?.Trim(), out var modeResult))
                {
                    return null;
                }

                var evaluateTestItem = new EvaluateTestItem { Mode = modeResult }.Update(module);
                return evaluateTestItem;
            }).Where(m => m != null).ToList();

            ProgramItems = new ObservableCollection<ProgramItem<EvaluateTestMode, EvaluateTestItem>>(items.Select(m => new ProgramItem<EvaluateTestMode, EvaluateTestItem>
            {
                Mode = m.Mode,
                Item = m
            }));

            IGameHost gameHost = null;
            _ = Crs_DialogEx.Show(Crs_Dialog.SubGamePanel, Crs_DialogToken.SubTopContent)
                .UseConfig_ContentStretch()
                .Initialize<IGameHost>(vm => gameHost = vm)
                .GetResultAsync<object>();

            GameHost = gameHost;

            await TryProgramSelectedChanged();
        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
