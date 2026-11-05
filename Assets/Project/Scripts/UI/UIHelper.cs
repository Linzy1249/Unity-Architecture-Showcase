using UnityEngine;
using UnityEngine.UI;

namespace UnityArchitectureShowcase.UI
{
    public enum ScreenRatio
    {
        Unknown,
        Phone,
        Tablet
    }

    public class UIHelper : MonoBehaviour
    {
        public static UIHelper instance { get; private set; } = null;

        public CanvasScaler canvasScaler;

        private RectOffset safeArea = null;
        private ScreenRatio screenRatio = ScreenRatio.Unknown;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        public void SetScreenRatio(ScreenRatio screenRatio)
        {
            canvasScaler.referenceResolution = GetScreenRatioSize(screenRatio);
            canvasScaler.matchWidthOrHeight = 0;
        }

        public ScreenRatio GetScreenRatio()
        {
            if (screenRatio == ScreenRatio.Unknown)
            {
                Vector2 size = new Vector2(Screen.width, Screen.height);

                float ratio = size.x > size.y ? size.y / size.x : size.x / size.y;

                if (ratio <= 0.5625f)
                    screenRatio = ScreenRatio.Phone;
                else
                    screenRatio = ScreenRatio.Tablet;
            }
            return screenRatio;
        }

        public Vector2 GetScreenRatioSize(ScreenRatio screenRatio)
        {
            switch (screenRatio)
            {
                case ScreenRatio.Phone:
                    return new Vector2(375f, 734f);
                case ScreenRatio.Tablet:
                    return new Vector2(627, 835);
            }
            return new Vector2(375f, 734f);
        }

        public Vector2 GetCanvasSize()
        {
            float match = canvasScaler.matchWidthOrHeight;

            float offect = (Screen.width / canvasScaler.referenceResolution.x) * (1 - match) + (Screen.height / canvasScaler.referenceResolution.y) * match;

            return new Vector2(Screen.width, Screen.height) / offect;
        }

        public float GetCanvasScale()
        {
            Vector2 resolution = canvasScaler.referenceResolution;

            float widthScale = Screen.width / resolution.x;
            float heightScale = Screen.height / resolution.y;

            switch (canvasScaler.screenMatchMode)
            {
                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                    return Mathf.Lerp(widthScale, heightScale, canvasScaler.matchWidthOrHeight);
            }
            return canvasScaler.scaleFactor; // 其他模式由Unity自行計算
        }

        public RectOffset GetSafeArea()
        {
            if (safeArea == null)
            {
                float scale = canvasScaler.matchWidthOrHeight == 0 ? GetCanvasSize().x / (float)Screen.width : GetCanvasSize().y / (float)Screen.height;

                Rect rect = Screen.safeArea;

                safeArea = new RectOffset
                {
                    top = Mathf.RoundToInt((Screen.height - (rect.height + rect.y)) * scale),
                    bottom = Mathf.RoundToInt(rect.y * scale),
                    left = Mathf.RoundToInt(rect.x * scale),
                    right = Mathf.RoundToInt((Screen.width - (rect.width + rect.x)) * scale)
                };
            }
            return safeArea;
        }
    }
}