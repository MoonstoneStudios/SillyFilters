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
        /// <summary>
        /// Override what is returned by the launcher.
        /// </summary>
        /// <param name="__result">The image that the probe produces.</param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProbeCamera), nameof(ProbeCamera.TakeSnapshot))]
        public static void ProbeCamera_TakeSnapshot_Postfix(ref RenderTexture __result)
        {
            // don't change anything
            if (SillyFilters.instance.imageFileName == "none.png") return;

            // image loading credit
            // https://github.com/Pau318/OuterPictures
            var overlay = SillyFilters.instance.ModHelper.Assets.GetTexture("Assets\\" + SillyFilters.instance.imageFileName);
            var result2d = __result.ToTexture2D();

            // use alpha blending to merge images
            var basePixels = result2d.GetPixels();
            var overlayPixels = overlay.GetPixels();
            var newImage = new Texture2D(512, 512, TextureFormat.RGBA32, 1, false);

            // 512 image size.
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    // https://softwareengineering.stackexchange.com/a/212813
                    // convert the 2d coords to the index
                    var i = x + 512 * y;

                    // i accidentally swapped these two in the formula
                    // so here is my fix lol.
                    var basePixel = overlayPixels[i];
                    var overlayPixel = basePixels[i];

                    // https://en.wikipedia.org/wiki/Alpha_compositing
                    var newAlpha = basePixel.a + (overlayPixel.a * (1f - basePixel.a));
                    var newR = ((basePixel.r * basePixel.a) + (overlayPixel.r * overlayPixel.a * (1f - basePixel.a))) / newAlpha;
                    var newG = ((basePixel.g * basePixel.a) + (overlayPixel.g * overlayPixel.a * (1f - basePixel.a))) / newAlpha;
                    var newB = ((basePixel.b * basePixel.a) + (overlayPixel.b * overlayPixel.a * (1f - basePixel.a))) / newAlpha;

                    newImage.SetPixel(x, y, new Color(newR, newG, newB, newAlpha));
                }
            }

            // apply our changes and send them to the GPU
            newImage.Apply();

            // trun the texture2d into a render texture and return.
            Graphics.CopyTexture(newImage, __result);
        }
    }
}
