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
    /// <summary>
    /// Locate <see cref="ApplicationPOMModel"/> elements.
    /// </summary>
    /// <typeparam name="TElement">The type of elements to locate.</typeparam>
    internal sealed class POMElementLocator<TElement>
    {
        /// <summary>
        /// Delegate for locating elements based on primitive locators.
        /// <br/><br/>
        /// <b>NOTE</b><br/>
        /// Only primitive locators like <see cref="eLocateBy.ByID"/>, <see cref="eLocateBy.ByXPath"/> etc. will be passed to this delegate. <br/>
        /// Complex locators like <see cref="eLocateBy.POMElement"/> will not be passed.
        /// </summary>
        /// <param name="locateBy">The primitive locator type. </param>
        /// <param name="locateValue">The value to locate the element.</param>
        /// <returns>The located elements.</returns>
        internal delegate Task<IEnumerable<TElement>> ElementsProvider(eLocateBy locateBy, string locateValue);

        internal sealed class LocateResult
        {
            /// <summary>
            /// Get or initialize the located elements.
            /// </summary>
            internal required IEnumerable<TElement> Elements { get; init; }

            /// <summary>
            /// Get or initialize a value indicating whether the POM was auto-updated during the locate operation.
            /// </summary>
            internal required bool WasAutoUpdated { get; init; }
        }

        internal sealed class Args
        {
            /// <summary>
            /// Get or initialize the information about the element to locate.
            /// </summary>
            internal required ElementInfo ElementInfo { get; init; }

            /// <summary>
            /// Get or initialize the provider for locating elements.
            /// </summary>
            internal required ElementsProvider ElementsProvider { get; init; }

            /// <summary>
            /// Get or initialize the current business flow.
            /// </summary>
            internal required BusinessFlow BusinessFlow { get; init; }

            /// <summary>
            /// Get or initialize the current project environment.
            /// </summary>
            internal required ProjEnvironment Environment { get; init; }

            /// <summary>
            /// Get or initialize a value indicating whether the POM should be auto-updated during the locate operation.
            /// </summary>
            internal required bool AutoUpdatePOM { get; init; }
        }

        private readonly bool _autoUpdatePOM;
        private readonly ElementInfo _elementInfo;
        private readonly ElementsProvider _elementsProvider;

        //we only need BusinessFlow and ProjEnvironment to evaluate custom POM locators. can't we just take ValueExpression as input?
        private readonly BusinessFlow _businessFlow;
        private readonly ProjEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of <see cref="POMElementLocator{TElement}"/>.
        /// </summary>
        /// <param name="args">The arguments for initializing <see cref="POMElementLocator{TElement}"/>.</param>
        internal POMElementLocator(Args args)
        {
            _autoUpdatePOM = args.AutoUpdatePOM;
            _elementInfo = args.ElementInfo;
            _elementsProvider = args.ElementsProvider;
            _businessFlow = args.BusinessFlow;
            _environment = args.Environment;
        }

        /// <summary>
        /// Locate POM elements. This method also tries to modify the POM if no elements are found initially and appropriate configurations are set via <see cref="Args"/>.
        /// </summary>
        /// <returns><see cref="LocateResult"/> containing the result of the locate operation.</returns>
        internal async Task<LocateResult> LocateAsync()
        {
            IEnumerable<TElement>? elements = null;
            bool wasAutoUpdated = false;

            elements = await GetElementsByLocators(_elementInfo.Locators);

            bool noElementFound = elements == null || !elements.Any();

            if (noElementFound && _autoUpdatePOM)
            {
                UpdatePOM();
                wasAutoUpdated = true;
                elements = await GetElementsByLocators(_elementInfo.Locators);
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

        /// <summary>
        /// Get collection of <typeparamref name="TElement"/> matching any of the provided locators.
        /// </summary>
        /// <param name="locators">Collection of <see cref="ElementLocator"/> to search for.</param>
        /// <returns>Collection of <typeparamref name="TElement"/> matching any of the provided locators</returns>
        private async Task<IEnumerable<TElement>> GetElementsByLocators(IEnumerable<ElementLocator> locators)
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
