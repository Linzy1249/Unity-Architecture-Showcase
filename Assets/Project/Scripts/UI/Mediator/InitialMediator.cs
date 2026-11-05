using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityArchitectureShowcase.UI
{
    public class InitialMediator : IMediator
    {
        private static InitialMediator instance = null;

        private enum ViewType
        {
            None = -1,
            SplashView
        }

        private class ViewData
        {
            public ViewType viewType = ViewType.None;

            public void Clear()
            {
                viewType = ViewType.None;
            }
        }

        private bool isInit = false;
        private bool isSaveViewData = true;  // 判斷要不要儲存上一個介面的資訊，目前在返回時會設為false避免重複記錄
        private bool isStaticMethod = false; // 判斷是不是從Static呼叫的
        private ViewType currentView = ViewType.None;
        private List<ViewData> viewDatas = new List<ViewData>();
        private Dictionary<ViewType, BasicView> viewDictionary = new Dictionary<ViewType, BasicView>();

        public void OnDispose()
        {
            instance = null;

            for (int i = 0; i < viewDictionary.Count; i++)
            {
                var view = viewDictionary.ElementAt(i).Value;

                if (view == null)
                    continue;

                view.Dispose(false);
                GameObject.Destroy(view.gameObject);
            }
            viewDatas.Clear();
            viewDictionary.Clear();

            isSaveViewData = true;
            isStaticMethod = false;
            currentView = ViewType.None;
        }

        public async void OnSetup()
        {
            instance = this;

            ChangeToSplashView();

            if (!isInit)
            {
                SetupSettings();

                await GetSystemConfig();
            }
            ChangeToHomePage();

            isInit = true;
        }

        private void SetupSettings()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:
                    Debug.unityLogger.logEnabled = true;
                    break;
                default:
                    Debug.unityLogger.logEnabled = false;
                    break;
            }
        }

        // ===== 切換頁面相關 =====
        private void ChangeToHomePage()
        {
            StateMachine.instance.ChangeState(nameof(State.HomeState));
        }

        private SplashView ChangeToSplashView()
        {
            return ChangeToView<SplashView>(ViewType.SplashView);
        }

        private T ChangeToView<T>(ViewType nextView, int sort = -1, bool isHide = true) where T : BasicView
        {
            if (!isStaticMethod && currentView == nextView) // 從靜態呼叫的頁面可以忽略相同介面
                return null;

            if (currentView != ViewType.None)
            {
                if (viewDictionary.TryGetValue(currentView, out BasicView lastView) && lastView != null)
                {
                    lastView.Dispose(isHide);
                }
            }
            if (isSaveViewData && currentView != nextView)
            {
                viewDatas.Add(new ViewData
                {
                    viewType = currentView
                });
            }
            currentView = nextView;

            var isContainsKey = viewDictionary.TryGetValue(nextView, out BasicView basicView);

            if (!isContainsKey || basicView == null)
            {
                if (sort < 0)
                    basicView = UICreator.instance.Create(nextView.ToString());
                else
                    basicView = UICreator.instance.Create(nextView.ToString(), (int)nextView);

                if (basicView == null)
                    return null;

                basicView.Setup();

                if (isContainsKey)
                    viewDictionary[nextView] = basicView;
                else
                    viewDictionary.Add(nextView, basicView);
            }
            else
                basicView.Setup();

            return basicView as T;
        }

        // ===== 取得資料相關 =====
        private async Task GetSystemConfig()
        {
            await Task.Delay(1000); // 1s
        }
    }
}