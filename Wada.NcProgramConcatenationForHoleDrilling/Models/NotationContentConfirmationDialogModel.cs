using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Reactive.Bindings;
using System.Reflection;
using System.Xml;
using System;

namespace Wada.NcProgramConcatenationForHoleDrilling.Models
{
    internal record class NotationContentConfirmationDialogModel
    {
        public NotationContentConfirmationDialogModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var xshd = assembly.GetManifestResourceStream("Wada.NcProgramConcatenationForHoleDrilling.ViewModels.NcHighlighting.xshd")
            ?? throw new InvalidOperationException("Could not find embedded resource");

            using XmlReader reader = new XmlTextReader(xshd);

            NcHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        internal ReactivePropertySlim<string?> OperationTypeString { get; } = new();
        internal ReactivePropertySlim<TextDocument?> SubProgramSource { get; } = new();
        public IHighlightingDefinition NcHighlighting { get; }
    }
}