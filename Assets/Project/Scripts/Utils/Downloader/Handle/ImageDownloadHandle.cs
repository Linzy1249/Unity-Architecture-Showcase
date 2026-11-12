using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityArchitectureShowcase.Utils
{
    public class ImageDownloadHandle : DownloadHandleBase
    {
        private const int RetryLimit = 2; // 網路異常時最多重試兩次，總計最多執行三次
        private const string CachePath = "Download/Image";

        public Action<string, string, bool> OnDownloadComplete; // outputPath, error, isCloud

        private bool isCloud;
        private int retryCount;
        private string outputPath;
        private CancellationTokenSource cancelSource;

        public ImageDownloadHandle(string url, string outputDirectory = "") : base(Path.Combine(url))
        {
            var bytes = Encoding.UTF8.GetBytes(url);

            outputPath = Path.Combine(Application.persistentDataPath, string.IsNullOrEmpty(outputDirectory) ? CachePath : outputDirectory, Convert.ToBase64String(bytes));
        }

        public override void Dispose()
        {
            if (cancelSource != null)
                cancelSource.Dispose();
        }

        public override void Cancel()
        {
            if (cancelSource != null)
                cancelSource.Cancel();
        }

        public override async Task Download()
        {
            if (client == null)
            {
                OnDownloadComplete?.Invoke("", "HttpClient is null.", isCloud);
                return;
            }

            try
            {
                if (!File.Exists(outputPath))
                {
                    isCloud = true;

                    cancelSource = new CancellationTokenSource();

                    var task = client.GetAsync(url, cancelSource.Token);

                    while (!task.IsCompleted)
                    {
                        await Task.Delay(100);
                    }

                    if (task.IsCanceled)
                    {
                        OnDownloadComplete?.Invoke("", "The operation was canceled.", isCloud);
                        return;
                    }

                    var bytes = await task.Result.Content.ReadAsByteArrayAsync();

                    if (cancelSource.IsCancellationRequested) // ReadAsByteArrayAsync 沒有辦法傳 CancellationToken
                    {
                        OnDownloadComplete?.Invoke("", "The operation was canceled.", isCloud);
                        return;
                    }

                    if (!IsValid(bytes))
                    {
                        OnDownloadComplete?.Invoke("", "The file type is invalid", isCloud);
                        return;
                    }

                    CreateDirectory();
                    File.WriteAllBytes(outputPath, bytes);
                }
                OnDownloadComplete?.Invoke(outputPath, "", isCloud);
            }
            catch (TaskCanceledException e)
            {
                OnDownloadComplete?.Invoke("", e.Message, isCloud);
            }
            catch (HttpRequestException e)
            {
                if (retryCount < RetryLimit)
                {
                    retryCount++;

                    await Download();
                }
                else
                    OnDownloadComplete?.Invoke("", e.Message, isCloud);
            }
            catch (Exception e)
            {
                OnDownloadComplete?.Invoke("", e.Message, isCloud);
            }
            finally
            {
                Dispose();
            }
        }

        public override int GetTimeout()
        {
            return 30; // 拉長到30秒，不然下載時很容易Timeout
        }

        private void CreateDirectory()
        {
            string directoryPath = Path.GetDirectoryName(outputPath);

            if (Directory.Exists(directoryPath))
                return;

            Directory.CreateDirectory(directoryPath);
        }

        private bool IsValid(byte[] bytes)
        {
            if (bytes.Length >= 4)
            {
                if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) // JPG
                    return true;
                
                if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) // PNG
                    return true;
            }
            return false;
        }
    }
}