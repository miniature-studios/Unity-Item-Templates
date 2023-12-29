using Microsoft.VisualStudio.Shell;

namespace UnityItemTemplates
{
    public class TemplatesOptionsPage : DialogPage
    {
        private readonly TemplatesOptions model;

        public TemplatesOptionsPage()
        {
            model = ThreadHelper.JoinableTaskFactory.Run(TemplatesOptions.GetLiveInstanceAsync);
        }

        public override object AutomationObject => model;

        public override void LoadSettingsFromStorage()
        {
            model.Load();
        }

        public override void SaveSettingsToStorage()
        {
            model.Save();
        }
    }
}
