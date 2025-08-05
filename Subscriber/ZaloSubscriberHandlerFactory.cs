using PX.Data;
using PX.BusinessProcess.Subscribers.Factories;
using PX.BusinessProcess.DAC;
using PX.BusinessProcess.Event;
using PX.BusinessProcess.Subscribers.ActionHandlers;
using PX.BusinessProcess.UI;
using PX.SM;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AnNhienCafe
{
    // Thêm vào ZaloSubscriberHandlerFactory để trigger registration
    public class ZaloSubscriberHandlerFactory : IBPSubscriberActionHandlerFactoryWithCreateAction
    {
        // Static constructor để tự động trigger registration
        static ZaloSubscriberHandlerFactory()
        {
            ZaloFactoryRegistration.Initialize();
        }

        // Các method của bạn giữ nguyên...
        public IEventAction CreateActionHandler(Guid handlerId, bool stopOnError, IEventDefinitionsProvider eventDefinitionsProvider)
        {
            var graph = PXGraph.CreateInstance<PXGraph>();
            var notification = PXSelect<Notification,
                Where<Notification.noteID, Equal<Required<Notification.noteID>>>>
                .Select(graph, handlerId)
                .FirstOrDefault();
            return new ZaloSubscriberEventAction(handlerId, notification);
        }

        public IEnumerable<BPHandler> GetHandlers(PXGraph graph)
        {
            return PXSelect<Notification,
                    Where<Notification.screenID, Equal<Current<BPEvent.screenID>>,
                        Or<Current<BPEvent.screenID>, IsNull>>>
                .Select(graph).FirstTableItems
                .Where(c => c != null)
                .Select(c => new BPHandler
                {
                    Id = c.NoteID,
                    Name = c.Name,
                    Type = LocalizableMessages.ZaloNotification
                });
        }

        public void RedirectToHandler(Guid? handlerId)
        {
            var graph = PXGraph.CreateInstance<SMNotificationMaint>();
            graph.Message.Current = graph.Notifications.Search<Notification.noteID>(handlerId);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
        }

        public string Type => "ZALO";
        public string TypeName => LocalizableMessages.ZaloNotification;
        public string CreateActionName => "NewZaloNotification";
        public string CreateActionLabel => LocalizableMessages.CreateZaloNotification;

        public Tuple<PXButtonDelegate, PXEventSubscriberAttribute[]> getCreateActionDelegate(BusinessProcessEventMaint maintGraph)
        {
            PXButtonDelegate handler = (PXAdapter adapter) =>
            {
                if (maintGraph.Events?.Current?.ScreenID == null)
                    return adapter.Get();

                var graph = PXGraph.CreateInstance<SMNotificationMaint>();
                var cache = graph.Caches<Notification>();
                var notification = (Notification)cache.CreateInstance();
                var row = cache.InitNewRow(notification);
                row.ScreenID = maintGraph.Events.Current.ScreenID;
                cache.Insert(row);

                var subscriber = new BPEventSubscriber();
                var subscriberRow = maintGraph.Subscribers.Cache.InitNewRow(subscriber);
                subscriberRow.Type = Type;
                subscriberRow.HandlerID = row.NoteID;
                graph.Caches[typeof(BPEventSubscriber)].Insert(subscriberRow);

                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
                return adapter.Get();
            };

            return Tuple.Create(handler, new PXEventSubscriberAttribute[]
            {
                new PXButtonAttribute
                {
                    OnClosingPopup = PXSpecialButtonType.Refresh
                }
            });
        }
    }
}