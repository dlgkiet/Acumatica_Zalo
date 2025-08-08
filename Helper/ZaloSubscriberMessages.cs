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
    }
}

