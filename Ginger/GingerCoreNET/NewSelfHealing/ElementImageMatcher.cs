using ImageDiff;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.NewSelfHealing
{
    internal sealed class ElementImageMatcher
    {
        [SupportedOSPlatform("windows")]
        internal double Match(string source, string target)
        {
            byte[] sourceBytes = Convert.FromBase64String(source);
            byte[] targetBytes = Convert.FromBase64String(target);

            MagickImage sourceImg = new(sourceBytes);
            MagickImage targetImg = new(targetBytes);

            return sourceImg.Compare(targetImg, ErrorMetric.MeanSquared);
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
