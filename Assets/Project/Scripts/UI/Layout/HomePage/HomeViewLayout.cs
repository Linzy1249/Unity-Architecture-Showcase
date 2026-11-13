using UnityEngine;
using UnityEngine.UI;

namespace UnityArchitectureShowcase.UI
{
    public class HomeViewLayout : BasicLayout
    {
        public LayoutElement topAreaElement;
        public HorizontalOrVerticalLayoutGroup mainGroup;

        private bool isInit;
        private float topAreaHeight = 0;

        public override void Setup(RectOffset safeArea)
        {
            if (!isInit)
            {
                isInit = true;
                topAreaHeight = topAreaElement.minHeight;
            }
            mainGroup.padding.bottom = safeArea.bottom;
            topAreaElement.minHeight = topAreaHeight + safeArea.top;
        }
    }
}