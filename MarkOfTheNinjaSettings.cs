using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.MarkOfTheNinja
{
    public partial class MarkOfTheNinjaSettings : UserControl
    {
        public bool AutoStart { get; set; } = true;
        public bool SplitOnLevelComplete { get; set; } = true;
        public bool SplitOnFinalChoice { get; set; } = true;
        public bool AutoReset { get; set; } = true;
        public bool RemoveLoads { get; set; } = true;

        public MarkOfTheNinjaSettings()
        {
            InitializeComponent();
            RefreshCheckBoxes();

            chkAutoStart.CheckedChanged += (s, e) => AutoStart = chkAutoStart.Checked;
            chkSplitOnLevelComplete.CheckedChanged += (s, e) => SplitOnLevelComplete = chkSplitOnLevelComplete.Checked;
            chkSplitOnFinalChoice.CheckedChanged += (s, e) => SplitOnFinalChoice = chkSplitOnFinalChoice.Checked;
            chkAutoReset.CheckedChanged += (s, e) => AutoReset = chkAutoReset.Checked;
            chkRemoveLoads.CheckedChanged += (s, e) => RemoveLoads = chkRemoveLoads.Checked;
        }

        private void RefreshCheckBoxes()
        {
            chkAutoStart.Checked = AutoStart;
            chkSplitOnLevelComplete.Checked = SplitOnLevelComplete;
            chkSplitOnFinalChoice.Checked = SplitOnFinalChoice;
            chkAutoReset.Checked = AutoReset;
            chkRemoveLoads.Checked = RemoveLoads;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var root = document.CreateElement("Settings");
            AppendBool(document, root, nameof(AutoStart), AutoStart);
            AppendBool(document, root, nameof(SplitOnLevelComplete), SplitOnLevelComplete);
            AppendBool(document, root, nameof(SplitOnFinalChoice), SplitOnFinalChoice);
            AppendBool(document, root, nameof(AutoReset), AutoReset);
            AppendBool(document, root, nameof(RemoveLoads), RemoveLoads);
            return root;
        }

        public void SetSettings(XmlNode settings)
        {
            AutoStart = ReadBool(settings, nameof(AutoStart), true);
            SplitOnLevelComplete = ReadBool(settings, nameof(SplitOnLevelComplete), true);
            SplitOnFinalChoice = ReadBool(settings, nameof(SplitOnFinalChoice), true);
            AutoReset = ReadBool(settings, nameof(AutoReset), true);
            RemoveLoads = ReadBool(settings, nameof(RemoveLoads), true);
            RefreshCheckBoxes();
        }

        private static void AppendBool(XmlDocument document, XmlElement root, string name, bool value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString();
            root.AppendChild(element);
        }

        private static bool ReadBool(XmlNode settings, string name, bool defaultValue)
        {
            var node = settings?[name];
            return node != null && bool.TryParse(node.InnerText, out bool value) ? value : defaultValue;
        }
    }
}
