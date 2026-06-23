using NUnit.Framework;
using ClaudeForInventor;
using System.Collections.Generic;

namespace ClaudeForInventor.Tests
{
    [TestFixture]
    public class ParamBlockParserTests
    {
        // ── Basic parsing ─────────────────────────────────────────────────────

        [Test]
        public void Parse_SingleParam_ReturnsNameAndExpression()
        {
            var result = ParamBlockParser.Parse("Width = 50 mm");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name,       Is.EqualTo("Width"));
            Assert.That(result[0].Expression, Is.EqualTo("50 mm"));
        }

        [Test]
        public void Parse_MultipleParams_ReturnsAll()
        {
            var block  = "Width = 50 mm\nHeight = 100 mm\nDepth = 25 mm";
            var result = ParamBlockParser.Parse(block);

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Name, Is.EqualTo("Width"));
            Assert.That(result[1].Name, Is.EqualTo("Height"));
            Assert.That(result[2].Name, Is.EqualTo("Depth"));
        }

        [Test]
        public void Parse_ExpressionWithEquals_Captures_FullRightHandSide()
        {
            // Expression like "Width / 2" should be captured even if it contained no '='
            var result = ParamBlockParser.Parse("Radius = Width / 2");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Expression, Is.EqualTo("Width / 2"));
        }

        // ── Comment and whitespace handling ───────────────────────────────────

        [Test]
        public void Parse_SlashComments_AreSkipped()
        {
            var block  = "// set the width\nWidth = 50 mm";
            var result = ParamBlockParser.Parse(block);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Width"));
        }

        [Test]
        public void Parse_HashComments_AreSkipped()
        {
            var block  = "# set the width\nWidth = 50 mm";
            var result = ParamBlockParser.Parse(block);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void Parse_EmptyLines_AreIgnored()
        {
            var block  = "\n\nWidth = 50 mm\n\n";
            var result = ParamBlockParser.Parse(block);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void Parse_WindowsLineEndings_Handled()
        {
            var block  = "Width = 50 mm\r\nHeight = 100 mm";
            var result = ParamBlockParser.Parse(block);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void Parse_EmptyBlock_ReturnsEmpty()
        {
            var result = ParamBlockParser.Parse("");
            Assert.That(result, Is.Empty);
        }

        // ── Feature suppression entries (PR #8) ───────────────────────────────

        [Test]
        public void Parse_SuppressionEntry_IsIncludedAsPair()
        {
            var result = ParamBlockParser.Parse("Fillet_R2.Suppressed = True");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name,       Is.EqualTo("Fillet_R2.Suppressed"));
            Assert.That(result[0].Expression, Is.EqualTo("True"));
        }

        [Test]
        public void Parse_MixedParamsAndSuppression_ReturnsAll()
        {
            var block = "Width = 80 mm\nFillet_R2.Suppressed = True\nHeight = 50 mm";
            var result = ParamBlockParser.Parse(block);

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void IsSuppressionEntry_DotSuppressedSuffix_ReturnsTrue()
        {
            Assert.That(ParamBlockParser.IsSuppressionEntry("Fillet_R2.Suppressed"), Is.True);
        }

        [Test]
        public void IsSuppressionEntry_CaseInsensitive()
        {
            Assert.That(ParamBlockParser.IsSuppressionEntry("Boss.suppressed"), Is.True);
            Assert.That(ParamBlockParser.IsSuppressionEntry("Boss.SUPPRESSED"), Is.True);
        }

        [Test]
        public void IsSuppressionEntry_RegularParam_ReturnsFalse()
        {
            Assert.That(ParamBlockParser.IsSuppressionEntry("Width"), Is.False);
        }

        [Test]
        public void FeatureNameFromSuppression_StripsCorrectSuffix()
        {
            var name = ParamBlockParser.FeatureNameFromSuppression("Fillet_R2.Suppressed");
            Assert.That(name, Is.EqualTo("Fillet_R2"));
        }

        // ── Param diff view (PR #3) — parsing side ────────────────────────────

        [Test]
        public void Parse_ExpressionWithSpaces_Trimmed()
        {
            var result = ParamBlockParser.Parse("  Width  =  50 mm  ");

            Assert.That(result[0].Name,       Is.EqualTo("Width"));
            Assert.That(result[0].Expression, Is.EqualTo("50 mm"));
        }
    }
}
