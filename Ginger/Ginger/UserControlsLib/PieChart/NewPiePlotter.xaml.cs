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

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media.Animation;
using Ginger.ScottLogic.PieChart;


namespace Ginger.UserControlsLib.PieChart
{
    /// <summary>
    /// Renders a bound dataset as a pie chart
    /// </summary>
    public partial class NewPiePlotter : UserControl
    {
        #region dependency properties

        /// <summary>
        /// The property of the bound object that will be plotted
        /// </summary>
        public String PlottedProperty
        {
            get { return NewPieChartLayout.GetPlottedProperty(this); }
            set { NewPieChartLayout.SetPlottedProperty(this, value); }
        }

        /// <summary>
        /// A class which selects a color based on the item being rendered.
        /// </summary>
        public IColorSelector ColorSelector
        {
            get { return NewPieChartLayout.GetColorSelector(this); }
            set { NewPieChartLayout.SetColorSelector(this, value); }
        }


        /// <summary>
        /// The size of the hole in the center of circle (as a percentage)
        /// </summary>
        public double HoleSize
        {
            get { return (double)GetValue(HoleSizeProperty); }
            set
            {
                SetValue(HoleSizeProperty, value);
                ConstructPiePieces();
            }
        }

        public static readonly DependencyProperty HoleSizeProperty =
                       DependencyProperty.Register("HoleSize", typeof(double), typeof(NewPiePlotter), new UIPropertyMetadata(0.0));
        
        #endregion
        
        /// <summary>
        /// A list which contains the current piece pieces, where the piece index
        /// is the same as the index of the item within the collection view which 
        /// it represents.
        /// </summary>
        private List<PiePiece> piePieces = new List<PiePiece>();

        public NewPiePlotter()
        {
            // register any dependency property change handlers
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(NewPieChartLayout.PlottedPropertyProperty, typeof(NewPiePlotter));
            dpd.AddValueChanged(this, PlottedPropertyChanged);

            InitializeComponent();

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(DataContextChangedHandler);
        }

        #region property change handlers

        /// <summary>
        /// Handle changes in the datacontext. When a change occurs handlers are registered for events which
        /// occur when the collection changes or any items within the collection change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DataContextChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            // handle the events that occur when the bound collection changes
            if (this.DataContext is INotifyCollectionChanged)
            {
                INotifyCollectionChanged observable = (INotifyCollectionChanged)this.DataContext;
                observable.CollectionChanged += new NotifyCollectionChangedEventHandler(BoundCollectionChanged);
            }

            // handle the selection change events
            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);
            collectionView.CurrentChanged += new EventHandler(CollectionViewCurrentChanged);
            collectionView.CurrentChanging += new CurrentChangingEventHandler(CollectionViewCurrentChanging);

            ConstructPiePieces();
            ObserveBoundCollectionChanges();
        }

        /// <summary>
        /// Handles changes to the PlottedProperty property.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlottedPropertyChanged(object sender, EventArgs e)
        {
            ConstructPiePieces();
        }

        #endregion

        #region event handlers

        /// <summary>
        /// Handles the MouseUp event from the individual Pie Pieces
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PiePieceMouseUp(object sender, MouseButtonEventArgs e)
        {
            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);
            if (collectionView == null)
                return;

            PiePiece piece = sender as PiePiece;
            if (piece == null)
                return;

            // select the item which this pie piece represents
            int index = (int)piece.Tag;
            collectionView.MoveCurrentToPosition(index);
        }

        /// <summary>
        /// Handles the event which occurs when the selected item is about to change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CollectionViewCurrentChanging(object sender, CurrentChangingEventArgs e)
        {
            CollectionView collectionView = (CollectionView)sender;

            if (collectionView != null && collectionView.CurrentPosition >= 0 && collectionView.CurrentPosition <= piePieces.Count)
            {
                PiePiece piece = piePieces[collectionView.CurrentPosition];

                DoubleAnimation a = new DoubleAnimation();
                a.To = 0;
                a.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            }
        }

        /// <summary>
        /// Handles the event which occurs when the selected item has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CollectionViewCurrentChanged(object sender, EventArgs e)
        {
            CollectionView collectionView = (CollectionView)sender;

            if (collectionView != null && collectionView.CurrentPosition >= 0 && collectionView.CurrentPosition <= piePieces.Count)
            {
                PiePiece piece = piePieces[collectionView.CurrentPosition];

                DoubleAnimation a = new DoubleAnimation();
                a.To = 10;
                a.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            }
        }

        /// <summary>
        /// Handles events which are raised when the bound collection changes (i.e. items added/removed)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoundCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ConstructPiePieces();
            ObserveBoundCollectionChanges();
        }

        /// <summary>
        /// Iterates over the items in the bound collection, adding handlers for PropertyChanged events
        /// </summary>
        private void ObserveBoundCollectionChanges()
        {
            CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);

            foreach(object item in myCollectionView)
            {
                if (item is INotifyPropertyChanged)
                {
                    INotifyPropertyChanged observable = (INotifyPropertyChanged)item;
                    observable.PropertyChanged += new PropertyChangedEventHandler(ItemPropertyChanged);
                }
            }
        }
        
        /// <summary>
        /// Handles events which occur when the properties of bound items change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // if the property which this pie chart represents has changed, re-construct the pie
            if (e.PropertyName.Equals(PlottedProperty))
            {
                ConstructPiePieces();
            }
        }

        #endregion

        private double GetPlottedPropertyValue(object item)
        {
            PropertyDescriptorCollection filterPropDesc = TypeDescriptor.GetProperties(item);
            object itemValue = filterPropDesc[PlottedProperty].GetValue(item);

            //TODO possible type conversion?

            return (double)itemValue;
        }

        /// <summary>
        /// Constructs pie pieces and adds them to the visual tree for this control's canvas
        /// </summary>
        private void ConstructPiePieces()
        {
            CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);
            if (myCollectionView == null)
                return;

            double halfWidth = this.Width / 2;
            double innerRadius = halfWidth * HoleSize;            

            // compute the total for the property which is being plotted
            double total = 0;
            foreach (Object item in myCollectionView)
            {
                total += GetPlottedPropertyValue(item);
            }
            
            // add the pie pieces
            canvas.Children.Clear();
            piePieces.Clear();
                        
            double accumulativeAngle=0;
            foreach (Object item in myCollectionView)
            {
                double wedgeAngle = GetPlottedPropertyValue(item) * 360 / total;

                PiePiece piece = new PiePiece()
                    {
                        Radius = halfWidth,
                        InnerRadius = innerRadius,
                        CentreX = halfWidth,
                        CentreY = halfWidth,
                        WedgeAngle = wedgeAngle,
                        PieceValue = GetPlottedPropertyValue(item),
                        RotationAngle = accumulativeAngle,
                        Fill = ColorSelector != null ? ColorSelector.SelectBrush(item, myCollectionView.IndexOf(item)) : Brushes.Black,
                        // record the index of the item which this pie slice represents
                        Tag = myCollectionView.IndexOf(item),
                        ToolTip = new ToolTip()
                    };

                piePieces.Add(piece);
                canvas.Children.Insert(0, piece);
                accumulativeAngle += wedgeAngle;
            }
        }
    }
}
