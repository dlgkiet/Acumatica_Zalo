using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnNhienCafe
{
    public class ZaloMessageBuilder
    {
        /// <summary>
        /// Thay thế các {{FieldName}} trong template bằng dữ liệu từ object values.
        /// </summary>
        public static string BuildMessage(string template, object values)
        {
            if (string.IsNullOrWhiteSpace(template))
                return "";
            var dict = values.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(values)?.ToString() ?? "");

            return Regex.Replace(template, @"{{(\w+)}}", match =>
            {
                var key = match.Groups[1].Value;
                return dict.TryGetValue(key, out var val) ? val : match.Value;
            });
        }

        /// <summary>
        /// Tạo danh sách dòng lệch từ list đối tượng (chuyển thành string).
        /// </summary>
        public static string FormatChenhlechLines(IEnumerable<object> items)
        {
            return string.Join("\n", items.Select(item =>
            {
                var type = item.GetType();
                var tenSP = type.GetProperty("TenSP")?.GetValue(item)?.ToString() ?? "";
                var soSach = type.GetProperty("SoSach")?.GetValue(item)?.ToString() ?? "";
                var thucTe = type.GetProperty("ThucTe")?.GetValue(item)?.ToString() ?? "";
                var chenhLech = type.GetProperty("ChenhLech")?.GetValue(item)?.ToString() ?? "";
                var tienChenhLech = type.GetProperty("TienChenhLech")?.GetValue(item)?.ToString() ?? "";

                return $"- {tenSP}: Sổ sách: {soSach} – Thực tế: {thucTe} → Lệch: {chenhLech} ({tienChenhLech})";
            }));
        }

        /// <summary>
        /// Parse tiền từ "-25,000 đ" → -25000
        /// </summary>
        //private static int ParseTien(string tien)
        //{
        //    string cleaned = tien.Replace(",", "").Replace("đ", "").Replace(" ", "").Replace("₫", "");
        //    _ = int.TryParse(cleaned, out int result);
        //    return result;
        //}

        /// <summary>
        /// Format tiền dạng 10000 → "10,000 đ"
        /// </summary>
        public static string FormatTien(int amount)
        {
            return (amount < 0 ? "-" : "") + string.Format("{0:#,0} đ", Math.Abs(amount));
        }

    }
}