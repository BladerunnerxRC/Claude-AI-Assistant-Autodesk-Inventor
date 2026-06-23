using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ClaudeForInventor
{
    internal static class ConversationStore
    {
        private static string StorePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClaudeForInventor", "conversations");

        public static void Save(string name, IList<ChatMessage> messages)
        {
            Directory.CreateDirectory(StorePath);
            var saved = new List<SavedMessage>();
            foreach (var m in messages)
                saved.Add(new SavedMessage { Role = m.Role, Content = m.Content });

            var entry = new SavedConversation { Name = name, Saved = DateTime.Now, Messages = saved };
            var safe  = SanitizeName(name);
            var file  = Path.Combine(StorePath, $"{entry.Saved:yyyyMMdd_HHmmss}_{safe}.json");
            File.WriteAllText(file, JsonConvert.SerializeObject(entry, Formatting.Indented), Encoding.UTF8);
        }

        public static List<SavedConversation> ListAll()
        {
            var result = new List<SavedConversation>();
            if (!Directory.Exists(StorePath)) return result;
            foreach (var file in Directory.GetFiles(StorePath, "*.json"))
            {
                try
                {
                    var entry = JsonConvert.DeserializeObject<SavedConversation>(File.ReadAllText(file));
                    if (entry != null) { entry.FilePath = file; result.Add(entry); }
                }
                catch { }
            }
            result.Sort((a, b) => b.Saved.CompareTo(a.Saved));
            return result;
        }

        public static SavedConversation? LoadFrom(string filePath)
        {
            try { return JsonConvert.DeserializeObject<SavedConversation>(File.ReadAllText(filePath)); }
            catch { return null; }
        }

        public static void Delete(string filePath)
        {
            try { if (File.Exists(filePath)) File.Delete(filePath); } catch { }
        }

        private static string SanitizeName(string name)
        {
            var sb = new StringBuilder();
            foreach (char c in name)
                sb.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_');
            return sb.Length > 30 ? sb.ToString().Substring(0, 30) : sb.ToString();
        }
    }

    internal class SavedConversation
    {
        public string            Name     { get; set; } = "";
        public DateTime          Saved    { get; set; }
        public List<SavedMessage> Messages { get; set; } = new List<SavedMessage>();
        [JsonIgnore] public string? FilePath { get; set; }
        public override string   ToString() => $"{Saved:yyyy-MM-dd HH:mm}  —  {Name}";
    }

    internal class SavedMessage
    {
        public string Role    { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
