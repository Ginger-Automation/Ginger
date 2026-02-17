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
using Amdocs.Ginger.Common.UIElement;
using ImageMagick;
using Microsoft.Extensions.Logging;
using System;

#nullable enable
namespace Amdocs.Ginger.CoreNET.NewSelfHealing
{
    internal sealed class ElementImageMatcher
    {
        private readonly ILogger? _logger;

        internal ElementImageMatcher(ILogger? logger = null)
        {
            _logger = logger;
        }

        internal double Match(ElementInfo expected, ElementInfo actual)
        {
            if (expected == null)
            {
                throw new ArgumentNullException(paramName: nameof(expected));
            }
            if (actual == null)
            {
                throw new ArgumentNullException(paramName: nameof(actual));
            }

            _logger?.LogTrace("matching expected element({expectedElementName}-{expectedElementId}) image with actual element({actualElementName}-{actualElementId}) image", expected.ElementName, expected.Guid, actual.ElementName, actual.Guid);

            double matchScore;

            if (string.IsNullOrWhiteSpace(expected.ScreenShotImage))
            {
                matchScore = 0;
                _logger?.LogTrace("expected element({expectedElementName}-{expectedElementId}) does not have a screenshot image, match score {matchScore}", expected.ElementName, expected.Guid, matchScore);
                return matchScore;
            }

            if (string.IsNullOrWhiteSpace(actual.ScreenShotImage))
            {
                matchScore = 0;
                _logger?.LogTrace("actual element({actualElementName}-{actualElementId}) does not have a screenshot image, match score {matchScore}", actual.ElementName, actual.Guid, matchScore);
                return matchScore;
            }

            byte[] expectedBytes = Convert.FromBase64String(expected.ScreenShotImage);
            byte[] actualBytes = Convert.FromBase64String(actual.ScreenShotImage);

            using MagickImage expectedImg = new(expectedBytes);
            using MagickImage actualImg = new(actualBytes);

            double differPixelCount = expectedImg.Compare(actualImg, ErrorMetric.Absolute);
            _logger?.LogTrace("expected element({expectedElementName}-{expectedElementId}) image and actual element({actualElementName}-{actualElementId}) image differ by {differPixelCount} pixels", expected.ElementName, expected.Guid, actual.ElementName, actual.Guid, differPixelCount);

            double totalPixelCount = expectedImg.Width * expectedImg.Height;
            _logger?.LogTrace("expected element({expectedElementName}-{expectedElementId}) image contain {totalPixelCount} pixels", expected.ElementName, expected.Guid, totalPixelCount);

            matchScore = Math.Round(Math.Max(totalPixelCount - differPixelCount, 0) / totalPixelCount, 2);
            _logger?.LogTrace("expected element({expectedElementName}-{expectedElementId}) image and actual element({actualElementName}-{actualElementId}) image matched with score {matchScore}", expected.ElementName, expected.Guid, actual.ElementName, actual.Guid, matchScore);

            return matchScore;
        }

        /*
        Error Metrics
        Absolute: Measures the absolute difference between the pixels of the two images. This metric is useful when you want to see the total difference without considering the direction of the difference.

        MeanAbsolute: Calculates the mean of the absolute differences between the pixels. This provides an average difference per pixel, giving a sense of the overall discrepancy.

        MeanSquared: Computes the mean of the squared differences between the pixels. This metric emphasizes larger differences more than smaller ones, making it useful for highlighting significant discrepancies.

        PeakAbsolute: Finds the maximum absolute difference between the pixels. This metric is useful for identifying the largest single difference between the images.

        PeakSignalToNoiseRatio: Measures the ratio between the maximum possible pixel value and the mean squared error. This metric is commonly used in image compression and quality assessment, where higher values indicate better quality.

        RootMeanSquared: Calculates the square root of the mean squared differences between the pixels. This metric provides a balanced measure of the differences, combining aspects of both mean and peak differences.

        StructuralSimilarity: Evaluates the structural similarity between the images, considering luminance, contrast, and structure. This metric is designed to mimic human visual perception and is often used in image quality assessment.
         */
    }
}
