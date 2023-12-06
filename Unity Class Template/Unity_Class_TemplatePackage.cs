using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Unity_Class_Template
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Unity_Class_TemplatePackage.PackageGuidString)]
    public sealed class Unity_Class_TemplatePackage : AsyncPackage
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
