using PX.BusinessProcess.Event;
using PX.BusinessProcess.Subscribers.ActionHandlers;
using PX.Data;
using PX.Data.BusinessProcess;
using System;
using System.Reflection;
using System.Threading;

namespace AnNhienCafe
{
    public class ZaloSubscriberEventAction : EventActionBase
    {
        private readonly Guid _handlerID;

        public ZaloSubscriberEventAction(Guid handlerID) : base(handlerID)
        {
            _handlerID = handlerID;
        }

        public void Execute(object rowObject)
        {
            PXTrace.WriteInformation($"[ZALO] Triggered ZaloSubscriberEventAction for handlerID = {_handlerID}");

            if (rowObject != null)
            {
                Type rowType = rowObject.GetType();
                foreach (PropertyInfo prop in rowType.GetProperties())
                {
                    try
                    {
                        object value = prop.GetValue(rowObject);
                        PXTrace.WriteInformation($"{prop.Name} = {value}");
                    }
                    catch { }
                }
            }
            else
            {
                PXTrace.WriteWarning("Row object is null.");
            }
        }

        public override void Process(MatchedRow[] matches, CancellationToken cancellationToken)
        {
            foreach (var match in matches)
            {
                var rowProperty = match.GetType().GetProperty("Row", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var rowObject = rowProperty?.GetValue(match);
                Execute(rowObject);
            }
        }
    }
}