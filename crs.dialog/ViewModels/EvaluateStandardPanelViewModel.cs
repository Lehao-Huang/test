using crs.core;
using crs.core.DbModels;
using crs.extension;
using crs.extension.Models;
using crs.theme.Extensions;
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
using System.Linq;
using System.Threading.Tasks;
using static crs.extension.Crs_Enum;
using static SkiaSharp.HarfBuzz.SKShaper;
using Result = crs.core.DbModels.Result;

namespace crs.dialog.ViewModels
{
    public class EvaluateStandardPanelViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<int?>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        bool init = false;
        int? programId;
        int? scheduleId;

        public EvaluateStandardPanelViewModel() { }
        public EvaluateStandardPanelViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
        private ObservableCollection<EvaluateStandardItem> evaluateStandardItems;
        public ObservableCollection<EvaluateStandardItem> EvaluateStandardItems
        {
            get { return evaluateStandardItems; }
            set { SetProperty(ref evaluateStandardItems, value); }
        }

        private EvaluateStandardItem evaluateStandardSelectedItem;
        public EvaluateStandardItem EvaluateStandardSelectedItem
        {
            get { return evaluateStandardSelectedItem; }
            set { SetProperty(ref evaluateStandardSelectedItem, value); }
        }

        private ObservableCollection<SubjectItem> subjectItems;
        public ObservableCollection<SubjectItem> SubjectItems
        {
            get { return subjectItems; }
            set { SetProperty(ref subjectItems, value); }
        }

        private SubjectItem subjectSelectedItem;
        public SubjectItem SubjectSelectedItem
        {
            get { return subjectSelectedItem; }
            set { SetProperty(ref subjectSelectedItem, value); }
        }

        private int subjectSelectedIndex;
        public int SubjectSelectedIndex
        {
            get { return subjectSelectedIndex; }
            set { SetProperty(ref subjectSelectedIndex, value); }
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

            CloseAction?.Invoke();
        }

        private DelegateCommand lastCommand;
        public DelegateCommand LastCommand =>
            lastCommand ?? (lastCommand = new DelegateCommand(ExecuteLastCommand));

        void ExecuteLastCommand()
        {
            var selectedItem = SubjectSelectedItem;
            if (selectedItem == null || !init)
            {
                return;
            }

            SubjectSelectedIndex--;
        }

        private DelegateCommand nextCommand;
        public DelegateCommand NextCommand =>
            nextCommand ?? (nextCommand = new DelegateCommand(ExecuteNextCommand));

        void ExecuteNextCommand()
        {
            var selectedItem = SubjectSelectedItem;
            if (selectedItem == null || !init)
            {
                return;
            }

            SubjectSelectedIndex++;
        }

        private DelegateCommand completeCommand;
        public DelegateCommand CompleteCommand =>
            completeCommand ?? (completeCommand = new DelegateCommand(ExecuteCompleteCommand));

        async void ExecuteCompleteCommand()
        {
            var selectedItem = EvaluateStandardSelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var subjectItems = selectedItem.SubjectItems;
            var _selectedItem = subjectItems.FirstOrDefault(m =>
            {
                var answerItems = m.AnswerItems.Where(m => m.IsUse).ToList();
                if (answerItems.Count == 0)
                {
                    return true;
                }

                if (answerItems.Count(m => !string.IsNullOrWhiteSpace(m.GroupName)) == 0)
                {
                    return answerItems.Count(m => !m.IsRight && !m.IsWrong) > 0;
                }

                return answerItems.GroupBy(m => m.GroupName).Count(m => m.Count(m => !m.IsRight && !m.IsWrong) == m.Count()) > 0;
            });

            if (_selectedItem != null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync($"“{_selectedItem.Name}”未完成答题");
                return;
            }

            if (programId == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("方案ID为空");
                return;
            }

            var module = selectedItem.Data;
            if (module == null)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("模块信息为空");
                return;
            }

