using Amdocs.Ginger.Common.UIElement;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.External.Katalon.Conversion
{
    internal static class KatalonElementToElementInfoConverter
    {
        /// <summary>
        /// Converts a collection of Katalon elements to Ginger ElementInfo objects.
        /// </summary>
        /// <param name="katalonElementList">The list of Katalon elements to convert.</param>
        /// <returns>A collection of converted ElementInfo objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when katalonElementList is null.</exception>
        internal static IEnumerable<ElementInfo> Convert(IEnumerable<KatalonElementEntity> katalonElementList)
        {
            if (katalonElementList == null)
            {
                throw new System.ArgumentNullException(nameof(katalonElementList));
            }

            if (!katalonElementList.Any())
            {
                return Enumerable.Empty<ElementInfo>();
            }

            List<ElementInfo> elementInfoList = [];

            var katalonWebElementList = katalonElementList
                .OfType<KatalonWebElementEntity>()
                .ToList();

            foreach (KatalonWebElementEntity katalonWebElement in katalonWebElementList)
            {
                elementInfoList.Add(katalonWebElement.ToElementInfo(katalonWebElementList));
            }
            return elementInfoList;
        }
    }
}
