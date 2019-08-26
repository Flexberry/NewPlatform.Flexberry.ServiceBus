namespace NewPlatform.Flexberry.ServiceBus.Exceptions
{
    using System;

    public class ServiceBusSettingsInvalidTypeException : Exception
    {
        protected Type ExpectedType { get; }

        protected Type ActualType { get; }

        public ServiceBusSettingsInvalidTypeException(Type expectedType, Type actualType)
        {
            ExpectedType = expectedType;
            ActualType = actualType;
            Data.Add(nameof(ExpectedType), expectedType.AssemblyQualifiedName);
            Data.Add(nameof(ActualType), actualType?.AssemblyQualifiedName);
        }
    }
}
