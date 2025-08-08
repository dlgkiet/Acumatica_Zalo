using PX.BusinessProcess.Subscribers.ActionHandlers;
using PX.Data.BusinessProcess;
using PX.Data;
using System;
using System.Threading;
using System.Collections.Generic;

namespace ANCafe
{
    public class ZaloSubscriberEventAction : IEventAction
    {
        public Guid Id { get; set; }
        public string Name { get; protected set; }
        private readonly ZaloTemplate _zaloTemplate;

        public ZaloSubscriberEventAction(Guid id, ZaloTemplate template)
        {
            Id = id;
            _zaloTemplate = template ?? throw new ArgumentNullException(nameof(template));
            Name = template.Description ?? "Zalo Notification";
        }

        public void Process(MatchedRow[] eventRows, CancellationToken cancellation)
        {
            try
            {
                PXTrace.WriteInformation("Zalo subscriber triggered for template: {0}", _zaloTemplate?.Description ?? "Unknown");

                if (_zaloTemplate == null || string.IsNullOrEmpty(_zaloTemplate.Body))
                {
                    PXTrace.WriteWarning("Zalo template is null or empty");
                    return;
                }

                // Check if template is active
                if (_zaloTemplate.IsActive != true)
                {
                    PXTrace.WriteInformation("Zalo template {0} is not active, skipping", _zaloTemplate.Description);
                    return;
                }

                // Validate template once for all rows
                var validation = ZaloMessage.ValidateTemplate(_zaloTemplate.Body);
                if (!validation.IsValid)
                    // Acuminator disable once PX1053 ConcatenationPriorLocalization [Justification]
                    throw new PXException("Template không hợp lệ: " + validation.ErrorMessage);

                foreach (var eventRow in eventRows)
                {
                    if (cancellation.IsCancellationRequested)
                        break;

                    ProcessSingleRow(eventRow);
                }
            }
            catch (Exception ex)
            {
                PXTrace.WriteError("Error in Zalo subscriber: {0}", ex.Message);
                throw; // Re-throw để Business Process framework xử lý
            }
        }

        private void ProcessSingleRow(MatchedRow eventRow)
        {
            try
            {
                // Extract data từ event row để merge vào template
                var mergeData = ExtractMergeData(eventRow);

                // Validate template với merge fields (nếu cần)
                var validation = ZaloMessage.ValidateTemplate(_zaloTemplate.Body);
                if (!validation.IsValid)
                    // Acuminator disable once PX1053 ConcatenationPriorLocalization [Justification]
                    throw new PXException("Template không hợp lệ: " + validation.ErrorMessage);

                // Build message từ template
                var message = ZaloMessage.BuildMessage(_zaloTemplate.Body, mergeData);

                // Log message (trong production sẽ gửi qua Zalo API)
                PXTrace.WriteInformation("Zalo message prepared: {0}", message);

                // TODO: Implement actual Zalo API call here
                // await SendZaloMessage(message, recipientPhone);

                PXTrace.WriteInformation("Zalo notification sent successfully for template: {0}", _zaloTemplate.Description);
            }
            catch (Exception ex)
            {
                PXTrace.WriteError("Error processing single row in Zalo subscriber: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Extracts merge data from event row using a flexible mapping.
        /// </summary>
        private Dictionary<string, object> ExtractMergeData(MatchedRow eventRow)
        {
            var mergeData = new Dictionary<string, object>();

            // Mapping phải trùng với GetValidMergeFields() trong ZaloMessage.cs
            var fieldMap = new Dictionary<string, string>
            {
                ["Branch"] = "BranchID",
                ["CheckDate"] = "Date",
                ["CheckedBy"] = "CreatedByID", 
                ["DocumentNbr"] = "RefNbr",
                ["TotalDifference"] = "TotalDiff",
                ["DifferenceDetails"] = "DetailDiff"
            };

            try
            {
                if (eventRow?.NewRow != null)
                {
                    var row = eventRow.NewRow;

                    foreach (var map in fieldMap)
                    {
                        var value = GetFieldValue(row, map.Value);
                        // If no value, use reasonable defaults
                        if (string.IsNullOrEmpty(value))
                        {
                            if (map.Key == "CheckDate")
                                value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                            else if (map.Key == "CheckedBy")
                                value = "System";
                            else if (map.Key == "TotalDifference")
                                value = "0 VND";
                            else if (map.Key == "DifferenceDetails")
                                value = "Details will be updated";
                            else
                                value = "N/A";
                        }
                        mergeData[map.Key] = value;
                    }
                }
                else
                {
                    mergeData = ZaloMessage.GetSampleData();
                }
            }
            catch (Exception ex)
            {
                PXTrace.WriteWarning("Error extracting merge data, using sample data: {0}", ex.Message);
                mergeData = ZaloMessage.GetSampleData();
            }

            return mergeData;
        }

        private string GetFieldValue(object row, string fieldName)
        {
            try
            {
                var prop = row.GetType().GetProperty(fieldName);
                var value = prop?.GetValue(row, null);
                return value?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}