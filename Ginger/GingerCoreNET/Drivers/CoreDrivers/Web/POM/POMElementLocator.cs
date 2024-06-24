using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Environments;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM
{
    internal sealed class POMElementLocator<TElement>
    {
        internal delegate Task<IEnumerable<TElement>> ElementsProvider(eLocateBy locateBy, string locateValue);

        internal sealed class LocateResult
        {
            internal required IEnumerable<TElement> Elements { get; init; }

            internal required bool WasAutoUpdated { get; init; }
        }

        internal sealed class Args
        {
            internal required ElementInfo ElementInfo { get; init; }
            internal required ElementsProvider ElementsProvider { get; init; }
            internal required BusinessFlow BusinessFlow { get; init; }
            internal required ProjEnvironment Environment { get; init; }
            internal required bool AutoUpdatePOM { get; init; }
        }

        private readonly bool _autoUpdatePOM;
        private readonly ElementInfo _elementInfo;
        private readonly ElementsProvider _elementsProvider;

        //we only need BusinessFlow and ProjEnvironment to evaluate custom POM locators. can't we just take ValueExpression as input?
        private readonly BusinessFlow _businessFlow;
        private readonly ProjEnvironment _environment;

        internal POMElementLocator(Args args)
        {
            _autoUpdatePOM = args.AutoUpdatePOM;
            _elementInfo = args.ElementInfo;
            _elementsProvider = args.ElementsProvider;
            _businessFlow = args.BusinessFlow;
            _environment = args.Environment;
        }

        internal async Task<LocateResult> LocateAsync()
        {
            IEnumerable<TElement>? elements = null;
            bool wasAutoUpdated = false;

            elements = await LocateAsync(_elementInfo.Locators);

            bool noElementFound = elements == null || !elements.Any();

            if (noElementFound && _autoUpdatePOM)
            {
                UpdatePOM();
                wasAutoUpdated = true;
                elements = await LocateAsync(_elementInfo.Locators);
            }

            if (elements == null)
            {
                elements = [];
            }

            return new LocateResult()
            {
                Elements = elements,
                WasAutoUpdated = wasAutoUpdated,
            };
        }

        private async Task<IEnumerable<TElement>> LocateAsync(IEnumerable<ElementLocator> locators)
        {
            IEnumerable<TElement>? elements = null;

            foreach (ElementLocator locator in locators)
            {
                locator.StatusError = string.Empty;
                locator.LocateStatus = ElementLocator.eLocateStatus.Pending;

                //TODO: handle Shadow Root

                if (!locator.Active)
                {
                    continue;
                }

                try
                {
                    string locateValue = EvaluteLocateValue(locator.LocateValue);
                    elements = await _elementsProvider(locator.LocateBy, locateValue);
                }
                catch (Exception) { }

                if (elements != null && elements.Any())
                {
                    locator.StatusError = string.Empty;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Passed;
                    break;
                }
                else
                {
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                }
            }

            if (elements == null)
            {
                elements = [];
            }

            return elements;
        }

        private string EvaluteLocateValue(string locateValue)
        {
            GingerCore.ValueExpression valueExpression = new(_environment, _businessFlow);
            return valueExpression.Calculate(locateValue);
        }

        private void UpdatePOM()
        {
            throw new NotImplementedException();
        }
    }
}
