#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Core;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using GingerCore;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ginger;
using Amdocs.Ginger.Common;

namespace GingerWPF.UserControlsLib.UCTreeView
{
    public static class TreeViewUtils
    {
        public static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            var parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent;
        }

        /// <summary>
        /// Used to set the tree node header with Icon + Title + SourceControlState Icon + ModifiedIndicator
        /// </summary>
        /// <param name="itemObj">The object which the tree node represent</param>
        /// <param name="itemObjTitleProperty">The object title/name field name to bind the node title to</param>
        /// <param name="itemIcon">The ImageMaker icon type to show as the node icon</param>
        /// <param name="itemSourceControlStateIcon">The ImageMaker icon type to show as the SourceControlState icon</param>
        /// <param name="addItemModifiedIndication">Define is to show Change/Modified/Dirty indication or not</param>
        /// <param name="objItemModifiedIndicationBoolPropertyName">The obj Bool field name which reflect if the obj is Dirty/Changed or not- to be used for binding</param>
        /// <returns></returns>
        public static StackPanel NewRepositoryItemTreeHeader(Object itemObj, string itemObjTitleProperty, eImageType itemIcon, eImageType itemSourceControlStateIcon, bool addItemModifiedIndication = false, string objItemModifiedIndicationBoolPropertyName="")
        {
            StackPanel headerStack = new StackPanel();
            headerStack.Orientation = Orientation.Horizontal;

            //Add icon
            if (itemIcon != eImageType.Null)
            {
                try
                {
                    ImageMakerControl icon = new ImageMakerControl();
                    icon.ImageType = itemIcon;
                    icon.Height = 16;
                    icon.Width = 16;
                    headerStack.Children.Add(icon);
                }
                catch(Exception e)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, e.StackTrace);
                }
            }

            //Add source control icon
            if (itemSourceControlStateIcon != eImageType.Null)
            {
                try
                {
                    ImageMakerControl sourceControlIcon = new ImageMakerControl();
                    sourceControlIcon.BindControl((RepositoryFolderBase)itemObj, nameof(RepositoryFolderBase.SourceControlStatus));
                    ((RepositoryFolderBase)itemObj).RefreshFolderSourceControlStatus();
                    sourceControlIcon.Height = 10;
                    sourceControlIcon.Width = 10;                   
                    headerStack.Children.Add(sourceControlIcon);
                }
                catch(Exception ex)
                {
                    // TODO: write to log
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                }
            }

            //Add Item Title
            try
            {
                Label itemTitleLbl = new Label();
                if (itemObj != null)
                    BindingLib.ControlsBinding.ObjFieldBinding(itemTitleLbl, Label.ContentProperty, itemObj, itemObjTitleProperty);
                else
                    itemTitleLbl.Content = itemObjTitleProperty;
                headerStack.Children.Add(itemTitleLbl);
            }
            catch
            {
            }

            //Add Modified icon
            if (addItemModifiedIndication == true)
            {
                try
                {
                    ImageMakerControl modifiedIcon = new ImageMakerControl();
                    modifiedIcon.ImageType = eImageType.ItemModified;
                    modifiedIcon.Height = 6;
                    modifiedIcon.Width = 6;
                    modifiedIcon.SetAsFontImageWithSize = 6;
                    modifiedIcon.Foreground = Brushes.OrangeRed;
                    modifiedIcon.VerticalAlignment = VerticalAlignment.Top;
                    modifiedIcon.Margin = new Thickness(0, 10, 10, 0);
                    modifiedIcon.ToolTip = "This item was modified";
                    if (string.IsNullOrEmpty(objItemModifiedIndicationBoolPropertyName) == false)
                        BindingLib.ControlsBinding.ObjFieldBinding(modifiedIcon, ImageMakerControl.VisibilityProperty, itemObj, objItemModifiedIndicationBoolPropertyName, BindingMode:BindingMode.OneWay, bindingConvertor: new System.Windows.Controls.BooleanToVisibilityConverter());
                    headerStack.Children.Add(modifiedIcon);
                }
                catch
                {
                }
            }
            
            return headerStack;
        }

        

        public static void AddSubMenuItem(MenuItem subMenu, string Header, RoutedEventHandler RoutedEventHandler, object CommandParameter = null, eImageType icon = eImageType.Null )
        {
            MenuItem mnuItem = CreateMenuItem(Header, RoutedEventHandler, CommandParameter, icon);
            subMenu.Items.Add(mnuItem);
        }

        private static MenuItem CreateMenuItem(string Header, RoutedEventHandler RoutedEventHandler, object CommandParameter = null, eImageType imageType = eImageType.Null)
        {
            MenuItem mnuItem = new MenuItem();
            if (imageType != eImageType.Null)
            {
                ImageMakerControl actionIcon = new ImageMakerControl();
                actionIcon.ImageType = imageType;
                actionIcon.Height = 16;
                actionIcon.Width = 16;
                mnuItem.Icon = actionIcon;
            }
            mnuItem.Header = Header;
            mnuItem.Click += RoutedEventHandler;
            mnuItem.CommandParameter = CommandParameter;
            return mnuItem;
        }

        public static StackPanel CreateLinkedItemHeader(Object obj, string ObjProperty, string ImageFile, BitmapImage ImageFile2 = null)
        {
            return CreateItemHeader(null, ImageFile, ImageFile2, obj, ObjProperty);
        }

        public static StackPanel CreateItemHeader(string Header = null, string ImageFile = null, BitmapImage ImageFile2 = null, Object obj = null, string ObjProperty = null, bool IsDirty = false)
        {

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            // create Image
            Image image = new Image();
            try
            {
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + ImageFile));
            }
            catch
            {               
                Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, "Missing Header Image");
            }

            //Image 2 i.e.: Source Control Image
            Image SCimage = null;
            if (ImageFile2 != null)
            {
                SCimage = new Image();
                SCimage.Source = ImageFile2;

                // It is not must that this is SC image
                // SCimage.ToolTip = "Source Control Status";
            }
           
            // Label
            Label lbl = new Label();

            if (obj == null)
            {
                lbl.Content = Header;
            }
            else
            {
                // TODO: use lbl.BindControl
                // Can bind the obj property directly to label content - so auto update when changed
                BindTVItemHeader(lbl, Label.ContentProperty, obj, ObjProperty);
            }

            // Add into stack
            stack.Children.Add(image);
            if (ImageFile2 != null)
            {
                stack.Children.Add(SCimage);
            }
            stack.Children.Add(lbl);
            if (IsDirty)
            {
                Image Dirty = new Image();
                Dirty.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/RedStar8x8.png"));
                Dirty.ToolTip = "This item was modified";
                stack.Children.Add(Dirty);
            }
            return stack;
        }

        private static void BindTVItemHeader(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property)
        {
            Binding b = new Binding();
            b.Source = obj;
            b.Path = new PropertyPath(property);
            b.Mode = BindingMode.TwoWay;
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            control.SetBinding(dependencyProperty, b);
        }

        private static MenuItem CreateMenuItem(string Header, RoutedEventHandler RoutedEventHandler, object CommandParameter = null, string icon = "")
        {
            MenuItem mnuItem = new MenuItem();
            if (string.IsNullOrEmpty(icon) == false)
            {
                Image image = new Image();
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + icon));
                mnuItem.Icon = image;
            }
            mnuItem.Header = Header;
            mnuItem.Click += RoutedEventHandler;
            mnuItem.CommandParameter = CommandParameter;
            return mnuItem;
        }

        public static void AddMenuItem(ContextMenu menu, string Header, RoutedEventHandler RoutedEventHandler, object CommandParameter = null, string icon = "")
        {
            MenuItem mnuItem = CreateMenuItem(Header, RoutedEventHandler, CommandParameter, icon);
            menu.Items.Add(mnuItem);
        }

        public static void AddMenuItem(ContextMenu menu, string Header, RoutedEventHandler RoutedEventHandler, object CommandParameter = null, eImageType imageType = eImageType.Null)
        {
            MenuItem mnuItem = CreateMenuItem(Header, RoutedEventHandler, CommandParameter, imageType);
            menu.Items.Add(mnuItem);
        }

        public static MenuItem CreateSubMenu(ContextMenu menu, string Header, eImageType icon = eImageType.Null)
        {
            MenuItem mnuItem = new MenuItem();
            if (icon != eImageType.Null)
            {
                ImageMakerControl menuIcon = new ImageMakerControl();
                menuIcon.ImageType = icon;
                menuIcon.Height = 16;
                menuIcon.Width = 16;
                mnuItem.Icon = menuIcon;
            }
            mnuItem.Header = Header;
            menu.Items.Add(mnuItem);
            return mnuItem;
        }

        public static void AddSubMenuItem(MenuItem subMenu, string Header, RoutedEventHandler RoutedEventHandler, object CommandParameter = null, string icon = "")
        {
            MenuItem mnuItem = CreateMenuItem(Header, RoutedEventHandler, CommandParameter, icon);
            subMenu.Items.Add(mnuItem);
        }
    }
}
