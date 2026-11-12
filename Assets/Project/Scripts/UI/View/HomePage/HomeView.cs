using UnityEngine;

namespace UnityArchitectureShowcase.UI
{
    public class HomeView : BasicView
    {
        public HomeViewLayout viewLayout;

        public DynamicScrollRect scrollRect;
        public ScrollContent scrollContent;

        private bool isInit;

        public override void Dispose(bool isHide)
        {
            if (isHide)
                SetActive(false);
        }

        public override void Setup()
        {
            SetSafeArea();
            SetScreenRatio();
            SetActive(true);

            isInit = true;
        }

        protected override void SetSafeArea()
        {
            if (isInit)
                return;

            viewLayout.Setup(UIHelper.instance.GetSafeArea());
        }

        protected override void SetScreenRatio()
        {
            if (isInit)
                return;
        }

        private void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive); // 部分頁面可能會改成滑動效果
        }
    }
}