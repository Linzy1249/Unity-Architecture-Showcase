using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityArchitectureShowcase.UI
{
    public class UICreator : MonoBehaviour
    {
        public static UICreator instance { get; private set; } = null;

        public Canvas canvas;
        public BasicView[] basicViews;

        private Dictionary<int, BasicView> sortDictionary;
        private Dictionary<string, BasicView> uiDictionary;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            uiDictionary = new Dictionary<string, BasicView>();

            for (int i=0; i<basicViews.Length; i++)
            {
                if (uiDictionary.ContainsKey(basicViews[i].name))
                    continue;

                uiDictionary.Add(basicViews[i].name, basicViews[i]);
            }
            sortDictionary = new Dictionary<int, BasicView>();
        }

        public BasicView Create(string prefabName, int sort = -1)
        {
            return Create(prefabName, canvas.transform, sort);
        }

        public BasicView Create(string prefabName, Transform root, int sort = -1)
        {
            if (!uiDictionary.TryGetValue(prefabName, out BasicView basicView))
                return null;

            BasicView view = GameObject.Instantiate<BasicView>(basicView, root);
            view.name = prefabName;

            if (sort != -1)
            {
                if (sortDictionary.ContainsKey(sort))
                {
                    int nextSort = sort + 1;

                    while (sortDictionary.ContainsKey(nextSort))
                    {
                        nextSort += 1;
                    }
                    sortDictionary.Add(nextSort, view);
                }
                else
                    sortDictionary.Add(sort, view);

                sortDictionary = sortDictionary.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            }

            Sorting();
            return view;
        }

        private void Sorting()
        {
            List<int> removeIndexs = null;

            for (int i=0; i<sortDictionary.Count; i++)
            {
                BasicView view = sortDictionary.ElementAt(i).Value;

                if (view == null)
                {
                    if (removeIndexs == null)
                        removeIndexs = new List<int>();

                    removeIndexs.Add(i);
                }
                else
                    view.transform.SetAsLastSibling();
            }

            if (removeIndexs != null)
            {
                for (int i=0; i<removeIndexs.Count; i++)
                {
                    sortDictionary.Remove(removeIndexs[i]);
                }
            }
        }
    }
}