using Amdocs.Ginger.Repository;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerCore.GeneralLib
{
    public class BindingHandler
    {
        #region Binding
        public static void ActInputValueBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, ActInputValue actInputValue, BindingMode BindingMode = BindingMode.TwoWay)
        {
            ObjFieldBinding(control, dependencyProperty, actInputValue, ActInputValue.Fields.Value, BindingMode);
        }

        public static void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property, IValueConverter bindingConvertor, BindingMode BindingMode = BindingMode.TwoWay)
        {
            //TODO: add Inotify on the obj.attr - so code changes to property will be reflected
            //TODO: check perf impact + reuse exisitng binding on same obj.prop
            try
            {
                System.Windows.Data.Binding b = new System.Windows.Data.Binding();
                b.Source = obj;
                b.Path = new PropertyPath(property);
                b.Mode = BindingMode;
                if (bindingConvertor != null)
                    b.Converter = bindingConvertor;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                control.SetBinding(dependencyProperty, b);
            }
            catch (Exception ex)
            {
                //it is possible we load an old enum or something else which will cause the binding to fail
                // Can happen also if the bind field name is incorrect
                // mark the control in red, instead of not openning the Page
                // Set a tool tip with the error

                // control.IsEnabled = false; // Do not disable as the red will not show
                control.Style = null; // remove style so red will show
                //control.Foreground = System.Windows.Media.Brushes.Red;
                control.Background = System.Windows.Media.Brushes.LightPink;
                control.BorderThickness = new Thickness(2);
                control.BorderBrush = System.Windows.Media.Brushes.Red;

                control.ToolTip = "Error binding control to property: " + Environment.NewLine + property + " Please open a defect with all information,  " + Environment.NewLine + ex.Message;
            }
        }

        public static void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property, BindingMode BindingMode = BindingMode.TwoWay)
        {
            //TODO: add Inotify on the obj.attr - so code changes to property will be reflected
            //TODO: check perf impact + reuse exisitng binding on same obj.prop
            try
            {
                Binding b = new Binding();
                b.Source = obj;
                b.Path = new PropertyPath(property);
                b.Mode = BindingMode;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                b.NotifyOnValidationError = true;
                control.SetBinding(dependencyProperty, b);
            }
            catch (Exception ex)
            {
                //it is possible we load an old enum or something else which will cause the binding to fail
                // Can happen also if the bind field name is incorrect
                // mark the control in red, instead of not openning the Page
                // Set a tool tip with the error

                control.Style = null; // remove style so red will show
                control.Background = System.Windows.Media.Brushes.LightPink;
                control.BorderThickness = new Thickness(2);
                control.BorderBrush = System.Windows.Media.Brushes.Red;

                control.ToolTip = "Error binding control to property: " + Environment.NewLine + property + " Please open a defect with all information,  " + Environment.NewLine + ex.Message;
            }
        }

        public static void ObjFieldBinding(TextBlock textBlockControl, DependencyProperty dependencyProperty, object obj, string property, BindingMode BindingMode = BindingMode.TwoWay)
        {
            //TODO: add Inotify on the obj.attr - so code changes to property will be reflected
            //TODO: check perf impact + reuse exisitng binding on same obj.prop
            try
            {
                Binding b = new Binding();
                b.Source = obj;
                b.Path = new PropertyPath(property);
                b.Mode = BindingMode;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                textBlockControl.SetBinding(dependencyProperty, b);
            }
            catch (Exception ex)
            {
                //it is possible we load an old enum or something else which will cause the binding to fail
                // Can happen also if the bind field name is incorrect
                // mark the control in red, instead of not openning the Page
                // Set a tool tip with the error

                textBlockControl.Style = null; // remove style so red will show
                textBlockControl.Background = System.Windows.Media.Brushes.LightPink;
                textBlockControl.ToolTip = "Error binding control to property: " + Environment.NewLine + property + " Please open a defect with all information,  " + Environment.NewLine + ex.Message;
            }
        }

        
        #endregion Binding
    }
}
