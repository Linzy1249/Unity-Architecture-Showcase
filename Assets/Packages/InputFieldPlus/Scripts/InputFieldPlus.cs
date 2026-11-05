using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AR2VR.UI
{
    public class InputFieldPlus : InputField
    {
        private struct SelectData
        {
            public string text;
            public int positionCache;
            public int selectPositionCache;
        }

        public InputMenu inputMenu;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS // 桌機版不執行輸入選單功能
        private bool isInit;
        private bool isUpdateSelectPosition;

        private float clickTime;
        private float pressTime;

        private int currentCaretPositionInternal;
        private int currentCaretSelectPositionInternal;

        private BaseEventData baseEventData;

        protected override void Start()
        {
#if UNITY_ANDROID && UNITY_2020_1_OR_NEWER
            TouchScreenKeyboard.Android.consumesOutsideTouches = false; // 必須加上這段才能在開啟原生鍵盤的情況下控制輸入框
#endif
            base.Start();
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            pressTime = 0;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            SelectData selectData = GetSelectedData();

            SetInputMenuActive(selectData.positionCache != selectData.selectPositionCache);

            base.OnDrag(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            baseEventData = eventData;
            pressTime = Time.unscaledTime;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (IsDoubleClick())
            {
                SetInputMenuActive(true);
                inputMenu.selectButton.onClick.Invoke();
            }
            else
            {
                if (IsLongPress())
                {
                    SetInputMenuActive(true);
                }
                else
                {
                    var selectData = GetSelectedData();

                    if (selectData.positionCache == selectData.selectPositionCache)
                    {
                        SetInputMenuActive(false);
                    }
                }
            }
            pressTime = 0;
        }

        public override async void OnDeselect(BaseEventData eventData)
        {
            await WaitAFrame();

            GameObject obj = EventSystem.current.currentSelectedGameObject;

            if (obj == gameObject || (inputMenu != null && inputMenu.IsSelectMenu(obj)))
                return;

            SetInputMenuActive(false);
            base.OnDeselect(eventData);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (isUpdateSelectPosition)
            {
                isUpdateSelectPosition = false;
                caretPosition = currentCaretPositionInternal;
                selectionAnchorPosition = currentCaretPositionInternal;
                selectionFocusPosition = currentCaretSelectPositionInternal;

                if (m_Keyboard != null)
                {
                    m_Keyboard.selection = new RangeInt(selectionAnchorPosition, selectionFocusPosition - selectionAnchorPosition);
                }
                ForceLabelUpdate();
            }
            currentCaretPositionInternal = caretPositionInternal;
            currentCaretSelectPositionInternal = caretSelectPositionInternal;
        }

        private void SetInputMenuActive(bool isActive)
        {
            if (inputMenu == null)
                inputMenu = Instantiate(Resources.Load<InputMenu>("Prefabs/InputMenu"), transform, false);

            if (!isInit)
            {
                inputMenu.OnDeselect = () =>
                {
                    OnDeselect(baseEventData);
                };
                inputMenu.OnSelectText = () =>
                {
                    currentCaretPositionInternal = 0;
                    currentCaretSelectPositionInternal = text.Length;
                    isUpdateSelectPosition = true;
                };
                inputMenu.OnCutOutText = () =>
                {
                    var selectedData = GetSelectedData();

                    if (!string.IsNullOrEmpty(selectedData.text))
                    {
                        GUIUtility.systemCopyBuffer = selectedData.text;

                        text = text.Remove(selectedData.positionCache, selectedData.selectPositionCache - selectedData.positionCache);
                        currentCaretSelectPositionInternal = currentCaretPositionInternal;
                    }
                    isUpdateSelectPosition = true;
                    inputMenu.gameObject.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(gameObject);
                };
                inputMenu.OnCopyText = () =>
                {
                    var selectedData = GetSelectedData();

                    if (!string.IsNullOrEmpty(selectedData.text))
                    {
                        GUIUtility.systemCopyBuffer = selectedData.text;
                    }
                    isUpdateSelectPosition = true;
                    inputMenu.gameObject.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(gameObject);
                };
                inputMenu.OnPasteText = () =>
                {
                    var selectedData = GetSelectedData();

                    if (!string.IsNullOrEmpty(selectedData.text))
                    {
                        text = text.Remove(selectedData.positionCache, selectedData.selectPositionCache - selectedData.positionCache);
                    }

                    string copyText = GUIUtility.systemCopyBuffer;

                    if (!string.IsNullOrEmpty(copyText))
                    {
                        text = text.Insert(selectedData.positionCache, copyText);
                        currentCaretPositionInternal = selectedData.positionCache + copyText.Length;
                        currentCaretSelectPositionInternal = currentCaretPositionInternal;
                    }
                    isUpdateSelectPosition = true;
                    inputMenu.gameObject.SetActive(false);
                    EventSystem.current.SetSelectedGameObject(gameObject);
                };
                inputMenu.Setup();
            }
            isInit = true;
            inputMenu.gameObject.SetActive(isActive);
        }

        private SelectData GetSelectedData()
        {
            var selectData = new SelectData();

            if (currentCaretPositionInternal > currentCaretSelectPositionInternal)
            {
                selectData.positionCache = currentCaretSelectPositionInternal;
                selectData.selectPositionCache = currentCaretPositionInternal;
            }
            else
            {
                selectData.positionCache = currentCaretPositionInternal;
                selectData.selectPositionCache = caretSelectPositionInternal;
            }

            if (currentCaretPositionInternal != caretSelectPositionInternal)
                selectData.text = text.Substring(selectData.positionCache, selectData.selectPositionCache - selectData.positionCache);
            else
                selectData.text = "";

            return selectData;
        }

        private bool IsDoubleClick()
        {
            var isDoubleClick = Time.unscaledTime - clickTime < 0.3f;

            clickTime = isDoubleClick ? 0 : Time.unscaledTime;

            return isDoubleClick;
        }

        private bool IsLongPress()
        {
            return pressTime != 0 && Time.unscaledTime - pressTime > 0.5f;
        }

        private async Task WaitAFrame()
        {
            var completionSource = new TaskCompletionSource<bool>();

            StartCoroutine(WaitForEndOfFrame(() => 
            {
                completionSource.SetResult(true);
            }));
            await completionSource.Task;
        }

        IEnumerator WaitForEndOfFrame(Action callback)
        {
            yield return new WaitForEndOfFrame();

            callback?.Invoke();
        }
#endif
    }
}