using crs.core;
using crs.extension;
using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using crs.core.DbModels;
using crs.extension.Models;
using Microsoft.EntityFrameworkCore;
using LiveChartsCore.Kernel.Sketches;
using static SkiaSharp.HarfBuzz.SKShaper;
using Result = crs.core.DbModels.Result;

namespace crs.dialog.ViewModels
{
    public class EvaluateReportViewModel : BindableBase, IDialogResultable<object>, IDialogCommon<int?, int?, int?>
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public EvaluateReportViewModel() { }
        public EvaluateReportViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
        private PatientItem patienttem;
        public PatientItem PatientItem
        {
            get { return patienttem; }
            set { SetProperty(ref patienttem, value); }
        }


        private ModuleItem moduleItem;
        public ModuleItem ModuleItem
        {
            get { return moduleItem; }
            set { SetProperty(ref moduleItem, value); }
        }

        private DataTable reportDataTable;
        public DataTable ReportDataTable
        {
            get { return reportDataTable; }
            set { SetProperty(ref reportDataTable, value); }
        }

        private ICartesianAxis[] xAxes;
        public ICartesianAxis[] XAxes
        {
            get { return xAxes; }
            set { SetProperty(ref xAxes, value); }
        }

        private ICartesianAxis[] yAxes;
        public ICartesianAxis[] YAxes
        {
            get { return yAxes; }
            set { SetProperty(ref yAxes, value); }
        }
        #endregion

        private DelegateCommand cancelCommand;
        public DelegateCommand CancelCommand =>
            cancelCommand ?? (cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        void ExecuteCancelCommand()
        {
            CloseAction?.Invoke();
        }

        public async void Execute(int? patientId, int? moduleId, int? resultId)
        {
            var (status, msg, multiItem) = await Crs_DialogEx.ProgressShow().GetProgressResultAsync<(bool, string, (OrganizationPatient patient, Module module, List<Result> results))>(async exception =>
            {
                exception.Exception = async ex =>
                {
                    exception.Message = "获取报告信息错误";
                    return (false, $"{exception.Message},{ex.Message}", default);
                };

                var patient = await db.OrganizationPatients.AsNoTracking().FirstOrDefaultAsync(m => m.Id == patientId);
                var module = await db.Modules.AsNoTracking().FirstOrDefaultAsync(m => m.ModuleId == moduleId);

                var results = await db.Results.AsNoTracking().Include(m => m.ResultDetails).Where(m => m.ResultId == resultId).ToListAsync();

                return (true, null, (patient, module, results));
            });

            if (!status)
            {
                await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync(msg);
                return;
            }

            if (multiItem.patient != null)
            {
                PatientItem = new PatientItem().Update(multiItem.patient);
            }

            if (multiItem.module != null)
            {
                ModuleItem = new ModuleItem().Update(multiItem.module);
            }

            if (multiItem.results.Count > 0)
            {
                var columns = (from item in multiItem.results from _item in item.ResultDetails select _item).OrderBy(m => m.Order).Select(m => m.ValueName?.Trim()).ToHashSet().ToList();
                //columns.Insert(0, "Lv");

                var table = new DataTable();
                table.Columns.AddRange(columns.Select(m => new DataColumn(m)).ToArray());

                var groups = (from item in multiItem.results from _item in item.ResultDetails select _item).OrderBy(m => m.Lv).GroupBy(m => m.Lv);
                foreach (var item in groups)
                {
                    var newRow = table.NewRow();
                    //newRow["Lv"] = item.Key?.ToString();

                    foreach (var _item in item)
                    {
                        newRow[_item.ValueName?.Trim()] = _item.Value.ToString();
                    }

                    table.Rows.Add(newRow);
                }

                ReportDataTable = table;
            }
        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
