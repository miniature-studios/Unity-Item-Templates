using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;

namespace UnityItemTemplates
{
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideOptionPage(typeof(TemplatesOptionsPage), "Unity Item Template", "General", 0, 0, true)]
    public sealed class UnityItemTemplatesPackage : AsyncPackage
    {
        public const string PackageGuidString = "0e3062a1-46cb-4312-855d-10c347f6f953";

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress
        )
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            SolutionEvents.OnAfterOpenSolution += HandleOpenSolution;
        }

        private void HandleOpenSolution(object sender, OpenSolutionEventArgs e)
        {
            TemplatesOptions.Instance.Load();
        }
    }
}
