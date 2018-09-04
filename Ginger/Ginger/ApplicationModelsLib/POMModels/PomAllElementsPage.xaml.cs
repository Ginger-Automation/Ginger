﻿using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for PomAllElementsPage.xaml
    /// </summary>
    public partial class PomAllElementsPage : Page
    {
        ApplicationPOMModel mPOM;
        public IWindowExplorer mWinExplorer;

        public enum eElementsContext
        {
            Mapped,
            Unmapped

        }

        public PomElementsPage mappedUIElementsPage;
        public PomElementsPage unmappedUIElementsPage;

        public PomAllElementsPage(ApplicationPOMModel POM, IWindowExplorer winExplorer)
        {
            InitializeComponent();
            mPOM = POM;
            mPOM.MappedUIElements.CollectionChanged += MappedUIElements_CollectionChanged;
            mPOM.UnMappedUIElements.CollectionChanged += UnMappedUIElements_CollectionChanged;

            mWinExplorer = winExplorer;

            mappedUIElementsPage = new PomElementsPage(mPOM, eElementsContext.Mapped, mWinExplorer);
            xMappedElementsFrame.Content = mappedUIElementsPage;

            unmappedUIElementsPage = new PomElementsPage(mPOM, eElementsContext.Unmapped, mWinExplorer);
            xUnMappedElementsFrame.Content = unmappedUIElementsPage;

            UnMappedUIElementsUpdate();
            MappedUIElementsUpdate();
        }

        private void UnMappedUIElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UnMappedUIElementsUpdate();
        }

        private void UnMappedUIElementsUpdate()
        {
            Dispatcher.Invoke(() =>
            {
                xUnMappedElementsTextBlock.Text = string.Format("Unmapped Elements ({0})", mPOM.UnMappedUIElements.Count);
            });
        }


        private void MappedUIElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MappedUIElementsUpdate();
        }

        private void MappedUIElementsUpdate()
        {
            Dispatcher.Invoke(() =>
            {
                xMappedElementsTextBlock.Text = string.Format("Mapped Elements ({0})", mPOM.MappedUIElements.Count);
            });
        }

        private void ActionTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void SetWindowExplorer(IWindowExplorer windowExplorerDriver)
        {
            mWinExplorer = windowExplorerDriver;
            mappedUIElementsPage.SetWindowExplorer(windowExplorerDriver);
            unmappedUIElementsPage.SetWindowExplorer(windowExplorerDriver);
        }
    }
}
