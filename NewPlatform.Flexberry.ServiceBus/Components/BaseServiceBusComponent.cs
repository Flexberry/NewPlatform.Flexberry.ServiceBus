namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;

    /// <summary>
    /// Base abstract implementation of <see cref="IServiceBusComponent"/>.
    /// </summary>
    public abstract class BaseServiceBusComponent : IServiceBusComponent, IDisposable
    {
        /// <summary>
        /// Prepare to start component.
        /// Component should be able to execute its methods and methods of other components after prepare.
        /// </summary>
        public virtual void Prepare()
        {
        }

        /// <summary>
        /// Start component.
        /// Dependent components (services) should be initialized here.
        /// Threads for processing should be started here.
        /// </summary>
        public virtual void Start()
        {
        }

        /// <summary>
        /// Stop component.
        /// Components (services) and threads should be stopped here.
        /// </summary>
        public virtual void Stop()
        {
        }

        /// <summary>
        /// Actoins that should be performed after stopping the component.
        /// Сomplete shutdown of the component.
        /// </summary>
        public virtual void AfterStop()
        {
        }

        /// <summary>
        /// Implement IDisposable.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implements disposing of resources in two distinct scenarios.
        /// </summary>
        /// <param name="disposing">
        /// If disposing equals true, the method has been called directly or indirectly by a user's code.
        /// Managed and unmanaged resources can be disposed.
        ///
        /// If disposing equals false, the method has been called by the runtime from inside the finalizer and you should not reference other objects.
        /// Only unmanaged resources can be disposed.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
