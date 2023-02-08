#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Windows;
using System.Windows.Controls;

namespace Amdocs.Ginger.UserControls
{
    public static class ImageMaker
    {
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.RegisterAttached(
                "Content",
                typeof(eImageType),
                typeof(ImageMaker),
                new PropertyMetadata(eImageType.Empty, ContentChanged));
        
        public static eImageType GetContent(DependencyObject target)
        {
            return (eImageType)target.GetValue(ContentProperty);
        }
        
        public static void SetContent(DependencyObject target, ImageMakerControl value)
        {
            target.SetValue(ContentProperty, value);
        }

        private static void ContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs evt)
        {
            // If target is not a ContenControl just ignore: 
            if (!(sender is ContentControl)) return;

            ContentControl target = (ContentControl)sender;

            // If value is not a eIcon ignore
            if (!(evt.NewValue is eImageType)) return;

            // We create a new ImageMAkerControl for display in the content control
            ImageMakerControl IM = new ImageMakerControl();
            IM.ImageType = (eImageType)evt.NewValue;
            target.Content = IM;            
        }
    }
}
