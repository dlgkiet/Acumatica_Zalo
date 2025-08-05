using PX.Data;
using PX.BusinessProcess.Subscribers.Factories;
using System;

namespace AnNhienCafe
{
    // Đăng ký factory thông qua static constructor
    public static class ZaloFactoryRegistration
    {
        static ZaloFactoryRegistration()
        {
            try
            {
                // Force khởi tạo factory
                var factory = new ZaloSubscriberHandlerFactory();
                PXTrace.WriteInformation("Zalo Subscriber Factory initialized successfully");
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"Failed to initialize Zalo Factory: {ex.Message}");
            }
        }

        // Method để trigger static constructor
        public static void Initialize()
        {
            // Không cần làm gì, chỉ để trigger static constructor
        }
    }
}