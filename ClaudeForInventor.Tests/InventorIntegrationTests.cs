using NUnit.Framework;

namespace ClaudeForInventor.Tests
{
    /// <summary>
    /// Stubs for features that require a live Autodesk Inventor process.
    /// Run with a real Inventor session via the in-process test runner once
    /// the COM add-in is loaded. All tests here are skipped in CI.
    /// </summary>
    [TestFixture]
    [Ignore("Requires a live Inventor 2027 process with the add-in loaded")]
    public class InventorIntegrationTests
    {
        // ── PR #6 — BOM extraction ────────────────────────────────────────────

        [Test]
        public void BuildBomText_ActiveAssembly_ReturnsPartNumbers()
        {
            // Arrange: open a known assembly in Inventor beforehand.
            // Act: call InventorContextBuilder.Build() and check the BOM section.
            // Assert: result contains "BOM" header and at least one part number.
            Assert.Ignore("Manual test — open an assembly in Inventor then run this.");
        }

        [Test]
        public void BuildBomText_NoAssemblyOpen_ReturnsEmptyOrOmitsBomSection()
        {
            Assert.Ignore("Manual test — verify BOM section absent when no assembly active.");
        }

        // ── PR #7 — Sub-assembly context ─────────────────────────────────────

        [Test]
        public void ContextBuilder_SubAssembly_AnnotatesComponentCount()
        {
            // Assert the context string for a nested assembly includes "[sub-asm: N components]".
            Assert.Ignore("Manual test — open an assembly with sub-assemblies in Inventor.");
        }

        [Test]
        public void ContextBuilder_Part_AnnotatesMaterial()
        {
            // Assert the context string for a part occurrence includes "[Material]" annotation.
            Assert.Ignore("Manual test — open an assembly whose parts have materials assigned.");
        }

        // ── PR #8 — Feature suppression via param block ───────────────────────

        [Test]
        public void ApplyParams_SuppressionEntry_SuppressesFeature()
        {
            // Arrange: model with an unsuppressed fillet named "Fillet_R2".
            // Act: send block "Fillet_R2.Suppressed = True" through the apply path.
            // Assert: Inventor feature is suppressed afterwards.
            Assert.Ignore("Manual test — requires part with a known feature name.");
        }

        [Test]
        public void ApplyParams_SuppressionFalse_UnsuppressesFeature()
        {
            Assert.Ignore("Manual test — requires part with a suppressed feature.");
        }

        // ── PR #3 — iLogic auto-retry ─────────────────────────────────────────

        [Test]
        public void RunILogicRule_FailsOnFirstCall_RetriesToClean()
        {
            // Simulate iLogic rule execution failing and verify the retry flag
            // is cleared after the follow-up SendMessage call.
            Assert.Ignore("Manual test — trigger an iLogic rule failure in the UI.");
        }
    }
}
