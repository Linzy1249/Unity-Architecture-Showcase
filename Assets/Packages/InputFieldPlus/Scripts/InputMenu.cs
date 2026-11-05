using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AR2VR.UI
{
    public class InputMenu : MonoBehaviour
    {
        public Button selectButton;
        public Button cutoutButton;
        public Button copyButton;
        public Button pasteButton;

        public Action OnDeselect;
        public Action OnSelectText;
        public Action OnCutOutText;
        public Action OnCopyText;
        public Action OnPasteText;

        public void Setup()
        {
            SetEventTrigger(selectButton.gameObject, EventTriggerType.Deselect, () => 
            {
                OnDeselect?.Invoke();
            });
            selectButton.onClick.AddListener(() => 
            {
                OnSelectText?.Invoke();
            });

            SetEventTrigger(cutoutButton.gameObject, EventTriggerType.Deselect, () => 
            {
                OnDeselect?.Invoke();
            });
            cutoutButton.onClick.AddListener(() => 
            {
                OnCutOutText?.Invoke();
            });

            SetEventTrigger(copyButton.gameObject, EventTriggerType.Deselect, () => 
            {
                OnDeselect?.Invoke();
            });
            copyButton.onClick.AddListener(() => 
            {
                OnCopyText?.Invoke();
            });

            SetEventTrigger(pasteButton.gameObject, EventTriggerType.Deselect, () => 
            {
                OnDeselect?.Invoke();
            });
            pasteButton.onClick.AddListener(() => 
            {
                OnPasteText?.Invoke();
            });
        }

        public bool IsSelectMenu(GameObject obj)
        {
            if (obj == selectButton.gameObject || obj == cutoutButton.gameObject || obj == copyButton.gameObject || obj == pasteButton.gameObject)
                return true;

            return false;
        }

        private void SetEventTrigger(GameObject obj, EventTriggerType triggerType, Action callback)
        {
            EventTrigger eventTrigger = obj.GetComponent<EventTrigger>();

            if (eventTrigger == null)
                eventTrigger = obj.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = triggerType
            };
            entry.callback.AddListener((BaseEventData) => { callback?.Invoke(); });
            eventTrigger.triggers.Add(entry);
        }
    }
}