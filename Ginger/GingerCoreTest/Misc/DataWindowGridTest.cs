#region License
/*
Copyright © 2014-2026 European Support Limited

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

extern alias UIAComWrapperNetstandard;
using Accessibility;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.Common.LegacyAutomation;
using GingerCore.Drivers.PBDriver;
using GingerCore.Drivers.PBDriver.DesktopAutomation;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCoreTest.Misc
{
    /// <summary>
    /// A PowerBuilder DataWindow that UI Automation lists no cells of, which is what a PowerBuilder 12.5
    /// application published through Citrix reaches Ginger as, has to be worked through its accessible
    /// object, and every DataWindow UI Automation can see into has to be left exactly as it was.
    /// </summary>
    /// <remarks>
    /// The stand-in windows here answer the request for an accessible object the way PowerBuilder does,
    /// from a thread of their own, so the reads cross threads the way they cross processes against the
    /// real control. None of these tests uses the mouse or the keyboard: the clicks are window messages,
    /// and the one test that would need the keyboard runs only where the desktop cannot take it.
    /// </remarks>
    [TestClass]
    public class DataWindowGridTest
    {
        private static readonly string[] ItemListColumns = ["item_code", "item_name", "active"];

        #region Reading the cells

        [TestMethod]
        public void Describe_TakesTheNamesOnEveryRowAsTheColumnsAndLeavesOutHeadersAndFooters()
        {
            string[] names = ["item_code_t", "item_code", "item_name", "item_code", "item_name", "item_code", "item_name", "total_t"];

            DataWindowGrid.Describe(names, out List<string> columns, out int rows);

            CollectionAssert.AreEqual(new[] { "item_code", "item_name" }, columns);
            Assert.AreEqual(3, rows);
        }

        [TestMethod]
        public void Describe_IgnoresCellsWithoutANameAndReadsAnEmptyGridAsHavingNoColumns()
        {
            DataWindowGrid.Describe(["", null, "a", "a"], out List<string> columns, out int rows);
            CollectionAssert.AreEqual(new[] { "a" }, columns);
            Assert.AreEqual(2, rows);

            DataWindowGrid.Describe([], out columns, out rows);
            Assert.AreEqual(0, columns.Count);
            Assert.AreEqual(0, rows);
        }

        [TestMethod]
        public void Describe_OfASingleRowLeavesOutTheHeaderOfEveryColumnItNames()
        {
            // With one row a header appears as often as a column does, so it is told by its name.
            string[] names = ["item_code_t", "item_name_t", "item_code", "item_name", "total_t"];

            DataWindowGrid.Describe(names, out List<string> columns, out int rows);

            CollectionAssert.AreEqual(new[] { "item_code", "item_name", "total_t" }, columns, "total_t heads no column that is listed, so it is not taken for a header");
            Assert.AreEqual(1, rows);
        }

        [TestMethod]
        public void Describe_OfSeveralRowsKeepsEveryNameThatHasACellOnEveryRow()
        {
            string[] names = ["price", "price_t", "price", "price_t"];

            DataWindowGrid.Describe(names, out List<string> columns, out int rows);

            CollectionAssert.AreEqual(new[] { "price", "price_t" }, columns, "a name that ends in _t is a column when it has a cell on every row");
            Assert.AreEqual(2, rows);
        }

        [TestMethod]
        public void Describe_OfAGridWithNoRowsTakesNoneOfItsHeadersForAColumn()
        {
            DataWindowGrid.Describe(["item_code_t", "item_name_t"], out List<string> columns, out int rows);

            Assert.AreEqual(0, columns.Count);
            Assert.AreEqual(0, rows);
        }

        [TestMethod]
        public void Describe_OfAListingCutShortKeepsTheColumnsItsLastRowDidNotReachAndLeavesThatRowOut()
        {
            string[] names = ["item_code_t", "item_name_t", "item_code", "item_name", "item_code", "item_name", "item_code", "item_name", "item_code"];

            DataWindowGrid.Describe(names, out List<string> columns, out int rows, listingCutShort: true);

            CollectionAssert.AreEqual(new[] { "item_code", "item_name" }, columns);
            Assert.AreEqual(3, rows);
        }

        [TestMethod]
        public void TryRead_ListsTheSameColumnsForAGridWithOneRowAsForOneWithMany()
        {
            Assert.IsTrue(DataWindowGrid.TryRead(FakeDataWindowModel.FlaggedList(rows: 1).Root, out DataWindowGrid one, out string details), details);
            Assert.IsTrue(DataWindowGrid.TryRead(FakeDataWindowModel.FlaggedList().Root, out DataWindowGrid many, out details), details);

            CollectionAssert.AreEqual(many.ColumnNames.ToList(), one.ColumnNames.ToList());
            CollectionAssert.AreEqual(FakeDataWindowModel.FlaggedColumns, one.ColumnNames.ToList());
            Assert.AreEqual(1, one.RowCount);
            Assert.AreEqual("Item Code", one.CaptionOf("item_code"), "the headers are left out of the columns, not out of the grid");
        }

        [TestMethod]
        public void IsInside_MeansTheCentreOfTheElementLiesInTheArea()
        {
            Rectangle area = new Rectangle(0, 0, 100, 50);

            Assert.IsTrue(DataWindowGrid.IsInside(area, new Rectangle(10, 10, 20, 20)), "a cell");
            Assert.IsTrue(DataWindowGrid.IsInside(area, new Rectangle(80, 10, 30, 20)), "a cell cut off by the edge keeps its centre inside");
            Assert.IsFalse(DataWindowGrid.IsInside(area, new Rectangle(100, 0, 17, 50)), "a scroll bar lies along the edge, outside");
            Assert.IsFalse(DataWindowGrid.IsInside(area, new Rectangle(95, 10, 30, 20)), "an element mostly outside");
            Assert.IsFalse(DataWindowGrid.IsInside(area, new Rectangle(10, 10, 0, 0)), "an element with no size");
        }

        [TestMethod]
        public void OffsetInGrid_IsMeasuredFromTheTopLeftOfTheGridAndDefaultsToTheCentreOfTheCell()
        {
            Rectangle grid = new Rectangle(100, 200, 400, 240);
            Rectangle cell = new Rectangle(110, 252, 100, 20);

            Assert.AreEqual("60,62", DataWindowGrid.OffsetInGrid(grid, cell, null), "no point names the centre of the cell");
            Assert.AreEqual("15,58", DataWindowGrid.OffsetInGrid(grid, cell, "5,6"), "a point is measured from the cell, not from the grid");
            Assert.AreEqual("17,60", DataWindowGrid.OffsetInGrid(grid, cell, " 7 , 8 "), "blanks around the numbers are ignored");
            Assert.AreEqual("60,62", DataWindowGrid.OffsetInGrid(grid, cell, "not a point"), "something that is not a point is the centre too");
        }

        [TestMethod]
        public void FindRow_AppliesEachOperatorToTheValuesWithoutTheirPadding()
        {
            string[] values = ["  21 ", "22", "", "23-A"];

            Assert.AreEqual(0, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.Equals, "21"), "padding is not part of the value");
            Assert.AreEqual(1, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.Equals, " 22 "), "nor is padding on the expected value");
            Assert.AreEqual(1, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.NotEquals, "21"));
            Assert.AreEqual(3, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.Contains, "3-"));
            Assert.AreEqual(2, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.NotContains, "2"));
            Assert.AreEqual(3, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.StartsWith, "23"));
            Assert.AreEqual(2, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.NotStartsWith, "2"));
            Assert.AreEqual(3, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.EndsWith, "-A"));
            Assert.AreEqual(1, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.NotEndsWith, "1"));
            Assert.AreEqual(-1, DataWindowGrid.FindRow(values, ActTableElement.eRunColOperator.Equals, "nothing like it"));
            Assert.AreEqual(1, DataWindowGrid.FindRow(["a", null], ActTableElement.eRunColOperator.Equals, null), "a cell that shows nothing equals nothing");
        }

        [TestMethod]
        public void FindRow_RefusesAnOperatorItDoesNotKnowRatherThanMatchingTheFirstRow()
        {
            Assert.ThrowsException<NotImplementedException>(
                () => DataWindowGrid.FindRow(["a"], (ActTableElement.eRunColOperator)99, "a"));
        }

        [TestMethod]
        public void Cell_ReportsWhatTheAccessibleObjectShowsOfIt()
        {
            FakeDataWindowModel model = FakeDataWindowModel.ItemList();
            IAccessible root = model.Root;

            DataWindowCell code = DataWindowCell.TryCreate(root, ChildIdOf(model, 3));
            DataWindowCell name = DataWindowCell.TryCreate(root, ChildIdOf(model, 4));
            DataWindowCell unchecked1 = DataWindowCell.TryCreate(root, ChildIdOf(model, 5));
            DataWindowCell checked2 = DataWindowCell.TryCreate(root, ChildIdOf(model, 8));

            Assert.AreEqual("item_code", code.Name);
            Assert.AreEqual("101", code.Value);
            Assert.AreEqual("101", code.Text);
            Assert.IsFalse(code.IsCheckBox);
            Assert.AreEqual("Alpha      ", name.Value, "the value is what the DataWindow shows, padding included");
            Assert.IsTrue(unchecked1.IsCheckBox);
            Assert.IsFalse(unchecked1.IsChecked);
            Assert.AreEqual("Unchecked", unchecked1.Text, "a check box shows its state, not the letter the column stores");
            Assert.IsTrue(checked2.IsChecked);
            Assert.AreEqual("Checked", checked2.Text);
            Assert.IsTrue(code.TryGetBounds(out Rectangle bounds));
            Assert.AreEqual(model.ScreenBoundsOf(model.Cells[3]), bounds);
        }

        [TestMethod]
        public void Cell_IsMadeForACellFlaggedInvisible_ButNotForOneWithNoNameOrOneThatIsNotThere()
        {
            FakeDataWindowModel flaggedEvery = FakeDataWindowModel.FlaggedList(1);
            int firstDataCell = ChildIdOf(flaggedEvery, 4);
            Assert.AreEqual(FakeCell.StateInvisible, Convert.ToInt32(flaggedEvery.Root.get_accState(firstDataCell)) & FakeCell.StateInvisible, "the stand-in flags the cell");

            DataWindowCell flagged = DataWindowCell.TryCreate(flaggedEvery.Root, firstDataCell);
            Assert.IsNotNull(flagged, "PowerBuilder 12.5 flags every cell invisible, so the flag cannot decide that there is no cell");
            Assert.AreEqual("item_code", flagged.Name);

            FakeDataWindowModel model = FakeDataWindowModel.ItemList();
            Assert.IsNull(DataWindowCell.TryCreate(model.Root, ChildIdOf(model, 12)), "a part with no name to find it by");
            Assert.IsNull(DataWindowCell.TryCreate(null, 0), "nothing at all");
            Assert.IsNull(DataWindowCell.TryCreate(model.Root, 999), "a part the control has no record of");
        }

        [TestMethod]
        public void Cell_OfACheckBoxChangesStateThroughItsDefaultActionAndReportsAnActionItDoesNotHave()
        {
            FakeDataWindowModel model = FakeDataWindowModel.ItemList();
            DataWindowCell box = DataWindowCell.TryCreate(model.Root, ChildIdOf(model, 5));
            DataWindowCell text = DataWindowCell.TryCreate(model.Root, ChildIdOf(model, 3));

            Assert.IsTrue(box.TryDoDefaultAction());
            Assert.IsTrue(box.IsChecked);
            Assert.IsTrue(box.TryDoDefaultAction(), "the same action undoes it");
            Assert.IsFalse(box.IsChecked);
            Assert.IsFalse(text.TryDoDefaultAction(), "a text cell has no default action to take");
        }

        [TestMethod]
        public void TryRead_GroupsTheListedCellsIntoColumnsAndRowsWhetherTheyAreObjectsOrChildIds()
        {
            foreach (bool nested in new[] { true, false })
            {
                FakeDataWindowModel model = FakeDataWindowModel.ItemList();
                model.NestedChildren = nested;

                bool read = DataWindowGrid.TryRead(model.Root, out DataWindowGrid grid, out string details);

                Assert.IsTrue(read, "nested=" + nested + ": " + details);
                CollectionAssert.AreEqual(ItemListColumns, grid.ColumnNames.ToList(), "nested=" + nested);
                Assert.AreEqual(3, grid.RowCount, "the part with no name is not a row; nested=" + nested);
                CollectionAssert.AreEqual(new[] { "101", "102", "103" }, grid.Column("item_code").Select(cell => cell.Value).ToList(), "nested=" + nested);
            }
        }

        [TestMethod]
        public void TryRead_SaysWhyWhenTheControlHasNoCells()
        {
            FakeDataWindowModel model = new FakeDataWindowModel();

            Assert.IsFalse(DataWindowGrid.TryRead(model.Root, out DataWindowGrid grid, out string details));

            Assert.IsNull(grid);
            StringAssert.Contains(details, "lists 0 children", details);
        }

        [TestMethod]
        public void TryRead_ReportsAControlThatStopsAnswering()
        {
            FakeDataWindowModel failing = FakeDataWindowModel.ItemList();
            failing.FailsToListChildren = true;

            Assert.IsFalse(DataWindowGrid.TryRead(failing.Root, out DataWindowGrid grid, out string details));

            Assert.IsNull(grid);
            StringAssert.Contains(details, "failed", details);
        }

        [TestMethod]
        public void Grid_AnswersForAColumnItDoesNotHaveWithNoCellsAndAnUnknownNameWithNone()
        {
            FakeDataWindowModel model = FakeDataWindowModel.ItemList();
            Assert.IsTrue(DataWindowGrid.TryRead(model.Root, out DataWindowGrid grid, out string details), details);

            Assert.IsTrue(grid.HasColumn("item_name"));
            Assert.IsFalse(grid.HasColumn("Item_Name"), "names are the DataWindow's, and are matched as written");
            Assert.IsFalse(grid.HasColumn(null));
            Assert.AreEqual(0, grid.Column("nothing").Count);
            Assert.AreEqual(0, grid.Column(null).Count);
        }

        [TestMethod]
        public void CaptionOf_IsWhatTheHeaderNamedAfterTheColumnShows_AndNothingForAColumnWithNoHeader()
        {
            FakeDataWindowModel model = FakeDataWindowModel.FlaggedList(2);
            Assert.IsTrue(DataWindowGrid.TryRead(model.Root, out DataWindowGrid grid, out string details), details);

            Assert.AreEqual("Item Code", grid.CaptionOf("item_code"));
            Assert.AreEqual("Quantity", grid.CaptionOf("item_qty"));
            Assert.IsNull(grid.CaptionOf("item_calc"), "a column with no header");
            Assert.IsNull(grid.CaptionOf("nothing"), "a name that is no column");
            Assert.IsNull(grid.CaptionOf(null));
        }

        [TestMethod]
        public void ColumnsCaptioned_IgnoresTheCaseAndTheWhiteSpaceAroundAndInsideTheCaption()
        {
            FakeDataWindowModel model = FakeDataWindowModel.ItemList();
            model.Cells[1] = new FakeCell("item_name_t", " Item\r\n  Name ", new Rectangle(120, 6, 160, 20), FakeCell.RoleStaticText);
            Assert.IsTrue(DataWindowGrid.TryRead(model.Root, out DataWindowGrid grid, out string details), details);

            Assert.AreEqual("Item Name", grid.CaptionOf("item_name"), "a caption broken over lines is read as one line");
            CollectionAssert.AreEqual(new[] { "item_name" }, grid.ColumnsCaptioned("Item Name").ToList());
            CollectionAssert.AreEqual(new[] { "item_name" }, grid.ColumnsCaptioned("ITEM NAME").ToList());
            CollectionAssert.AreEqual(new[] { "item_name" }, grid.ColumnsCaptioned("  item   name ").ToList());
            Assert.AreEqual(0, grid.ColumnsCaptioned("Item").Count, "part of a caption is not the caption");
            Assert.AreEqual(0, grid.ColumnsCaptioned("   ").Count);
            Assert.AreEqual(0, grid.ColumnsCaptioned(null).Count);
        }

        [TestMethod]
        public void ColumnsCaptioned_ListsEveryColumnWhoseHeaderShowsTheCaption()
        {
            FakeDataWindowModel model = FakeDataWindowModel.ItemList();
            model.Cells[1] = new FakeCell("item_name_t", "Item Code", new Rectangle(120, 6, 160, 20), FakeCell.RoleStaticText);
            Assert.IsTrue(DataWindowGrid.TryRead(model.Root, out DataWindowGrid grid, out string details), details);

            CollectionAssert.AreEqual(new[] { "item_code", "item_name" }, grid.ColumnsCaptioned("Item Code").ToList());
            CollectionAssert.AreEqual(new[] { "active" }, grid.ColumnsCaptioned("Active").ToList());
        }

        [TestMethod]
        public void DescribeColumns_PutsTheCaptionOfEachColumnNextToItsName()
        {
            FakeDataWindowModel model = FakeDataWindowModel.FlaggedList(2);
            Assert.IsTrue(DataWindowGrid.TryRead(model.Root, out DataWindowGrid grid, out string details), details);

            Assert.AreEqual("item_code (Item Code), item_text (Description), item_day (Day), item_qty (Quantity), item_calc", grid.DescribeColumns());
        }

        #endregion

        #region The decision to read through the accessible object

        [TestMethod]
        public void TryReadDataWindowCells_ReadsADataWindowUiAutomationListsNoCellOf()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());

            bool read = UIAComWrapperHelper.TryReadDataWindowCells(window.Element, NothingListed(), out DataWindowGrid cells);

            Assert.IsTrue(read);
            CollectionAssert.AreEqual(ItemListColumns, cells.ColumnNames.ToList());
            Assert.AreEqual(3, cells.RowCount);
        }

        [TestMethod]
        public void TryReadDataWindowCells_StillReadsWhenTheOnlyElementsListedAreOutsideTheCellArea()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());

            // What the scan of a DataWindow that is blind to its cells finds are the parts of its scroll
            // bars, which lie outside the client area. The desktop does too, and is the one element that
            // is always there to stand in for them.
            Dictionary<string, UIAuto.AutomationElement[]> found = new Dictionary<string, UIAuto.AutomationElement[]>
            {
                ["Position"] = [UIAuto.AutomationElement.RootElement]
            };

            Assert.IsTrue(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, found, out DataWindowGrid cells));
            Assert.AreEqual(3, cells.RowCount);
        }

        [TestMethod]
        public void TryReadDataWindowCells_DoesNotTakeAScrollBarInsideTheCellAreaForACell()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList(), scrollBarInClientArea: true);
            Assert.IsTrue(DataWindowGrid.TryGetClientArea(window.Handle, out Rectangle area));
            Assert.AreEqual(UIAuto.ControlType.ScrollBar.Id, window.ScrollBar.Current.ControlType.Id, "the stand-in's scroll bar is one");
            Assert.IsTrue(DataWindowGrid.IsInside(area, window.ScrollBar.Current.BoundingRectangle),
                "the stand-in's scroll bar has to lie inside the client area, or this proves nothing");

            // A grid that draws its own scroll bars, and cannot be seen into, would have them found where
            // its cells should be.
            Dictionary<string, UIAuto.AutomationElement[]> found = new Dictionary<string, UIAuto.AutomationElement[]>
            {
                ["Scroll bar"] = [window.ScrollBar]
            };

            Assert.IsTrue(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, found, out DataWindowGrid cells));
            CollectionAssert.AreEqual(ItemListColumns, cells.ColumnNames.ToList());
        }

        [TestMethod]
        public void TryReadDataWindowCells_DoesNotTakeTheThumbOfAScrollBarInsideTheCellAreaForACell()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList(), scrollBarInClientArea: true);
            UIAuto.AutomationElement thumb = ThumbOf(window.ScrollBar);
            if (thumb == null)
            {
                Assert.Inconclusive("UI Automation lists no thumb under the stand-in's scroll bar on this machine.");
            }

            Assert.IsTrue(DataWindowGrid.TryGetClientArea(window.Handle, out Rectangle area));
            Assert.IsTrue(DataWindowGrid.IsInside(area, thumb.Current.BoundingRectangle),
                "the stand-in's thumb has to lie inside the client area, or this proves nothing");

            Dictionary<string, UIAuto.AutomationElement[]> found = new Dictionary<string, UIAuto.AutomationElement[]>
            {
                [thumb.Current.Name] = [thumb]
            };

            Assert.IsTrue(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, found, out DataWindowGrid cells));
            CollectionAssert.AreEqual(ItemListColumns, cells.ColumnNames.ToList());
        }

        [TestMethod]
        public void TryReadDataWindowCells_LeavesADataWindowUiAutomationCanSeeIntoAloneAndNeverReadsItsAccessibleObject()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());

            // The window's own element covers the client area, which puts its centre among the cells.
            Dictionary<string, UIAuto.AutomationElement[]> found = new Dictionary<string, UIAuto.AutomationElement[]>
            {
                ["item_code"] = [window.Element]
            };
            WarmUp(window);
            int readsBefore = window.Model.ChildCountReads;

            Assert.IsFalse(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, found, out DataWindowGrid cells));

            Assert.IsNull(cells);
            Assert.AreEqual(readsBefore, window.Model.ChildCountReads, "a grid that is already listed costs no accessible read");
        }

        [TestMethod]
        public void TryReadDataWindowCells_LeavesEveryWindowThatIsNotADataWindowAlone()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList(), "pbxx172");
            WarmUp(window);
            int readsBefore = window.Model.ChildCountReads;

            Assert.IsFalse(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, NothingListed(), out DataWindowGrid cells));

            Assert.IsNull(cells);
            Assert.AreEqual(readsBefore, window.Model.ChildCountReads, "only a DataWindow is read this way");
        }

        [TestMethod]
        public void TryReadDataWindowCells_LeavesADataWindowThatIsNotShownAloneAndNeverReadsItsAccessibleObject()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            WarmUp(window);
            window.Hide();
            int readsBefore = window.Model.ChildCountReads;

            Assert.IsFalse(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, NothingListed(), out DataWindowGrid cells));

            Assert.IsNull(cells);
            Assert.AreEqual(readsBefore, window.Model.ChildCountReads, "a DataWindow that is not shown is not asked for its cells");
        }

        [TestMethod]
        public void ListsNamedCells_IsTrueForADataWindowWhoseFirstChildrenHaveNamesAndAsksForNoMoreThanTheFirstFew()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList());

            Assert.IsTrue(DataWindowGrid.ListsNamedCells(window.Handle));
            Assert.IsTrue(window.Model.ChildCountReads <= 2, "the children were counted " + window.Model.ChildCountReads + " times, by the code and by the call that lists them");
            Assert.IsTrue(window.Model.NameReads is >= 1 and <= 8, "the cells asked for their names were " + window.Model.NameReads + " of " + window.Model.Cells.Count);
        }

        [TestMethod]
        public void ListsNamedCells_IsFalseForADataWindowThatListsNothingOrNothingWithAName()
        {
            using FakeDataWindow empty = new FakeDataWindow(new FakeDataWindowModel());
            FakeDataWindowModel unnamed = new FakeDataWindowModel();
            unnamed.Add(string.Empty, "padding", 10, 10, 100);
            using FakeDataWindow nameless = new FakeDataWindow(unnamed);

            Assert.IsFalse(DataWindowGrid.ListsNamedCells(empty.Handle));
            Assert.IsFalse(DataWindowGrid.ListsNamedCells(nameless.Handle));
            Assert.IsFalse(DataWindowGrid.ListsNamedCells(IntPtr.Zero));
        }

        [TestMethod]
        public void ListsNamedCells_IsFalseForADataWindowThatDoesNotAnswerForItsAccessibleObjectOrFailsToListChildren()
        {
            FakeDataWindowModel failing = FakeDataWindowModel.ItemList();
            failing.FailsToListChildren = true;
            using FakeDataWindow silent = new FakeDataWindow(FakeDataWindowModel.ItemList()) { AnswersAccessibility = false };
            using FakeDataWindow broken = new FakeDataWindow(failing);

            Assert.IsFalse(DataWindowGrid.ListsNamedCells(silent.Handle));
            Assert.IsFalse(DataWindowGrid.ListsNamedCells(broken.Handle));
        }

        [TestMethod]
        public void TryReadDataWindowCells_FallsBackToTheOldPathWhenTheDataWindowListsNothingEither()
        {
            using FakeDataWindow window = new FakeDataWindow(new FakeDataWindowModel());

            Assert.IsFalse(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, NothingListed(), out DataWindowGrid cells));
            Assert.IsNull(cells);
        }

        [TestMethod]
        public void TryReadDataWindowCells_FallsBackToTheOldPathWhenTheDataWindowDoesNotAnswerForItsAccessibleObject()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList())
            {
                AnswersAccessibility = false
            };

            Assert.IsFalse(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, NothingListed(), out DataWindowGrid cells));
            Assert.IsNull(cells);
        }

        [TestMethod]
        public void TryReadDataWindowCells_FallsBackToTheOldPathWhenTheDataWindowFailsWhileBeingRead()
        {
            FakeDataWindowModel model = FakeDataWindowModel.ItemList();
            model.FailsToListChildren = true;
            using FakeDataWindow window = new FakeDataWindow(model);

            Assert.IsFalse(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, NothingListed(), out DataWindowGrid cells));
            Assert.IsNull(cells);
        }

        #endregion

        #region Running table actions against the cells

        [TestMethod]
        public void GetRowCount_CountsTheRowsOfTheCells()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            ActTableElement action = Table(ActTableElement.eTableAction.GetRowCount, "Row Number", "0", "item_code");

            Run(window, action);

            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            Assert.AreEqual("3", action.ExInfo);
            Assert.AreEqual("3", action.GetReturnParam("Actual"));
        }

        [TestMethod]
        public void GetValue_ReadsACellByRowNumberAndColumnTitle()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());

            Assert.AreEqual("101", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "item_code")));
            Assert.AreEqual("103", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "2", "item_code")));
            Assert.AreEqual("Beta", ValueOf(window, Table(ActTableElement.eTableAction.GetText, "Row Number", "1", "item_name")).Trim());
        }

        [TestMethod]
        public void GetValue_ReadsACellByColumnNumber()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            ActTableElement action = Table(ActTableElement.eTableAction.GetValue, "Row Number", "2", "1");
            action.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;

            Assert.AreEqual("Gamma", ValueOf(window, action).Trim());
        }

        [TestMethod]
        public void GetValue_OnAGridWithOneRow_ReadsTheColumnByNumberAndByTitleAndNeverAHeader()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList(rows: 1));
            ActTableElement first = Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "0");
            first.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            ActTableElement last = Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "4");
            last.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;

            Assert.AreEqual("101", ValueOf(window, first), "the first column is the first column and not its header");
            Assert.AreEqual("C1", ValueOf(window, last));
            Assert.AreEqual("100", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "Quantity")));
        }

        [TestMethod]
        public void GetValue_ReadsACheckBoxAsTheStateItIsIn()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());

            Assert.AreEqual("Unchecked", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "active")));
            Assert.AreEqual("Checked", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "1", "active")));
        }

        [TestMethod]
        public void GetValue_FindsTheRowByTheValueOfAColumn()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());

            ActTableElement byTitle = Where(ActTableElement.eTableAction.GetValue, "item_name", ActTableElement.eRunColOperator.StartsWith, "Bet", "item_code");
            ActTableElement byNumber = Where(ActTableElement.eTableAction.GetValue, "1", ActTableElement.eRunColOperator.Equals, "Gamma", "item_code");
            byNumber.WhereColSelector = ActTableElement.eRunColSelectorValue.ColNum;

            Assert.AreEqual("102", ValueOf(window, byTitle), "the padding the DataWindow puts on a value does not stop it matching");
            Assert.AreEqual("103", ValueOf(window, byNumber), "the column to search can be named by its position as well");
        }

        [TestMethod]
        public void GetValue_FindsAColumnByTheTitleItsHeaderShows_AsWellAsByItsName()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());

            Assert.AreEqual("Beta", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "1", "Item Name")).Trim());
            Assert.AreEqual("Gamma", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "2", " item  name ")).Trim(), "the case and the spacing of the title do not matter");
            Assert.AreEqual("Beta", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "1", "item_name")).Trim(), "the name of the column still works");
            Assert.AreEqual("Checked", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "1", "Active")));

            ActTableElement where = Where(ActTableElement.eTableAction.GetValue, "Item Name", ActTableElement.eRunColOperator.StartsWith, "Bet", "Item Code");
            Assert.AreEqual("102", ValueOf(window, where), "the column to search can be named by its title too");
        }

        [TestMethod]
        public void GetValue_OfATitleThatSeveralColumnsShow_AsksForTheNameOfTheColumn()
        {
            FakeDataWindowModel model = FakeDataWindowModel.ItemList();
            model.Cells[1] = new FakeCell("item_name_t", "Item Code", new Rectangle(120, 6, 160, 20), FakeCell.RoleStaticText);
            using FakeDataWindow window = new FakeDataWindow(model);

            Exception failure = Assert.ThrowsException<Exception>(() => Run(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "Item Code")));

            StringAssert.Contains(failure.Message, "Input Column Name Item Code is the title of more than one column in Grid: item_code, item_name");
            Assert.AreEqual("101", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "item_code")), "the name of the column is never in doubt");
        }

        [TestMethod]
        public void GetValue_OfAnyRowReadsOneOfTheRows()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());

            string value = ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Any Row", null, "item_code"));

            CollectionAssert.Contains(new[] { "101", "102", "103" }, value);
        }

        [TestMethod]
        public void GetValue_OfARowOrColumnThatIsNotThere_SaysSoTheWayEveryOtherGridDoes()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            UIAComWrapperHelper helper = CreateHelper();

            ActTableElement negative = Table(ActTableElement.eTableAction.GetValue, "Row Number", "-1", "item_code");
            helper.HandleDataWindowGridAction(window.Element, cells, negative);
            ActTableElement past = Table(ActTableElement.eTableAction.GetValue, "Row Number", "3", "item_code");
            helper.HandleDataWindowGridAction(window.Element, cells, past);
            ActTableElement selected = Table(ActTableElement.eTableAction.GetValue, "By Selected Row", null, "item_code");
            helper.HandleDataWindowGridAction(window.Element, cells, selected);

            Assert.AreEqual("Given Row Number -1 is not present in Column", negative.Error);
            Assert.AreEqual("Given Row Number 3 is not present in Column", past.Error);
            StringAssert.Contains(selected.Error, "'By Selected Row' is not supported");

            ActTableElement noColumn = Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "item_nope");
            Exception noColumnError = Assert.ThrowsException<Exception>(() => helper.HandleDataWindowGridAction(window.Element, cells, noColumn));
            StringAssert.Contains(noColumnError.Message, "Input Column Name item_nope not present in Grid");
            StringAssert.Contains(noColumnError.Message, "item_code (Item Code), item_name (Item Name), active (Active)", "the message names the columns there are, and the titles to find them by");

            ActTableElement noColumnNumber = Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "7");
            noColumnNumber.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            Exception noColumnNumberError = Assert.ThrowsException<Exception>(() => helper.HandleDataWindowGridAction(window.Element, cells, noColumnNumber));
            StringAssert.Contains(noColumnNumberError.Message, "Input Column Number 7 not present in Grid");

            ActTableElement noMatch = Where(ActTableElement.eTableAction.GetValue, "item_code", ActTableElement.eRunColOperator.Equals, "404", "item_name");
            Exception noMatchError = Assert.ThrowsException<Exception>(() => helper.HandleDataWindowGridAction(window.Element, cells, noMatch));
            Assert.AreEqual("Grid Cell matching to given condition not present.", noMatchError.Message);
        }

        [TestMethod]
        public void AnActionTheGridDoesNotSupport_IsRefusedWithTheWordingOfTheOtherGrids()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            UIAComWrapperHelper helper = CreateHelper();

            ActTableElement unsupported = Table(ActTableElement.eTableAction.SendKeys, "Row Number", "0", "item_code");
            helper.HandleDataWindowGridAction(window.Element, cells, unsupported);
            ActTableElement setText = Table(ActTableElement.eTableAction.SetText, "Row Number", "0", "item_code", "x");
            helper.HandleDataWindowGridAction(window.Element, cells, setText);

            Assert.AreEqual("Action  - SendKeys not supported for Grids", unsupported.Error);
            StringAssert.Contains(setText.Error, "SetText Table Action is currently supported only by ColumnTitle with Where Option");
        }

        [TestMethod]
        public void Toggle_ChangesACheckBoxAndRefusesACellThatIsNotOne()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            UIAComWrapperHelper helper = CreateHelper();

            ActTableElement toggle = Table(ActTableElement.eTableAction.Toggle, "Row Number", "0", "active");
            helper.HandleDataWindowGridAction(window.Element, cells, toggle);
            ActTableElement notABox = Table(ActTableElement.eTableAction.Toggle, "Row Number", "0", "item_code");
            helper.HandleDataWindowGridAction(window.Element, cells, notABox);

            Assert.IsTrue(string.IsNullOrEmpty(toggle.Error), toggle.Error);
            Assert.AreEqual("Checked", toggle.ExInfo);
            Assert.IsTrue(window.Model.Cells[5].IsChecked, "the DataWindow's check box changed");
            StringAssert.Contains(notABox.Error, "only for check box cells");
        }

        [TestMethod]
        public void SetValue_OfACheckBoxBringsItToTheStateAskedForAndLeavesItWhereItIs()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            UIAComWrapperHelper helper = CreateHelper();
            FakeCell box = window.Model.Cells[11];

            helper.HandleDataWindowGridAction(window.Element, cells, Table(ActTableElement.eTableAction.SetValue, "Row Number", "2", "active", "Checked"));
            Assert.IsTrue(box.IsChecked, "asked to check an unchecked box");

            ActTableElement again = Table(ActTableElement.eTableAction.SetValue, "Row Number", "2", "active", "Checked");
            helper.HandleDataWindowGridAction(window.Element, cells, again);
            Assert.IsTrue(box.IsChecked, "asked to check a box that is checked, it is left alone rather than toggled off");
            Assert.IsTrue(string.IsNullOrEmpty(again.Error), again.Error);

            helper.HandleDataWindowGridAction(window.Element, cells, Table(ActTableElement.eTableAction.SetValue, "Row Number", "2", "active", "Unchecked"));
            Assert.IsFalse(box.IsChecked);

            ActTableElement nonsense = Table(ActTableElement.eTableAction.SetValue, "Row Number", "2", "active", "maybe");
            helper.HandleDataWindowGridAction(window.Element, cells, nonsense);
            StringAssert.Contains(nonsense.Error, "Checked or Unchecked");
            Assert.IsFalse(box.IsChecked, "a value that is not a state changes nothing");
        }

        [TestMethod]
        public void SetValue_WritesTheCellThroughTheAccessibleObjectAndConfirmsIt()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            FakeCell target = window.Model.Cells[6];
            ActTableElement action = Table(ActTableElement.eTableAction.SetValue, "Row Number", "1", "item_code", "77");

            // Asking for window messages is what keeps the keyboard out of it.
            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            Assert.AreEqual("77 set", action.ExInfo);
            Assert.AreEqual("77", target.Value);
            Assert.AreEqual("101", window.Model.Cells[3].Value, "the cell above it is not touched");
            Assert.AreEqual("103", window.Model.Cells[9].Value, "nor is the cell below it");
        }

        [TestMethod]
        public void SetValue_ByTheValueOfAColumnWritesTheRowItFound()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            ActTableElement action = Where(ActTableElement.eTableAction.SetValue, "item_code", ActTableElement.eRunColOperator.Equals, "103", "item_name", "Delta");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            Assert.AreEqual("Delta", window.Model.Cells[10].Value);
            Assert.AreEqual("Alpha      ", window.Model.Cells[4].Value, "the other rows keep what they had");
        }

        [TestMethod]
        public void SetValue_OnACellThatRefusesTheWrite_IsReportedAsFailedWhereTheKeyboardCannotTakeOver()
        {
            if (InteractiveDesktop.IsAvailable())
            {
                Assert.Inconclusive("A refused write is typed in behind a click where the keyboard reaches the machine, so this is only exercised on a desktop that cannot take it: "
                    + InteractiveDesktop.DescribeSession());
            }

            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            window.Model.Cells[6].RefusesWrites = true;
            ActTableElement action = Table(ActTableElement.eTableAction.SetValue, "Row Number", "1", "item_code", "77");

            Exception failure = Assert.ThrowsException<Exception>(() => CreateHelper().HandleDataWindowGridAction(window.Element, cells, action));

            StringAssert.Contains(failure.Message, "Unable to set value");
            Assert.AreEqual("102", window.Model.Cells[6].Value, "nothing was written");
        }

        [TestMethod]
        public void Click_OnACheckBoxTakesItsDefaultActionAndNeedsNoMouse()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            ActTableElement action = Table(ActTableElement.eTableAction.Click, "Row Number", "0", "active");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            StringAssert.Contains(action.ExInfo, "Clicked Successfully");
            Assert.IsTrue(window.Model.Cells[5].IsChecked);
            Assert.AreEqual(0, window.LeftClicks, "the box was not clicked at, it was activated");
        }

        [TestMethod]
        public void ClickXY_LandsAtThePointInTheCellItNamesAndNotInTheGrid()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            Rectangle bounds = window.Model.ScreenBoundsOf(window.Model.Cells[7]);
            ActTableElement action = Table(ActTableElement.eTableAction.ClickXY, "Row Number", "1", "item_name", "5,6");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            Assert.IsTrue(window.WaitForLeftClicks(1), "the click never reached the DataWindow: " + action.Error);
            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            StringAssert.Contains(action.ExInfo, "Clicked Successfully");
            Assert.AreEqual(new Point(bounds.X + 5 - window.Model.Origin.X, bounds.Y + 6 - window.Model.Origin.Y), window.LastClick);
        }

        [TestMethod]
        public void Click_OnATextCellLandsInTheMiddleOfIt()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            Rectangle bounds = window.Model.ScreenBoundsOf(window.Model.Cells[9]);
            ActTableElement action = Table(ActTableElement.eTableAction.Click, "Row Number", "2", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            Assert.IsTrue(window.WaitForLeftClicks(1), "the click never reached the DataWindow: " + action.Error);
            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            Assert.AreEqual(new Point(bounds.X + (bounds.Width / 2) - window.Model.Origin.X, bounds.Y + (bounds.Height / 2) - window.Model.Origin.Y), window.LastClick);
        }

        [TestMethod]
        public void DoubleClick_LandsInTheCellAsADoubleClick()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            Rectangle bounds = window.Model.ScreenBoundsOf(window.Model.Cells[3]);
            ActTableElement action = Table(ActTableElement.eTableAction.DoubleClick, "Row Number", "0", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            Assert.IsTrue(window.WaitForDoubleClicks(1), "the double click never reached the DataWindow: " + action.Error);
            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            Assert.AreEqual(new Point(bounds.X + (bounds.Width / 2) - window.Model.Origin.X, bounds.Y + (bounds.Height / 2) - window.Model.Origin.Y), window.LastClick);
        }

        [TestMethod]
        public void ClickXY_WithoutAPointInTheCell_SaysSoInsteadOfClickingSomewhere()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            ActTableElement action = Table(ActTableElement.eTableAction.ClickXY, "Row Number", "0", "item_code", "left of it");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            StringAssert.Contains(action.Error, "ClickXY needs the point inside the cell as x,y");
            Assert.AreEqual(0, window.LeftClicks);
        }

        [TestMethod]
        public void ACellOutsideTheVisiblePartIsReadButNotClicked()
        {
            FakeDataWindowModel model = new FakeDataWindowModel();
            model.Add("order_no", "1", 10, 10, 100);
            model.Add("order_no", "2", 10, 400, 100);
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            UIAComWrapperHelper helper = CreateHelper();

            ActTableElement read = Table(ActTableElement.eTableAction.GetValue, "Row Number", "1", "order_no");
            helper.HandleDataWindowGridAction(window.Element, cells, read);
            ActTableElement click = Table(ActTableElement.eTableAction.Click, "Row Number", "1", "order_no");
            helper.HandleDataWindowGridAction(window.Element, cells, click);

            Assert.AreEqual("2", read.ExInfo, "a value is read wherever the cell is");
            StringAssert.Contains(click.Error, "Unable to bring cell on screen");
            Assert.AreEqual(0, window.LeftClicks);
        }

        [TestMethod]
        public void ACellOfAGridThatIsNotShownIsReadButNotClicked()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            UIAComWrapperHelper helper = CreateHelper();
            window.Hide();

            ActTableElement read = Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "item_code");
            helper.HandleDataWindowGridAction(window.Element, cells, read);
            ActTableElement click = Table(ActTableElement.eTableAction.Click, "Row Number", "0", "item_code");
            helper.HandleDataWindowGridAction(window.Element, cells, click);
            ActTableElement set = Table(ActTableElement.eTableAction.SetValue, "Row Number", "0", "item_code", "5");
            helper.HandleDataWindowGridAction(window.Element, cells, set);

            Assert.AreEqual("101", read.ExInfo, "what a hidden grid holds can still be read");
            StringAssert.Contains(click.Error, "Unable to bring cell on screen");
            StringAssert.Contains(set.Error, "Unable to bring cell on screen");
            Assert.AreEqual("101", window.Model.Cells[3].Value, "nothing was written");
            Assert.AreEqual(0, window.LeftClicks, "a click aimed at a grid that is not shown would land on whatever is drawn there");
        }

        #endregion

        #region Scrolling to a cell that is not in view

        [TestMethod]
        public void ActivateRow_OfARowBelowTheView_ScrollsDownToItAndClicksItWhereItIsDrawnThen()
        {
            FakeDataWindowModel model = ScrollingList();
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell target = model.FlaggedCell("item_code", 28);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "28", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            AssertShownBelowTheHeader(window, target);
            Assert.AreEqual(1, window.LeftClicks, "the row is clicked once");
            StringAssert.Contains(action.ExInfo, "Row 28 activated");
            Assert.IsTrue(model.ScrollY > 0, "the rows were not scrolled");
            Assert.IsTrue(model.ScrollMessages <= 10, "the grid was sent " + model.ScrollMessages + " scroll messages, where pages and lines take about seven");
        }

        [TestMethod]
        public void ActivateRow_OfARowInView_ClicksItWithoutScrolling()
        {
            FakeDataWindowModel model = ScrollingList();
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "4", "item_text");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, model.FlaggedCell("item_text", 4), action);
            Assert.AreEqual(0, model.ScrollMessages, "a row in view was scrolled for");
            Assert.AreEqual(0, model.ScrollY);
        }

        [TestMethod]
        public void ActivateRow_OfARowAboveTheView_ScrollsBackUpToIt()
        {
            FakeDataWindowModel model = ScrollingList();
            model.ScrollY = model.LineHeight * 20;
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell target = model.FlaggedCell("item_code", 3);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "3", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            AssertShownBelowTheHeader(window, target);
        }

        [TestMethod]
        public void ActivateRow_OfARowHiddenUnderTheHeader_ScrollsItBelowTheHeaderInsteadOfClickingTheHeader()
        {
            // The header does not scroll, and the row above the first one in view lies inside the client area, under it.
            FakeDataWindowModel model = ScrollingList();
            model.ScrollY = model.LineHeight;
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell target = model.FlaggedCell("item_code", 0);
            Assert.IsTrue(DataWindowGrid.TryGetClientArea(window.Handle, out Rectangle client));
            Assert.IsTrue(DataWindowGrid.IsInside(client, model.ScreenBoundsOf(target)), "the row is not inside the client area, so this proves nothing");
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "0", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            AssertShownBelowTheHeader(window, target);
            Assert.AreEqual(0, model.ScrollY, "the rows were scrolled back to the top");
            Assert.AreEqual(1, model.ScrollMessages);
        }

        [TestMethod]
        public void ActivateRow_OfARowAtTheFootOfTheView_ScrollsItUpARowBeforeClickingIt()
        {
            // The row below the last one a DataWindow draws lies inside the client area, at its foot, which the DataWindow
            // leaves empty or to its footer, and a click there selects nothing.
            FakeDataWindowModel model = ScrollingList();
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell target = model.FlaggedCell("item_code", 5);
            Assert.IsTrue(DataWindowGrid.TryGetClientArea(window.Handle, out Rectangle client));
            Rectangle atTheFoot = model.ScreenBoundsOf(target);
            Assert.IsTrue(DataWindowGrid.IsInside(client, atTheFoot) && atTheFoot.Bottom + model.LineHeight > client.Bottom, "the row is not at the foot of the client area, so this proves nothing");
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "5", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            Assert.AreEqual(model.LineHeight, model.ScrollY, "the rows were scrolled up by one");
            Assert.AreEqual(1, model.ScrollMessages);
        }

        [TestMethod]
        public void ActivateRow_OfARowAtTheFootOfAGridThatScrollsNoFurther_ClicksThePartOfItAboveTheFoot()
        {
            // A DataWindow that scrolls no further down may show its last row only in part, above its footer.
            FakeDataWindowModel model = ScrollingList();
            model.ScrollLimitY = 0;
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell target = model.FlaggedCell("item_code", 5);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "5", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            Assert.IsTrue(window.WaitForLeftClicks(1), "the click never reached the DataWindow: " + action.Error);
            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            Rectangle cell = model.ScreenBoundsOf(target);
            cell.Offset(-model.Origin.X, -model.Origin.Y);
            int foot = model.ClientSize.Height - model.LineHeight;
            Assert.IsTrue(cell.Contains(window.LastClick) && window.LastClick.Y < foot,
                "the click at " + window.LastClick + " did not land on the part of the row " + cell + " above the foot of the client area, " + foot + " down");
            Assert.AreEqual(0, model.ScrollY);
            Assert.AreEqual(1, model.ScrollMessages, "the grid was sent more than the line that shows it scrolls no further, such as the wheel");
        }

        [TestMethod]
        public void ActivateRow_OnACheckBoxColumn_ClicksAnotherCellOfTheRowAndLeavesTheBoxAlone()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell box = window.Model.Cells[8];
            bool wasChecked = box.IsChecked;
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "1", "active");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, window.Model.Cells[6], action);
            Assert.AreEqual(wasChecked, box.IsChecked, "the click changed the check box");
            StringAssert.Contains(action.ExInfo, "item_code");
        }

        [TestMethod]
        public void ActivateRow_OnARowWithNoCellButCheckBoxes_ClicksNothing()
        {
            FakeDataWindowModel model = new FakeDataWindowModel();
            model.Add("option_on", "N", 10, 10, 60, FakeCell.RoleCheckButton);
            model.Add("option_on", "N", 10, 40, 60, FakeCell.RoleCheckButton);
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "0", "option_on");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            StringAssert.Contains(action.Error, "no cell to click that is not a check box");
            Assert.AreEqual(0, window.LeftClicks);
            Assert.IsFalse(model.Cells[0].IsChecked);
        }

        [TestMethod]
        public void ActivateRow_FindsTheRowByTheValueOfAColumnAndScrollsToIt()
        {
            FakeDataWindowModel model = ScrollingList();
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            ActTableElement action = Where(ActTableElement.eTableAction.ActivateRow, "item_qty", ActTableElement.eRunColOperator.Equals, "2900", "item_text");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, model.FlaggedCell("item_text", 28), action);
            AssertShownBelowTheHeader(window, model.FlaggedCell("item_text", 28));
        }

        [TestMethod]
        public void Click_OfARowBelowTheView_ScrollsToItBeforeClicking()
        {
            FakeDataWindowModel model = ScrollingList();
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell target = model.FlaggedCell("item_text", 25);
            ActTableElement action = Table(ActTableElement.eTableAction.Click, "Row Number", "25", "item_text");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            AssertShownBelowTheHeader(window, target);
        }

        [TestMethod]
        public void SetValue_OfARowBelowTheView_ScrollsToItAndWritesThatRowOnly()
        {
            FakeDataWindowModel model = ScrollingList();
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            FakeCell target = model.FlaggedCell("item_code", 25);
            ActTableElement action = Table(ActTableElement.eTableAction.SetValue, "Row Number", "25", "item_code", "77");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            Assert.AreEqual("77 set", action.ExInfo);
            Assert.AreEqual("77", target.Value);
            Assert.AreEqual("125", model.FlaggedCell("item_code", 24).Value, "the row above it is not written");
            Assert.AreEqual("127", model.FlaggedCell("item_code", 26).Value, "nor is the row below it");
        }

        [TestMethod]
        public void ActivateRow_ScrollsAGridThatTakesOnlyTheWheelOfTheMouse()
        {
            FakeDataWindowModel model = ScrollingList(ScrollSupport.Wheel);
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell target = model.FlaggedCell("item_code", 20);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "20", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            AssertShownBelowTheHeader(window, target);
        }

        [TestMethod]
        public void ActivateRow_FallsBackToLinesWhereTheGridTakesNoMessageForAPage()
        {
            FakeDataWindowModel model = ScrollingList();
            model.IgnoresPageMessages = true;
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell target = model.FlaggedCell("item_code", 28);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "28", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            AssertShownBelowTheHeader(window, target);
        }

        [TestMethod]
        public void ActivateRow_ScrollsInOneBatchWhereALineMovesTheRowOnlyAFewPixels()
        {
            FakeDataWindowModel model = ScrollingList();
            model.LineHeight = 4;
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            FakeCell target = model.FlaggedCell("item_code", 8);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "8", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            AssertShownBelowTheHeader(window, target);
            Assert.IsTrue(model.ScrollMessages <= 20, "the grid was sent " + model.ScrollMessages + " scroll messages, one for each of the pixels asked for or close to it");
        }

        [TestMethod]
        public void ActivateRow_OfAColumnBeyondTheRightEdge_ScrollsSidewaysToIt()
        {
            FakeDataWindowModel model = FakeDataWindowModel.WideList();
            model.Scrolling = ScrollSupport.ScrollBar;
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window, 0);
            FakeCell target = model.Cells[5 + 5 + 4];
            Assert.AreEqual("col_e", target.Name);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "1", "col_e");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            Assert.IsTrue(model.ScrollX > 0, "the cells were not scrolled sideways");
            Assert.AreEqual(0, model.ScrollY, "the rows were scrolled although the row was in view");
        }

        [TestMethod]
        public void ActivateRow_OfACellBelowAndBeyondTheView_ScrollsBothWays()
        {
            FakeDataWindowModel model = FakeDataWindowModel.WideList(rows: 12);
            model.Scrolling = ScrollSupport.ScrollBar;
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window, 0);
            FakeCell target = model.Cells[5 + (11 * 5) + 4];
            Assert.AreEqual("col_e", target.Name);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "11", "col_e");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, target, action);
            AssertShownBelowTheHeader(window, target);
            Assert.IsTrue(model.ScrollX > 0 && model.ScrollY > 0, "scrolled sideways " + model.ScrollX + " and down " + model.ScrollY);
        }

        [TestMethod]
        public void ActivateRow_OfARowBelowTheViewOfAGridThatDoesNotScroll_SaysSoAndClicksNothing()
        {
            FakeDataWindowModel model = ScrollingList(ScrollSupport.None);
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "28", "item_code");

            Stopwatch clock = Stopwatch.StartNew();
            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            StringAssert.Contains(action.Error, "Unable to bring cell on screen");
            StringAssert.Contains(action.Error, "did not scroll down");
            Assert.AreEqual(0, window.LeftClicks, "a click at the place the row would be drawn would land beyond the grid");
            Assert.IsTrue(model.ScrollMessages > 0, "the grid was not asked to scroll");
            Assert.IsTrue(clock.Elapsed < TimeSpan.FromSeconds(10), "giving up on a grid that does not scroll took " + clock.Elapsed);
        }

        [TestMethod]
        public void ActivateRow_OfARowTheGridCannotScrollFarEnoughTo_SaysSoAndClicksNothing()
        {
            FakeDataWindowModel model = ScrollingList();
            model.ScrollLimitY = model.LineHeight * 3;
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "28", "item_code");

            Stopwatch clock = Stopwatch.StartNew();
            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            StringAssert.Contains(action.Error, "Unable to bring cell on screen");
            StringAssert.Contains(action.Error, "did not scroll down far enough");
            Assert.AreEqual(model.LineHeight * 3, model.ScrollY, "the grid scrolled as far as it goes");
            Assert.AreEqual(0, window.LeftClicks);
            Assert.IsTrue(clock.Elapsed < TimeSpan.FromSeconds(10), "giving up on a grid that stops scrolling took " + clock.Elapsed);
        }

        [TestMethod]
        public void ActivateRow_OfAGridThatIsNotShown_SendsItNoScrollMessage()
        {
            FakeDataWindowModel model = ScrollingList();
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            window.Hide();
            ActTableElement action = Table(ActTableElement.eTableAction.ActivateRow, "Row Number", "28", "item_code");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            StringAssert.Contains(action.Error, "Unable to bring cell on screen");
            StringAssert.Contains(action.Error, "not shown");
            Assert.AreEqual(0, model.ScrollMessages, "a grid that is not shown was sent scroll messages");
            Assert.AreEqual(0, window.LeftClicks);
        }

        [TestMethod]
        public void ACellBesideALabelAtTheTopIsNotTakenForOneHiddenByAHeader()
        {
            // The label of a field in a form lies at the top of the client area, as a header does, and is beside the field.
            FakeDataWindowModel model = new FakeDataWindowModel { ClientSize = new Size(300, 120), Scrolling = ScrollSupport.Both };
            model.Add("field_one_t", "Field One", 4, 2, 80, FakeCell.RoleStaticText);
            FakeCell field = model.Add("field_one", "x", 90, 2, 120);
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window, 0);
            ActTableElement action = Table(ActTableElement.eTableAction.Click, "Row Number", "0", "field_one");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, field, action);
            Assert.AreEqual(0, model.ScrollMessages, "a cell in view was scrolled for");
        }

        [TestMethod]
        public void ACellThatFillsMostOfTheClientAreaIsNoHeader()
        {
            FakeDataWindowModel model = new FakeDataWindowModel { ClientSize = new Size(300, 120), Scrolling = ScrollSupport.Both };
            model.Add("banner", "Banner", 0, 0, 300, FakeCell.RoleStaticText, 0, 100);
            FakeCell first = model.Add("order_no", "1", 10, 30, 100);
            model.Add("order_no", "2", 10, 52, 100);
            using FakeDataWindow window = new FakeDataWindow(model);
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window, 1);
            ActTableElement action = Table(ActTableElement.eTableAction.Click, "Row Number", "0", "order_no");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            AssertClickedInTheMiddleOf(window, first, action);
            Assert.AreEqual(0, model.ScrollMessages, "a cell in view was scrolled for");
        }

        [TestMethod]
        public void MeasuringInThePixelsOfAGridsApplicationLastsUntilDisposedAndLeavesTheThreadAsItWas()
        {
            IntPtr original = GetThreadDpiAwarenessContext();
            IntPtr unaware = IntPtr.Zero;
            try
            {
                // A window made by a thread that does not follow the scaling of the display, as PowerBuilder's are.
                // Made without Windows Forms, which makes its windows in a scaling of its own.
                SetThreadDpiAwarenessContext(DpiUnaware);
                unaware = CreateWindowEx(0, "STATIC", string.Empty, 0, 0, 0, 10, 10, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                Assert.AreNotEqual(IntPtr.Zero, unaware, "no window was made");
                SetThreadDpiAwarenessContext(DpiPerMonitorAware);
                Assert.AreEqual(AwarenessPerMonitor, GetAwarenessFromDpiAwarenessContext(GetThreadDpiAwarenessContext()));

                using (DataWindowGrid.InCoordinatesOf(unaware))
                {
                    Assert.AreEqual(AwarenessUnaware, GetAwarenessFromDpiAwarenessContext(GetThreadDpiAwarenessContext()), "the thread measures as the window's application does");
                }

                Assert.AreEqual(AwarenessPerMonitor, GetAwarenessFromDpiAwarenessContext(GetThreadDpiAwarenessContext()), "the thread measures as it did before");
                using (DataWindowGrid.InCoordinatesOf(IntPtr.Zero))
                {
                    Assert.AreEqual(AwarenessPerMonitor, GetAwarenessFromDpiAwarenessContext(GetThreadDpiAwarenessContext()), "no window leaves the thread as it is");
                }
            }
            finally
            {
                if (unaware != IntPtr.Zero)
                {
                    DestroyWindow(unaware);
                }

                SetThreadDpiAwarenessContext(original);
            }
        }

        private static readonly IntPtr DpiUnaware = new IntPtr(-1);
        private static readonly IntPtr DpiPerMonitorAware = new IntPtr(-4);
        private const int AwarenessUnaware = 0;
        private const int AwarenessPerMonitor = 2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetThreadDpiAwarenessContext();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetAwarenessFromDpiAwarenessContext(IntPtr context);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool DestroyWindow(IntPtr hwnd);

        #endregion

        #region A DataWindow that flags every cell invisible, as PowerBuilder 12.5 does

        [TestMethod]
        public void TryRead_ListsACellTheDataWindowFlagsInvisibleLikeAnyOther()
        {
            // PowerBuilder 12.6 flags the columns it does not draw, and PowerBuilder 12.5 flags every cell, so the
            // flag cannot tell the cells that are shown from the ones that are not, and no cell is left out for it.
            FakeDataWindowModel model = FakeDataWindowModel.ItemList();
            for (int row = 0; row < 3; row++)
            {
                model.Add("item_extra", "x" + row, 360, 30 + (row * 22), 5, FakeCell.RoleText, FakeCell.StateInvisible);
            }

            Assert.IsTrue(DataWindowGrid.TryRead(model.Root, out DataWindowGrid grid, out string details), details);

            CollectionAssert.AreEqual(new[] { "item_code", "item_name", "active", "item_extra" }, grid.ColumnNames.ToList());
            Assert.AreEqual(3, grid.RowCount);
        }

        [TestMethod]
        public void TryRead_ReadsEveryRowOfAGridThatFlagsEveryCellInvisible()
        {
            FakeDataWindowModel model = FakeDataWindowModel.FlaggedList();

            Assert.IsTrue(DataWindowGrid.TryRead(model.Root, out DataWindowGrid grid, out string details), details);

            Assert.AreEqual(154, grid.Cells.Count, "the four headers and the five columns of the 30 rows");
            CollectionAssert.AreEqual(FakeDataWindowModel.FlaggedColumns, grid.ColumnNames.ToList());
            Assert.AreEqual(30, grid.RowCount);
            Assert.AreEqual("101", grid.Column("item_code")[0].Value);
            Assert.AreEqual("Item 103", grid.Column("item_text")[2].Value.Trim());
            Assert.AreEqual("130", grid.Column("item_code")[29].Value, "a row scrolled out of view is read as well");
        }

        [TestMethod]
        public void TryReadDataWindowCells_ReadsAllTheRowsOfAGridUiAutomationListsNoCellOfBecauseEveryCellIsFlaggedInvisible()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList(), nonClientScrollBars: true);

            Assert.IsTrue(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, NothingListed(), out DataWindowGrid cells));

            CollectionAssert.AreEqual(FakeDataWindowModel.FlaggedColumns, cells.ColumnNames.ToList());
            Assert.AreEqual(30, cells.RowCount);
        }

        [TestMethod]
        public void GetValue_ReadsAnyRowOfAGridThatFlagsEveryCellInvisible_ByRowNumberAndByTheValueOfAColumn()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList());

            Assert.AreEqual("101", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "item_code")));
            Assert.AreEqual("Item 123", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "22", "item_text")).Trim());
            Assert.AreEqual("30", ValueOf(window, Table(ActTableElement.eTableAction.GetRowCount, "Row Number", "0", "item_code")));
            Assert.AreEqual("C4", ValueOf(window, Where(ActTableElement.eTableAction.GetValue, "item_qty", ActTableElement.eRunColOperator.Equals, "400", "item_calc")));
        }

        [TestMethod]
        public void GetValue_FindsAColumnOfAGridThatFlagsEveryCellInvisibleByTheTitleItsHeaderShows()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList());

            Assert.AreEqual("700", ValueOf(window, Table(ActTableElement.eTableAction.GetValue, "Row Number", "6", "Quantity")));
            Assert.AreEqual("107", ValueOf(window, Where(ActTableElement.eTableAction.GetValue, "Quantity", ActTableElement.eRunColOperator.Equals, "700", "Item Code")));
            Assert.AreEqual("C7", ValueOf(window, Where(ActTableElement.eTableAction.GetValue, "Quantity", ActTableElement.eRunColOperator.Equals, "700", "item_calc")), "a column with no header is found by its name only");
        }

        [TestMethod]
        public void GetValue_OfAColumnTheGridDoesNotHave_NamesTheColumnsItHas()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList());
            ActTableElement position = Table(ActTableElement.eTableAction.GetValue, "Row Number", "1", "Position");

            // This is the column the scroll bar of the grid gave a table action before its cells were read.
            Exception failure = Assert.ThrowsException<Exception>(() => Run(window, position));

            StringAssert.Contains(failure.Message, "Input Column Name Position not present in Grid");
            StringAssert.Contains(failure.Message, "item_code (Item Code), item_text (Description), item_day (Day), item_qty (Quantity), item_calc.");
        }

        [TestMethod]
        public void AGridWithNoRowsCountsNoneAndHasNoRowToWork()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList(rows: 0));
            ActTableElement read = Table(ActTableElement.eTableAction.GetValue, "Row Number", "0", "item_code");

            Assert.AreEqual("0", ValueOf(window, Table(ActTableElement.eTableAction.GetRowCount, "Row Number", "0", "item_code")));
            Run(window, read);

            StringAssert.Contains(read.Error, "Given Row Number 0 is not present");
        }

        [TestMethod]
        public void ARowScrolledOutOfViewOfAGridThatFlagsEveryCellInvisibleIsReadButNotClicked()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList());
            DataWindowGrid cells = ReadCells(window);
            UIAComWrapperHelper helper = CreateHelper();
            FakeCell scrolledOut = window.Model.FlaggedCell("item_code", 20);

            ActTableElement read = Table(ActTableElement.eTableAction.GetValue, "Row Number", "20", "item_code");
            helper.HandleDataWindowGridAction(window.Element, cells, read);
            ActTableElement click = Table(ActTableElement.eTableAction.Click, "Row Number", "20", "item_code");
            helper.HandleDataWindowGridAction(window.Element, cells, click);
            ActTableElement clickAt = Table(ActTableElement.eTableAction.ClickXY, "Row Number", "20", "item_code", "3,3");
            helper.HandleDataWindowGridAction(window.Element, cells, clickAt);
            ActTableElement set = Table(ActTableElement.eTableAction.SetValue, "Row Number", "20", "item_code", "9");
            helper.HandleDataWindowGridAction(window.Element, cells, set);

            Assert.AreEqual("121", read.ExInfo, "a value is read wherever the row is");
            StringAssert.Contains(click.Error, "Unable to bring cell on screen");
            StringAssert.Contains(clickAt.Error, "Unable to bring cell on screen");
            StringAssert.Contains(set.Error, "Unable to bring cell on screen");
            Assert.AreEqual("121", scrolledOut.Value, "nothing was written");
            Assert.AreEqual(0, window.LeftClicks, "a click at the place the row would be drawn would land on the scroll bar or beyond the grid");
        }

        [TestMethod]
        public void ARowInViewOfAGridThatFlagsEveryCellInvisibleIsClickedWhereItIsDrawn()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList());
            DataWindowGrid cells = ReadCells(window);
            RequireClicksByMessage(window);
            Rectangle bounds = window.Model.ScreenBoundsOf(window.Model.FlaggedCell("item_text", 3));
            ActTableElement action = Table(ActTableElement.eTableAction.Click, "Row Number", "3", "item_text");

            CreateHelper().HandleDataWindowGridAction(window.Element, cells, action);

            Assert.IsTrue(window.WaitForLeftClicks(1), "the click never reached the DataWindow: " + action.Error);
            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            Assert.AreEqual(new Point(bounds.X + (bounds.Width / 2) - window.Model.Origin.X, bounds.Y + (bounds.Height / 2) - window.Model.Origin.Y), window.LastClick);
        }

        [TestMethod]
        [Timeout(180000)]
        public void TheSpyListsTheColumnsAndRowsOfAGridUiAutomationSeesNoCellOf()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList(), nonClientScrollBars: true, inAHostWindow: true);
            UIAComWrapperHelper helper = CreateHelperIn(window.Host);

            // A PowerBuilder 12.5 DataWindow as UI Automation sees it: the cells are flagged invisible, the
            // proxy leaves them out, and what is listed under the DataWindow is its scroll bars.
            List<UIAuto.AutomationElement> listed = ChildrenOf(window.Element);
            Assert.IsFalse(listed.Any(child => FakeDataWindowModel.FlaggedColumns.Contains(child.Current.Name)),
                "UI Automation lists a cell of the stand-in, so this proves nothing: " + DescribeChildren(listed));

            TableElementInfo table = helper.GetTableElementInfoForGrid(window.Element);

            CollectionAssert.AreEqual(FakeDataWindowModel.FlaggedColumns, table.ColumnNames,
                "the columns offered were " + string.Join(", ", table.ColumnNames) + " and UI Automation lists " + DescribeChildren(listed));
            Assert.AreEqual(30, table.RowCount);

            // What the scan of the grid found is still what the table holds, and it holds no cell, so the
            // columns above did not come from it.
            Assert.IsFalse(table.MainDict.Keys.Any(FakeDataWindowModel.FlaggedColumns.Contains), "the scan found a cell: " + string.Join(", ", table.MainDict.Keys));
        }

        [TestMethod]
        [Timeout(180000)]
        public void TheGridIsNotSearchedPointByPointWhenTheDataWindowListsItsCellsItself()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList(), nonClientScrollBars: true, inAHostWindow: true);
            UIAComWrapperHelper helper = CreateHelperIn(window.Host);
            bool searched = false;

            Dictionary<string, UIAuto.AutomationElement[]> found = helper.GetDictionaryForGrid(window.Element, ref searched);

            Assert.IsFalse(searched, "the grid was searched point by point, which puts the focus into it and asks it about hundreds of points");
            Assert.IsFalse(found.Keys.Any(FakeDataWindowModel.FlaggedColumns.Contains), "a cell was found: " + string.Join(", ", found.Keys));
        }

        [TestMethod]
        [Timeout(180000)]
        public void TheGridIsStillSearchedPointByPointWhenTheDataWindowListsNoCellItself()
        {
            using FakeDataWindow window = new FakeDataWindow(new FakeDataWindowModel(), nonClientScrollBars: true, inAHostWindow: true);
            UIAComWrapperHelper helper = CreateHelperIn(window.Host);
            bool searched = false;

            helper.GetDictionaryForGrid(window.Element, ref searched);

            Assert.IsTrue(searched, "a DataWindow that lists nothing is searched as it always was");
        }

        [TestMethod]
        [Timeout(180000)]
        public void TheGridIsStillSearchedPointByPointWhenItIsNotADataWindow()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList(), "pbxx172", nonClientScrollBars: true, inAHostWindow: true);
            UIAComWrapperHelper helper = CreateHelperIn(window.Host);
            bool searched = false;

            helper.GetDictionaryForGrid(window.Element, ref searched);

            Assert.IsTrue(searched, "only a DataWindow is left out of the search");
        }

        [TestMethod]
        [Timeout(180000)]
        public void TheSpyListsTheColumnsOfAGridWithOneRowWithoutItsHeaders()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList(rows: 1), nonClientScrollBars: true, inAHostWindow: true);
            UIAComWrapperHelper helper = CreateHelperIn(window.Host);

            TableElementInfo table = helper.GetTableElementInfoForGrid(window.Element);

            CollectionAssert.AreEqual(FakeDataWindowModel.FlaggedColumns, table.ColumnNames, "the columns offered were " + string.Join(", ", table.ColumnNames));
            Assert.AreEqual(1, table.RowCount);
        }

        [TestMethod]
        [Timeout(180000)]
        public void TheSpyListsTheColumnsOfAGridWhoseScrollBarLiesInsideItsCellArea()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList(), scrollBarInClientArea: true, inAHostWindow: true);
            UIAComWrapperHelper helper = CreateHelperIn(window.Host);

            TableElementInfo table = helper.GetTableElementInfoForGrid(window.Element);

            CollectionAssert.AreEqual(FakeDataWindowModel.FlaggedColumns, table.ColumnNames,
                "the columns offered were " + string.Join(", ", table.ColumnNames) + " and the scan found " + string.Join(", ", table.MainDict.Keys));
            Assert.AreEqual(30, table.RowCount);
        }

        [TestMethod]
        [Timeout(180000)]
        public void TableActionsRunThroughTheDriverAgainstAGridUiAutomationSeesNoCellOf()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.FlaggedList(), nonClientScrollBars: true, inAHostWindow: true);
            UIAComWrapperHelper helper = CreateHelperIn(window.Host);
            PBDriver driver = (PBDriver)helper.WindowExplorer;

            ActTableElement count = OnTheList(Table(ActTableElement.eTableAction.GetRowCount, "Row Number", "0", "item_code"));
            driver.RunAction(count);
            ActTableElement value = OnTheList(Table(ActTableElement.eTableAction.GetValue, "Row Number", "2", "item_text"));
            driver.RunAction(value);
            ActTableElement byValue = OnTheList(Where(ActTableElement.eTableAction.GetValue, "item_code", ActTableElement.eRunColOperator.Equals, "110", "item_qty"));
            driver.RunAction(byValue);

            Assert.IsTrue(string.IsNullOrEmpty(count.Error), count.Error);
            Assert.AreEqual("30", count.ExInfo);
            Assert.IsTrue(string.IsNullOrEmpty(value.Error), value.Error);
            Assert.AreEqual("Item 103", value.ExInfo.Trim());
            Assert.IsTrue(string.IsNullOrEmpty(byValue.Error), byValue.Error);
            Assert.AreEqual("1000", byValue.ExInfo, "the row whose item_code is 110 is the tenth");
        }

        #endregion

        #region The layer, aimed at a cell that has no element

        [TestMethod]
        public void DataWindowMsaa_WritesTheCellDrawnAtThePointItIsGiven_WhenTheTargetHasNoElement()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            FakeCell target = window.Model.Cells[7];

            LayerResult result = new DataWindowMsaaLayer().TryExecute(CellContext(window, target, DesktopOperation.SetValue, "Delta"));

            Assert.AreEqual(LayerExecutionStatus.Succeeded, result.Status, result.Message);
            Assert.AreEqual("Delta", target.Value);
            Assert.AreEqual("101", window.Model.Cells[3].Value, "no other cell is written");
            Assert.AreEqual("Alpha      ", window.Model.Cells[4].Value);
        }

        [TestMethod]
        public void DataWindowMsaa_ReadsTheCellDrawnAtThePointItIsGiven_WhenTheTargetHasNoElement()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());

            LayerResult result = new DataWindowMsaaLayer().TryExecute(CellContext(window, window.Model.Cells[9], DesktopOperation.GetValue, null));

            Assert.AreEqual(LayerExecutionStatus.Succeeded, result.Status, result.Message);
            Assert.AreEqual("103", result.OutputValue);
        }

        [TestMethod]
        public void DataWindowMsaa_DeclinesAPointThatIsNoCellAndATargetWithNeitherElementNorPoint()
        {
            using FakeDataWindow window = new FakeDataWindow(FakeDataWindowModel.ItemList());
            DesktopActionContext between = CellContext(window, window.Model.Cells[3], DesktopOperation.SetValue, "x");
            between.TargetScreenX = window.Model.Origin.X + window.Model.ClientSize.Width - 2;
            between.TargetScreenY = window.Model.Origin.Y + window.Model.ClientSize.Height - 2;
            DesktopActionContext nowhere = CellContext(window, window.Model.Cells[3], DesktopOperation.SetValue, "x");
            nowhere.HasTargetPoint = false;
            DataWindowMsaaLayer layer = new DataWindowMsaaLayer();

            Assert.AreEqual(LayerExecutionStatus.Skipped, layer.TryExecute(between).Status, "a point on no cell has nothing to write");
            Assert.AreEqual(LayerExecutionStatus.Skipped, layer.TryExecute(nowhere).Status, "with no element and no point there is nothing to aim at");
            Assert.AreEqual("101", window.Model.Cells[3].Value);
        }

        #endregion

        /// <summary>A list of 30 rows in a client area that shows six, which scrolls for the requests it is given.</summary>
        private static FakeDataWindowModel ScrollingList(ScrollSupport scrolling = ScrollSupport.ScrollBar)
        {
            FakeDataWindowModel model = FakeDataWindowModel.FlaggedList();
            model.Scrolling = scrolling;
            return model;
        }

        /// <summary>Waits for the click an action sent and checks it landed in the middle of the cell where the cell is drawn now.</summary>
        private static void AssertClickedInTheMiddleOf(FakeDataWindow window, FakeCell cell, ActTableElement action)
        {
            Assert.IsTrue(window.WaitForLeftClicks(1), "the click never reached the DataWindow: " + action.Error);
            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            Rectangle bounds = window.Model.ScreenBoundsOf(cell);
            Assert.AreEqual(
                new Point(bounds.X + (bounds.Width / 2) - window.Model.Origin.X, bounds.Y + (bounds.Height / 2) - window.Model.Origin.Y),
                window.LastClick,
                "the click did not land in the middle of the cell where it is drawn after the scroll");
        }

        /// <summary>Checks the middle of a cell lies in the client area below the header, which is where a click on it reaches the cell.</summary>
        private static void AssertShownBelowTheHeader(FakeDataWindow window, FakeCell cell)
        {
            const int headerBottom = 22;
            Rectangle bounds = window.Model.ScreenBoundsOf(cell);
            int middle = bounds.Y + (bounds.Height / 2) - window.Model.Origin.Y;
            Assert.IsTrue(middle >= headerBottom && middle < window.Model.ClientSize.Height,
                "the middle of the cell is drawn " + middle + " down the client area, which is not below the header and above its foot");
        }

        /// <summary>
        /// Lets UI Automation settle what it asks the window for the first time it is used, so a count of
        /// the reads of the accessible object taken afterwards is the reads of the code under test alone.
        /// </summary>
        private static void WarmUp(FakeDataWindow window)
        {
            _ = window.Element.Current.ClassName;
            _ = window.Element.Current.NativeWindowHandle;
            _ = window.Element.Current.BoundingRectangle;
        }

        private static string ValueOf(FakeDataWindow window, ActTableElement action)
        {
            Run(window, action);
            Assert.IsTrue(string.IsNullOrEmpty(action.Error), action.Error);
            return action.ExInfo;
        }

        private static void Run(FakeDataWindow window, ActTableElement action)
        {
            CreateHelper().HandleDataWindowGridAction(window.Element, ReadCells(window), action);
        }

        private static DataWindowGrid ReadCells(FakeDataWindow window)
        {
            Assert.IsTrue(UIAComWrapperHelper.TryReadDataWindowCells(window.Element, NothingListed(), out DataWindowGrid cells),
                "the stand-in DataWindow's cells could not be read");
            return cells;
        }

        private static Dictionary<string, UIAuto.AutomationElement[]> NothingListed()
        {
            return new Dictionary<string, UIAuto.AutomationElement[]>();
        }

        /// <summary>The thumb of a scroll bar, when UI Automation lists one under it.</summary>
        private static UIAuto.AutomationElement ThumbOf(UIAuto.AutomationElement scrollBar)
        {
            for (UIAuto.AutomationElement child = UIAuto.TreeWalker.RawViewWalker.GetFirstChild(scrollBar); child != null; child = UIAuto.TreeWalker.RawViewWalker.GetNextSibling(child))
            {
                if (child.Current.ControlType.Id == UIAuto.ControlType.Thumb.Id)
                {
                    return child;
                }
            }

            return null;
        }

        private static List<UIAuto.AutomationElement> ChildrenOf(UIAuto.AutomationElement parent)
        {
            List<UIAuto.AutomationElement> children = [];
            for (UIAuto.AutomationElement child = UIAuto.TreeWalker.RawViewWalker.GetFirstChild(parent); child != null; child = UIAuto.TreeWalker.RawViewWalker.GetNextSibling(child))
            {
                children.Add(child);
            }

            return children;
        }

        private static string DescribeChildren(List<UIAuto.AutomationElement> children)
        {
            return children.Count == 0
                ? "nothing"
                : string.Join(", ", children.Select(child => child.Current.ControlType.ProgrammaticName + " '" + child.Current.Name + "'"));
        }

        /// <summary>
        /// The agent a table action runs through. It asks for window messages, as an operator can, so no
        /// test reaches for the mouse or the keyboard of whoever is running it.
        /// </summary>
        private static UIAComWrapperHelper CreateHelper()
        {
            return new UIAComWrapperHelper
            {
                mPlatform = ePlatformType.PowerBuilder,
                NonIntrusiveInput = true,
                ImplicitWait = 0
            };
        }

        /// <summary>The same agent, attached to a window, with the driver that hands it the actions.</summary>
        private static UIAComWrapperHelper CreateHelperIn(UIAuto.AutomationElement window)
        {
            UIAComWrapperHelper helper = new UIAComWrapperHelper
            {
                mPlatform = ePlatformType.PowerBuilder,
                CurrentWindow = window,
                CurrentWindowName = window.Current.Name,
                NonIntrusiveInput = true,
                ImplicitWait = 0
            };
            helper.UpdateRootElement();
            helper.WindowExplorer = new PBDriver { mUIAutomationHelper = helper };
            return helper;
        }

        /// <summary>Aims a table action at the DataWindow of the stand-in the way a recorded one is, by the control id.</summary>
        private static ActTableElement OnTheList(ActTableElement action)
        {
            action.LocateBy = eLocateBy.ByAutomationID;
            action.LocateValueCalculated = "1000";
            return action;
        }

        /// <summary>
        /// A click is sent as window messages when the agent asks for them, and falls back to the real
        /// mouse only if the window does not take them. This proves the stand-in takes them first, so the
        /// fallback is never reached by the test that follows.
        /// </summary>
        private static void RequireClicksByMessage(FakeDataWindow window, int cellIndex = 3)
        {
            FakeCell cell = window.Model.Cells[cellIndex];
            Rectangle bounds = window.Model.ScreenBoundsOf(cell);
            PointClickOutcome outcome = Win32Native.ClickAtScreenPoint(window.Handle, bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2), 2000);
            if (outcome != PointClickOutcome.Delivered || !window.WaitForLeftClicks(1))
            {
                Assert.Inconclusive("The stand-in DataWindow did not take a click sent as window messages (" + outcome + "), so a click that would fall back to the mouse is not exercised.");
            }

            window.ForgetClicks();
        }

        private static DesktopActionContext CellContext(FakeDataWindow window, FakeCell cell, DesktopOperation operation, string value)
        {
            Rectangle bounds = window.Model.ScreenBoundsOf(cell);
            return new DesktopActionContext
            {
                Operation = operation,
                NativeWindowHandle = IntPtr.Zero,
                PointOwnerWindowHandle = window.Handle,
                Value = value,
                HasTargetPoint = true,
                TargetScreenX = bounds.X + (bounds.Width / 2),
                TargetScreenY = bounds.Y + (bounds.Height / 2),
                AllowPhysicalInput = false,
                DesktopCanTakePhysicalInput = true,
                TimeoutMs = 2000
            };
        }

        /// <summary>The child id the DataWindow gives the cell at an index, which counts from one.</summary>
        private static int ChildIdOf(FakeDataWindowModel model, int index)
        {
            Assert.IsTrue(index < model.Cells.Count, "the stand-in has no cell " + index);
            return index + 1;
        }

        private static ActTableElement Table(ActTableElement.eTableAction action, string rowType, string rowValue, string column, string value = null)
        {
            ActTableElement table = new ActTableElement
            {
                ControlAction = action,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                LocateColTitle = column,
                LocateRowType = rowType,
                LocateRowValue = rowValue,
                AddNewReturnParams = true
            };

            if (value != null)
            {
                table.AddOrUpdateInputParamValueAndCalculatedValue("Value", value);
            }

            return Prepared(table);
        }

        private static ActTableElement Where(ActTableElement.eTableAction action, string whereColumn, ActTableElement.eRunColOperator comparison, string whereValue, string column, string value = null)
        {
            ActTableElement table = Table(action, "Where", null, column, value);
            table.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            table.WhereColumnTitle = whereColumn;
            table.WhereOperator = comparison;
            table.WhereColumnValue = whereValue;
            return Prepared(table);
        }

        /// <summary>What the runner does before handing an action to the driver.</summary>
        private static ActTableElement Prepared(ActTableElement action)
        {
            foreach (ActInputValue input in action.InputValues)
            {
                input.ValueForDriver = input.Value;
            }

            return action;
        }
    }

    /// <summary>
    /// A DataWindow of a PowerBuilder application that UI Automation can see into is worked exactly as it
    /// was, and what its accessible object lists agrees with what UI Automation lists.
    /// </summary>
    /// <remarks>
    /// This half of the change is proved against the real control, because the stand-in windows can only
    /// show that the code does what it was written to do. It starts the grid test application and needs
    /// neither the mouse nor the keyboard.
    /// </remarks>
    [Level3]
    [TestClass]
    public class DataWindowGridLiveTest
    {
        [TestMethod]
        [Timeout(300000)]
        public void ADataWindowUiAutomationCanSeeInto_IsNotReadThroughItsAccessibleObject()
        {
            using LiveGrid grid = LiveGrid.Open();

            TableElementInfo table = grid.Helper.GetTableElementInfoForGrid(grid.DataWindow);
            bool taken = UIAComWrapperHelper.TryReadDataWindowCells(grid.DataWindow, table.MainDict, out DataWindowGrid cells);

            Assert.IsFalse(taken, "a grid UI Automation lists the cells of has to be read as it always was");
            Assert.IsNull(cells);
            Assert.IsTrue(table.ColumnNames.Count > 1, "UI Automation lists the columns " + string.Join(", ", table.ColumnNames));
            Assert.IsTrue(table.RowCount > 0, "UI Automation lists no rows");
        }

        [TestMethod]
        [Timeout(300000)]
        public void TheAccessibleObjectOfARealDataWindowListsTheColumnsAndRowsUiAutomationDoes()
        {
            using LiveGrid grid = LiveGrid.Open();
            TableElementInfo table = grid.Helper.GetTableElementInfoForGrid(grid.DataWindow);

            bool read = DataWindowGrid.TryRead(new IntPtr(grid.DataWindow.Current.NativeWindowHandle), out DataWindowGrid cells, out string details);

            Assert.IsTrue(read, details);
            Assert.AreEqual(table.RowCount, cells.RowCount, "rows: UI Automation lists " + table.RowCount + ", the accessible object " + cells.RowCount);

            // The DataWindow lists the columns it does not draw as well, which UI Automation leaves out, so the
            // columns UI Automation lists are among the ones the accessible object lists, in the same order.
            List<string> listed = cells.ColumnNames.ToList();
            int from = 0;
            foreach (string column in table.ColumnNames)
            {
                int at = listed.IndexOf(column, from);
                Assert.IsTrue(at >= 0, "the column " + column + " UI Automation lists is not among the columns of the accessible object: " + string.Join(", ", listed));
                from = at + 1;
                Assert.AreEqual(table.MainDict[column].Length, cells.Column(column).Count, "the cells of " + column);
            }
        }

        [TestMethod]
        [Timeout(300000)]
        public void EveryCellOfARealDataWindowReadsTheSameThroughItsAccessibleObjectAsThroughUiAutomation()
        {
            using LiveGrid grid = LiveGrid.Open();
            TableElementInfo table = grid.Helper.GetTableElementInfoForGrid(grid.DataWindow);
            Assert.IsTrue(DataWindowGrid.TryRead(new IntPtr(grid.DataWindow.Current.NativeWindowHandle), out DataWindowGrid cells, out string details), details);

            // What an action reads from a cell must not depend on which of the two routes found the cell,
            // or a test recorded on one machine would read differently on another.
            List<string> differences = [];
            int compared = 0;
            foreach (string column in table.ColumnNames)
            {
                UIAuto.AutomationElement[] listed = table.MainDict[column];
                IReadOnlyList<DataWindowCell> shown = cells.Column(column);
                Assert.AreEqual(listed.Length, shown.Count, "the cells of " + column);
                for (int row = 0; row < listed.Length; row++)
                {
                    string viaUiAutomation = grid.Helper.GetControlValue(listed[row]);
                    string viaAccessibleObject = shown[row].Text;
                    compared++;
                    if (!string.Equals(viaUiAutomation, viaAccessibleObject, StringComparison.Ordinal))
                    {
                        differences.Add(column + "[" + row + "] reads '" + viaUiAutomation + "' through UI Automation and '" + viaAccessibleObject + "' through the accessible object");
                    }
                }
            }

            Assert.IsTrue(compared > 0, "no cell was compared");
            Assert.AreEqual(0, differences.Count, string.Join(Environment.NewLine, differences));
        }

        [TestMethod]
        [Timeout(300000)]
        public void TheCaptionOfAColumnOfARealDataWindowIsWhatItsHeaderShowsThroughUiAutomation()
        {
            using LiveGrid grid = LiveGrid.Open();
            TableElementInfo table = grid.Helper.GetTableElementInfoForGrid(grid.DataWindow);
            Assert.IsTrue(DataWindowGrid.TryRead(new IntPtr(grid.DataWindow.Current.NativeWindowHandle), out DataWindowGrid cells, out string details), details);

            List<string> differences = [];
            int compared = 0;
            foreach (string column in cells.ColumnNames)
            {
                string caption = cells.CaptionOf(column);
                if (caption == null || !table.MainDict.TryGetValue(column + "_t", out UIAuto.AutomationElement[] headers) || headers.Length != 1)
                {
                    continue;
                }

                compared++;
                string shown = string.Join(" ", (grid.Helper.GetControlValue(headers[0]) ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
                if (!string.Equals(shown, caption, StringComparison.Ordinal))
                {
                    differences.Add(column + " has the caption '" + caption + "' through the accessible object and '" + shown + "' through UI Automation");
                }

                CollectionAssert.Contains(cells.ColumnsCaptioned(caption).ToList(), column, "the caption '" + caption + "' does not lead back to " + column);
            }

            Assert.IsTrue(compared > 0, "no column has a header named after it: " + cells.DescribeColumns());
            Assert.AreEqual(0, differences.Count, string.Join(Environment.NewLine, differences));
        }

        [TestMethod]
        [Timeout(300000)]
        public void TableActionsOnARealDataWindowAreAnsweredFromTheCellsUiAutomationLists()
        {
            using LiveGrid grid = LiveGrid.Open();
            TableElementInfo table = grid.Helper.GetTableElementInfoForGrid(grid.DataWindow);
            string xpath = grid.Helper.GetElementAbsoluteXPath(grid.DataWindow);
            string column = table.ColumnNames[0];
            PBDriver driver = (PBDriver)grid.Helper.WindowExplorer;

            ActTableElement count = GridAction(xpath, ActTableElement.eTableAction.GetRowCount, "Row Number", "0", column);
            driver.RunAction(count);
            ActTableElement value = GridAction(xpath, ActTableElement.eTableAction.GetValue, "Row Number", "0", column);
            driver.RunAction(value);

            Assert.IsTrue(string.IsNullOrEmpty(count.Error), count.Error);
            Assert.AreEqual(table.RowCount.ToString(), count.ExInfo, "the row count");
            Assert.IsTrue(string.IsNullOrEmpty(value.Error), value.Error);
            Assert.AreEqual(grid.Helper.GetControlValue(table.MainDict[column][0]), value.ExInfo, "the value of the first cell of " + column);
        }

        [TestMethod]
        [Timeout(300000)]
        public void ARowAndAColumnOutOfViewOfARealDataWindowAreScrolledToAndActivated()
        {
            using LiveGrid grid = LiveGrid.Open();
            IntPtr handle = new IntPtr(grid.DataWindow.Current.NativeWindowHandle);

            // The cells are placed in the pixels of the grid's application, which a scaled display does not share.
            using IDisposable inItsPixels = DataWindowGrid.InCoordinatesOf(handle);
            Assert.IsTrue(DataWindowGrid.TryRead(handle, out DataWindowGrid whole, out string details), details);
            Assert.IsTrue(whole.RowCount >= 2, "the grid test application lists " + whole.RowCount + " rows, and a row has to lie out of view");
            int lastRow = whole.RowCount - 1;

            // A column of a size that can be clicked, as far right and as far left as the DataWindow has one.
            List<string> clickable = whole.ColumnNames.Where(name => BoundsOf(whole.Column(name)[0]).Width >= 30).ToList();
            string farRight = clickable.OrderByDescending(name => BoundsOf(whole.Column(name)[0]).X).First();
            string farLeft = clickable.OrderBy(name => BoundsOf(whole.Column(name)[0]).X).First();
            Assert.AreNotEqual(farRight, farLeft);

            // The DataWindow is made too small for its last row and its last column, which is what a long list
            // in a small window looks like: it shows a little over one row, and the left of its columns.
            Assert.IsTrue(DataWindowGrid.TryGetClientArea(handle, out Rectangle client));
            Rectangle firstRow = BoundsOf(whole.Column(farLeft)[0]);
            const int border = 4;
            int height = (firstRow.Bottom + 20 - client.Top) + border;
            Assert.IsTrue(SetWindowPos(handle, IntPtr.Zero, 0, 0, 429 + border, height, 0x0002 | 0x0004 | 0x0010), "the DataWindow was not resized");
            Assert.IsTrue(WaitUntil(() => DataWindowGrid.TryGetClientArea(handle, out Rectangle resized) && resized.Width < client.Width), "the DataWindow " + client + " kept its size when it was made " + (429 + border) + "x" + height);
            Assert.IsTrue(DataWindowGrid.TryRead(handle, out DataWindowGrid cells, out details), details);
            Assert.IsTrue(DataWindowGrid.TryGetClientArea(handle, out client));
            if (DataWindowGrid.IsInside(client, BoundsOf(cells.Column(farRight)[lastRow])))
            {
                Assert.Inconclusive("The last row and the last column are in view of the resized DataWindow " + client + ", so nothing has to be scrolled.");
            }

            ActTableElement toTheEnd = GridAction(string.Empty, ActTableElement.eTableAction.ActivateRow, "Row Number", lastRow.ToString(), farRight);
            grid.Helper.HandleDataWindowGridAction(grid.DataWindow, cells, toTheEnd);

            Assert.IsTrue(DataWindowGrid.TryGetClientArea(handle, out client));
            Rectangle shown = BoundsOf(cells.Column(farRight)[lastRow]);
            Assert.IsTrue(DataWindowGrid.IsInside(client, shown), "the cell of the last row and the last column was left at " + shown + ", outside the client area " + client + ": " + toTheEnd.Error);
            // The footer of the grid test application covers the foot of the client area, where the DataWindow may leave its
            // last row when it scrolls no further, so the part of the row above the foot is what is aimed at.
            int pitch = shown.Y - BoundsOf(cells.Column(farRight)[lastRow - 1]).Y;
            Assert.IsTrue(cells.TryBringIntoView(handle, cells.Column(farRight)[lastRow], out Rectangle aimed, out details), details);
            Assert.IsTrue(aimed.Y + (aimed.Height / 2) + pitch < client.Bottom,
                "the point aimed at in the last row, the middle of " + aimed + ", lies at the foot of the client area " + client + ", which the footer covers");

            ActTableElement backToTheStart = GridAction(string.Empty, ActTableElement.eTableAction.ActivateRow, "Row Number", "0", farLeft);
            grid.Helper.HandleDataWindowGridAction(grid.DataWindow, cells, backToTheStart);

            shown = BoundsOf(cells.Column(farLeft)[0]);
            Assert.IsTrue(DataWindowGrid.IsInside(client, shown), "the cell of the first row and the first column was left at " + shown + ", outside the client area " + client + ": " + backToTheStart.Error);

            // Both rows are in view, as the scroll is checked first. What is left to go wrong is the click, which
            // goes by window messages to the window at the point, and is not delivered where another window covers it.
            string clickFailure = (toTheEnd.Error + backToTheStart.Error).Trim();
            if (clickFailure.Length > 0)
            {
                Assert.Inconclusive("The cells were scrolled into view, but a click was not delivered: " + clickFailure);
            }

            StringAssert.Contains(toTheEnd.ExInfo, "activated");
            StringAssert.Contains(backToTheStart.ExInfo, "activated");
        }

        private static Rectangle BoundsOf(DataWindowCell cell)
        {
            Assert.IsTrue(cell.TryGetBounds(out Rectangle bounds), "the grid does not say where the cell " + cell.Name + " is");
            return bounds;
        }

        private static bool WaitUntil(Func<bool> condition)
        {
            Stopwatch clock = Stopwatch.StartNew();
            while (!condition())
            {
                if (clock.Elapsed > TimeSpan.FromSeconds(10))
                {
                    return false;
                }

                Thread.Sleep(100);
            }

            return true;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        private static ActTableElement GridAction(string locator, ActTableElement.eTableAction action, string rowType, string rowValue, string column)
        {
            ActTableElement table = new ActTableElement
            {
                LocateBy = eLocateBy.ByXPath,
                LocateValueCalculated = locator,
                ControlAction = action,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                LocateColTitle = column,
                LocateRowType = rowType,
                LocateRowValue = rowValue
            };

            // What the runner does before handing the action to the driver.
            foreach (ActInputValue input in table.InputValues)
            {
                input.ValueForDriver = input.Value;
            }

            return table;
        }

        /// <summary>
        /// The grid test application, running, with the agent that drives it.
        /// </summary>
        private sealed class LiveGrid : IDisposable
        {
            private readonly Process mApp;

            private LiveGrid(Process app, UIAComWrapperHelper helper, UIAuto.AutomationElement dataWindow)
            {
                mApp = app;
                Helper = helper;
                DataWindow = dataWindow;
            }

            internal UIAComWrapperHelper Helper { get; }

            internal UIAuto.AutomationElement DataWindow { get; }

            internal static LiveGrid Open()
            {
                Process app = Process.Start(new ProcessStartInfo
                {
                    FileName = "csm_grid_target.exe",
                    WorkingDirectory = TestResources.GetTestResourcesFolder("PBTestApp"),
                    UseShellExecute = true
                });

                try
                {
                    UIAuto.AutomationElement window = WaitForWindow(app);
                    Assert.IsNotNull(window, "csm_grid_target did not show a window");

                    UIAuto.AutomationElement dataWindow = WaitForDataWindow(window);
                    Assert.IsNotNull(dataWindow, "csm_grid_target shows no DataWindow with cells in it");

                    UIAComWrapperHelper helper = new UIAComWrapperHelper
                    {
                        mPlatform = ePlatformType.PowerBuilder,
                        CurrentWindow = window,
                        CurrentWindowName = window.Current.Name,
                        ImplicitWait = 0,
                        NonIntrusiveInput = true
                    };
                    helper.UpdateRootElement();
                    helper.WindowExplorer = new PBDriver { mUIAutomationHelper = helper };
                    return new LiveGrid(app, helper, dataWindow);
                }
                catch (Exception)
                {
                    Close(app);
                    throw;
                }
            }

            public void Dispose()
            {
                Close(mApp);
            }

            private static UIAuto.AutomationElement WaitForWindow(Process app)
            {
                Stopwatch clock = Stopwatch.StartNew();
                while (clock.Elapsed < TimeSpan.FromSeconds(60))
                {
                    Thread.Sleep(500);
                    app.Refresh();
                    if (app.MainWindowHandle != IntPtr.Zero)
                    {
                        return UIAuto.AutomationElement.FromHandle(app.MainWindowHandle);
                    }
                }

                return null;
            }

            /// <summary>
            /// The grid takes a moment to fill, so it is asked for until it lists its cells.
            /// </summary>
            private static UIAuto.AutomationElement WaitForDataWindow(UIAuto.AutomationElement window)
            {
                Stopwatch clock = Stopwatch.StartNew();
                while (clock.Elapsed < TimeSpan.FromSeconds(30))
                {
                    Thread.Sleep(500);
                    foreach (UIAuto.AutomationElement candidate in Descendants(window, 4))
                    {
                        if (candidate.Current.ClassName.StartsWith("pbdw", StringComparison.OrdinalIgnoreCase)
                            && Children(candidate).Count(child => !string.IsNullOrEmpty(child.Current.Name)) >= 4)
                        {
                            return candidate;
                        }
                    }
                }

                return null;
            }

            private static List<UIAuto.AutomationElement> Children(UIAuto.AutomationElement parent)
            {
                List<UIAuto.AutomationElement> children = [];
                for (UIAuto.AutomationElement child = UIAuto.TreeWalker.RawViewWalker.GetFirstChild(parent); child != null; child = UIAuto.TreeWalker.RawViewWalker.GetNextSibling(child))
                {
                    children.Add(child);
                }

                return children;
            }

            private static List<UIAuto.AutomationElement> Descendants(UIAuto.AutomationElement parent, int depth)
            {
                List<UIAuto.AutomationElement> descendants = [];
                foreach (UIAuto.AutomationElement child in Children(parent))
                {
                    descendants.Add(child);
                    if (depth > 1)
                    {
                        descendants.AddRange(Descendants(child, depth - 1));
                    }
                }

                return descendants;
            }

            private static void Close(Process app)
            {
                try
                {
                    app?.Kill();
                    app?.WaitForExit(10000);
                }
                catch (InvalidOperationException)
                {
                    // already gone
                }

                app?.Dispose();
            }
        }
    }
}
