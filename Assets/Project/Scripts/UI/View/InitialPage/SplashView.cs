namespace UnityArchitectureShowcase.UI
{
    public class SplashView : BasicView
    {
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
            // 初始頁面不執行
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