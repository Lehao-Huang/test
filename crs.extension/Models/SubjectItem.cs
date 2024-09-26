using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class SubjectItem : BindableBase
    {
        public SubjectItem()
        {
            answerItems = new ObservableCollection<AnswerItem>(Enumerable.Range(0, 32).Select(m =>
            {
                var item = new AnswerItem();
                item.PropertyChanged += Item_PropertyChanged;
                return item;
            }));
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var answerItems = AnswerItems.Where(m => m.IsUse).ToList();

            if (StandardType == EvaluateStandardType.MoCA量表 && Name == "题目10")
            {
                AllAnswerCount = answerItems.Count(m => !m.Ignore);
                RightAnswerCount = answerItems.Count(m => !m.Ignore && m.IsRight);
                WrongAnswerCount = AllAnswerCount - RightAnswerCount;
                return;
            }

            if (StandardType == EvaluateStandardType.MoCA量表 && Name == "题目6")
            {
                AllAnswerCount = 3;
                RightAnswerCount = answerItems.Count(m => m.IsRight) switch
                {
                    >= 4 => 3,
                    >= 2 => 2,
                    >= 1 => 1,
                    _ => 0
                };
                WrongAnswerCount = AllAnswerCount - RightAnswerCount;
                return;
            }

            AllAnswerCount = answerItems.Count();
            RightAnswerCount = answerItems.Count(m => m.IsRight);
            WrongAnswerCount = answerItems.Count(m => m.IsWrong);
        }

        public string TemplateName => $"{StandardType}.{Name}";

        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string originName;
        public string OriginName
        {
            get { return originName; }
            set { SetProperty(ref originName, value); }
        }

        private EvaluateStandardType standardType;
        public EvaluateStandardType StandardType
        {
            get { return standardType; }
            set { SetProperty(ref standardType, value); }
        }

        private bool isFirst;
        public bool IsFirst
        {
            get { return isFirst; }
            set { SetProperty(ref isFirst, value); }
        }

        private bool isLast;
        public bool IsLast
        {
            get { return isLast; }
            set { SetProperty(ref isLast, value); }
        }

        private bool isComplete;
        public bool IsComplete
        {
            get { return isComplete; }
            set { SetProperty(ref isComplete, value); }
        }

        private readonly ObservableCollection<AnswerItem> answerItems;
        public ObservableCollection<AnswerItem> AnswerItems => answerItems;

        private int allAnswerCount;
        public int AllAnswerCount
        {
            get { return allAnswerCount; }
            set { SetProperty(ref allAnswerCount, value); }
        }

        private int rightAnswerCount;
        public int RightAnswerCount
        {
            get { return rightAnswerCount; }
            set { SetProperty(ref rightAnswerCount, value); }
        }

        private int wrongAnswerCount;
        public int WrongAnswerCount
        {
            get { return wrongAnswerCount; }
            set { SetProperty(ref wrongAnswerCount, value); }
        }
    }
}
