using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SillyFilters
{
    [HarmonyPatch]
    public class ProbeCameraPatch
    {
        private static SillyFilters ModInstance => SillyFilters.instance;

        private static Texture2D currentOverlayTexture;
        private static string currentFilterName;

        /// <summary>Override what is returned by the launcher.</summary>
        /// <param name="__result">The image that the probe produces.</param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProbeCamera), nameof(ProbeCamera.TakeSnapshot))]
        public static void ProbeCamera_TakeSnapshot_Postfix(ref RenderTexture __result)
        {
            var newFilterName = ModInstance.imageFileName;

            // don't change anything
            if (newFilterName == "none.png") return;

            if (newFilterName != currentFilterName)
            {
                // image loading credit
                // https://github.com/Pau318/OuterPictures
                currentOverlayTexture = ModInstance.ModHelper.Assets.GetTexture("Assets\\" + newFilterName);
                currentFilterName = newFilterName;
            }

            var result2d = __result.ToTexture2D();

            var basePixels = result2d.GetPixels();
            var overlayPixels = currentOverlayTexture.GetPixels();

            // add filter to the probe image.
            var newImage = CombineImages(basePixels, overlayPixels);

            // turn the texture2d into a render texture and return.
            Graphics.CopyTexture(newImage, __result);
        }


        /// <summary>Combine two images using alpha compositing.</summary>
        /// <param name="basePixels">The base image's pixels.</param>
        /// <param name="overlayPixels">The overlay image's pixels.</param>
        /// <param name="newImage">A reference to the resulting image.</param>
        private static Texture2D CombineImages(Color[] basePixels, Color[] overlayPixels)
        {
            var newImage = new Texture2D(512, 512, TextureFormat.RGBA32, 1, false);

            // 512 image size.
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    // instead of using Texture2D.GetPixel(x, y) we use Texture2D.GetPixels() (see above)
                    // to convert all pixels into Color classes in one go
                    // instead of creating new objects each loop.

                    // convert the 2d coords to the index
                    // https://softwareengineering.stackexchange.com/a/212813
                    var i = x + 512 * y;

                    var basePixel = basePixels[i];
                    var overlayPixel = overlayPixels[i];

                    // https://en.wikipedia.org/wiki/Alpha_compositing
                    var invAlpha = 1f - overlayPixel.a;
                    var newAlpha = overlayPixel.a + (basePixel.a * invAlpha);

                    var newR = ((overlayPixel.r * overlayPixel.a) + (basePixel.r * basePixel.a * invAlpha)) / newAlpha;
                    var newG = ((overlayPixel.g * overlayPixel.a) + (basePixel.g * basePixel.a * invAlpha)) / newAlpha;
                    var newB = ((overlayPixel.b * overlayPixel.a) + (basePixel.b * basePixel.a * invAlpha)) / newAlpha;

                    newImage.SetPixel(x, y, new Color(newR, newG, newB, newAlpha));
                }
            }

            // apply our changes and send them to the GPU
            newImage.Apply();
            return newImage;
        }
    }
}
