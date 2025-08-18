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
        static ZaloFactoryRegistration()
        {
            try
            {
                var factory = new ZaloSubscriberHandlerFactory();
                PXTrace.WriteInformation("Zalo Subscriber Factory initialized successfully");
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"Zalo Factory initialization failed: {ex.Message}");
            }
        }

        public static void Initialize()
        {
            // Triggers static constructor
        }
    }
}