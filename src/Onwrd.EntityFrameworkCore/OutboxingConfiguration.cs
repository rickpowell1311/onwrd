namespace Onwrd.EntityFrameworkCore
{
    public class OutboxingConfiguration
    {
        internal Func<IOnwardProcessor> OnwardProcessorFactory { get; private set; }

        public OutboxingConfiguration()
        {

        }

        public void UseOnwardProcessor(Func<IOnwardProcessor> onwardProcessorFactory)
        {
            this.OnwardProcessorFactory = onwardProcessorFactory;
        }
    }
}
