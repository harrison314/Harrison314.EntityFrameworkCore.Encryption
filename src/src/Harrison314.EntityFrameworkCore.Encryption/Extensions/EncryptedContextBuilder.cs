namespace Microsoft.Extensions.DependencyInjection
{
    public class EncryptedContextBuilder
    {
        public IServiceCollection ServiceCollection
        {
            get;
            protected set;
        }

        internal EncryptedContextBuilder(IServiceCollection serviceCollection)
        {
            this.ServiceCollection = serviceCollection;
        }
    }
}
