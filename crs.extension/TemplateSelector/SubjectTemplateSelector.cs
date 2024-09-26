using crs.extension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace crs.extension.TemplateSelector
{
    public class SubjectTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element != null && item is SubjectItem _item)
            {
                var template = (element.FindResource(_item.TemplateName ?? string.Empty) as DataTemplate) ?? (element.FindResource("Null") as DataTemplate);
                return template;
            }
            return null;
        }
    }
}
