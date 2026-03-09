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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Drivers.Common;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal sealed class BrowserElementLocator : IBrowserElementLocator
    {
        internal sealed class Context
        {
            internal required BusinessFlow BusinessFlow { get; init; }
            internal required ProjEnvironment Environment { get; init; }
            internal required POMExecutionUtils POMExecutionUtils { get; init; }
            internal IAgent? Agent { get; init; }
        }

        //split by comma outside brackets
        private static readonly Regex FramesFromElementPathSplitter = new(@",(?![^\[]*[\]])");

        private readonly IBrowserTab _browserTab;
        private readonly Context _context;

        internal BrowserElementLocator(IBrowserTab browserTab, Context context)
        {
            _browserTab = browserTab;
            _context = context;
        }

        public async Task<IBrowserElement?> FindFirstMatchingElement(eLocateBy locateBy, string locateValue)
        {
            IEnumerable<IBrowserElement> elements = await FindMatchingElements(locateBy, locateValue);

            IBrowserElement? firstElement = elements.FirstOrDefault();
            if (firstElement == null)
            {
                throw new EntityNotFoundException($"No element found by locator '{locateBy}' and value '{locateValue}'");
            }

            return firstElement;
        }

        public Task<IEnumerable<IBrowserElement>> FindMatchingElements(eLocateBy locateBy, string locateValue)
        {
            if (locateBy == eLocateBy.POMElement)
            {
                return FindAllMatchingElementsFromPOMAsync(locateValue);
            }
            else
            {
                return _browserTab.GetElementsAsync(locateBy, locateValue);
            }
        }

        private async Task<IEnumerable<IBrowserElement>> FindAllMatchingElementsFromPOMAsync(string locateValue)
        {
            Func<Guid, ApplicationPOMModel> pomByIdProvider = WorkSpace
                .Instance
                .SolutionRepository
                .GetRepositoryItemByGuid<ApplicationPOMModel>;

            POMLocatorParser pomLocatorParser = POMLocatorParser.Create(locateValue, pomByIdProvider);
            if (pomLocatorParser.ElementInfo == null)
            {
                return [];
            }

            await SwitchToFrameOfElementAsync(pomLocatorParser.ElementInfo);

            POMElementLocator<IBrowserElement>.ElementsProvider elementsProvider = _browserTab.GetElementsAsync;

            ElementInfo element = pomLocatorParser.ElementInfo;

            IBrowserElement? parentElement = await FindParentElementInPOMAsync(pomLocatorParser.POMId, element);
            bool isParentElementShadowHost = parentElement != null && (await parentElement.ShadowRootAsync()) != null;
            if (isParentElementShadowHost)
            {
                element = (ElementInfo)element.CreateCopy(setNewGUID: false, deepCopy: true);
                foreach (ElementLocator locator in element.Locators)
                {
                    if (locator.LocateBy is eLocateBy.ByXPath or eLocateBy.ByRelXPath)
                    {
                        locator.LocateBy = eLocateBy.ByCSS;
                        locator.LocateValue = new ShadowDOM().ConvertXPathToCssSelector(locator.LocateValue);
                    }
                }
            }

            POMElementLocator<IBrowserElement> pomElementLocator = new(new POMElementLocator<IBrowserElement>.Args
            {
                BusinessFlow = _context.BusinessFlow,
                Environment = _context.Environment,
                ElementInfo = element,
                ElementsProvider = elementsProvider,
                POMExecutionUtils = _context.POMExecutionUtils,
                Agent = _context.Agent,
            });
            POMElementLocator<IBrowserElement>.LocateResult result = await pomElementLocator.LocateAsync();

            return result.Elements;
        }

        private async Task<IBrowserElement?> FindParentElementInPOMAsync(Guid pomId, ElementInfo childElement)
        {
            if (childElement is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }

            string parentElementIdAsString = htmlElementInfo.FindParentPOMGuid();
            if (!Guid.TryParse(parentElementIdAsString, out Guid parentElementId) || parentElementId == Guid.Empty)
            {
                return null;
            }
            string parentLocateValue = POMLocatorParser.Convert(pomId, parentElementId);
            IBrowserElement? parentElement = (await FindAllMatchingElementsFromPOMAsync(parentLocateValue)).FirstOrDefault();

            return parentElement;
        }

        private async Task SwitchToFrameOfElementAsync(ElementInfo elementInfo)
        {
            string pathToElement = elementInfo.Path;
            if (!elementInfo.IsAutoLearned)
            {
                GingerCore.ValueExpression valueExpression = new(_context.Environment, _context.BusinessFlow);
                pathToElement = valueExpression.Calculate(pathToElement);
            }

            if (string.IsNullOrEmpty(pathToElement))
            {
                return;
            }

            await _browserTab.SwitchToMainFrameAsync();

            string[] iframesPaths = FramesFromElementPathSplitter.Split(pathToElement);

            foreach (string iframePath in iframesPaths)
            {
                await _browserTab.SwitchFrameAsync(eLocateBy.ByRelXPath, iframePath);
            }
        }
    }
}
