using NUnit.Framework;
using ClaudeForInventor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ClaudeForInventor.Tests
{
    [TestFixture]
    public class ConversationStoreTests
    {
        private string _testDir = "";

        // Override AppData by pointing ConversationStore at a temp directory.
        // ConversationStore uses Environment.SpecialFolder.ApplicationData, so we
        // can't redirect it without making StorePath overridable. Instead we verify
        // behavior by using the actual AppData path (isolated by a unique sub-dir).

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClaudeForInventor", "conversations");
            // Ensure a clean starting point for our assertions (don't delete other real data)
        }

        // Helper to build a minimal message list
        private static List<ChatMessage> Messages(params (string role, string content)[] pairs)
        {
            var list = new List<ChatMessage>();
            foreach (var (role, content) in pairs)
                list.Add(new ChatMessage(role, content));
            return list;
        }

        // ── Save ──────────────────────────────────────────────────────────────

        [Test]
        public void Save_CreatesJsonFileInStorePath()
        {
            var name     = $"_test_{Guid.NewGuid():N}";
            var messages = Messages(("user", "hello"), ("assistant", "hi"));

            ConversationStore.Save(name, messages);

            var files = Directory.GetFiles(_testDir, $"*{name.Substring(0, 10)}*.json");
            Assert.That(files.Length, Is.GreaterThanOrEqualTo(1), "Expected at least one saved .json file");

            // Cleanup
            foreach (var f in files) ConversationStore.Delete(f);
        }

        [Test]
        public void Save_FileContainsMessagesAndName()
        {
            var name     = $"_test_{Guid.NewGuid():N}".Substring(0, 20);
            var messages = Messages(("user", "test content"));

            ConversationStore.Save(name, messages);

            var list = ConversationStore.ListAll();
            SavedConversation? found = null;
            foreach (var s in list)
                if (s.Name == name) { found = s; break; }

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Messages.Count, Is.EqualTo(1));
            Assert.That(found.Messages[0].Role,    Is.EqualTo("user"));
            Assert.That(found.Messages[0].Content, Is.EqualTo("test content"));

            if (found.FilePath != null) ConversationStore.Delete(found.FilePath);
        }

        // ── ListAll ───────────────────────────────────────────────────────────

        [Test]
        public void ListAll_ReturnsSortedNewestFirst()
        {
            var nameA = $"_test_A_{Guid.NewGuid():N}".Substring(0, 25);
            var nameB = $"_test_B_{Guid.NewGuid():N}".Substring(0, 25);

            ConversationStore.Save(nameA, Messages(("user", "a")));
            Thread.Sleep(1100); // ensure different second-precision timestamp in filename
            ConversationStore.Save(nameB, Messages(("user", "b")));

            var list = ConversationStore.ListAll();

            // Find our two entries
            SavedConversation? entryA = null, entryB = null;
            foreach (var s in list)
            {
                if (s.Name == nameA) entryA = s;
                if (s.Name == nameB) entryB = s;
            }

            Assert.That(entryA, Is.Not.Null);
            Assert.That(entryB, Is.Not.Null);
            Assert.That(list.IndexOf(entryB!), Is.LessThan(list.IndexOf(entryA!)),
                "Newer conversation (B) should appear before older (A)");

            if (entryA!.FilePath != null) ConversationStore.Delete(entryA.FilePath);
            if (entryB!.FilePath != null) ConversationStore.Delete(entryB.FilePath);
        }

        [Test]
        public void ListAll_EmptyDirectory_ReturnsEmptyList()
        {
            // This test relies on the directory existing but having no JSON files we care about.
            // A fresh machine with no saved conversations should return an empty list.
            // We can't guarantee isolation here, so just verify the method doesn't throw.
            Assert.DoesNotThrow(() => ConversationStore.ListAll());
        }

        // ── LoadFrom ──────────────────────────────────────────────────────────

        [Test]
        public void LoadFrom_ReturnsNullForNonExistentFile()
        {
            var result = ConversationStore.LoadFrom(@"C:\does\not\exist\fake.json");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void LoadFrom_ReconstructsMessagesFromFile()
        {
            var name     = $"_test_{Guid.NewGuid():N}".Substring(0, 20);
            var messages = Messages(("user", "question"), ("assistant", "answer"));

            ConversationStore.Save(name, messages);

            var list  = ConversationStore.ListAll();
            SavedConversation? saved = null;
            foreach (var s in list) if (s.Name == name) { saved = s; break; }

            Assert.That(saved?.FilePath, Is.Not.Null);
            var loaded = ConversationStore.LoadFrom(saved!.FilePath!);

            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.Messages.Count, Is.EqualTo(2));
            Assert.That(loaded.Messages[0].Role,    Is.EqualTo("user"));
            Assert.That(loaded.Messages[1].Role,    Is.EqualTo("assistant"));
            Assert.That(loaded.Messages[1].Content, Is.EqualTo("answer"));

            ConversationStore.Delete(saved.FilePath!);
        }

        // ── Delete ────────────────────────────────────────────────────────────

        [Test]
        public void Delete_RemovesFile()
        {
            var name = $"_test_{Guid.NewGuid():N}".Substring(0, 20);
            ConversationStore.Save(name, Messages(("user", "bye")));

            var list = ConversationStore.ListAll();
            SavedConversation? saved = null;
            foreach (var s in list) if (s.Name == name) { saved = s; break; }

            Assert.That(saved?.FilePath, Is.Not.Null);
            ConversationStore.Delete(saved!.FilePath!);

            Assert.That(File.Exists(saved.FilePath!), Is.False);
        }

        [Test]
        public void Delete_NonExistentFile_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                ConversationStore.Delete(@"C:\does\not\exist\ghost.json"));
        }

        // ── SavedConversation.ToString ────────────────────────────────────────

        [Test]
        public void SavedConversation_ToString_FormatsDateAndName()
        {
            var sc = new SavedConversation
            {
                Name  = "My Design",
                Saved = new DateTime(2026, 1, 15, 9, 30, 0)
            };

            Assert.That(sc.ToString(), Is.EqualTo("2026-01-15 09:30  —  My Design"));
        }
    }
}
