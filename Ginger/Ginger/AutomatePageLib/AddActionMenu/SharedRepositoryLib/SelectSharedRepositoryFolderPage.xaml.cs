using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GingerWPF.GeneralLib;

namespace Ginger.Repository
{
    public partial class SelectSharedRepositoryFolderPage : Page
    {
        public RepositoryFolderBase SelectedFolder { get; private set; }

        public SelectSharedRepositoryFolderPage()
        {
            InitializeComponent();

            // tree setup - folders only
            xFolderTree.EnableDragDrop = false;
            xFolderTree.EnableRightClick = true;
            xFolderTree.TreeChildFolderOnly = true;
            xFolderTree.ClearTreeItems();
            xFolderTree.AddItem(new SharedRepositoryTreeItem());

            xFolderTree.ItemDoubleClick += (_, __) => TryAcceptSelection();
            xFolderTree.ItemSelected += (_, __) => { /* no-op; OK enabled on selection in ShowWindow */ };
        }

        private void TryAcceptSelection()
        {
            var itvi = xFolderTree.CurrentSelectedTreeViewItem;
            if (itvi?.NodeObject() is RepositoryFolderBase folder)
            {
                SelectedFolder = folder;
                Window.GetWindow(this)?.Close();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        public static RepositoryFolderBase ShowWindow(Window owner = null, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            SelectSharedRepositoryFolderPage page = new();

            // buttons
            ObservableList<Button> buttons = [];
            Button okBtn = new() { Content = "Select" };
            Button cancelBtn = new() { Content = "Cancel" };

            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(okBtn, nameof(ButtonBase.Click), (_, __) =>
            {
                page.TryAcceptSelection();
            });
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(cancelBtn, nameof(ButtonBase.Click), (_, __) =>
            {
                Window.GetWindow(page)?.Close();
            });

            buttons.Add(okBtn);
            buttons.Add(cancelBtn);

            GenericWindow genWin = null;
            GingerCore.General.LoadGenericWindow(ref genWin, owner, windowStyle, "Select Shared Repository Folder", page, buttons, true, "Close");

            return page.SelectedFolder;
        }
    }
}