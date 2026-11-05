namespace UnityArchitectureShowcase.UI
{
    public class InitialState : IState
    {
        private InitialMediator mediator = new InitialMediator();

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