using System.Collections.Generic;

namespace ClaudeForInventor
{
    internal static class ParamBlockParser
    {
        /// <summary>
        /// Parses a params block into name/expression pairs.
        /// Supports both regular parameters ("Width = 50 mm") and
        /// feature suppression entries ("Fillet_R2.Suppressed = True").
        /// </summary>
        internal static List<(string Name, string Expression)> Parse(string block)
        {
            var result = new List<(string, string)>();
            foreach (var rawLine in block.Split('\n'))
            {
                var line = rawLine.Trim('\r', ' ', '\t');
                if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                    continue;

                int eq = line.IndexOf('=');
                if (eq <= 0) continue;

                var name = line.Substring(0, eq).Trim();
                var expr = line.Substring(eq + 1).Trim();
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(expr))
                    result.Add((name, expr));
            }
            return result;
        }

        internal static bool IsSuppressionEntry(string name) =>
            name.EndsWith(".Suppressed", System.StringComparison.OrdinalIgnoreCase);

        internal static string FeatureNameFromSuppression(string name) =>
            name.Substring(0, name.Length - ".Suppressed".Length);
    }
}
