using Moq;
using System;
using System.Collections.Generic;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Mock of IServiceProvider to test your own service
    /// </summary>
    public class ServiceProviderMock : Mock<IServiceProvider>
    {
        /// <summary>
        ///     Fake services returned by the GetServiceMethod
        /// </summary>
        public IDictionary<Type, object?> Services { get; } = new Dictionary<Type, object?>();
        /// <summary>
        ///     Default constructor
        /// </summary>
        public ServiceProviderMock()
        {
            Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(t => Services.TryGetValue(t, out var obj) ? obj : null);
        }
    }
}