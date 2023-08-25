using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Reactive.Bindings;
using System.Reflection;
using System.Xml;
using System;
using ICSharpCode.AvalonEdit.Document;

namespace Wada.NcProgramConcatenationForHoleDrilling.Models
{
    internal record class PreviewPageModel
    {
        public PreviewPageModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var xshd = assembly.GetManifestResourceStream("Wada.NcProgramConcatenationForHoleDrilling.ViewModels.NcHighlighting.xshd")
            ?? throw new InvalidOperationException("Could not find embedded resource");

            using XmlReader reader = new XmlTextReader(xshd);

            NcHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        internal ReactivePropertySlim<TextDocument> CombinedProgramSource { get; } = new();
        public IHighlightingDefinition NcHighlighting { get; }
    }
}
