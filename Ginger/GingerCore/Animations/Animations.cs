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

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GingerCore
{
    public static class StoryboardExtensions
    {
        public static Task BeginAsync(this Storyboard storyboard)
        {
            System.Threading.Tasks.TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                EventHandler onComplete = null;
                onComplete = (s, e) =>
                {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                };
                storyboard.Completed += onComplete;
                storyboard.Begin();
            }
            return tcs.Task;
        }
    }
    public static class Animations
    {
        public static Storyboard AnimateResize(Window w, double changeWidth = 0d, double changeHeight = 0d, double durationMilisec = 200.0,
            AlignmentX LocationLockHorizontal = AlignmentX.Left, AlignmentY LocationLockVertical = AlignmentY.Top)
        {
            Storyboard sb = new Storyboard { Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec)) };

            DoubleAnimationUsingKeyFrames daw;
            DoubleAnimationUsingKeyFrames dah;

            // animate window width
            // animate window height
            if (changeHeight != 0.0)
            {
                dah = new DoubleAnimationUsingKeyFrames();
                dah.KeyFrames.Add(new EasingDoubleKeyFrame(w.ActualHeight, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 00))));
                dah.KeyFrames.Add(new EasingDoubleKeyFrame(w.ActualHeight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));
                dah.KeyFrames.Add(new EasingDoubleKeyFrame(w.ActualHeight + changeHeight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec))));
                dah.Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec));
                dah.AccelerationRatio = 0.4;
                dah.DecelerationRatio = 0.6;
                Storyboard.SetTarget(dah, w);
                Storyboard.SetTargetProperty(dah, new PropertyPath(Window.HeightProperty));
                sb.Children.Add(dah); 
            }

            if (changeWidth != 0.0)
            {
                daw = new DoubleAnimationUsingKeyFrames();
                daw.KeyFrames.Add(new EasingDoubleKeyFrame(w.ActualWidth, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 00))));
                daw.KeyFrames.Add(new EasingDoubleKeyFrame(w.ActualWidth + changeWidth, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));
                daw.Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec / 2));
                daw.AccelerationRatio = 0.4;
                daw.DecelerationRatio = 0.6;
                w.BeginAnimation(Window.WidthProperty, daw);
                Storyboard.SetTarget(daw, w);
                Storyboard.SetTargetProperty(daw, new PropertyPath(Window.WidthProperty));
                sb.Children.Add(daw);
            }

            DoubleAnimationUsingKeyFrames dax;
            DoubleAnimationUsingKeyFrames day;

            // animate window move in horizontal way
            if (LocationLockHorizontal == AlignmentX.Center || LocationLockHorizontal == AlignmentX.Right)
            {
                dax = new DoubleAnimationUsingKeyFrames();
                dax.KeyFrames.Add(new EasingDoubleKeyFrame(w.Left, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 00))));
                switch (LocationLockHorizontal)
                {
                    case AlignmentX.Center:
                        dax.KeyFrames.Add(new EasingDoubleKeyFrame(w.Left - changeWidth / 2.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));
                        break;
                    case AlignmentX.Right:
                        dax.KeyFrames.Add(new EasingDoubleKeyFrame(w.Left - changeWidth, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));
                        break;
                }
                dax.Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec));

                dax.AccelerationRatio = 0.4; dax.DecelerationRatio = 0.6;

                Storyboard.SetTarget(dax, w);
                Storyboard.SetTargetProperty(dax, new PropertyPath(Window.LeftProperty));
                sb.Children.Add(dax); 
            }

            // animate window move vertical 
            if (LocationLockVertical == AlignmentY.Center || LocationLockVertical == AlignmentY.Bottom)
            {
                day = new DoubleAnimationUsingKeyFrames();
                day.KeyFrames.Add(new EasingDoubleKeyFrame(w.Top, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 00))));
                day.KeyFrames.Add(new EasingDoubleKeyFrame(w.Top, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec / 2))));

                switch (LocationLockVertical)
                {
                    case AlignmentY.Center:
                        day.KeyFrames.Add(new EasingDoubleKeyFrame(w.Top - changeWidth / 2.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec))));
                        break;
                    case AlignmentY.Bottom:
                        day.KeyFrames.Add(new EasingDoubleKeyFrame(w.Top - changeHeight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMilisec))));
                        break;
                }
                day.Duration = new Duration(TimeSpan.FromMilliseconds(durationMilisec));
                day.AccelerationRatio = 0.4; day.DecelerationRatio = 0.6;

                Storyboard.SetTarget(day, w);
                Storyboard.SetTargetProperty(day, new PropertyPath(Window.TopProperty));
                sb.Children.Add(day); 
            }
            return sb;
        }
    }
}
