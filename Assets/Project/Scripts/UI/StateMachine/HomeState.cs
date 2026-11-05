namespace UnityArchitectureShowcase.UI
{
    public class HomeState : IState
    {
        private HomeMediator mediator = new HomeMediator();

        public void Dispose()
        {
            mediator.OnDispose();
        }

        public void Setup()
        {
            mediator.OnSetup();
        }
    }
}