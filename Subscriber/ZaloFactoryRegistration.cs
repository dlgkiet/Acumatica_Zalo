using PX.Data;
using PX.BusinessProcess.Subscribers.Factories;
using System;
using System.Collections.Generic;
using AnNhienCafe;

namespace AnNhienCafe
{
    // Đăng ký factory với Business Process framework
    public static class ZaloFactoryRegistration
    {
        private static volatile bool _isInitialized = false;
        private static readonly object _lockObject = new object();

        static ZaloFactoryRegistration()
        {
            Initialize();
        }

        // Method để trigger static constructor và đăng ký factory
        public static void Initialize()
        {
            if (_isInitialized) return;

            lock (_lockObject)
            {
                if (_isInitialized) return;

                try
                {
                    PXTrace.WriteInformation("Initializing Zalo Subscriber Factory...");

                    // Đăng ký factory với Business Process framework
                    var factory = new ZaloSubscriberHandlerFactory();

                    // Đăng ký factory type với hệ thống
                    SubscriberActionHandlerFactoryProvider.RegisterFactory(factory.Type, factory);

                    _isInitialized = true;
                    PXTrace.WriteInformation("Zalo Subscriber Factory registered successfully with type: {0}", factory.Type);
                }
                catch (Exception ex)
                {
                    PXTrace.WriteError("Failed to initialize Zalo Factory: {0}", ex.Message);
                    throw;
                }
            }
        }
    }

    // Nếu SubscriberActionHandlerFactoryProvider không có sẵn
    internal static class SubscriberActionHandlerFactoryProvider
    {
        private static readonly Dictionary<string, IBPSubscriberActionHandlerFactoryWithCreateAction> _factories
            = new Dictionary<string, IBPSubscriberActionHandlerFactoryWithCreateAction>();

        public static void RegisterFactory(string type, IBPSubscriberActionHandlerFactoryWithCreateAction factory)
        {
            _factories[type] = factory;
            PXTrace.WriteInformation("Factory registered for type: {0}", type);
        }

        public static IBPSubscriberActionHandlerFactoryWithCreateAction GetFactory(string type)
        {
            _factories.TryGetValue(type, out var factory);
            return factory;
        }
    }
}