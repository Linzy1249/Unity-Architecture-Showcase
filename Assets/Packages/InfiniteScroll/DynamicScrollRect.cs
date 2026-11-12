using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityArchitectureShowcase.UI
{
    [Serializable]    
    public class DynamicScrollRestrictionSettings
    {
        [SerializeField] private float _contentOverflowRange = 125f;
        public float ContentOverflowRange => _contentOverflowRange;

        [SerializeField] [Range(0, 1)] private float _contentDecelerationInOverflow = 0.5f;
        public float ContentDecelerationInOverflow => _contentDecelerationInOverflow;
    }
    
    [Serializable] 
    public class FocusSettings
    {
        [SerializeField] private float _focusOffset = 0;
        public float FocusOffset =>  _focusOffset;

        [SerializeField] private float _focusDuration = 0.25f;
        public float FocusDuration => _focusDuration;
    }

    // TODO :: Hidden Rules Between TryRestrictionMovement - GetContentPosition etc.
    public class DynamicScrollRect : ScrollRect
    {
        public Action OnRefresh;

        [SerializeField] private DynamicScrollRestrictionSettings _restrictionSettings = null;
        [SerializeField] private FocusSettings _focusSettings = null;
        
        private bool _isDragging = false;
        private bool _isFocusActive = false;
        private bool _needRunBack = false;
        private bool _runningBack = false;

        private Vector2 _contentStartPos = Vector2.zero;
        private Vector2 _dragStartingPosition = Vector2.zero;
        private Vector2 _dragCurPosition = Vector2.zero;
        private Vector2 _lastDragDelta = Vector2.zero;
        private Vector2 _scrollDelta = Vector2.zero;

        private IEnumerator _focusRoutine = null;
        private IEnumerator _runBackRoutine = null;
        private PointerEventData _pointerEventData = null;

        private ScrollContent _content;
        private ScrollContent _Content
        {
            get
            {
                if (_content == null)
                {
                    _content = GetComponentInChildren<ScrollContent>();
                }
                return _content;
            }
        }

        protected override void OnDestroy()
        {
            onValueChanged.RemoveListener(OnScrollRectValueChanged);
        
            base.OnDestroy();
        }

        protected override void Awake()
        {
            movementType = MovementType.Unrestricted;

            onValueChanged.AddListener(OnScrollRectValueChanged);
            
            vertical = !horizontal;
        
            base.Awake();
        }

        public override void OnScroll(PointerEventData data)
        {
            _pointerEventData = data;

            StopMovement();

            base.OnScroll(data);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            StopRunBackRoutine();
        
            _isDragging = true;

            _contentStartPos = content.anchoredPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport,
                eventData.position,
                eventData.pressEventCamera,
                out _dragStartingPosition);

            _dragCurPosition = _dragStartingPosition;
            
            CancelFocus();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!IsActive())
            {
                return;
            }
        
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out Vector2 localCursor))
            {
                return;
            }
            StopRunBackRoutine();
        
            if (!IsDragValid(localCursor - _dragCurPosition))
            {
                Vector2 restrictedPos = GetRestrictedContentPositionOnDrag(eventData);
            
                _needRunBack = true;

                SetContentAnchoredPosition(restrictedPos);

                return;
            }

            UpdateBounds();
        
            _needRunBack = false;

            _lastDragDelta = localCursor - _dragCurPosition;

            _dragCurPosition = localCursor;
        
            SetContentAnchoredPosition(CalculateContentPos(localCursor));

            UpdateItems(_lastDragDelta);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
        
            _isDragging = false;

            if (!_needRunBack)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out Vector2 localCursor))
            {
                return;
            }

            Vector2 delta = localCursor - _dragCurPosition;

            if (GetVerticalRestrictionWeight(delta) >= 1 && normalizedPosition.y >= 1)
            {
                OnRefresh?.Invoke();
            }
            StopMovement();
            StartRunBackRoutine();
        }

        public void ResetContent()
        {
            StopMovement();

            StopRunBackRoutine();

            content.anchoredPosition = Vector2.zero;
        }

        public void StartFocus(ScrollItem focusItem)
        {
            StartFocusItemRoutine(focusItem);
        }

        public void CancelFocus()
        {
            StopFocusItemRoutine();
        }

        public void CheckRunBackRoutine()
        {
            var isNeedRunBack = !IsDragValid(Vector2.one);

            if (isNeedRunBack)
                StartRunBackRoutine();
        }

        private void OnScrollRectValueChanged(Vector2 value)
        {
            if (_runningBack || _isDragging || _isFocusActive)
            {
                return;
            }

            var delta = velocity.normalized;

            if (delta == Vector2.zero && _pointerEventData != null)
            {
                delta = -_pointerEventData.scrollDelta;
            }

            if (!IsDragValid(delta))
            {
                Vector2 contentPos = GetRestrictedContentPositionOnScroll(delta);

                SetContentAnchoredPosition(contentPos);

                if ((velocity * Time.deltaTime).magnitude < 5)
                {
                    StopMovement();
                    StartRunBackRoutine();
                }
                return;
            }
            UpdateItems(delta);
        }
        
        private void UpdateItems(Vector2 delta)
        {
            if (vertical)
            {
                var isDelta = delta.y > 0;

                if (isDelta && -_Content.GetLastItemPos().y - content.anchoredPosition.y <= viewport.rect.height + _Content.Spacing.y)
                {
                    _Content.AddIntoTail();
                }

                if (isDelta && content.anchoredPosition.y - -_Content.GetFirstItemPos().y >= (2 * _Content.ItemHeight) + _Content.Spacing.y)
                {
                    _Content.DeleteFromHead();
                }

                if (!isDelta && content.anchoredPosition.y + _Content.GetFirstItemPos().y <= _Content.ItemHeight + _Content.Spacing.y)
                {
                    _Content.AddIntoHead();
                }

                if (!isDelta && -_Content.GetLastItemPos().y - content.anchoredPosition.y >= viewport.rect.height + _Content.ItemHeight + _Content.Spacing.y)
                {
                    _Content.DeleteFromTail();
                }
            }

            if (horizontal)
            {
                var isDelta = delta.x > 0;

                if (isDelta && _Content.GetFirstItemPos().x + content.anchoredPosition.x >= -_Content.ItemWidth - _Content.Spacing.x)
                {
                    _Content.AddIntoHead();
                }

                if (isDelta && _Content.GetLastItemPos().x + content.anchoredPosition.x >= viewport.rect.width + _Content.ItemWidth + _Content.Spacing.x)
                {
                    _Content.DeleteFromTail();
                }

                if (!isDelta && _Content.GetLastItemPos().x + content.anchoredPosition.x <= viewport.rect.width + _Content.Spacing.x)
                {
                    _Content.AddIntoTail();
                }

                if (!isDelta && _Content.GetFirstItemPos().x + content.anchoredPosition.x <= (-2 * _Content.ItemWidth) - _Content.Spacing.x)
                {
                    _Content.DeleteFromHead();
                }
            }
        }

        private bool IsDragValid(Vector2 delta)
        {
            if (vertical)
            {
                return CheckDragValidVertical(delta);
            }

            if (horizontal)
            {
                return CheckDragValidHorizontal(delta);
            }
            return false;
        }

        private bool CheckDragValidVertical(Vector2 delta)
        {
            var isDelta = delta.y > 0;

            if (isDelta)
            {
                Vector2 lastItemPos = _Content.GetLastItemPos();

                // Calculate local position of last item's end position in viewport rect
                if (!_Content.CanAddNewItemIntoTail() &&  content.anchoredPosition.y + viewport.rect.height + lastItemPos.y - _Content.ItemHeight > 0)
                {
                    return false;
                }
            }
            else
            {
                if (!_Content.CanAddNewItemIntoHead() && content.anchoredPosition.y <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckDragValidHorizontal(Vector2 delta)
        {
            var isDelta = delta.x > 0;

            if (isDelta)
            {
                if (!_Content.CanAddNewItemIntoHead() && content.anchoredPosition.x >= 0)
                {
                    return false;
                }
            }
            else
            {
                Vector2 lastItemPos = _Content.GetLastItemPos();

                // Calculate local position of last item's end position in viewport rect 
                if (!_Content.CanAddNewItemIntoTail() && content.anchoredPosition.x + lastItemPos.x <= viewport.rect.width - _Content.ItemWidth)
                {
                    return false;
                }
            }
            return true;
        }

        private Vector2 GetRestrictedContentPositionOnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out Vector2 localCursor);

            var delta = localCursor - _dragCurPosition;

            var position = CalculateContentPos(localCursor);

            if (vertical)
            {
                var restriction = GetVerticalRestrictionWeight(delta);

                var result = CalculateRestrictedPosition(content.anchoredPosition, position, restriction);
            
                result.x = content.anchoredPosition.x;
            
                return result;
            }

            if (horizontal)
            {
                var restriction = GetHorizontalRestrictionWeight(delta);
                
                var result = CalculateRestrictedPosition(content.anchoredPosition, position, restriction);

                result.y = content.anchoredPosition.y;

                return result;
            }
            return Vector2.zero;
        }

        private Vector2 GetRestrictedContentPositionOnScroll(Vector2 delta)
        {
            var restriction = 0f;

            if (vertical)
                restriction = GetVerticalRestrictionWeight(delta);
            else
                restriction = GetHorizontalRestrictionWeight(delta);

            var res = Vector2.zero;
            var deltaPos = velocity * Time.deltaTime;

            if (vertical)
            {
                deltaPos.x = 0;

                var curPos = content.anchoredPosition;
                var nextPos = curPos + deltaPos;
            
                res = CalculateRestrictedPosition(curPos, nextPos, restriction);
                res.x = 0;
            }

            if (horizontal)
            {
                deltaPos.y = 0;

                var curPos = content.anchoredPosition;
                var nextPos = curPos + deltaPos;

                res = CalculateRestrictedPosition(curPos, nextPos, restriction);
                res.y = 0;
            }
            velocity *= _restrictionSettings.ContentDecelerationInOverflow;

            return res;
        }

        private float GetVerticalRestrictionWeight(Vector2 delta)
        {
            var isDelta = delta.y > 0;

            var maxLimit = _restrictionSettings.ContentOverflowRange;

            if (isDelta)
            {
                Vector2 lastItemPos = _Content.GetLastItemPos();

                if (Mathf.Abs(lastItemPos.y) <= viewport.rect.height - _Content.ItemHeight)
                {
                    var max = lastItemPos.y + maxLimit;
                    var cur = content.anchoredPosition.y + lastItemPos.y;
                    var diff = max - cur;

                    return 1f - Mathf.Clamp(diff / maxLimit, 0, 1);
                }
                else
                {
                    var max = -(viewport.rect.height - maxLimit - _Content.ItemHeight);
                    var cur = content.anchoredPosition.y + lastItemPos.y;
                    var diff = max - cur;

                    return 1f - Mathf.Clamp(diff / maxLimit, 0, 1);
                }
            }
            return Mathf.Clamp(Mathf.Abs(content.anchoredPosition.y) / maxLimit, 0, 1);
        }

        private float GetHorizontalRestrictionWeight(Vector2 delta)
        {
            var isDelta = delta.x > 0;

            var maxLimit = _restrictionSettings.ContentOverflowRange;

            if (!isDelta)
            {
                var lastItemPos = _Content.GetLastItemPos();
                
                if (lastItemPos.x <= viewport.rect.width - _Content.ItemWidth)
                {
                    var max = lastItemPos.x - maxLimit;
                    var cur = content.anchoredPosition.x + lastItemPos.x;
                    var diff = cur - max;
                    
                    return 1f - Mathf.Clamp(diff / maxLimit, 0, 1);
                }
                else
                {
                    var max = viewport.rect.width - maxLimit - _Content.ItemWidth;
                    var cur = content.anchoredPosition.x + lastItemPos.x;
                    var diff = cur - max;

                    return 1 - Mathf.Clamp(diff / maxLimit, 0, 1);
                }
            }
            return Mathf.Clamp(Mathf.Abs(content.anchoredPosition.x) / maxLimit, 0, 1);
        }

        private Vector2 CalculateSnapPosition()
        {
            if (vertical)
            {
                if (content.anchoredPosition.y <= 0)
                {
                    return Vector2.zero;
                }
                else
                {
                    var lastItemPos = _Content.GetLastItemPos();

                    if (Mathf.Abs(lastItemPos.y) <= viewport.rect.height - _Content.ItemHeight)
                    {                    
                        return Vector2.zero;
                    }
                    var target = -(viewport.rect.height - _Content.ItemHeight);
                    var cur = content.anchoredPosition.y + lastItemPos.y;
                    var diff = target - cur;

                    return content.anchoredPosition + new Vector2(0, diff);
                }
            }

            if (horizontal)
            {
                if (content.anchoredPosition.x >= 0)
                {
                    return Vector2.zero;
                }
                var lastItemPos = _Content.GetLastItemPos();

                if (lastItemPos.x <= viewport.rect.width - _Content.ItemWidth)
                {
                    return Vector2.zero;
                }
                var target = viewport.rect.width - _Content.ItemWidth;
                var cur = content.anchoredPosition.x + lastItemPos.x;
                var diff = target - cur; 

                return content.anchoredPosition + new Vector2(diff, 0);
            }
            return Vector2.zero;
        }

        private Vector2 CalculateContentPos(Vector2 localCursor)
        {
            var dragDelta = localCursor - _dragStartingPosition;

            var position = _contentStartPos + dragDelta;

            return position;
        }

        private Vector2 CalculateRestrictedPosition(Vector2 curPos, Vector2 nextPos, float restrictionWeight)
        {
            var weightedPrev = curPos * restrictionWeight;

            var weightedNext = nextPos * (1 - restrictionWeight);

            var result = weightedPrev + weightedNext;

            return result;
        }

        // TODO : Consider Renaming
        #region Run Back Routine

        private void StartRunBackRoutine()
        {
            StopRunBackRoutine();

            _runBackRoutine = RunBackProgress();

            StartCoroutine(_runBackRoutine);
        }

        private void StopRunBackRoutine()
        {
            if (_runBackRoutine != null)
            {
                StopCoroutine(_runBackRoutine);

                _runningBack = false;
            }
        }

        private IEnumerator RunBackProgress()
        {
            _runningBack = true;

            var timePassed = 0f;

            var duration = 0.25f;

            Vector2 startPos = content.anchoredPosition;

            Vector2 endPos = CalculateSnapPosition();

            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;

                var pos = Vector2.Lerp(startPos, endPos, timePassed / duration);

                SetContentAnchoredPosition(pos);
            
                yield return null;
            }
            SetContentAnchoredPosition(endPos);
        
            _runningBack = false;
        }

        #endregion
        
        #region Focus
        
        private Vector2 GetFocusPosition(ScrollItem focusItem)
        {
            Vector2 contentPos = content.anchoredPosition;
            
            if (vertical)
            {
                // focus item above the viewport
                if (contentPos.y + focusItem.RectTransform.anchoredPosition.y > 0)
                {
                    var diff = contentPos.y + focusItem.RectTransform.anchoredPosition.y + _focusSettings.FocusOffset;

                    var focusPos = contentPos - new Vector2(0, diff);

                    focusPos.y = Mathf.Max(focusPos.y, 0);

                    return focusPos;
                }

                // focus item under the viewport
                if (viewport.rect.height + (contentPos.y + focusItem.RectTransform.anchoredPosition.y - _Content.ItemHeight) < 0)
                {
                    var diff = -contentPos.y - viewport.rect.height + -focusItem.RectTransform.anchoredPosition.y + _Content.ItemHeight + _focusSettings.FocusOffset;

                    if (_Content.AtTheEndOfContent(focusItem))
                    {
                        return CalculateSnapPosition();
                    }
                    var focusPos = contentPos + new Vector2(0, diff);

                    var contentMovementLimit = contentPos.y + _Content.GetLastItemPos().y - _Content.ItemHeight + viewport.rect.height;

                    focusPos.y = Mathf.Max(focusPos.y, contentMovementLimit);

                    return focusPos;
                }
            }

            if (horizontal)
            {
                // focus item at the left of the viewport
                if (contentPos.x + focusItem.RectTransform.anchoredPosition.x < 0)
                {
                    var diff = contentPos.x + focusItem.RectTransform.anchoredPosition.x - _focusSettings.FocusOffset;

                    var focusPos = contentPos - new Vector2(0, diff);

                    focusPos.x = Mathf.Max(focusPos.x, 0);

                    return focusPos;
                }

                // focus item at the right of the viewport
                if (viewport.rect.width + (-contentPos.x - focusItem.RectTransform.anchoredPosition.x - _Content.ItemWidth) < 0)
                {
                    var diff = -viewport.rect.width + contentPos.x + focusItem.RectTransform.anchoredPosition.x + _Content.ItemWidth - _focusSettings.FocusOffset;

                    if (_Content.AtTheEndOfContent(focusItem))
                    {
                        return CalculateSnapPosition();
                    }
                    var focusPos = contentPos + new Vector2(0, diff);

                    var contentMoveLimit = -contentPos.x - _Content.GetLastItemPos().x + _Content.ItemWidth + -viewport.rect.width;

                    focusPos.x = Mathf.Max(focusPos.x, contentMoveLimit);

                    return focusPos;
                }
            }
            return content.anchoredPosition;
        }

        private void StartFocusItemRoutine(ScrollItem scrollItem)
        {
            StopFocusItemRoutine();

            _focusRoutine = FocusProgress(GetFocusPosition(scrollItem));

            StartCoroutine(_focusRoutine);
        }

        private void StopFocusItemRoutine()
        {
            if (_focusRoutine != null)
            {
                StopCoroutine(_focusRoutine);
            }
            _isFocusActive = false;
        }

        private IEnumerator FocusProgress(Vector2 focusPos)
        {
            _isFocusActive = true;

            var timePassed = 0f;

            var startPos = content.anchoredPosition;

            while (timePassed < _focusSettings.FocusDuration)
            {
                timePassed += Time.deltaTime;

                var pos = Vector2.Lerp(startPos, focusPos, timePassed / _focusSettings.FocusDuration);

                var delta = pos - content.anchoredPosition;

                UpdateItems(delta);
                SetContentAnchoredPosition(pos);

                yield return null;
            }
            SetContentAnchoredPosition(focusPos);

            _isFocusActive = false;
        }
        #endregion
    }
}