using UnityEngine;

namespace UnityArchitectureShowcase.UI
{
    public abstract class BasicLayout : MonoBehaviour
    {
        public abstract void Setup(RectOffset safeArea);
    }
}