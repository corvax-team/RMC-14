// CM14 rework: non-RMC edit marker.
using Content.Shared._RMC14.CCVar;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;

namespace Content.Client.Stylesheets
{
    public sealed class StylesheetManager : IStylesheetManager
    {
        [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IConfigurationManager _configManager = default!;

        public Stylesheet SheetNano { get; private set; } = default!;
        public Stylesheet SheetSpace { get; private set; } = default!;

        public void Initialize()
        {
            _configManager.OnValueChanged(RMCCVars.RMCUIColorTheme, ApplyColorTheme, true);
        }

        private void ApplyColorTheme(string theme)
        {
            var oldNano = SheetNano;
            var oldSpace = SheetSpace;
            SheetNano = new StyleNano(_resourceCache, theme).Stylesheet;
            SheetSpace = new StyleSpace(_resourceCache, theme).Stylesheet;
            _userInterfaceManager.Stylesheet = SheetNano;

            foreach (var root in _userInterfaceManager.AllRoots)
            {
                if (root is Control control)
                    RefreshControlTree(control, oldNano, oldSpace);
            }
        }

        private void RefreshControlTree(Control control, Stylesheet? oldNano, Stylesheet? oldSpace)
        {
            if (ReferenceEquals(control.Stylesheet, oldNano))
                control.Stylesheet = SheetNano;
            else if (ReferenceEquals(control.Stylesheet, oldSpace))
                control.Stylesheet = SheetSpace;
            else
                control.InvalidateStyleSheet();

            control.DoStyleUpdate();

            foreach (var child in control.Children)
            {
                RefreshControlTree(child, oldNano, oldSpace);
            }
        }
    }
}
