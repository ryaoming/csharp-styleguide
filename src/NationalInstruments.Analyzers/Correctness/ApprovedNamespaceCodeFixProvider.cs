using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using NationalInstruments.Analyzers.Properties;

namespace NationalInstruments.Analyzers.Correctness
{
    /// <summary>
    /// Fix for NI1017 - Unapproved namespace violation.
    /// Adds the namespace to the approved namespaces file.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApprovedNamespaceCodeFixProvider))]
    [Shared]
    public class ApprovedNamespaceCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ApprovedNamespaceAnalyzer.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.FirstOrDefault();
            if (diagnostic != null)
            {
                var location = diagnostic.Location;
                var namespaceName = (await location.SourceTree.GetRootAsync(context.CancellationToken).ConfigureAwait(false))
                    .FindNode(location.SourceSpan)
                    .ToString();
                var codeAction = CodeAction.Create(
                    Resources.NI1800_CodeFixTitle,
                    cancellationToken => AddNamespaceAsync(context, namespaceName),
                    nameof(ApprovedNamespaceCodeFixProvider));
                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }
        }

        private Task<Document> AddNamespaceAsync(CodeFixContext context, string namespaceName)
        {
            ApprovedNamespaceAnalyzer.ApproveNamespace(namespaceName);
            return Task.FromResult(context.Document);
        }
    }
}