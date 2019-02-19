using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET
{
    /// <summary>
    /// This class is used to add methods for action conversion helpers
    /// </summary>
    public class ActionConversionUtils
    {
        ///// <summary>
        ///// This method is used to find the relative element from POM for the existing action
        ///// </summary>
        ///// <param name="selectedPOM"></param>
        ///// <param name="newActUIElement"></param>
        ///// <param name="currentAction"></param>
        ///// <returns></returns>
        //private ActUIElement GetMappedElementFromPOMForAction(ApplicationPOMModel selectedPOM, ActUIElement newActUIElement, Act currentAction)
        //{
        //    bool isPOM = true;
        //    ElementInfo elementInfo = null;
        //    IEnumerable<ElementInfo> lst = selectedPOM.MappedUIElements.Where(x => x.ElementTypeEnum != eElementType.Div);
        //    foreach (var item in lst)
        //    {
        //        if (item != null)
        //        {
        //            if (currentAction.LocateBy == eLocateBy.ByXY)
        //            {
        //                int oldX = 0;
        //                int oldY = 0;

        //                string[] oldxy = currentAction.LocateValue.Split(',');
        //                if (oldxy != null && oldxy.Length > 0)
        //                {
        //                    int.TryParse(oldxy[0], out oldX);
        //                    if (oldxy.Length > 1)
        //                    {
        //                        int.TryParse(oldxy[1], out oldY);
        //                    }
        //                }

        //                if (!string.IsNullOrEmpty(currentAction.LocateValue))
        //                {
        //                    newActUIElement.ElementLocateBy = eLocateBy.ByXY;
        //                    int x = 0;
        //                    int y = 0;
        //                    foreach (var prop in item.Properties)
        //                    {
        //                        if (!string.IsNullOrEmpty(prop.Value) && prop.Name == "X")
        //                        {
        //                            int.TryParse(prop.Value, out x);
        //                        }
        //                        if (!string.IsNullOrEmpty(prop.Value) && prop.Name == "Y")
        //                        {
        //                            int.TryParse(prop.Value, out y);
        //                        }
        //                        if (x > 0 && y > 0)
        //                        {
        //                            break;
        //                        }
        //                    }
        //                    if (oldX == x && oldY == y)
        //                    {
        //                        isPOM = false;
        //                        elementInfo = item;
        //                        elementInfo.X = x;
        //                        elementInfo.Y = y;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (currentAction.LocateBy == eLocateBy.ByXPath || currentAction.LocateBy == eLocateBy.ByRelXPath)
        //                {
        //                    if (currentAction.LocateValue == item.XPath)
        //                    {
        //                        elementInfo = item;
        //                        break;
        //                    }
        //                }
        //                else if (currentAction.LocateBy == eLocateBy.ByName)
        //                {
        //                    if (item.ElementName.Contains(currentAction.LocateValue))
        //                    {
        //                        elementInfo = item;
        //                        break;
        //                    }
        //                }
        //                else if (currentAction.LocateBy == eLocateBy.ByID)
        //                {
        //                    foreach (var prop in item.Properties)
        //                    {
        //                        if (!string.IsNullOrEmpty(prop.Value) &&
        //                            prop.Name == "id" && prop.Value == currentAction.LocateValue)
        //                        {
        //                            elementInfo = item;
        //                            break;
        //                        }
        //                    }
        //                }
        //                if (item.Properties != null && elementInfo == null)
        //                {
        //                    foreach (var prop in item.Properties)
        //                    {
        //                        if (!string.IsNullOrEmpty(prop.Value) &&
        //                            (prop.Name == "XPath" && currentAction.LocateValue.Contains(prop.Value)) ||
        //                            (prop.Name == "Relative XPath" && currentAction.LocateValue.Contains(prop.Value)) ||
        //                            (prop.Name == "Name" && currentAction.LocateValue.Contains(prop.Value)))
        //                        {
        //                            elementInfo = item;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            if (elementInfo != null)
        //            {
        //                if (isPOM)
        //                {
        //                    newActUIElement.ElementLocateBy = eLocateBy.POMElement;
        //                    newActUIElement.ElementLocateValue = string.Format("{0}_{1}", selectedPOM.Guid.ToString(), elementInfo.Guid.ToString());
        //                }
        //                else
        //                {
        //                    newActUIElement.ElementLocateBy = eLocateBy.ByXY;
        //                    newActUIElement.ElementLocateValue = string.Format("X={0},Y={1}", elementInfo.X, elementInfo.Y);
        //                }
        //                newActUIElement.Value = currentAction.Value;
        //                newActUIElement.ElementType = elementInfo.ElementTypeEnum;
        //                ResetActUIFields(newActUIElement);
        //                break;
        //            }
        //        }
        //    }
        //    return newActUIElement;
        //}

        ///// <summary>
        ///// This method with reset the ActUIFields
        ///// </summary>
        ///// <param name="action"></param>
        //private void ResetActUIFields(ActUIElement action)
        //{
        //    action.AddOrUpdateInputParamValue(ActUIElement.Fields.XCoordinate, String.Empty);
        //    action.AddOrUpdateInputParamValue(ActUIElement.Fields.Value, String.Empty);
        //    action.AddOrUpdateInputParamValue(ActUIElement.Fields.ValueToSelect, String.Empty);
        //    action.AddOrUpdateInputParamValue(ActUIElement.Fields.YCoordinate, String.Empty);
        //    action.AddOrUpdateInputParamValue(ActUIElement.Fields.ControlAction, String.Empty);
        //}
    }
}
