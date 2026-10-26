using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;

namespace LiveSplit.MarkOfTheNinja
{
    public class MarkOfTheNinjaFactory : IComponentFactory
    {
        public string ComponentName => "Mark of the Ninja Autosplitter";
        public string Description => "Autosplitter and load remover for Mark of the Ninja";
        public ComponentCategory Category => ComponentCategory.Control;

        public string UpdateName => ComponentName;
        public string UpdateURL => "https://github.com/Lusther/LiveSplit.MarkOfTheNinja/releases/latest/download/";
        public string XMLURL => "https://raw.githubusercontent.com/Lusther/LiveSplit.MarkOfTheNinja/main/Components/Updates.xml";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public IComponent Create(LiveSplitState state) => new MarkOfTheNinjaComponent(state);
    }
}
