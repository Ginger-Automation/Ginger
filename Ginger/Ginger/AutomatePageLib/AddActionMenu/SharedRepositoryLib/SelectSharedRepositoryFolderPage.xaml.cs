using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GingerWPF.GeneralLib;
using amdocs.ginger.GingerCoreNET;
using GingerCore.Activities;
using GingerCore.Actions;
using GingerCore.Variables;

namespace Ginger.Repository
{
    public enum SharedRepoRootFilter
    {
        All,
        Activities,
        Actions,
        ActivitiesGroups,
        Variables
    }

    public partial class SelectSharedRepositoryFolderPage : Page
    {
        private readonly SharedRepoRootFilter _filter;

        public RepositoryFolderBase SelectedFolder { get; private set; }

        public SelectSharedRepositoryFolderPage(SharedRepoRootFilter filter = SharedRepoRootFilter.All)
        {
            InitializeComponent();
            _filter = filter;

            // tree setup - folders only
            xFolderTree.EnableDragDrop = false;
            xFolderTree.EnableRightClick = true;
            xFolderTree.TreeChildFolderOnly = true;
            xFolderTree.ClearTreeItems();

            // Add only the requested root(s)
            switch (_filter)
            {
                case SharedRepoRootFilter.ActivitiesGroups:
                    xFolderTree.AddItem(new SharedActivitiesGroupsFolderTreeItem(
                        WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ActivitiesGroup>()));
                    break;

                case SharedRepoRootFilter.Activities:
                    xFolderTree.AddItem(new SharedActivitiesFolderTreeItem(
                        WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>()));
                    break;

                case SharedRepoRootFilter.Actions:
                    xFolderTree.AddItem(new SharedActionsFolderTreeItem(
                        WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>()));
                    break;

                case SharedRepoRootFilter.Variables:
                    xFolderTree.AddItem(new SharedVariablesFolderTreeItem(
                        WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<VariableBase>()));
                    break;
                case SharedRepoRootFilter.All:
                default:
                    xFolderTree.AddItem(new SharedRepositoryTreeItem());
                    break;
            }

            xFolderTree.ItemDoubleClick += (_, _) => TryAcceptSelection();
            xFolderTree.ItemSelected += (_, _) => { /* no-op; OK enabled on selection in ShowWindow */ };
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

        public static RepositoryFolderBase ShowWindow(Window owner = null, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, SharedRepoRootFilter filter = SharedRepoRootFilter.All)
        {
            SelectSharedRepositoryFolderPage page = new(filter);

            // buttons
            ObservableList<Button> buttons = [];
            Button okBtn = new() { Content = "Select" };
            Button cancelBtn = new() { Content = "Cancel" };

            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(okBtn, nameof(ButtonBase.Click), (_, _) =>
            {
                page.TryAcceptSelection();
            });
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(cancelBtn, nameof(ButtonBase.Click), (_, _) =>
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