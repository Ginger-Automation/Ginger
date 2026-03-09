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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

#nullable enable
namespace Amdocs.Ginger.CoreNET.External.Katalon.Conversion
{
    public static class KatalonObjectRepositoryToPOMConverter
    {
        public sealed class Result(ApplicationPOMModel pom, ePlatformType platform)
        {
            public ApplicationPOMModel POM { get; } = pom;

            public ePlatformType Platform { get; } = platform;
        }

        /// <summary>
        /// Create multiple <see cref="ApplicationPOMModel"/> from Object Repositories in the given directory. This method also checks sub directories for Object Repositories.
        /// </summary>
        /// <param name="directory">Path of directory containing Object Repositories.</param>
        /// <returns>Collection of <see cref="ApplicationPOMModel"/> for each Object Repository found in the given <paramref name="directory"/> and all the sub directories.</returns>
        public static Task ConvertAsync(string directory, ObservableList<Result> results)
        {
            return Task.Run(() => Convert(directory, results));
        }

        internal static IEnumerable<Result> Convert(string directory)
        {
            try
            {
                List<Result> results = [];
                Convert(directory, results);
                return results;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while converting Katalon Object Repository to POM", ex);
                return Array.Empty<Result>();
            }
        }

        internal static void Convert(string directory, ICollection<Result> results)
        {
            if (directory == null || string.Equals(directory.Trim(), string.Empty))
            {
                throw new ArgumentException("Directory path is null, empty or whitespaces");
            }

            if (!Directory.Exists(directory))
            {
                throw new ArgumentException($"Directory does not exist at path '{directory}'");
            }

            if (results == null)
            {
                results = [];
            }

            try
            {
                Dictionary<ePlatformType, IEnumerable<ElementInfo>> platformToElementListMap = ParseKatalonObjectFilesInDirectory(directory);

                foreach (KeyValuePair<ePlatformType, IEnumerable<ElementInfo>> kv in platformToElementListMap)
                {
                    ePlatformType platform = kv.Key;
                    IEnumerable<ElementInfo>? elements = kv.Value;

                    if (elements == null || !elements.Any())
                    {
                        continue;
                    }

                    ApplicationPOMModel pom = new()
                    {
                        Name = GeneratePOMName(platform, directory),
                        MappedUIElements = new(elements),
                    };

                    results.Add(new(pom, platform));
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, $"Cannot create POM from Object Repository at directory path '{directory}'", ex);
            }

            IEnumerable<string> subDirectories = Directory.EnumerateDirectories(directory);
            foreach (string subDirectory in subDirectories)
            {
                Convert(subDirectory, results);
            }
        }

        private static Dictionary<ePlatformType, IEnumerable<ElementInfo>> ParseKatalonObjectFilesInDirectory(string directory)
        {
            IEnumerable<string> objectFilesInDirectory = GetKatalonObjectFilesInDirectory(directory);
            bool hasAnyObjectFile = objectFilesInDirectory.Any();

            if (!hasAnyObjectFile)
            {
                return [];
            }

            Dictionary<ePlatformType, List<KatalonElementEntity>> platformWiseElements = [];
            List<KatalonElementEntity> allElements = [];
            foreach (string objectFile in objectFilesInDirectory)
            {
                Reporter.ToLog(eLogLevel.INFO, $"Processing Rust Source file (.rs) at path '{objectFile}'.");

                if (!TryGetRootXmlElement(objectFile, out XmlElement? xmlElement) || xmlElement == null)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Unable to get root Katalon element from XML");
                    continue;
                }

                if (!TryParseKatalonObject(xmlElement, out KatalonElementEntity? element, out ePlatformType platform) || element == null)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Unable to parse Katalon element");
                    continue;
                }

                if (!platformWiseElements.TryGetValue(platform, out List<KatalonElementEntity>? platformElements) || platformElements == null)
                {
                    platformElements = [];
                    platformWiseElements[platform] = platformElements;
                }

                platformElements.Add(element);
                allElements.Add(element);
            }

            return new(platformWiseElements
                .Select(kv => new KeyValuePair<ePlatformType, IEnumerable<ElementInfo>>(
                    kv.Key,
                    KatalonElementToElementInfoConverter.Convert(kv.Value))));
        }

        internal static string GeneratePOMName(ePlatformType platform, string directory)
        {
            string name = $"{Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar))} - {platform}";
            string uniqueIdentifiier = string.Empty;
            int duplicateCount = 0;

            while (HasPOMWithName($"{name}{uniqueIdentifiier}"))
            {
                duplicateCount++;

                if (duplicateCount <= 1)
                {
                    uniqueIdentifiier = "_Copy";
                }
                else
                {
                    uniqueIdentifiier = $"_Copy{duplicateCount}";
                }
            }

            return $"{name}{uniqueIdentifiier}";
        }

        internal static bool HasPOMWithName(string name)
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetAllRepositoryItems<ApplicationPOMModel>().Any(pom => string.Equals(pom?.Name, name));
        }

        /// <summary>
        /// Get all the Rust Source files (.rs) in <paramref name="directory"/>.
        /// </summary>
        /// <returns>Collection of paths of all the Rust files (.rs).</returns>
        private static IEnumerable<string> GetKatalonObjectFilesInDirectory(string directory)
        {
            return Directory.EnumerateFiles(directory, searchPattern: "*.rs");
        }

        private static bool TryGetRootXmlElement(string file, out XmlElement? rootElement)
        {
            XmlDocument document = new();
            FileStream stream = File.OpenRead(file);
            try
            {
                document.Load(stream);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, $"Unable to parse WebElementEntity XML stream", ex);
                rootElement = null;
                return false;
            }

            foreach (XmlNode child in document.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element && child is XmlElement element)
                {
                    rootElement = element;
                    return true;
                }
            }

            rootElement = null;
            return false;
        }

        internal static bool TryParseKatalonObject(XmlElement xmlElement, out KatalonElementEntity? katalonElementEntity, out ePlatformType platform)
        {
            if (KatalonWebElementEntity.CanParse(xmlElement))
            {
                if (KatalonWebElementEntity.TryParse(xmlElement, out KatalonWebElementEntity? katalonWebElementEntity) && katalonWebElementEntity != null)
                {
                    katalonElementEntity = katalonWebElementEntity;
                    platform = ePlatformType.Web;
                    return true;
                }
            }

            katalonElementEntity = null;
            platform = ePlatformType.NA;
            return false;
        }
    }
}
