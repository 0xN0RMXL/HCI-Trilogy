using System.Collections;
using UnityEngine;

namespace HCITrilogy.Containment.Core
{
    /// <summary>
    /// VR scene transitions. Inherits from the shared core SceneFlow.
    /// VR fade-to-black is handled by the XR Origin's tunneling/vignette
    /// layer when present; here we simply yield a fixed delay.
    /// </summary>
    public class SceneFlow : HCITrilogy.Core.SceneFlow
    {
        protected override IEnumerator FadeOut()
        {
            // VR fade handled by XR vignette; just wait for the load.
            yield break;
        }

        protected override IEnumerator FadeIn()
        {
            yield return new WaitForSeconds(fadeSeconds);
        }
    }
}
