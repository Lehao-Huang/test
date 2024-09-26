using crs.core.DbModels;
using crs.core;
using crs.theme.Extensions;
using HandyControl.Tools.Extension;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using static crs.extension.Crs_Enum;
using static crs.extension.Crs_Interface;
using crs.extension.Models;
using crs.extension;
using crs.dialog.Views;
using crs.game;
using System.Windows;

namespace crs.dialog.ViewModels
{
    public class SubGamePanelViewModel : BindableBase, IDialogResultable<object>, IGameHost
    {
        readonly IRegionManager regionManager;
        readonly IContainerProvider containerProvider;
        readonly IEventAggregator eventAggregator;
        readonly Crs_Db2Context db;

        public SubGamePanelViewModel() { }
        public SubGamePanelViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IEventAggregator eventAggregator, Crs_Db2Context db)
        {
            this.regionManager = regionManager;
            this.containerProvider = containerProvider;
            this.eventAggregator = eventAggregator;
            this.db = db;
        }

        #region Property
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

        private Enum modeType;
        public Enum ModeType
        {
            get { return modeType; }
            set { SetProperty(ref modeType, value); }
        }

        private FrameworkElement gameDemoContent;
        public FrameworkElement GameDemoContent
        {
            get { return gameDemoContent; }
            set { SetProperty(ref gameDemoContent, value); }
        }

        private string gameDemoMessage;
        public string GameDemoMessage
        {
            get { return gameDemoMessage; }
            set { SetProperty(ref gameDemoMessage, value); }
        }

        private IGameBase gameContent;
        public IGameBase GameContent
        {
            get { return gameContent; }
            set { SetProperty(ref gameContent, value); }
        }

        private DateTime? currentCountdownTime;
        public DateTime? CurrentCountdownTime
        {
            get { return currentCountdownTime; }
            set { SetProperty(ref currentCountdownTime, value); }
        }
        #endregion

        public bool Init(DigitalHumanItem humanItem, PatientItem patientItem, Enum modeType)
        {
            DigitalHumanItem = humanItem;
            PatientItem = patientItem;
            ModeType = modeType;
            return true;
        }

        public bool ShowDemoInfo(FrameworkElement element, string message)
        {
            GameDemoContent = element;
            GameDemoMessage = message;
            return true;
        }

        public bool Show(IGameBase gameContent)
        {
            GameContent = gameContent;
            return true;
        }

        public bool Remove(IGameBase gameContent = null)
        {
            if (gameContent == null || gameContent == GameContent)
            {
                GameContent = null;
                return true;
            }
            return false;
        }

        public bool ShowTime(int? totalCountdownTime, int? currentCountdownTime)
        {
            CurrentCountdownTime = DateTime.MinValue.AddSeconds(Math.Max(currentCountdownTime ?? 0, 0));
            return true;
        }

        public object Result { get; set; }
        public Action CloseAction { get; set; }
    }
}
