using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.SelfHealingLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Drivers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal sealed class POMLocatorParser
    {
        internal Guid POMId { get; }

        internal ApplicationPOMModel? POM { get; }

        internal Guid ElementId { get; }

        internal ElementInfo? ElementInfo { get; }

        internal static POMLocatorParser Create(string locatorValue)
        {
            Func<Guid, ApplicationPOMModel> pomProvider = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>;

            return Create(locatorValue,pomProvider);
        }

        internal static POMLocatorParser Create(string locatorValue, Func<Guid, ApplicationPOMModel> pomProvider)
        {
            if (string.IsNullOrEmpty(locatorValue))
            {
                throw new ArgumentException($"{nameof(locatorValue)} cannot be null or empty");
            }

            string[] ids = locatorValue.Split("_");
            if (ids.Length != 2)
            {
                throw new FormatException($"{nameof(locatorValue)} '{locatorValue}' is an invalid POM locator");
            }

            if (!Guid.TryParse(ids[0], out Guid pomId))
            {
                throw new FormatException($"'{ids[0]}' is not a valid POM id");
            }

            ApplicationPOMModel? pom = pomProvider(pomId);

            if (!Guid.TryParse(ids[1], out Guid elementId))
            {
                throw new FormatException($"'{ids[1]}' is not a valid POM element id");
            }

            ElementInfo? element = null;
            if (pom != null && pom.MappedUIElements != null)
            {
                element = pom.MappedUIElements.FirstOrDefault(e => e != null && e.Guid == elementId);
            }

            return new(pomId, pom, elementId, element);
        }

        private POMLocatorParser(Guid pomId, ApplicationPOMModel? pom, Guid elementId, ElementInfo? element)
        {
            POMId = pomId;
            POM = pom;
            ElementId = elementId;
            ElementInfo = element;
        }
    }
}
