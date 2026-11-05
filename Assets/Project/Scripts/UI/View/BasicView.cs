using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityArchitectureShowcase.UI
{
    public abstract class BasicView : MonoBehaviour
    {
        public virtual bool IsActive()
        {
            return gameObject.activeSelf;
        }

        public abstract void Dispose(bool isHide);
        public abstract void Setup();

        protected abstract void SetSafeArea();
        protected abstract void SetScreenRatio();
    }
}