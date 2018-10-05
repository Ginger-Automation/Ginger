using Ginger;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GingerWPF
{
    /// <summary>
    /// This class is used to validate the object on forms
    /// </summary>
    public class ValidationDetails
    {
        /// <summary>
        /// Constructor for making the class singleton
        /// </summary>
        private static ValidationDetails mInstance;
        public static ValidationDetails Instance
        {
            get
            {
                if(mInstance == null)
                {
                    mInstance = new ValidationDetails();
                }
                return mInstance;
            }
        }

        /// <summary>
        /// private ctor
        /// </summary>
        private ValidationDetails()
        {

        }

        /// <summary>
        /// This method is used to validate the error on page
        /// </summary>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public bool SearchValidationsRecursive(DependencyObject depObj, bool errorsFound)
        {
            if (depObj != null)
            {
                int depObjCount = VisualTreeHelper.GetChildrenCount(depObj);
                for (int i = 0; i < depObjCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null)
                    {
                        //.NET Controls Validation
                        BindingExpression bindingExpression = null;
                        if (child is TextBox)
                        {
                            TextBox textBox = (TextBox)child;
                            bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                        }
                        else if (child is ComboBox)
                        {
                            ComboBox comboBox = (ComboBox)child;
                            bindingExpression = comboBox.GetBindingExpression(ComboBox.SelectedValueProperty);
                            if (bindingExpression == null)
                            {
                                bindingExpression = comboBox.GetBindingExpression(ComboBox.TextProperty);
                            }
                        }
                        else if (child is Ginger.Agents.ucAgentControl)
                        {
                            Ginger.Agents.ucAgentControl agentControl = (Ginger.Agents.ucAgentControl)child;
                            bindingExpression = agentControl.GetBindingExpression(Ginger.Agents.ucAgentControl.SelectedAgentProperty);
                        }

                        if (bindingExpression != null)
                        {
                            // do if there is validation bindingExpression.
                            bindingExpression.UpdateSource();
                            if (bindingExpression.HasValidationError)
                            {
                                errorsFound = true;
                                break;
                            }
                        }

                        //Custome controls Validations
                        if (errorsFound == false)
                        {
                            if (child is ucGrid)
                            { errorsFound = ((ucGrid)child).HasValidationError(); }
                        }
                    }

                    if (!errorsFound)
                    {
                        SearchValidationsRecursive(child, errorsFound);
                        if (errorsFound)
                        {
                            break;
                        }
                    }
                }
            }
            return errorsFound;
        }
    }
}
