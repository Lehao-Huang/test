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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using static crs.extension.Crs_Enum;
using static crs.extension.Crs_Interface;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Image = System.Windows.Controls.Image;
using MessageBoxButton = crs.theme.Extensions.MessageBoxButton;

namespace crs.dialog.ViewModels
{
    public class TrainGamePanelViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<int, DigitalHumanItem>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        int? programId;

        public TrainGamePanelViewModel() { }
        public TrainGamePanelViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
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

        private string synopsisContent;
        public string SynopsisContent
        {
            get { return synopsisContent; }
            set { SetProperty(ref synopsisContent, value); }
        }

        private StatisticsItem levelStatisticsItem;
        public StatisticsItem LevelStatisticsItem
        {
            get { return levelStatisticsItem; }
            set { SetProperty(ref levelStatisticsItem, value); }
        }

        private StatisticsItem rightStatisticsItem;
        public StatisticsItem RightStatisticsItem
        {
            get { return rightStatisticsItem; }
            set { SetProperty(ref rightStatisticsItem, value); }
        }

        private StatisticsItem wrongStatisticsItem;
        public StatisticsItem WrongStatisticsItem
        {
            get { return wrongStatisticsItem; }
            set { SetProperty(ref wrongStatisticsItem, value); }
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

        private ObservableCollection<MultiItem<TrainType, ObservableCollection<ProgramItem<TrainMode, TrainItem>>, bool>> programItems;
        public ObservableCollection<MultiItem<TrainType, ObservableCollection<ProgramItem<TrainMode, TrainItem>>, bool>> ProgramItems
        {
            get { return programItems; }
            set { SetProperty(ref programItems, value); }
        }

        private ProgramItem<TrainMode, TrainItem> programSelectedItem;
        public ProgramItem<TrainMode, TrainItem> ProgramSelectedItem
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
            if (await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("训练还未完成，是否退出？", button: MessageBoxButton.OKOrCancel) == null)
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
            if ((result = await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("训练还未完成，是否中断训练并生成报告？", button: MessageBoxButton.CustomReport)) == null)
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

                    await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("训练结束");

                    var gameHost = GameHost;
                    (gameHost as IDialogResultable<object>)?.CloseAction();

                    CloseAction?.Invoke();
                    GameStatus = false;
                    return;
                }
            } while (!await GameInit());

            async Task<ProgramItem<TrainMode, TrainItem>> ProgramSelectedChanged()
            {
                var items = ProgramItems;
                if (items == null)
                {
                    return null;
                }

                var selectedItem = ProgramSelectedItem;
                ProgramSelectedItem = null;

                await Task.Yield();

                var _items = (from item in items let __items = item.Item2 from __item in __items select __item).ToList();
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

                var expanded = false;
                foreach (var item in items.Reverse())
                {
                    if (expanded)
                    {
                        item.Item3 = expanded;
                        continue;
                    }

                    var __items = item.Item2;
                    foreach (var _item in __items)
                    {
                        if (_item == selectedItem)
                        {
                            expanded = true;
                        }
                    }
                    item.Item3 = expanded;
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

            var constructorInfo = type.GetConstructor(Type.EmptyTypes);
            if (constructorInfo == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”没有定义“无参构造函数”");
                return false;
            }

            if (type.BaseType != typeof(BaseUserControl) && type.BaseType != typeof(UserControl))
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"模块“{selectedItem.Mode}”不是“BaseUserControl类型”或“UserControl类型”");
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
            gameBase.SynopsisAction = content => SynopsisContent = content;
            gameBase.LevelStatisticsAction = (value, count) => LevelStatisticsItem = new StatisticsItem { Count = Math.Max(count, 0), Value = Math.Max(value, 0) };
            gameBase.RightStatisticsAction = (value, count) => RightStatisticsItem = new StatisticsItem { Count = Math.Max(count, 0), Value = Math.Max(value, 0) };
            gameBase.WrongStatisticsAction = (value, count) => WrongStatisticsItem = new StatisticsItem { Count = Math.Max(count, 0), Value = Math.Max(value, 0) };
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
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("训练已完成，报告已保存至数据中心");

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
            SynopsisContent = null;
            LevelStatisticsItem = null;
            RightStatisticsItem = null;
            WrongStatisticsItem = null;
            TotalCountdownTime = null;
            CurrentCountdownTime = null;
        }

        public async void Execute(int programId, DigitalHumanItem digitalHumanItem)
        {
            this.programId = programId;
            DigitalHumanItem = digitalHumanItem;

            var (status, msg, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<(ProgramModule programModule, List<ProgramModulePar> programModulePars)>)>(async exception =>
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

                var multiItems = (from programModule in db.ProgramModules.AsNoTracking().Include(m => m.Module).Where(m => m.ProgramId == programId)
                                  from programModulePar in db.ProgramModulePars.AsNoTracking().Include(m => m.ModulePar).Where(m => m.ProgramId == programId)
                                  where programModule.ModuleId == programModulePar.ModulePar.ModuleId
                                  select new { programModule, programModulePar })
                                  .ToList();

                var _multiItems = multiItems
                    .GroupBy(m => m.programModule.ModuleId)
                    .Select(m =>
                    {
                        var firstItem = m.FirstOrDefault();
                        return (firstItem.programModule, m.Select(m => m.programModulePar).ToList());
                    })
                    .ToList();

                return (true, null, _multiItems);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            var items = multiItems.Select(m =>
            {
                var module = m.programModule.Module;
                var multiModulePars = m.programModulePars.Select(m => (m.ModulePar, m.Value)).ToList();

                if (!Enum.TryParse<TrainType>(module.TrainType?.Trim(), out var typeResult))
                {
                    return null;
                }

                if (!Enum.TryParse<TrainMode>(module.Name?.Trim(), out var modeResult))
                {
                    return null;
                }

                var trainItem = new TrainItem { Type = typeResult, Mode = modeResult }.Update(module);

                var modulePars = multiModulePars.Where(m => m.ModulePar.FeedbackType == null).ToList();
                trainItem.ModuleParItems = new ObservableCollection<ModuleParItem>(modulePars.Select(m => new ModuleParItem().Update(m.ModulePar, defaultValue: m.Value)));

                var feedbackModulePars = multiModulePars.Where(m => m.ModulePar.FeedbackType != null).ToList();
                trainItem.FeedbackModuleParItems = new ObservableCollection<ModuleParItem>(feedbackModulePars.Select(m => new ModuleParItem().Update(m.ModulePar, isChecked: m.Value == 1)));

                return trainItem;
            }).Where(m => m != null).ToList();

            var programGroups = items.GroupBy(m => m.Type).Select(m => new MultiItem<TrainType, ObservableCollection<ProgramItem<TrainMode, TrainItem>>, bool>
            {
                Item1 = m.Key,
                Item2 = new ObservableCollection<ProgramItem<TrainMode, TrainItem>>(m.Select(m => new ProgramItem<TrainMode, TrainItem>
                {
                    Mode = m.Mode,
                    Item = m.UpdateMessageInfo()
                })),
                Item3 = false
            }).ToList();

            ProgramItems = new ObservableCollection<MultiItem<TrainType, ObservableCollection<ProgramItem<TrainMode, TrainItem>>, bool>>(programGroups);

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
