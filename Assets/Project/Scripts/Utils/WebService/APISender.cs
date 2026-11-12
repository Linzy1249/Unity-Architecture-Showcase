using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityArchitectureShowcase.Data;

namespace UnityArchitectureShowcase.Network
{
    public class APISender
    {
        public static APISender instance { get; } = new APISender();

        private WebService webService;
        private JsonSerializerSettings serializerSettings;

        private APISender()
        {
            webService = new WebService();

            serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public async Task<bool> GetSystemConfig()
        {
            try
            {
                await Task.Delay(1000); // await webService.Post(command, parameters, headers);

                var response = new JObject
                {
                    { "error", "" },
                    { "success", true },
                    {
                        "data", new JObject
                        {
                            { "newestVersion", "1.0.1" }
                        }
                    }
                }.ToString();

                var json = JObject.Parse(response);

                if (!json.TryGetValue("success", out JToken success) || !json.TryGetValue("data", out JToken data) || data is not JObject)
                {
                    throw new Exception("API response exception.");
                }
                if (!(bool)success)
                {
                    throw new GetResponseException();
                }
                AppData.instance.configData = JsonConvert.DeserializeObject<ConfigData>(data.ToString(), serializerSettings);
            }
            catch (HttpRequestException e)
            {
                throw new HttpRequestException(e.Message, e);
            }
            catch (GetResponseException e)
            {
                throw new GetResponseException(e.Message, e);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            return true;
        }
    }
}