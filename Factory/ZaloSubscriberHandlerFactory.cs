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

namespace ANCafe
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
            try
            {
                PXTrace.WriteInformation("ZaloSubscriberHandlerFactory static constructor called");
                ZaloFactoryRegistration.Initialize();
                PXTrace.WriteInformation("ZaloFactoryRegistration.Initialize() completed successfully");
            }
            catch (Exception ex)
            {
                PXTrace.WriteError("Error in ZaloSubscriberHandlerFactory static constructor: {0}", ex.Message);
                throw;
            }
        }

        #endregion

        #region Properties

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
            try
            {
                PXTrace.WriteInformation("CreateActionHandler called for handlerId: {0}", handlerId);

                var graph = PXGraph.CreateInstance<ZaloTemplateMaint>();
                var template = PXSelect<ZaloTemplate,
                    Where<ZaloTemplate.subscriberID, Equal<Required<ZaloTemplate.subscriberID>>>>
                    .Select(graph, handlerId)
                    .RowCast<ZaloTemplate>()
                    .FirstOrDefault();

                if (template == null)
                {
                    PXTrace.WriteWarning(LocalizableMessages.ZaloTemplateNotFound, handlerId);
                    throw new PXException(LocalizableMessages.ZaloTemplateNotFound, handlerId);
                }

                PXTrace.WriteInformation("Found Zalo template: {0} - {1}", template.Description, template.Body);
                return new ZaloSubscriberEventAction(handlerId, template);
            }
            catch (Exception ex)
            {
                PXTrace.WriteError("Error in CreateActionHandler: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the list of handlers for the current screen.
        /// </summary>
        public IEnumerable<BPHandler> GetHandlers(PXGraph graph)
        {
            try
            {
                PXTrace.WriteInformation("GetHandlers called");

                string screenID = null;
                // Try to get current screenID from BPEvent if available
                var bpEvent = graph.Caches[typeof(BPEvent)]?.Current as BPEvent;
                if (bpEvent != null && !string.IsNullOrEmpty(bpEvent.ScreenID))
                    screenID = bpEvent.ScreenID;

                PXTrace.WriteInformation("Getting handlers for screenID: {0}", screenID ?? "ALL");

                IEnumerable<ZaloTemplate> templates;
                if (!string.IsNullOrEmpty(screenID))
                {
                    templates = PXSelect<ZaloTemplate,
                        Where<ZaloTemplate.isActive, Equal<True>,
                            And<Where<ZaloTemplate.screen, Equal<Required<ZaloTemplate.screen>>,
                                Or<ZaloTemplate.screen, IsNull>>>>>
                        .Select(graph, screenID)
                        .RowCast<ZaloTemplate>();
                }
                else
                {
                    templates = PXSelect<ZaloTemplate,
                        Where<ZaloTemplate.isActive, Equal<True>>>
                        .Select(graph)
                        .RowCast<ZaloTemplate>();
                }

                var list = templates.Where(t => t != null && t.SubscriberID != null).ToList();
                PXTrace.WriteInformation("Found {0} active Zalo templates", list.Count);

                return list.Select(t => new BPHandler
                {
                    Id = t.SubscriberID, // Sử dụng SubscriberID
                    Name = !string.IsNullOrEmpty(t.Description) ? t.Description : "Zalo Template",
                    Type = LocalizableMessages.ZaloNotification
                }).ToList();
            }
            catch (Exception ex)
            {
                PXTrace.WriteError("Error in GetHandlers: {0}", ex.Message);
                return Enumerable.Empty<BPHandler>();
            }
        }

        /// <summary>
        /// Redirects to the ZaloTemplateMaint screen for the given handler.
        /// </summary>
        public void RedirectToHandler(Guid? handlerId)
        {
            try
            {
                PXTrace.WriteInformation("RedirectToHandler called for handlerId: {0}", handlerId);

                var graph = PXGraph.CreateInstance<ZaloTemplateMaint>();
                graph.Templates.Current = graph.Templates.Search<ZaloTemplate.subscriberID>(handlerId);

                if (graph.Templates.Current == null)
                {
                    PXTrace.WriteWarning("Template not found for redirect: {0}", handlerId);
                    throw new PXException(LocalizableMessages.TemplateNotFound);
                }

                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
            }
            catch (Exception ex)
            {
                PXTrace.WriteError("Error in RedirectToHandler: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Returns the delegate and attributes for the create action.
        /// </summary>
        public Tuple<PXButtonDelegate, PXEventSubscriberAttribute[]> getCreateActionDelegate(BusinessProcessEventMaint maintGraph)
        {
            PXButtonDelegate handler = (PXAdapter adapter) =>
            {
                try
                {
                    PXTrace.WriteInformation("Create new Zalo notification action called");

                    if (maintGraph.Events?.Current?.ScreenID == null)
                    {
                        PXTrace.WriteWarning("No current screen ID available");
                        return adapter.Get();
                    }

                    // Tạo mới và lưu ZaloTemplate vào DB
                    var graph = PXGraph.CreateInstance<ZaloTemplateMaint>();
                    
                    // Tạo description unique để tránh trùng lặp
                    var uniqueDescription = $"ZaloTemplate{DateTime.Now:yyyyMMddHHmmss}";
                    
                    var newTemplate = new ZaloTemplate
                    {
                        Description = uniqueDescription,
                        Body = "{{Branch}} - Notification from {{CheckDate}}", // Đổi sang tiếng Anh
                        Subject = $"Zalo Template {uniqueDescription}",
                        IsActive = true,
                        Screen = maintGraph.Events.Current.ScreenID,
                        ActivityType = "Zalo"
                    };

                    // Insert vào cache (RowInserting sẽ tự sinh SubscriberID nếu chưa có)
                    graph.Templates.Insert(newTemplate);

                    // Lưu vào DB
                    graph.Actions.PressSave();

                    // Sau khi save, SubscriberID đã có và đã lưu vào DB
                    var subscriber = new BPEventSubscriber
                    {
                        Type = Type,
                        HandlerID = graph.Templates.Current.SubscriberID // Sử dụng SubscriberID
                    };
                    maintGraph.Subscribers.Cache.Insert(subscriber);

                    PXTrace.WriteInformation("New Zalo subscriber created with ID: {0}", subscriber.HandlerID);
                    PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

                    return adapter.Get();
                }
                catch (Exception ex)
                {
                    PXTrace.WriteError("Error in create action delegate: {0}", ex.Message);
                    throw;
                }
            };

            return Tuple.Create(handler, new PXEventSubscriberAttribute[]
            {
                new PXButtonAttribute
                {
                    OnClosingPopup = PXSpecialButtonType.Refresh
                }
            });
        }

        #endregion
    }
}