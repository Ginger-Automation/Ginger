using Amdocs.Ginger.Common.UIElement;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.External.Katalon.Conversion
{
    internal static class KatalonElementToElementInfoConverter
    {
        internal static IEnumerable<ElementInfo> Convert(IEnumerable<KatalonElementEntity> katalonElementList)
        {
            List<ElementInfo> elementInfoList = [];

            foreach (KatalonElementEntity katalonElement in katalonElementList)
            {
                IEnumerable<KatalonWebElementEntity> katalonWebElementList = katalonElementList
                .Where(e => e is KatalonWebElementEntity)
                .Cast<KatalonWebElementEntity>();

                if (katalonElement is KatalonWebElementEntity katalonWebElement)
                {
                    elementInfoList.Add(katalonWebElement.ToElementInfo(katalonWebElementList));
                }
            }

            return elementInfoList;
        }
    }
}
