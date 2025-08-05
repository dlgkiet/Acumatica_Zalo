using PX.BusinessProcess.Subscribers.ActionHandlers;
using PX.Data.BusinessProcess;
using PX.Data;
using System;
using System.Threading;
using PX.SM;

namespace AnNhienCafe
{
    public class ZaloSubscriberEventAction : IEventAction
    {
        public Guid Id { get; set; }
        public string Name { get; protected set; }
        private readonly Notification _notificationTemplate;

        public void Process(MatchedRow[] eventRows, CancellationToken cancellation)
        {
            PXTrace.WriteInformation("ZALO subscriber triggered!");

            // Lấy thông tin từ event nếu cần
            // Gửi Zalo hoặc xử lý custom logic ở đây
        }

        public ZaloSubscriberEventAction(Guid id, Notification notification)
        {
            Id = id;
            Name = notification.Name;
            _notificationTemplate = notification;
        }
    }
}
