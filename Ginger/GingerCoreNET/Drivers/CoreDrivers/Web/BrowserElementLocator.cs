using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            POMElementLocator<IBrowserElement> pomElementLocator = new(new POMElementLocator<IBrowserElement>.Args
            {
                AutoUpdatePOM = false,
                BusinessFlow = _context.BusinessFlow,
                Environment = _context.Environment,
                ElementInfo = pomLocatorParser.ElementInfo,
                ElementsProvider = elementsProvider,
            });
            POMElementLocator<IBrowserElement>.LocateResult result = await pomElementLocator.LocateAsync();

            return result.Elements;
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
