using Moq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Mock of IServiceProvider to test your own service
    /// </summary>
    public class ServiceProviderMock : Mock<IServiceProvider>
    {
        private Lazy<IServiceProvider> _serviceProviderLazy;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public ServiceProviderMock()
        {
            Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(t => BuildServiceProvider().GetService(t));
        }

        private IServiceProvider BuildServiceProvider()
        {
            // We need an IServiceProvider that also provides IServiceScopeFactory and maybe more,
            // so it is easier to use an actual ServiceCollection instead of the dictionary.
            // But because this class already exposed the public dictionary and to avoid breaking changes we 
            // copy the content of the dictionary to an actual ServiceCollection when needed.
            
            var col = new ServiceCollection();
            foreach (var (key, value) in Services)
            {
                if (value != null) col.AddSingleton(key, value);
            }

            return col.BuildServiceProvider();
        }

        /// <summary>
        ///     Fake services returned by the GetServiceMethod
        /// </summary>
        public IDictionary<Type, object?> Services { get; } = new Dictionary<Type, object?>();
    }

}