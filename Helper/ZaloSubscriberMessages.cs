using PX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANCafe
{
    [PXLocalizable]
    public static class LocalizableMessages
    {
        public const string ZaloNotification = "Zalo Notification";
        public const string CreateZaloNotification = "Zalo Notification";

        public const string PreviewMessageTitle = "Preview Message";
        public const string ErrorTitle = "Error";
        public const string PreviewError = "Error creating preview: {0}";
        public const string WarningTitle = "Warning";
        public const string EnterTemplateFirst = "Please enter template before preview";
        public const string AvailableMergeFieldsTitle = "Available Merge Fields";
        public const string AvailableMergeFields = "Available merge fields:\n{0}\n\nPlease copy and paste into template.";
        public const string ValidationResultTitle = "Validation Result";
        public const string TemplateValid = "Template is valid!";
        public const string ValidationErrorTitle = "Validation Error";
        public const string TemplateInvalid = "Template invalid: {0}";
        public const string EnterTemplateToValidate = "Please enter template before validation";
        public const string TemplateCodeExists = "Template Code already exists";
        public const string TemplateCodeTooLong = "Template Code cannot exceed 30 characters";
        public const string TemplateCodeNoSpaces = "Template Code cannot contain spaces";
        public const string ZaloTemplateNotFound = "Zalo template not found for ID: {0}";
        public const string TemplateNotFoundForRedirect = "Template not found for redirect: {0}";
        public const string TemplateNotFound = "Template not found";
        public const string TemplateInvalidCannotPreview = "Template invalid: {0}. Cannot create preview.";
        // Template validation
        public const string TemplateEmpty = "Template cannot be empty";
        public const string TemplateTooLong = "Template is too long (maximum 4000 characters)";
        public const string InvalidMergeField = "Invalid merge field: {0}";

        // Data validation
        public const string ReferenceNbrEmpty = "Reference number cannot be empty";
        public const string PIReviewNotFound = "Physical Inventory Review {0} not found";
        public const string WarehouseNotFound = "Warehouse not found for SiteID: {0}";

        // Default text
        public const string UnknownWarehouse = "Unknown Warehouse";
        public const string NoDifferencesFound = "No differences found";
        public const string UnitCostFormat = "Unit Cost: {0} VND";

        // Log messages
        public const string BuildMessageCalled = "BuildMessage called with template: {0}";
        public const string TemplateNullEmpty = "Template is null or empty";
        public const string MergeDataNull = "MergeData is null, using sample data";
        public const string ReplacingField = "Replacing {0} with {1}";
        public const string GettingPIData = "Getting data for Physical Inventory Review: {0}";
        public const string MergeDataCreated = "Merge data created successfully";
        public const string BuildPreviewCalled = "BuildPreviewMessage called with referenceNbr: {0}";

        public const string NoDataFound = "No data found for the selected reference number.";
        public const string NoEventRowData = "No event row data available";
        public const string NoReferenceNumber = "Reference number not found in event data";
        public const string ErrorExtractingData = "Error extracting data: {0}";

    }
}

