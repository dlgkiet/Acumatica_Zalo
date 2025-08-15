using AnNhienCafe;
using PX.BusinessProcess.DAC;
using PX.BusinessProcess.Event;
using PX.BusinessProcess.Subscribers.ActionHandlers;
using PX.BusinessProcess.Subscribers.Factories;
using PX.BusinessProcess.UI;
using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnNhienCafe
{
    /// <summary>
    /// Factory for Zalo subscriber event handlers.
    /// </summary>
    public class ZaloSubscriberHandlerFactory : IBPSubscriberActionHandlerFactoryWithCreateAction
    {
        #region Static Constructor

        // Static constructor to trigger registration
        static ZaloSubscriberHandlerFactory()
        {
            ZaloFactoryRegistration.Initialize();
        }

        #endregion

        #region Properties

        public static class LocalizableMessages
        {
            public const string ZaloNotification = "Zalo Notification";
            public const string CreateZaloNotification = "Zalo Notification";
        }
        public string Type => "ZALO";

        public string TypeName => LocalizableMessages.ZaloNotification;

        public string CreateActionName => "NewZaloNotification";

        public string CreateActionLabel => LocalizableMessages.CreateZaloNotification;

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the event action handler for Zalo.
        /// </summary>
        public IEventAction CreateActionHandler(Guid handlerId, bool stopOnError, IEventDefinitionsProvider eventDefinitionsProvider)
        {
            return new ZaloSubscriberEventAction(handlerId);
        }

        /// <summary>
        /// Gets the list of handlers for the current screen.
        /// </summary>
        public IEnumerable<BPHandler> GetHandlers(PXGraph graph)
        {
            return PXSelect<
                ZaloTemplate,
                Where<ZaloTemplate.screen, Equal<Current<BPEvent.screenID>>,
                      Or<Current<BPEvent.screenID>, IsNull>>>
                .Select(graph)
                .FirstTableItems
                .Where(t => t != null)
                .Select(t => new BPHandler
                {
                    Id = t.SubscriberID,
                    Name = t.Subject ?? $"Template {t.NotificationID}",
                    Type = LocalizableMessages.ZaloNotification
                });
        }

        public void RedirectToHandler(Guid? handlerId)
        {
            var graph = PXGraph.CreateInstance<ZaloTemplateMaint>();

            var template = PXSelect<ZaloTemplate,
                Where<ZaloTemplate.subscriberID, Equal<Required<ZaloTemplate.subscriberID>>>>
                .Select(graph, handlerId);

            if (template != null)
                graph.Templates.Current = template;

            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
        }

        public Tuple<PXButtonDelegate, PXEventSubscriberAttribute[]> getCreateActionDelegate(BusinessProcessEventMaint maintGraph)
        {
            PXButtonDelegate handler = (PXAdapter adapter) =>
            {
                if (maintGraph.Events?.Current?.ScreenID == null)
                    return adapter.Get();

                var graph = PXGraph.CreateInstance<ZaloTemplateMaint>();
                var cache = graph.Templates.Cache;

                var newTemplate = (ZaloTemplate)cache.CreateInstance();
                newTemplate.SubscriberID = Guid.NewGuid();
                newTemplate.Description = "NEW";
                newTemplate.Subject = "";
                newTemplate.Screen = maintGraph.Events.Current.ScreenID;
                newTemplate.Body = "An Nhiên Cafe - Thông báo kết quả kiểm kê:\r\n\n- Chi nhánh: {{ChiNhanh}}\r\n- Ngày kiểm kê: {{NgayKiemKe}}\r\n- Người kiểm kê: {{NguoiKiemKe}}\r\nSố phiếu kiểm kê. {{SoPhieu}}\r\n\nTổng chênh lệch: {{TongChenhlech}}:\r\n\n{{ChiTietChenhlech}}\r\nVui lòng kiểm tra lại phiếu và phản hồi nếu có sai lệch.";
                newTemplate.ActivityType = "ZALO";

                var inserted = cache.Insert(newTemplate);
                graph.Templates.Current = (ZaloTemplate)inserted;

                graph.Actions.PressSave();

                var newSubscriberRow = new BPEventSubscriber
                {
                    Type = Type,
                    HandlerID = newTemplate.SubscriberID
                };

                maintGraph.Subscribers.Cache.Insert(newSubscriberRow);
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
                return adapter.Get();
            };

            return Tuple.Create(handler, new PXEventSubscriberAttribute[]
            {
        new PXButtonAttribute { OnClosingPopup = PXSpecialButtonType.Refresh }
            });
        }
        #endregion
    }
}