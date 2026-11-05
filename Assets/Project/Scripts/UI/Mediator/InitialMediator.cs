
using UnityEngine;

namespace UnityArchitectureShowcase.UI
{
    public class InitialMediator : IMediator
    {
        private bool isInit;

        public void OnDispose()
        {

        }

        public void OnSetup()
        {
            if (!isInit)
            {
                SetupSettings();
            }
            ChangeToHomePage();
        }

        private void SetupSettings()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:
                    Debug.unityLogger.logEnabled = true;
                    break;
                default:
                    Debug.unityLogger.logEnabled = false;
                    break;
            }
        }

        private void ChangeToHomePage()
        {
            StateMachine.instance.ChangeState(nameof(State.HomeState));
        }
    }
}