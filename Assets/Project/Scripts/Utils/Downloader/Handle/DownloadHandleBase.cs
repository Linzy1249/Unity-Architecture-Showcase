using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace UnityArchitectureShowcase.Utils
{
    public abstract class DownloadHandleBase
    {
        public string id { get; private set; }

        public HttpClient client;

        protected string url;

        public DownloadHandleBase(string _url)
        {
            url = _url;

            id = Guid.NewGuid().ToString();
        }

        public abstract void Dispose();
        public abstract void Cancel();
        public abstract Task Download();

        public virtual int GetTimeout()
        {
            return 5; // 複寫即可更改 Timeout 時間
        }
    }
}