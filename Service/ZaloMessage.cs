using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ANCafe
{
    public static class ZaloMessage
    {
        #region Static Methods
        /// <summary>
        /// Build message from template and merge data
        /// </summary>
        public static string BuildMessage(string template, Dictionary<string, object> mergeData)
        {
            if (string.IsNullOrEmpty(template) || mergeData == null)
                return template;

            string result = template;

            foreach (var kvp in mergeData)
            {
                string mergeField = $"{{{{{kvp.Key}}}}}";
                string value = kvp.Value?.ToString() ?? string.Empty;
                result = result.Replace(mergeField, value);
            }

            return result;
        }

        /// <summary>
        /// Create preview message with sample data
        /// </summary>
        public static string BuildPreviewMessage(string template)
        {
            var sampleData = GetSampleData();
            return BuildMessage(template, sampleData);
        }

        /// <summary>
        /// Validate template syntax and merge fields
        /// </summary>
        public static ValidationResult ValidateTemplate(string template)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(template))
            {
                result.IsValid = false;
                result.ErrorMessage = "Template cannot be empty";
                return result;
            }

            // Check valid merge field syntax
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

            // Check template length
            if (template.Length > 4000)
            {
                result.IsValid = false;
                result.ErrorMessage = "Template is too long (maximum 4000 characters)";
                return result;
            }

            return result;
        }

        /// <summary>
        /// Get list of available merge fields
        /// </summary>
        public static HashSet<string> GetValidMergeFields()
        {
            return new HashSet<string>
            {
                "Branch", "CheckDate", "CheckedBy",
                "DocumentNbr", "TotalDifference", "DifferenceDetails"
            };
        }

        /// <summary>
        /// Get sample data for preview
        /// </summary>
        public static Dictionary<string, object> GetSampleData()
        {
            return new Dictionary<string, object>
            {
                ["Branch"] = "Branch District 1 - HCMC",
                ["CheckDate"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                ["CheckedBy"] = "John Smith",
                ["DocumentNbr"] = "PI-2024-001",
                ["TotalDifference"] = FormatCurrency(1500000),
                ["DifferenceDetails"] = BuildSampleDetail()
            };
        }

        /// <summary>
        /// Format currency amount
        /// </summary>
        private static string FormatCurrency(decimal amount)
        {
            return amount.ToString("#,##0") + " VND";
        }

        /// <summary>
        /// Create sample detail string
        /// </summary>
        private static string BuildSampleDetail()
        {
            return @"SURPLUS ITEMS:
• Laptop Dell XPS 13: +2 pcs (40,000,000 VND)
• Mouse Logitech MX: +5 pcs (2,500,000 VND)

SHORTAGE ITEMS:
• Monitor Samsung 27"": -1 pc (-8,000,000 VND)
• Mechanical Keyboard: -3 pcs (-6,000,000 VND)

Total Difference: +1,500,000 VND";
        }

        /// <summary>
        /// Get real data from Generic Inquiry (to be implemented later)
        /// </summary>
        public static Dictionary<string, object> GetDataFromGenericInquiry(string inquiryName, Dictionary<string, object> parameters = null)
        {
            // TODO: Implement GI integration
            // Currently return sample data
            return GetSampleData();
        }
        #endregion
    }

    /// <summary>
    /// Validation result class
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}