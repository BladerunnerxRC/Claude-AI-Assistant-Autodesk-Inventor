using System;
using System.Text;
using Inventor;

namespace ClaudeForInventor
{
    /// <summary>
    /// Reads the active Inventor document and produces a structured context string
    /// that is injected into Claude's system prompt on every API call.
    /// </summary>
    internal static class InventorContextBuilder
    {
        private const int MaxFeatures    = 15;
        private const int MaxOccurrences = 20;

        // ──────────────────────────────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────────────────────────────

        public static string Build(Inventor.Application? app)
        {
            if (app?.ActiveDocument == null)
                return "No document is currently open in Inventor.";

            var doc = app.ActiveDocument;
            var sb  = new StringBuilder(512);

            sb.AppendLine($"Document : {doc.DisplayName}");
            sb.AppendLine($"Type     : {DocTypeName(doc.DocumentType)}");
            sb.AppendLine($"Path     : {doc.FullFileName}");

            try
            {
                switch (doc.DocumentType)
                {
                    case DocumentTypeEnum.kPartDocumentObject:
                        AppendPart(sb, (PartDocument)doc);
                        break;
                    case DocumentTypeEnum.kAssemblyDocumentObject:
                        AppendAssembly(sb, (AssemblyDocument)doc);
                        break;
                    case DocumentTypeEnum.kDrawingDocumentObject:
                        AppendDrawing(sb, (DrawingDocument)doc);
                        break;
                    default:
                        sb.AppendLine("(No additional context for this document type.)");
                        break;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[Partial context — extraction error: {ex.Message}]");
            }

            return sb.ToString();
        }

        /// <summary>Short one-liner for the context panel header.</summary>
        public static string SummaryLine(Inventor.Application? app)
        {
            if (app?.ActiveDocument == null) return "No document open";
            var doc = app.ActiveDocument;
            return $"{doc.DisplayName}  [{DocTypeName(doc.DocumentType)}]";
        }

        // ──────────────────────────────────────────────────────────────────────
        // Part
        // ──────────────────────────────────────────────────────────────────────

        private static void AppendPart(StringBuilder sb, PartDocument part)
        {
            var def = part.ComponentDefinition;

            // ── User Parameters ──────────────────────────────────────────────
            var paramSb    = new StringBuilder();
            int paramCount = 0;
            foreach (Parameter p in def.Parameters)
            {
                if (p.ParameterType != ParameterTypeEnum.kUserParameter) continue;
                paramCount++;
                try { paramSb.AppendLine($"  {p.Name} = {p.Expression}  [{p.get_Units()}]"); }
                catch { /* skip individual unreadable param */ }
            }
            if (paramCount > 0)
            {
                sb.AppendLine($"\nUser Parameters ({paramCount}):");
                sb.Append(paramSb);
            }

            // ── Model Parameters (non-user, capped) ──────────────────────────
            var modelSb    = new StringBuilder();
            int modelCount = 0;
            foreach (Parameter p in def.Parameters)
            {
                if (p.ParameterType == ParameterTypeEnum.kUserParameter) continue;
                if (++modelCount > 10) break;
                try { modelSb.AppendLine($"  {p.Name} = {p.Expression}  [{p.get_Units()}]"); }
                catch { }
            }
            if (modelCount > 0)
            {
                sb.AppendLine($"\nModel Parameters (first {Math.Min(modelCount, 10)}):");
                sb.Append(modelSb);
            }

            // ── Features ─────────────────────────────────────────────────────
            var feats = def.Features;
            sb.AppendLine($"\nFeatures ({feats.Count}):");
            int shown = 0;
            foreach (PartFeature f in feats)
            {
                if (++shown > MaxFeatures) { sb.AppendLine("  … (truncated)"); break; }
                sb.AppendLine($"  {f.Name}  [suppressed: {f.Suppressed}]");
            }

            // ── Active Sketch (if any) ────────────────────────────────────────
            try
            {
                var editObj = StandardAddInServer.InventorApp?.ActiveEditObject;
                if (editObj is PlanarSketch sketch)
                    sb.AppendLine($"\nActive Sketch: {sketch.Name}  " +
                        $"({sketch.SketchLines.Count} lines, " +
                        $"{sketch.SketchCircles.Count} circles)");
            }
            catch { /* ActiveEditObject not always available */ }

            // ── Material ─────────────────────────────────────────────────────
            try { sb.AppendLine($"\nMaterial: {def.Material?.Name ?? "Unassigned"}"); }
            catch { }

            AppendIProperties(sb, part.PropertySets);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Assembly
        // ──────────────────────────────────────────────────────────────────────

        private static void AppendAssembly(StringBuilder sb, AssemblyDocument asm)
        {
            var def = asm.ComponentDefinition;

            // ── Top-level occurrences ─────────────────────────────────────────
            sb.AppendLine($"\nTop-level Occurrences ({def.Occurrences.Count}):");
            int shown = 0;
            foreach (ComponentOccurrence occ in def.Occurrences)
            {
                if (++shown > MaxOccurrences) { sb.AppendLine("  … (truncated)"); break; }
                var grounded = occ.Grounded ? " [grounded]" : "";
                sb.AppendLine($"  {occ.Name}{grounded}");
            }

            // ── Constraints ───────────────────────────────────────────────────
            sb.AppendLine($"\nAssembly Constraints: {def.Constraints.Count}");

            // ── User Parameters ───────────────────────────────────────────────
            int pCount = 0;
            foreach (Parameter p in def.Parameters)
                if (p.ParameterType == ParameterTypeEnum.kUserParameter) pCount++;
            if (pCount > 0) sb.AppendLine($"User Parameters: {pCount}");

            AppendIProperties(sb, asm.PropertySets);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Drawing
        // ──────────────────────────────────────────────────────────────────────

        private static void AppendDrawing(StringBuilder sb, DrawingDocument drw)
        {
            sb.AppendLine($"\nSheets ({drw.Sheets.Count}):");
            foreach (Sheet s in drw.Sheets)
            {
                sb.AppendLine($"  [{s.Name}]  {s.Width:F0}×{s.Height:F0} mm " +
                    $"| Views: {s.DrawingViews.Count}");
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // iProperties
        // ──────────────────────────────────────────────────────────────────────

        private static void AppendIProperties(StringBuilder sb, PropertySets sets)
        {
            try
            {
                var summ   = sets["Inventor Summary Information"];
                var design = sets["Design Tracking Properties"];

                var ip = new StringBuilder();
                SafeAppend(ip, "Title",       summ["Title"]?.Value       as string);
                SafeAppend(ip, "Part Number", design["Part Number"]?.Value as string);
                SafeAppend(ip, "Description", design["Description"]?.Value as string);
                SafeAppend(ip, "Revision",    design["Revision Number"]?.Value as string);
                SafeAppend(ip, "Author",      summ["Author"]?.Value       as string);

                if (ip.Length > 0) { sb.AppendLine("\niProperties:"); sb.Append(ip); }
            }
            catch { /* PropertySets absent in some doc configurations */ }
        }

        private static void SafeAppend(StringBuilder sb, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                sb.AppendLine($"  {label}: {value}");
        }

        // ──────────────────────────────────────────────────────────────────────

        private static string DocTypeName(DocumentTypeEnum t) => t switch
        {
            DocumentTypeEnum.kPartDocumentObject         => "Part (.ipt)",
            DocumentTypeEnum.kAssemblyDocumentObject     => "Assembly (.iam)",
            DocumentTypeEnum.kDrawingDocumentObject      => "Drawing (.idw/.dwg)",
            DocumentTypeEnum.kPresentationDocumentObject => "Presentation (.ipn)",
            _                                            => "Unknown"
        };
    }
}
