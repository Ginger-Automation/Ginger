using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET
{
    /// <summary>
    /// This class is used to add methods for action conversion helpers
    /// </summary>
    public class ActionConversionUtils
    {
        /// <summary>
        /// private constructor
        /// </summary>
        private ActionConversionUtils()
        {
        }

        private static ActionConversionUtils mInstance;
        public static ActionConversionUtils Instance
        {
            get
            {
                if(mInstance == null)
                {
                    mInstance = new ActionConversionUtils();
                }
                return mInstance;
            }
        }

        /// <summary>
        /// This method is used to find the relative element from POM for the existing action
        /// </summary>
        /// <param name="newActUIElement"></param>
        /// <param name="pomModelObject"></param>
        /// <returns></returns>
        public Act GetMappedElementFromPOMForAction(Act newActUIElement, string pomModelObject)
        {
            ApplicationPOMModel selectedPOM = new ApplicationPOMModel();

            ObservableList<ApplicationPOMModel> pomLst = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>();
            selectedPOM = pomLst.Where(x => x.Guid.ToString() == pomModelObject).SingleOrDefault();

            bool isPOM = true;
            ElementInfo elementInfo = null;
            IEnumerable<ElementInfo> lst = selectedPOM.MappedUIElements.Where(x => x.ElementTypeEnum != eElementType.Div);

            string locateValue = Convert.ToString(newActUIElement.GetType().GetProperty("LocateValue").GetValue(newActUIElement, null));
            string elementLocateBy = Convert.ToString(newActUIElement.GetType().GetProperty("ElementLocateBy").GetValue(newActUIElement, null));

            PropertyInfo pLocateBy = newActUIElement.GetType().GetProperty("ElementLocateBy");
            if (pLocateBy != null)
            {
                if (elementLocateBy != "ByXY")
                {
                    if (pLocateBy.PropertyType.IsEnum)
                        pLocateBy.SetValue(newActUIElement, Enum.Parse(pLocateBy.PropertyType, "POMElement"));  
                }
                else
                {
                    if (pLocateBy.PropertyType.IsEnum)
                        pLocateBy.SetValue(newActUIElement, Enum.Parse(pLocateBy.PropertyType, "ByXY"));
                }
            }
            
            foreach (var item in lst)
            {
                if (item != null)
                {
                    if (elementLocateBy == eLocateBy.ByXY.ToString())
                    {
                        int oldX = 0;
                        int oldY = 0;

                        string[] oldxy = locateValue.Split(',');
                        if (oldxy != null && oldxy.Length > 0)
                        {
                            int.TryParse(oldxy[0], out oldX);
                            if (oldxy.Length > 1)
                            {
                                int.TryParse(oldxy[1], out oldY);
                            }
                        }

                        if (!string.IsNullOrEmpty(locateValue))
                        {
                            int x = 0;
                            int y = 0;
                            foreach (var prop in item.Properties)
                            {
                                if (!string.IsNullOrEmpty(prop.Value) && prop.Name == "X")
                                {
                                    int.TryParse(prop.Value, out x);
                                }
                                if (!string.IsNullOrEmpty(prop.Value) && prop.Name == "Y")
                                {
                                    int.TryParse(prop.Value, out y);
                                }
                                if (x > 0 && y > 0)
                                {
                                    break;
                                }
                            }
                            if (oldX == x && oldY == y)
                            {
                                isPOM = false;
                                elementInfo = item;
                                elementInfo.X = x;
                                elementInfo.Y = y;
                            }
                        }
                    }
                    else
                    {
                        if (elementLocateBy == eLocateBy.ByXPath.ToString() || elementLocateBy == eLocateBy.ByRelXPath.ToString())
                        {
                            if (locateValue == item.XPath)
                            {
                                elementInfo = item;
                            }
                        }
                        else if (elementLocateBy == eLocateBy.ByName.ToString())
                        {
                            if (item.ElementName.Contains(locateValue))
                            {
                                elementInfo = item;
                            }
                        }
                        else if (elementLocateBy == eLocateBy.ByID.ToString())
                        {
                            foreach (var prop in item.Properties)
                            {
                                if (!string.IsNullOrEmpty(prop.Value) &&
                                    prop.Name == "id" && prop.Value == locateValue)
                                {
                                    elementInfo = item;
                                    break;
                                }
                            }
                        }
                        if (item.Properties != null && elementInfo == null)
                        {
                            foreach (var prop in item.Properties)
                            {
                                if (!string.IsNullOrEmpty(prop.Value) &&
                                    (prop.Name == "XPath" && locateValue.Contains(prop.Value)) ||
                                    (prop.Name == "Relative XPath" && locateValue.Contains(prop.Value)) ||
                                    (prop.Name == "Name" && locateValue.Contains(prop.Value)))
                                {
                                    elementInfo = item;
                                    break;
                                }
                            }
                        }
                    }
                    if (elementInfo != null)
                    {
                        string strVal = string.Empty;
                        if (isPOM)
                        {
                            strVal = string.Format("{0}_{1}", selectedPOM.Guid.ToString(), elementInfo.Guid.ToString());
                        }
                        else
                        {
                            strVal = string.Format("X={0},Y={1}", elementInfo.X, elementInfo.Y);
                        }

                        PropertyInfo pLocateVal = newActUIElement.GetType().GetProperty("ElementLocateValue");
                        if (pLocateVal != null)
                        {
                            pLocateVal.SetValue(newActUIElement, strVal);
                        }

                        PropertyInfo pElementType = newActUIElement.GetType().GetProperty("ElementType");
                        if (pElementType != null && pElementType.PropertyType.IsEnum)
                            pElementType.SetValue(newActUIElement, Enum.Parse(pElementType.PropertyType, Convert.ToString(elementInfo.ElementTypeEnum)));
                        break;
                    }
                }
            }
            return newActUIElement;
        }
    }
}
