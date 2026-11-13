using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityArchitectureShowcase.UI
{
    public class SplashView : BasicView
    {
        public CanvasGroup logoGroup;

        private bool isInit;
        private string taskID = "";

        public override void Dispose(bool isHide)
        {
            taskID = "";

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

        public async Task SetSplashEffect(bool isActive)
        {
            var currentTaskID = Guid.NewGuid().ToString();

            taskID = currentTaskID;

            float time = 0;
            float startAlpha = isActive ? 0 : 1;
            float endAlpha = isActive ? 1 : 0;

            while (time < 1)
            {
                time += 0.05f; // 20次

                SetGroupAlpha(Mathf.Lerp(startAlpha, endAlpha, time));

                await Task.Delay(10); // 0.01s

                if (currentTaskID != taskID)
                    break;
            }
            SetGroupAlpha(endAlpha);

            await Task.Delay(500); // 0.5s

            void SetGroupAlpha(float alpha)
            {
                if (logoGroup != null)
                    logoGroup.alpha = alpha;
            }
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