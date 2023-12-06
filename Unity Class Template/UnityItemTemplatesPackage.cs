using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace UnityItemTemplates
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(UnityItemTemplatesPackage.PackageGuidString)]
    public sealed class UnityItemTemplatesPackage : AsyncPackage
    {
        public const string PackageGuidString = "0e3062a1-46cb-4312-855d-10c347f6f953";

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress
        )
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }
    }
}
