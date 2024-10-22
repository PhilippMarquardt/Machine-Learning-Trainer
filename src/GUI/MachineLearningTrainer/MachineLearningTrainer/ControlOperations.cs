﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MachineLearningTrainer
{
    static class ControlOperations
    {
        public static T GetChildOfType<T>(this DependencyObject depObj)
        where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }


        public static T GetParentOfType<T>(this DependencyObject depObj)
        where T : DependencyObject
        {
            if (depObj == null) return null;

            if (depObj is T)
                return depObj as T;
            return GetParentOfType<T>(VisualTreeHelper.GetParent(depObj));
           
        }
    }
}
