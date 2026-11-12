using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace UnityArchitectureShowcase.Utils
{
    public class DownloadHelper
    {
        public static DownloadHelper instance { get; } = new DownloadHelper();

        public const int Timeout = 30;

        public enum DownloadLimit
        {
            Slow = 5,
            Fast = 64
        }

        public DownloadLimit downloadLimit = DownloadLimit.Slow;

        private HttpClient client;
        private List<DownloadHandleBase> downloadTasks;
        private List<DownloadHandleBase> downloadingTasks;

        private DownloadHelper()
        {
            client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(Timeout)
            };
            downloadTasks = new List<DownloadHandleBase>();
            downloadingTasks = new List<DownloadHandleBase>();

            MainMono.instance.OnUpdate += CheckDownloadStatus; // 本身沒有繼承 MonoBehaviour 並掛在場景上，因此必須依賴 MainMono by Linzy
        }

        public void Cancel()
        {
            for (int i=0; i<downloadingTasks.Count; i++)
            {
                downloadingTasks[i].Cancel();
                downloadingTasks[i].Dispose();
            }
            downloadTasks.Clear();
            downloadingTasks.Clear();
        }

        public void Cancel(string id)
        {
            for (int i=0; i<downloadingTasks.Count; i++)
            {
                var downloadingTask = downloadingTasks[i];

                if (downloadingTask.id != id)
                    continue;

                downloadingTask.Dispose();
                downloadingTasks.RemoveAt(i);
                return;
            }

            for (int i=0; i<downloadTasks.Count; i++)
            {
                var downloadTask = downloadTasks[i];

                if (downloadTask.id != id)
                    continue;

                downloadTask.Dispose();
                downloadTasks.RemoveAt(i);
                return;
            }
        }

        public string Download(DownloadHandleBase handle)
        {
            if (handle == null)
                return "";

            downloadTasks.Add(handle);
            return handle.id;
        }

        private async void CheckDownloadStatus()
        {
            if (downloadTasks.Count <= 0 || downloadingTasks.Count >= (int)downloadLimit)
                return;

            var donwloadHandle = downloadTasks.First();

            downloadTasks.Remove(donwloadHandle);
            downloadingTasks.Add(donwloadHandle);

            donwloadHandle.client = client;
            await donwloadHandle.Download();

            downloadingTasks.Remove(donwloadHandle);
        }
    }
}