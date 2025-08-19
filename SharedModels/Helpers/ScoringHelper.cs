using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharedModels.Helpers
{
    public static class ScoringHelper
    {
        // Gom về 1 khoảng trắng, bỏ prefix "A.", "B)", "1.", "1)", ...
        private static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            // thay NBSP, full-width space bằng space thường
            s = s.Replace('\u00A0', ' ').Replace('\u3000', ' ').Trim();

            // bỏ prefix dạng "A. ", "B) ", "1. ", "2) "
            s = Regex.Replace(s, @"^\s*([A-Za-z]|\d+)[\.\)]\s*", "", RegexOptions.CultureInvariant);

            // gom nhiều khoảng trắng thành một
            s = Regex.Replace(s, @"\s+", " ");

            return s.ToUpperInvariant();
        }

        private static List<string> NormalizeChoices(List<string> choices)
        {
            return choices
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(Normalize)
                .ToList();
        }

        /// <summary>
        /// Convert selected string ("1", "A", "A.", "B) ...", or text) -> index
        /// </summary>
        public static int? ParseAnswerToIndex(string? selected, List<string> choices)
        {
            if (choices == null || choices.Count == 0) return null;
            if (string.IsNullOrWhiteSpace(selected)) return null;

            var normalizedChoices = NormalizeChoices(choices);

            // 1) Nếu là số
            if (int.TryParse(selected.Trim(), out int num))
            {
                // chấp nhận 0-based
                if (num >= 0 && num < normalizedChoices.Count) return num;
                // chấp nhận 1-based
                if (num >= 1 && num <= normalizedChoices.Count) return num - 1;
                return null;
            }

            // 2) Nếu là chữ cái (A/B/...)
            char c = char.ToUpperInvariant(selected.Trim()[0]);
            if (c >= 'A' && c <= 'Z')
            {
                int idx = c - 'A';
                if (idx >= 0 && idx < normalizedChoices.Count) return idx;
            }

            // 3) So theo text
            var normSel = Normalize(selected);
            int textIdx = normalizedChoices.FindIndex(ch => ch == normSel);
            return textIdx >= 0 ? textIdx : null;
        }

        /// <summary>
        /// Convert correct answer (int/string like "A. ...", "2)", or text) -> index
        /// </summary>
        public static int? NormalizeCorrectAnswerToIndex(string? correctAnswer, List<string> choices)
        {
            if (choices == null || choices.Count == 0) return null;
            if (string.IsNullOrWhiteSpace(correctAnswer)) return null;

            var normalizedChoices = NormalizeChoices(choices);
            var raw = correctAnswer.Trim();

            // 1) Nếu là số
            if (int.TryParse(raw, out int num))
            {
                if (num >= 0 && num < normalizedChoices.Count) return num;       // 0-based
                if (num >= 1 && num <= normalizedChoices.Count) return num - 1;  // 1-based
                return null;
            }

            // 2) Nếu là chữ cái
            char c = char.ToUpperInvariant(raw[0]);
            if (c >= 'A' && c <= 'Z')
            {
                int idx = c - 'A';
                if (idx >= 0 && idx < normalizedChoices.Count) return idx;
            }

            // 3) So theo text
            var norm = Normalize(correctAnswer);
            int textIdx = normalizedChoices.FindIndex(ch => ch == norm);
            return textIdx >= 0 ? textIdx : (int?)null;
        }
    }
}