            var (status, msg) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "保存结果错误";
                    return (false, $"{exception.Message},{ex.Message}");
                };

                var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    var result = await db.Results.FirstOrDefaultAsync(m => m.ProgramId == programId);
                    if (result == null)
                    {
                        result = new Result
                        {
                            ProgramId = programId.Value,
                            Report = module.Name?.Trim(),
                            Eval = true,
                            ScheduleId = scheduleId
                        };

                        db.Results.Add(result);
                        await db.SaveChangesAsync();
                        db.Entry(result);
                    }
                    else
                    {
                        result.ProgramId = programId.Value;
                        result.Report = module.Name?.Trim();
                        result.Eval = true;
                        result.ScheduleId = scheduleId;

                        db.Results.Update(result);
                    }

                    var resultId = result.ResultId;

                    foreach (var item in subjectItems)
                    {
                        var newResultDetail = new ResultDetail
                        {
                            ResultId = resultId,
                            ModuleId = module.ModuleId,
                            ValueName = item.Name,
                            Value = item.RightAnswerCount
                        };
                        db.ResultDetails.Add(newResultDetail);
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    selectedItem.IsComplete = true;
                    foreach (var item in subjectItems)
                    {
                        item.IsComplete = true;
                    }

                    return (true, "保存结果成功");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message, ex);
                }
            });

            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
            if (!status)
            {
                return;
            }

            selectedItem = EvaluateStandardItems.FirstOrDefault(m => !m.IsComplete);
            if (selectedItem != null)
            {
                EvaluateStandardSelectedItem = selectedItem;
                return;
            }

            await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("评估结束");

            var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
            if (program != null)
            {
                program.ActEndTime = DateTime.Now;
                db.Programs.Update(program);
                db.SaveChanges();
            }

            CloseAction?.Invoke();
        }

        private DelegateCommand evaluateStandardSelectedChangedCommand;
        public DelegateCommand EvaluateStandardSelectedChangedCommand =>
            evaluateStandardSelectedChangedCommand ?? (evaluateStandardSelectedChangedCommand = new DelegateCommand(ExecuteEvaluateStandardSelectedChangedCommand));

        async void ExecuteEvaluateStandardSelectedChangedCommand()
        {
            var selectedItem = EvaluateStandardSelectedItem;
            if (selectedItem == null || !init)
            {
                return;
            }

            SubjectItems = null;
            await Task.Yield();

            var _items = selectedItem.SubjectItems;
            if (_items == null)
            {
                var items = selectedItem.StandardType switch
                {
                    EvaluateStandardType.MoCA量表 => Enumerable.Range(1, 11).ToList(),
                    EvaluateStandardType.MMSE量表 => Enumerable.Range(1, 11).ToList(),
                    _ => throw new NotImplementedException()
                };

                _items = items.Select(index =>
                {
                    var isFirst = items.FirstOrDefault() == index;
                    var isLast = items.LastOrDefault() == index;

                    return new SubjectItem
                    {
                        Name = $"题目{index}",
                        StandardType = selectedItem.StandardType,
                        IsFirst = isFirst,
                        IsLast = isLast,
                    };
                }).ToList();
                selectedItem.SubjectItems = _items;
            }

            SubjectItems = new ObservableCollection<SubjectItem>(_items);
            SubjectSelectedItem = SubjectItems.FirstOrDefault();
        }

        public async void Execute(int? programId)
        {
            this.programId = programId;

            var (status, msg, multiItems) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, List<ProgramModule>)>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取方案信息错误";
                    return (false, $"{exception.Message},{ex.Message}", null);
                };

                var program = await db.Programs.FirstOrDefaultAsync(m => m.ProgramId == programId);
                if (program != null)
                {
                    program.ActStartTime = DateTime.Now;
                    db.Programs.Update(program);
                    db.SaveChanges();
                }

                var schedule = await db.Schedules.FirstOrDefaultAsync(m => m.ProgramId == programId);
                this.scheduleId = schedule?.ScheduleId;

                var multiItems = await db.ProgramModules.AsNoTracking().Include(m => m.Module).Where(m => m.ProgramId == programId).ToListAsync();

                return (true, null, multiItems);
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            var items = Enum.GetValues<EvaluateStandardType>().Cast<EvaluateStandardType>().Select(m =>
            {
                var type = m switch
                {
                    EvaluateStandardType.MoCA量表 => "MoCA",
                    EvaluateStandardType.MMSE量表 => "MMSE",
                    _ => null
                };

                var module = multiItems.FirstOrDefault(m => m.Module?.Name?.Trim() == type)?.Module;

                return new EvaluateStandardItem { StandardType = m, }.Update(module);
            });

            EvaluateStandardItems = new ObservableCollection<EvaluateStandardItem>(items);
            EvaluateStandardSelectedItem = EvaluateStandardItems.FirstOrDefault();

            init = true;
            EvaluateStandardSelectedChangedCommand?.Execute();
        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
