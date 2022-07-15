using UnityEngine;
using UnityEngine.Assertions;

namespace KaimiraGames.GameJam
{
    public class BetterMonoBehaviour : MonoBehaviour
    {
        protected new void Destroy(UnityEngine.Object @object)
        {
            if (@object == null) return;
            if (@object.GetType() != typeof(GameObject))
            {
                Assert.IsTrue(false, "Inadvertently using Destroy() on a component instead of a GameObject.");
            }
            else
            {
                try { UnityEngine.Object.Destroy(@object); }
                catch { } // unlimited power
            }
        }
    }

}
