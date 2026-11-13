using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityArchitectureShowcase.Network;
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

        private InitialModel model = new InitialModel(); // 儲存、取得資料用

        public void OnDispose()
        {
            instance = null;

            model.Dispose();

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

            var splashView = ChangeToSplashView();
            var splashTask = Task.Delay(3000);

            if (!isInit)
            {
                SetSettings();

                await GetSystemConfig(); // 呼叫系統設定API

                isInit = true;
            }
            await splashTask; // SplashView預計最短等待三秒，如果GetSystemConfig等待超過三秒的話，這段就會立刻跳過

            if (splashView != null)
            {
                await splashView.SetSplashEffect(false);
            }
            ChangeToHomePage();
        }

        private void SetSettings()
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
            var screenRatio = UIHelper.instance.GetScreenRatio();

            UIHelper.instance.SetScreenRatio(screenRatio);
        }

        // ===== 切換頁面相關 =====
        private void ChangeToHomePage()
        {
            StateMachine.instance.ChangeState(nameof(State.HomeState));
        }

        private SplashView ChangeToSplashView()
        {
            var view = ChangeToView<SplashView>(ViewType.SplashView);

            if (view != null)
            {
                view.SetSplashEffect(true); // 不需要await
            }
            return view;
        }

        private void ChangeToLastView()
        {
            if (viewDatas.Count <= 0)
                return;

            var index = viewDatas.Count - 1;
            var viewData = viewDatas[index];

            try
            {
                isSaveViewData = false;

                switch (viewData.viewType)
                {
                    case ViewType.None:
                        ChangeToView<BasicView>(ViewType.None);
                        break;
                }
            }
            finally
            {
                if (viewDatas.Count > index)
                {
                    viewDatas.RemoveAt(index);
                }
                isSaveViewData = true;
            }
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
            try
            {
                // 不需要顯示Loading，而是直接顯示SplashView

                await model.GetSystemConfig();
            }
            catch (HttpRequestException)
            {
                // todo 網路異常
            }
            catch (GetResponseException)
            {
                // todo 伺服器異常
            }
            catch (Exception)
            {
                // todo 未知異常
            }
            finally
            {
                // 因應不顯示Loading，因此也不需要隱藏
            }
        }
    }
}