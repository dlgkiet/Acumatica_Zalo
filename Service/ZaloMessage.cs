using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.SM;

namespace ANCafe
{
    public static class ZaloMessage
    {
        public static Dictionary<string, object> GetDataFromPhysicalInventoryReview(PXGraph graph, string referenceNbr)
        {
            PXTrace.WriteInformation($"Getting data for Physical Inventory Review: {referenceNbr}");

            if (string.IsNullOrEmpty(referenceNbr))
                throw new PXException(LocalizableMessages.ReferenceNbrEmpty);

            // Load Physical Inventory document by PIID
            var header = SelectFrom<INPIHeader>
                .Where<INPIHeader.pIID.IsEqual<@P.AsString>>
                .View.Select(graph, referenceNbr)
                .RowCast<INPIHeader>()
                .FirstOrDefault();

            if (header == null)
                throw new PXException(LocalizableMessages.PIReviewNotFound, referenceNbr);

            // Get warehouse info - modified to ensure site is found
            var site = SelectFrom<INSite>
                .Where<INSite.siteID.IsEqual<@P.AsInt>>
                .View.Select(graph, header.SiteID)
                .RowCast<INSite>()
                .FirstOrDefault();

            if (site == null)
                throw new PXException(LocalizableMessages.WarehouseNotFound, header.SiteID);

            // Ensure we have a valid warehouse description
            string warehouseDescr = site.Descr?.Trim();
            if (string.IsNullOrEmpty(warehouseDescr))
                warehouseDescr = site.SiteCD?.Trim(); // Fallback to SiteCD if Descr is empty

            // Load details with inventory items
            var details = SelectFrom<INPIDetail>
                .LeftJoin<InventoryItem>.On<INPIDetail.inventoryID.IsEqual<InventoryItem.inventoryID>>
                .Where<INPIDetail.pIID.IsEqual<@P.AsString>>
                .OrderBy<
                    Asc<INPIDetail.inventoryID>,
                    Asc<INPIDetail.subItemID>,
                    Asc<INPIDetail.locationID>>
                .View.Select(graph, header.PIID);

            // Build detail lines
            var detailLines = new List<string>();
            foreach (PXResult<INPIDetail, InventoryItem> row in details)
            {
                var detail = (INPIDetail)row;
                var item = (InventoryItem)row;

                if (detail?.VarQty == 0) continue;

                string qtyDisplay = detail.VarQty > 0
                    ? $"+{detail.VarQty:N2}"
                    : $"{detail.VarQty:N2}";

                string costDisplay = detail.UnitCost.HasValue
                    ? $"{detail.UnitCost.Value:N0}"
                    : "0";

                detailLines.Add(
                    $"{item.InventoryCD?.Trim()} - {item.Descr?.Trim()}: " +
                    $"{qtyDisplay} {item.BaseUnit} " +
                    $"(Unit Cost: {costDisplay} VND)"
                );
            }

            return new Dictionary<string, object>
            {
                ["Branch"] = warehouseDescr, // Use the validated warehouse description
                ["CheckDate"] = header.CountDate?.ToString("dd/MM/yyyy"),
                ["CheckedBy"] = Users.PK.Find(graph, header.CreatedByID)?.Username
                    ?? PXAccess.GetUserName(),
                ["DocumentNbr"] = header.PIID?.Trim(),
                ["TotalDifference"] = header.TotalVarCost.HasValue
                    ? $"{header.TotalVarCost.Value:N0} VND"
                    : "0 VND",
                ["DifferenceDetails"] = detailLines.Any()
                    ? string.Join("\n", detailLines)
                    : LocalizableMessages.NoDifferencesFound,
                ["Status"] = header.Status?.Trim(),
                ["TotalPhysicalQty"] = header.TotalPhysicalQty?.ToString("N2") ?? "0.00",
                ["TotalVarQty"] = header.TotalVarQty?.ToString("N2") ?? "0.00"
            };
        }

        public static ValidationResult ValidateTemplate(string template)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(template))
            {
                result.IsValid = false;
                result.ErrorMessage = LocalizableMessages.TemplateEmpty;
                return result;
            }

            var mergeFieldPattern = @"\{\{([^}]+)\}\}";
            var matches = Regex.Matches(template, mergeFieldPattern);
            var validFields = GetValidMergeFields();

            foreach (Match match in matches)
            {
                string fieldName = match.Groups[1].Value.Trim();
                if (!validFields.Contains(fieldName))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Invalid merge field: {{{{{fieldName}}}}}";
                    return result;
                }
            }

            if (template.Length > 4000)
            {
                result.IsValid = false;
                result.ErrorMessage = LocalizableMessages.TemplateTooLong;
                return result;
            }

            return result;
        }

        public static HashSet<string> GetValidMergeFields()
        {
            var fields = new HashSet<string>
            {
                "Branch", "CheckDate", "CheckedBy", "DocumentNbr",
                "TotalDifference", "DifferenceDetails",
                "Status", "TotalPhysicalQty", "TotalVarQty"
            };

            // Thêm tất cả property của ZaloTemplate trừ NoteID
            var templateProps = typeof(ZaloTemplate).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !string.Equals(p.Name, "NoteID", StringComparison.OrdinalIgnoreCase));
            foreach (var prop in templateProps)
                fields.Add(prop.Name);

            // Thêm các trường của INPIHeader 
            var headerProps = typeof(INPIHeader).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !string.Equals(p.Name, "NoteID", StringComparison.OrdinalIgnoreCase));
            foreach (var prop in headerProps)
                fields.Add(prop.Name);

            return fields;
        }

        public static string BuildMessage(string template, Dictionary<string, object> mergeData)
        {
            PXTrace.WriteInformation(LocalizableMessages.BuildMessageCalled, template);

            if (string.IsNullOrEmpty(template))
            {
                PXTrace.WriteWarning(LocalizableMessages.TemplateNullEmpty);
                return template;
            }

            if (mergeData == null)
            {
                PXTrace.WriteWarning(LocalizableMessages.MergeDataNull);
                throw new PXException(LocalizableMessages.NoDataFound);
            }

            string result = template;
            foreach (var kvp in mergeData)
            {
                string mergeField = $"{{{{{kvp.Key}}}}}";
                string value = kvp.Value?.ToString() ?? string.Empty;
                result = result.Replace(mergeField, value);
                PXTrace.WriteInformation(LocalizableMessages.ReplacingField, mergeField, value);
            }

            return result;
        }

        public static string BuildPreviewMessage(string template, PXGraph graph, string referenceNbr)
        {
            PXTrace.WriteInformation($"BuildPreviewMessage called with referenceNbr: {referenceNbr}");
            var data = GetDataFromPhysicalInventoryReview(graph, referenceNbr);
            return BuildMessage(template, data);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}