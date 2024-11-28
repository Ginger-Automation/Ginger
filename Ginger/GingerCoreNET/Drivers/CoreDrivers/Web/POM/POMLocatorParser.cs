#region License
/*
Copyright © 2014-2024 European Support Limited

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

using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using System;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM
{
    /// <summary>
    /// Parses the POM locator value and provides access to the POM and element information.
    /// </summary>
    internal sealed class POMLocatorParser
    {
        /// <summary>
        /// Id of the POM.
        /// </summary>
        internal Guid POMId { get; }

        /// <summary>
        /// POM model.
        /// </summary>
        internal ApplicationPOMModel? POM { get; }

        /// <summary>
        /// Id of the element.
        /// </summary>
        internal Guid ElementId { get; }

        /// <summary>
        /// Information about the identified element.
        /// </summary>
        internal ElementInfo? ElementInfo { get; }

        /// <summary>
        /// Creates a new instance of <see cref="POMLocatorParser"/>.
        /// </summary>
        /// <param name="locatorValue">The POM element locator value.</param>
        /// <param name="pomByIdProvider">The function to provide the <see cref="ApplicationPOMModel"/> based on the given id.</param>
        /// <returns>A new instance of <see cref="POMLocatorParser"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="locatorValue"/> is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when <paramref name="locatorValue"/> is an invalid POM locator or the POM id or element id is not valid.</exception>
        internal static POMLocatorParser Create(string locatorValue, Func<Guid, ApplicationPOMModel?> pomByIdProvider)
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

            ApplicationPOMModel? pom = pomByIdProvider(pomId);

            if (!Guid.TryParse(ids[1], out Guid elementId))
            {
                throw new FormatException($"'{ids[1]}' is not a valid POM element id");
            }

            ElementInfo? element = null;
            if (pom != null && pom.MappedUIElements != null)
            {
                element = pom.MappedUIElements.FirstOrDefault(e => e != null && e.Guid == elementId);
            }

            return new POMLocatorParser(pomId, pom, elementId, element);
        }

        private POMLocatorParser(Guid pomId, ApplicationPOMModel? pom, Guid elementId, ElementInfo? element)
        {
            POMId = pomId;
            POM = pom;
            ElementId = elementId;
            ElementInfo = element;
        }

        public static string Convert(Guid pomId, Guid elementId)
        {
            return $"{pomId}_{elementId}";
        }
    }
}
