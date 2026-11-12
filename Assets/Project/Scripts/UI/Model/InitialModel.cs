using System.Threading.Tasks;
using UnityArchitectureShowcase.Network;

namespace UnityArchitectureShowcase.UI
{
    public class InitialModel
    {
        public void Dispose()
        {
            // todo
        }

        public async Task<bool> GetSystemConfig()
        {
            return await APISender.instance.GetSystemConfig();
        }
    }
}