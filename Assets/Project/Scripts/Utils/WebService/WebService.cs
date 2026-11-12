using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityArchitectureShowcase.Network
{
    public struct FileParameter
    {
        public string key;
        public string fileName;
        public byte[] fileBinary;
    }

    public class WebService
    {
        private const int RetryDelay = 3;
        private const int RetryLimit = 2; // 網路異常時最多重試兩次，總計最多執行三次

        private HttpClient client;
        private HttpClient uploadClient;

        public WebService()
        {
            HttpClientHandler handler = new HttpClientHandler();

            client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10) // 一般API的Timeout時間為10秒
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            uploadClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30) // 上傳API的Timeout時間為30秒
            };
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<string> Delete(string url, JObject headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value.ToString());
                }
            }

            HttpResponseMessage response = null;

            for (int i = 0; i <= RetryLimit; i++)
            {
                try
                {
                    response = await client.SendAsync(request);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Unauthorized: // 401就是API有呼叫成功但資料異常
                            return await response.Content.ReadAsStringAsync();
                    }
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    if (i >= RetryLimit)
                        throw new HttpRequestException(e.Message, e);
                    else
                        await Task.Delay(RetryDelay * 1000); // 網路連線異常時等待三秒
                }
                finally
                {
                    response?.Dispose();
                    response = null;
                }
            }
            return "";
        }

        public async Task<string> Get(string url, JObject headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value.ToString());
                }
            }

            HttpResponseMessage response = null;

            for (int i = 0; i <= RetryLimit; i++)
            {
                try
                {
                    response = await client.SendAsync(request);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Unauthorized: // 401就是API有呼叫成功但資料異常
                            return await response.Content.ReadAsStringAsync();
                    }
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    if (i >= RetryLimit)
                        throw new HttpRequestException(e.Message, e);
                    else
                        await Task.Delay(RetryDelay * 1000); // 網路連線異常時等待三秒
                }
                finally
                {
                    response?.Dispose();
                    response = null;
                }
            }
            return "";
        }

        public async Task<string> Patch(string url, JObject headers = null)
        {
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), url); // 實機上不支援「HttpMethod.Patch」

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value.ToString());
                }
            }

            HttpResponseMessage response = null;

            for (int i = 0; i <= RetryLimit; i++)
            {
                try
                {
                    response = await client.SendAsync(request);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Unauthorized: // 401就是API有呼叫成功但資料異常
                            return await response.Content.ReadAsStringAsync();
                    }
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    if (i >= RetryLimit)
                        throw new HttpRequestException(e.Message, e);
                    else
                        await Task.Delay(RetryDelay * 1000); // 網路連線異常時等待三秒
                }
                finally
                {
                    response?.Dispose();
                    response = null;
                }
            }
            return "";
        }

        public async Task<string> Post(string url, JObject parameters, JObject headers = null)
        {
            using var content = new StringContent(parameters != null ? parameters.ToString(Formatting.None) : string.Empty, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value.ToString());
                }
            }
            request.Content = content;

            HttpResponseMessage response = null;

            for (int i = 0; i <= RetryLimit; i++)
            {
                try
                {
                    response = await client.SendAsync(request);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Unauthorized: // 401就是API有呼叫成功但資料異常
                            return await response.Content.ReadAsStringAsync();
                    }
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    if (i >= RetryLimit)
                        throw new HttpRequestException(e.Message, e);
                    else
                        await Task.Delay(RetryDelay * 1000); // 網路連線異常時等待三秒
                }
                finally
                {
                    response?.Dispose();
                    response = null;
                }
            }
            return "";
        }

        public async Task<string> Upload(string url, JObject headers, JObject parameters, params FileParameter[] fileParameters)
        {
            using var content = new MultipartFormDataContent();
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    content.Add(new StringContent(parameter.Value.ToString()), parameter.Key);
                }
            }

            if (fileParameters != null)
            {
                foreach (var parameter in fileParameters)
                {
                    content.Add(new ByteArrayContent(parameter.fileBinary, 0, parameter.fileBinary.Length), parameter.key, parameter.fileName);
                }
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value.ToString());
                }
            }
            request.Content = content; // 「MultipartFormDataContent」與「HttpRequestMessage」一起使用會Dispose兩次，而Unity 2022.3.62f2中該行為會報錯

            HttpResponseMessage response = null;

            for (int i = 0; i <= RetryLimit; i++)
            {
                try
                {
                    response = await uploadClient.SendAsync(request);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Unauthorized: // 401就是API有呼叫成功但資料異常
                            return await response.Content.ReadAsStringAsync();
                    }
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    if (i >= RetryLimit)
                        throw new HttpRequestException(e.Message, e);
                    else
                        await Task.Delay(RetryDelay * 1000); // 網路連線異常時等待三秒
                }
                finally
                {
                    response?.Dispose();
                    response = null;
                }
            }
            return "";
        }

        public async Task<string> UploadFromUnityWebRequest(string url, JObject headers, JObject parameters, params FileParameter[] fileParameters)
        {
            var content = new List<IMultipartFormSection>();

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    content.Add(new MultipartFormDataSection(parameter.Key, parameter.Value.ToString()));
                }
            }

            if (fileParameters != null)
            {
                foreach (var parameter in fileParameters)
                {
                    content.Add(new MultipartFormFileSection(parameter.key, parameter.fileBinary, parameter.fileName, "application/octet-stream"));
                }
            }

            for (int i = 0; i <= RetryLimit; i++)
            {
                var request = UnityWebRequest.Post(url, content);

                request.timeout = 30;

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value.ToString());
                    }
                }

                try
                {
                    await request.SendWebRequest();

                    switch (request.responseCode)
                    {
                        case (int)HttpStatusCode.OK:
                        case (int)HttpStatusCode.Unauthorized: // 401就是API有呼叫成功但資料異常
                            return request.downloadHandler.text;
                        case (int)HttpStatusCode.RequestTimeout:
                            throw new HttpRequestException("Timeout");
                    }
                    throw new HttpRequestException($"HTTP Error: {request.responseCode} - {request.error}");
                }
                catch (HttpRequestException e)
                {
                    if (i >= RetryLimit)
                        throw new HttpRequestException(e.Message, e);
                    else
                        await Task.Delay(RetryDelay * 1000); // 網路連線異常時等待三秒
                }
                finally
                {
                    request?.Dispose();
                }
            }
            return "";
        }
    }
}