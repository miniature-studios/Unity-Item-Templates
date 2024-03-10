using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace UnityItemTemplates
{
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CorrectingTemplate.PackageGuidString)]
    public sealed class CorrectingTemplate : AsyncPackage
    {
        private DTE2 _applicationObject;
        private DocumentEvents _documentEvents;

        public const string PackageGuidString = "1d77454c-bc85-4fc6-b63f-d97bedb09b8b";

        public CorrectingTemplate() { }

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress
        )
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _applicationObject = await GetServiceAsync(typeof(DTE)) as DTE2;
            _documentEvents = _applicationObject.Events.DocumentEvents;
            _documentEvents.DocumentOpened += DocumentOpenedFirstTime;
        }

        private void DocumentOpenedFirstTime(Document Document)
        {
            JoinableTaskFactory.Run(
                async delegate
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (!(Document.Object("TextDocument") is TextDocument textDocument))
                    {
                        return;
                    }

                    if (textDocument.Language != "CSharp")
                    {
                        return;
                    }

                    if (File.GetCreationTime(Document.FullName) <= DateTime.Now.AddSeconds(-10))
                    {
                        return;
                    }

                    // Validation
                    string validationString = "//" + PackageGuidString;
                    if (!textDocument.StartPoint.CreateEditPoint().FindPattern(validationString))
                    {
                        return;
                    }

                    // Remove Marker
                    _ = textDocument.ReplaceText(validationString, string.Empty);

                    // Replacements
                    foreach (string str in TemplatesOptions.Instance.NamespaceWordsToRemove)
                    {
                        _ = textDocument.ReplaceText(str + ".", string.Empty);
                    }

                    _ = textDocument.ReplaceText(
                        "[AddComponentMenu(\"",
                        "[AddComponentMenu(\"Scripts/"
                    );

                    EditPoint start = textDocument.StartPoint.CreateEditPoint();
                    if (start.FindPattern("[AddComponentMenu(\"Scripts/"))
                    {
                        string targetLine = start.GetLines(start.Line, start.Line + 1);
                        string[] splits = targetLine.Split('/');
                        for (int i = 0; i < splits.Length - 1; i++)
                        {
                            splits[i] = splits[i].Replace(".", "/");
                        }
                        string modifiedLine = splits.Aggregate((x, y) => x + "/" + y);
                        foreach (string ending in TemplatesOptions.Instance.ClassEndingsToRemove)
                        {
                            modifiedLine = modifiedLine.Replace(ending + "\")]", "\")]");
                        }

                        if (TemplatesOptions.Instance.DeleteDuplication)
                        {
                            int firstIndex = modifiedLine.IndexOf(modifiedLine.Split('.').Last());
                            int lastIndex = modifiedLine.IndexOf("\")]");
                            string scriptName = modifiedLine.Substring(
                                firstIndex,
                                lastIndex - firstIndex
                            );
                            modifiedLine = modifiedLine.Replace(
                                scriptName + "." + scriptName,
                                scriptName
                            );
                        }

                        _ = textDocument.ReplaceText(targetLine, modifiedLine);
                    }
                }
            );
        }
    }
}
