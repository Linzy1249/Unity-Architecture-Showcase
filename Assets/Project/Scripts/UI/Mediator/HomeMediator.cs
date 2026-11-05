using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityArchitectureShowcase.UI
{
    public class HomeMediator : IMediator
    {
        private static HomeMediator instance = null;

        private enum ViewType
        {
            None = -1,
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

        private HomeModel model = new HomeModel(); // 儲存、取得資料用

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

            if (!isInit)
            {
                // AnalyticsHelper.instance.Setup(); // AppData 中的部分欄位需要在 MainPage 的腳本中初始化，因此必須等待頁面切換完成才能呼叫

                // await ShowAgreement();    // 用戶協議
                // await ShowAnnouncement(); // 公告

                // ProcessDeepLinkMngr.instance.Setup();
            }
            // SetAccountUIStatus();

            isInit = true;
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

        private T ChangeToView<T>(ViewType nextView, Transform root = null, int sort = -1, bool isHide = true) where T : BasicView
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
            if (isSaveViewData && currentView != nextView) // 當前Studio沒有一個固定的首頁，因此類型為None時也會記錄
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

        private void HideView(ViewType viewType = ViewType.None)
        {
            if (viewType != ViewType.None && viewDatas.Count > 0)
            {
                for (int i = viewDatas.Count - 1; i >= 0; i--)
                {
                    var lastViewType = viewDatas[i].viewType;

                    ChangeToLastView();

                    if (viewType == lastViewType)
                        break;
                }
            }
            else
            {
                for (int i = 0; i < viewDictionary.Count; i++)
                {
                    var view = viewDictionary.ElementAt(i).Value;

                    if (view == null)
                        continue;

                    view.Dispose(true);
                }
                viewDatas.Clear();
            }
        }
    }
}