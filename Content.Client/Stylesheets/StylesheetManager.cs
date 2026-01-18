using Content.Shared._RMC14.CCVar;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
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
            SheetNano = new StyleNano(_resourceCache, theme).Stylesheet;
            SheetSpace = new StyleSpace(_resourceCache).Stylesheet;
            _userInterfaceManager.Stylesheet = SheetNano;
        }
    }
}
