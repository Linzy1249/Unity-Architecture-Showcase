using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityArchitectureShowcase.UI
{
    internal static class MultipleDisplayUtilities
    {
        public static bool GetRelativeMousePositionForDrag(PointerEventData eventData, ref Vector2 position)
        {
            position = eventData.position;
            return true;
        }

        public static Vector2 GetMousePositionRelativeToMainDisplayResolution()
        {
            return Input.mousePosition;
        }
    }
}