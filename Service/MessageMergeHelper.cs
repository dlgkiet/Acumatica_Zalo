using PX.Data;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AnNhienCafe
{
    /// <summary>
    /// Helper class để xử lý merge fields trong message template
    /// </summary>
    public static class MessageMergeHelper
    {
        /// <summary>
        /// Merge template với data thực tế
        /// </summary>
        /// <param name="template">Template string có chứa merge fields {{FieldName}}</param>
        /// <param name="mergeData">Dictionary chứa data để merge</param>
        /// <returns>String đã được merge</returns>
        public static string MergeTemplate(string template, Dictionary<string, object> mergeData)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            if (mergeData == null || mergeData.Count == 0)
                return template;

            string result = template;

            // Regex để tìm các merge fields có format {{FieldName}}
            var regex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.IgnoreCase);
            var matches = regex.Matches(template);

            foreach (Match match in matches)
            {
                string fieldName = match.Groups[1].Value.Trim();
                string placeholder = match.Value; // {{FieldName}}

                if (mergeData.ContainsKey(fieldName))
                {
                    object value = mergeData[fieldName];
                    string replacementValue = ConvertValueToString(value);
                    result = result.Replace(placeholder, replacementValue);
                }
                else
                {
                    // Nếu không tìm thấy data, có thể giữ nguyên hoặc thay bằng empty
                    // Ở đây ta giữ nguyên để user biết field nào chưa có data
                    PXTrace.WriteWarning($"⚠️ Merge field '{fieldName}' không tìm thấy data");
                }
            }

            return result;
        }

        /// <summary>
        /// Convert object value thành string để thay thế trong template
        /// </summary>
        private static string ConvertValueToString(object value)
        {
            if (value == null)
                return string.Empty;

            if (value is DateTime dateValue)
            {
                return dateValue.ToString("dd/MM/yyyy HH:mm");
            }

            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("#,##0.00");
            }

            if (value is double doubleValue)
            {
                return doubleValue.ToString("#,##0.00");
            }

            if (value is int intValue)
            {
                return intValue.ToString("#,##0");
            }

            return value.ToString();
        }

        /// <summary>
        /// Validate template - kiểm tra syntax của merge fields
        /// </summary>
        public static List<string> ValidateTemplate(string template)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(template))
                return errors;

            // Kiểm tra các merge field có format đúng không
            var regex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.IgnoreCase);
            var matches = regex.Matches(template);

            foreach (Match match in matches)
            {
                string fieldName = match.Groups[1].Value.Trim();

                // Kiểm tra tên field có hợp lệ không (chỉ chứa chữ, số, underscore)
                if (!Regex.IsMatch(fieldName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                {
                    errors.Add($"Invalid merge field name: '{fieldName}'. Only letters, numbers and underscore allowed.");
                }
            }

            // Kiểm tra có merge field nào không đóng đúng format không
            var invalidFields = Regex.Matches(template, @"\{[^}]*$|\{[^}]*\{|\}[^{]*\}");
            foreach (Match invalidMatch in invalidFields)
            {
                errors.Add($"Invalid merge field syntax: '{invalidMatch.Value}'");
            }

            return errors;
        }

        /// <summary>
        /// Lấy danh sách các merge fields có trong template
        /// </summary>
        public static List<string> GetMergeFields(string template)
        {
            var fields = new List<string>();

            if (string.IsNullOrEmpty(template))
                return fields;

            var regex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.IgnoreCase);
            var matches = regex.Matches(template);

            foreach (Match match in matches)
            {
                string fieldName = match.Groups[1].Value.Trim();
                if (!fields.Contains(fieldName))
                {
                    fields.Add(fieldName);
                }
            }

            return fields;
        }
    }
}