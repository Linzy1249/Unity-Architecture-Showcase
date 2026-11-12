using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityArchitectureShowcase.Utils
{
    public class ResourcesHelper
    {
        public static ResourcesHelper instance { get; } = new ResourcesHelper();

        private static SemaphoreSlim textureLimiter = new SemaphoreSlim(1); // 限制同時只能解析一張圖

        private Dictionary<string, int> textureUsedCount = new Dictionary<string, int>();
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public void Dispose()
        {
            for (int i = 0; i < audioClips.Count; i++)
            {
                var audioClip = audioClips.ElementAt(i).Value;

                if (audioClip == null)
                    continue;

                GameObject.Destroy(audioClip);
            }
            audioClips.Clear();

            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures.ElementAt(i).Value;

                if (texture == null)
                    continue;

                GameObject.Destroy(texture);
            }
            textures.Clear();
            textureUsedCount.Clear();
        }

        public void ClearAudio(string path)
        {
            path = string.IsNullOrEmpty(path) ? "" : path;

            if (!audioClips.TryGetValue(path, out AudioClip audioClip))
                return;

            GameObject.Destroy(audioClip);
            audioClips.Remove(path);
        }

        public void ClearTexture(string path)
        {
            path = string.IsNullOrEmpty(path) ? "" : path;

            if (textureUsedCount.TryGetValue(path, out int count))
            {
                count--;

                if (count > 0)
                {
                    textureUsedCount[path] = count;
                    return;
                }
                else
                    textureUsedCount.Remove(path);
            }

            if (!textures.TryGetValue(path, out Texture2D texture))
                return;

            GameObject.Destroy(texture);
            textures.Remove(path);
        }

        public async Task<AudioClip> GetAudio(string path, AudioType audioType = AudioType.MPEG)
        {
            AudioClip audioClip = null;

            if (string.IsNullOrEmpty(path) || audioClips.TryGetValue(path, out audioClip))
            {
                return audioClip;
            }
            using var request = UnityWebRequestMultimedia.GetAudioClip(new Uri(path), audioType);
            request.timeout = 10;

            await request.SendWebRequest();

            if (string.IsNullOrEmpty(request.error))
            {
                audioClip = DownloadHandlerAudioClip.GetContent(request);

                if (audioClip == null)
                {
                    var bytes = request.downloadHandler.data;

                    var dataIndex = 44;
                    var sampleCount = (bytes.Length - dataIndex) / 2;

                    var samples = new float[sampleCount];
                    var channels = BitConverter.ToInt16(bytes, 22);
                    var sampleRate = BitConverter.ToInt32(bytes, 24);

                    var offset = dataIndex;

                    for (int i = 0; i < sampleCount; i++)
                    {
                        var sample = BitConverter.ToInt16(bytes, offset);

                        samples[i] = sample / 32768f;

                        offset += 2;
                    }
                    audioClip = AudioClip.Create("wav", sampleCount / channels, channels, sampleRate, false);
                    audioClip.SetData(samples, 0);
                }

                if (!audioClips.TryAdd(path, audioClip))
                {
                    GameObject.Destroy(audioClip);

                    audioClips.TryGetValue(path, out audioClip);
                }
            }
            return audioClip;
        }

        public async Task<Texture2D> GetTexture(string path, float sizeLimit = 0)
        {
            Texture2D texture = null;

            try
            {
                if (string.IsNullOrEmpty(path) || textures.TryGetValue(path, out texture))
                {
                    return texture;
                }
                using var request = UnityWebRequestTexture.GetTexture(new Uri(path));
                request.timeout = 10;

                await request.SendWebRequest();

                if (string.IsNullOrEmpty(request.error))
                {
                    await textureLimiter.WaitAsync(); // 只允許同時解析一張圖片，已經下載好的會在這邊等待

                    texture = DownloadHandlerTexture.GetContent(request);

                    if (textures.TryAdd(path, texture))
                    {
                        if (sizeLimit > 0)
                        {
                            var scale = sizeLimit / Mathf.Max(texture.width, texture.height);

                            Scale(texture, Mathf.FloorToInt(texture.width * scale), Mathf.FloorToInt(texture.height * scale));
                        }
                    }
                    else
                    {
                        GameObject.Destroy(texture);

                        textures.TryGetValue(path, out texture);
                    }
                }
                return texture;
            }
            finally
            {
                if (texture != null)
                {
                    if (textureUsedCount.TryGetValue(path, out int count))
                        textureUsedCount[path] = count + 1;
                    else
                        textureUsedCount.TryAdd(path, 1);
                }
                textureLimiter.Release();
            }
        }

        private void Scale(Texture2D source, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            source.filterMode = mode;

            var renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            try
            {
                Graphics.SetRenderTarget(renderTexture);
                GL.LoadPixelMatrix(0, 1, 1, 0);
                GL.Clear(true, true, new Color(0, 0, 0, 0));
                Graphics.DrawTexture(new Rect(0, 0, 1, 1), source);

    #if UNITY_2021_1_OR_NEWER
                source.Reinitialize(width, height);
    #else
                source.Resize(width, height);
    #endif
                source.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                source.Apply(true);
            }
            finally
            {
                if (renderTexture != null)
                {
                    renderTexture.Release();
                    GameObject.Destroy(renderTexture);
                }
            }
        }
    }
}