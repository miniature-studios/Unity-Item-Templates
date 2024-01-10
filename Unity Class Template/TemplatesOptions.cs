using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace UnityItemTemplates
{
    public class TemplatesOptions
    {
        [Category("UnityItemTemplates - Options")]
        [DisplayName("Default namespace")]
        [Description("Default namespace when no namespace")]
        public string DefaultNamespace { get; set; } = "DefaultNamespace";

        [Category("UnityItemTemplates - Options")]
        [DisplayName("Namespace Words To Remove")]
        [Description("Namespace Words To Remove")]
        public string[] NamespaceWordsToRemove { get; set; } = new string[] { "Assets", "Scripts" };

        [Category("UnityItemTemplates - Options")]
        [DisplayName("Class Endings To Remove")]
        [Description("Class Endings To Remove")]
        public string[] ClassEndingsToRemove { get; set; } = new string[] { "Impl" };

        protected void LoadFrom(TemplatesOptions newInstance)
        {
            DefaultNamespace = newInstance.DefaultNamespace;
            NamespaceWordsToRemove = newInstance.NamespaceWordsToRemove;
            ClassEndingsToRemove = newInstance.ClassEndingsToRemove;
        }

        private static readonly AsyncLazy<TemplatesOptions> liveModel =
            new AsyncLazy<TemplatesOptions>(CreateAsync, ThreadHelper.JoinableTaskFactory);

        public static TemplatesOptions Instance
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return ThreadHelper.JoinableTaskFactory.Run(liveModel.GetValueAsync);
            }
        }

        public void Load()
        {
            ThreadHelper.JoinableTaskFactory.Run(LoadAsync);
        }

        public static Task<TemplatesOptions> GetLiveInstanceAsync()
        {
            return liveModel.GetValueAsync();
        }

        private static async Task<TemplatesOptions> CreateAsync()
        {
            TemplatesOptions instance = new TemplatesOptions();
            await instance.LoadAsync();
            return instance;
        }

        private async Task LoadAsync()
        {
            TemplatesOptions newInstance = new TemplatesOptions();

            await LoadOptionsFromFileAsync(
                GetSolutionOptionsFileNameAsync,
                optionsDto =>
                {
                    newInstance.DefaultNamespace = optionsDto.DefaultNamespace ?? "NullNamespace";
                    newInstance.NamespaceWordsToRemove =
                        optionsDto.NamespaceWordsToRemove ?? new string[0];
                    newInstance.ClassEndingsToRemove =
                        optionsDto.ClassEndingsToRemove ?? new string[0];
                }
            );

            LoadFrom(newInstance);
        }

        private async Task LoadOptionsFromFileAsync(
            Func<Task<string>> getFilePath,
            Action<OptionsDto> doStuff
        )
        {
            string filePath = await getFilePath();
            if (filePath == null || !File.Exists(filePath))
            {
                return;
            }

            string json = File.ReadAllText(filePath);
            OptionsDto optionsDto = JsonConvert.DeserializeObject<OptionsDto>(json);
            if (optionsDto == null)
            {
                return;
            }

            doStuff(optionsDto);
        }

        public void Save()
        {
            ThreadHelper.JoinableTaskFactory.Run(SaveAsync);
        }

        public async Task SaveAsync()
        {
            await SaveOptionsAsync(
                GetSolutionOptionsFileNameAsync,
                new OptionsDto
                {
                    DefaultNamespace = DefaultNamespace,
                    NamespaceWordsToRemove = NamespaceWordsToRemove,
                    ClassEndingsToRemove = ClassEndingsToRemove,
                }
            );
        }

        private async Task SaveOptionsAsync(Func<Task<string>> getFilePath, OptionsDto optionsDto)
        {
            string filePath = await getFilePath();
            if (filePath == null)
            {
                return;
            }
            string json = JsonConvert.SerializeObject(optionsDto);
            File.WriteAllText(filePath, json);
        }

        private async Task<string> GetSolutionOptionsFileNameAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsSolution solution =
                await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SVsSolution))
                as IVsSolution;

            return ErrorHandler.Succeeded(
                solution.GetSolutionInfo(out _, out _, out string userOptionsFile)
            )
                ? Path.Combine(Path.GetDirectoryName(userOptionsFile), "unity-templates.json")
                : throw new FileNotFoundException();
        }

        private class OptionsDto
        {
            public string DefaultNamespace { get; set; }
            public string[] NamespaceWordsToRemove { get; set; }
            public string[] ClassEndingsToRemove { get; set; }
        }
    }
}
