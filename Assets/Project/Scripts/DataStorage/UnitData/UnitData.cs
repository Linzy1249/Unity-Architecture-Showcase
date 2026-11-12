using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityArchitectureShowcase.Data
{
    public class UnitData
    {
        public T Copy<T>(T source) where T : UnitData
        {
            var json = JsonConvert.SerializeObject(source);

            JsonConvert.PopulateObject(json, this);

            return this as T;
        }

        public JObject ToJson()
        {
            return JObject.FromObject(this); // JsonConvert.SerializeObject(this);
        }
    }
}