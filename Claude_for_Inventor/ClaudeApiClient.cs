using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClaudeForInventor
{
    internal class ClaudeApiClient
    {
        // Single static instance; HttpClient is thread-safe and must not be disposed per-call
        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(90)
        };

        private const string ApiUrl  = "https://api.anthropic.com/v1/messages";
        public  const string Model   = "claude-opus-4-8";
        public  const string Version = "1.1";

        private const string SystemTemplate =
@"You are an expert Autodesk Inventor CAD engineer embedded directly inside Inventor.
You have access to the user's live document context below.

CAPABILITIES
- Parametric modeling: sketches, extrusions, revolutions, sweeps, lofts, work features
- iLogic rule authoring (VB.NET) — always fence code with ```ilogic
- Assembly: joints, constraints, degrees-of-freedom analysis, iPart/iAssembly, BOM
- Drawing: view creation, annotations, balloon/BOM, title-block automation
- Parameter analysis and DFM (design-for-manufacturing) advice
- Inventor API / COM automation in C# or VB.NET

RULES
- Wrap ALL iLogic / VB code in ```ilogic … ``` fences so the UI can detect and copy it.
- Wrap Inventor API (C#) code in ```csharp … ```.
- When suggesting parameter changes, list them in a ```params block — one Name = expression per line — using the exact names from the live context. Example:
  ```params
  Width = 50 mm
  Depth = Width / 2
  ```
- Be concise. Skip unnecessary caveats. The user is an experienced CAD professional.
- Reference specific parameter names, feature names, and component names from the context when relevant.
- If the user's question requires current part geometry you cannot see, say so and ask.

LIVE INVENTOR CONTEXT
{context}";

        // ──────────────────────────────────────────────────────────────────────

        public async Task<string> SendAsync(
            string userMessage,
            IReadOnlyList<ChatMessage> history,
            string documentContext,
            Action<string>? onChunk = null)
        {
            var apiKey = SettingsManager.GetApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "API key not configured. Click ⚙ Settings and paste your Anthropic API key.");

            // Build messages list from history + new user turn
            var messages = new List<object>(history.Count + 1);
            foreach (var m in history)
                messages.Add(new { role = m.Role, content = m.Content });
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model      = Model,
                max_tokens = 4096,
                stream     = true,
                system     = SystemTemplate.Replace("{context}", documentContext),
                messages
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            req.Headers.Add("x-api-key", apiKey);
            req.Headers.Add("anthropic-version", "2023-06-01");
            req.Content = new StringContent(
                JsonConvert.SerializeObject(requestBody, Formatting.None),
                Encoding.UTF8, "application/json");

            var res = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

            if (!res.IsSuccessStatusCode)
            {
                var errBody = await res.Content.ReadAsStringAsync();
                string detail;
                try   { detail = JObject.Parse(errBody)["error"]?["message"]?.ToString() ?? errBody; }
                catch { detail = errBody; }
                throw new HttpRequestException($"Anthropic API {(int)res.StatusCode}: {detail}");
            }

            var fullText = new StringBuilder();
            using var stream = await res.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line == null || !line.StartsWith("data: ", StringComparison.Ordinal)) continue;

                var data = line.Substring(6);
                if (data == "[DONE]") break;

                try
                {
                    var ev = JObject.Parse(data);
                    if (ev["type"]?.ToString() == "content_block_delta")
                    {
                        var delta = ev["delta"];
                        if (delta?["type"]?.ToString() == "text_delta")
                        {
                            var text = delta["text"]?.ToString() ?? string.Empty;
                            if (text.Length > 0)
                            {
                                fullText.Append(text);
                                onChunk?.Invoke(text);
                            }
                        }
                    }
                }
                catch { /* skip malformed SSE lines */ }
            }

            return fullText.ToString();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Simple immutable message record (avoids C# 9 record on .NET 4.8 edge cases)
    // ──────────────────────────────────────────────────────────────────────────

    internal sealed class ChatMessage
    {
        public string Role    { get; }
        public string Content { get; }
        public ChatMessage(string role, string content) { Role = role; Content = content; }
    }
}
