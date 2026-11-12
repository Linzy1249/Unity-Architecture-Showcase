using System;
using UnityArchitectureShowcase.Data;
using UnityArchitectureShowcase.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UnityArchitectureShowcase.UI
{
    public class ProjectItem : ScrollItem<ScrollItemData>
    {
        public Texture defaultTexture;

        public Button button;
        public Text titleText;
        public RawImage thumbnailImage;
        public AspectRatioFitter thumbnailAspectRatio;

        public Action OnClick;

        private string url = "";
        private string taskID = "";

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                OnClick?.Invoke();
            });
        }

        protected override void DeactivatedCustomActions()
        {
            if (!string.IsNullOrEmpty(taskID))
            {
                DownloadHelper.instance.Cancel(taskID);
                taskID = "";
            }
            ResourcesHelper.instance.ClearTexture(url);

            url = "";
            thumbnailImage.texture = defaultTexture;
            thumbnailAspectRatio.aspectRatio = (float)defaultTexture.width / defaultTexture.height;
        }

        public void SetData(ProjectData projectData)
        {
            DeactivatedCustomActions();

            if (!string.IsNullOrEmpty(projectData.previewImageUrl))
            {
                taskID = DownloadHelper.instance.Download(new ImageDownloadHandle(projectData.previewImageUrl)
                {
                    OnDownloadComplete = (output, error, isCloud) =>
                    {
                        taskID = "";

                        if (!string.IsNullOrEmpty(error))
                            return;

                        SetThumbnail(output);
                    }
                });
            }
            SetTitle(projectData.title);
        }

        private void SetTitle(string title)
        {
            if (titleText == null)
                return;

            titleText.SetEllipsisText(title);
        }

        private async void SetThumbnail(string _url)
        {
            url = _url;

            var texture = await ResourcesHelper.instance.GetTexture(url, 512);

            if (texture == null)
                return;

            thumbnailImage.texture = texture;
            thumbnailAspectRatio.aspectRatio = (float)texture.width / texture.height;
        }
    }
}