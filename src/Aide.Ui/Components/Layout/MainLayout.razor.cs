using Aide.Ui.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace Aide.Ui.Components.Layout;

public partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    public required AppStateService AppStateService { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    private static MudTheme AideTheme => new()
    {
        Typography = new()
        {
            Default = new DefaultTypography()
            {
                FontFamily = ["Poppins", "system-ui", "sans-serif"]
            }
        },

        Shadows = new MudBlazor.Shadow()
        {
            Elevation =
            [
                "none",
                "0px 1px 3px 1px rgba(0, 0, 0, 0.08)",
                "0px 2px 4px 1px rgba(0, 0, 0, 0.10)",
                "0px 3px 6px 2px rgba(0, 0, 0, 0.12)",
                "0px 4px 8px 3px rgba(0, 0, 0, 0.14)",
                "0px 5px 10px 4px rgba(0, 0, 0, 0.16)",
                "0px 6px 12px 5px rgba(0, 0, 0, 0.18)",
                "0px 7px 14px 6px rgba(0, 0, 0, 0.20)",
                "0px 8px 16px 7px rgba(0, 0, 0, 0.22)",
                "0px 9px 18px 8px rgba(0, 0, 0, 0.24)",
                "0px 10px 20px 9px rgba(0, 0, 0, 0.26)",
                "0px 11px 22px 10px rgba(0, 0, 0, 0.28)",
                "0px 12px 24px 11px rgba(0, 0, 0, 0.30)",
                "0px 13px 26px 12px rgba(0, 0, 0, 0.32)",
                "0px 14px 28px 13px rgba(0, 0, 0, 0.34)",
                "0px 15px 30px 14px rgba(0, 0, 0, 0.36)",
                "0px 16px 32px 15px rgba(0, 0, 0, 0.38)",
                "0px 17px 34px 16px rgba(0, 0, 0, 0.40)",
                "0px 18px 36px 17px rgba(0, 0, 0, 0.42)",
                "0px 19px 38px 18px rgba(0, 0, 0, 0.44)",
                "0px 20px 40px 19px rgba(0, 0, 0, 0.46)",
                "0px 21px 42px 20px rgba(0, 0, 0, 0.48)",
                "0px 22px 44px 21px rgba(0, 0, 0, 0.50)",
                "0px 23px 46px 22px rgba(0, 0, 0, 0.52)",
                "0px 24px 48px 23px rgba(0, 0, 0, 0.54)",
                "0px 25px 50px 24px rgba(0, 0, 0, 0.56)"
            ]
        },

        PaletteLight = new PaletteLight()
        {
            Background = "#FAFAFA",
            BackgroundGray = "#F5F5F5",
            Surface = "#FFFFFF",
            OverlayLight = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            Primary = "#7C3AED",
            PrimaryLighten = "#A78BFA",
            PrimaryDarken = "#5B21B6",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#8B5CF6",
            SecondaryLighten = "#C4B5FD",
            SecondaryDarken = "#6D28D9",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#EDE9FE",
            TextPrimary = "#18181B",
            TextSecondary = "#71717A",
            GrayLighter = "#F4F4F5",
            GrayLight = "#E4E4E7",
            GrayDefault = "#A1A1AA",
            GrayDark = "#52525B",
            GrayDarker = "#3F3F46",
            DrawerBackground = "#FFFFFF",
            Dark = "#18181B",
            DarkLighten = "#27272A",
            DarkDarken = "#09090B",
            Divider = "#E4E4E7",
            Success = "#10B981",
            SuccessLighten = "#34D399",
            SuccessDarken = "#059669",
            Error = "#EF4444",
            ErrorLighten = "#F87171",
            ErrorDarken = "#DC2626",
            Warning = "#F59E0B",
            WarningLighten = "#FBBF24",
            WarningDarken = "#D97706",
            Info = "#3B82F6",
            InfoLighten = "#60A5FA",
            InfoDarken = "#2563EB",
            ActionDefault = "#71717A",
            ActionDisabled = "#D4D4D8",
            LinesDefault = "#E4E4E7",
            LinesInputs = "#E4E4E7"
        },

        PaletteDark = new PaletteDark()
        {
            Background = "#0A0A0B",
            BackgroundGray = "#18181B",
            Surface = "#18181B",
            OverlayDark = "#000000",
            OverlayLight = "#27272A",
            AppbarBackground = "#18181B",
            Primary = "#A78BFA",
            PrimaryLighten = "#C4B5FD",
            PrimaryDarken = "#8B5CF6",
            PrimaryContrastText = "#18181B",
            Secondary = "#8B5CF6",
            SecondaryLighten = "#A78BFA",
            SecondaryDarken = "#7C3AED",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#2E1065",
            TextPrimary = "#FAFAFA",
            TextSecondary = "#A1A1AA",
            GrayLighter = "#3F3F46",
            GrayLight = "#27272A",
            GrayDefault = "#52525B",
            GrayDark = "#71717A",
            GrayDarker = "#A1A1AA",
            DrawerBackground = "#0F0F10",
            Dark = "#18181B",
            DarkLighten = "#27272A",
            DarkDarken = "#09090B",
            Divider = "#27272A",
            Success = "#34D399",
            SuccessLighten = "#6EE7B7",
            SuccessDarken = "#10B981",
            Error = "#F87171",
            ErrorLighten = "#FCA5A5",
            ErrorDarken = "#EF4444",
            Warning = "#FBBF24",
            WarningLighten = "#FCD34D",
            WarningDarken = "#F59E0B",
            Info = "#60A5FA",
            InfoLighten = "#93C5FD",
            InfoDarken = "#3B82F6",
            ActionDefault = "#A1A1AA",
            ActionDisabled = "#3F3F46",
            LinesDefault = "#27272A",
            LinesInputs = "#27272A"
        }
    };

    protected override void OnInitialized()
    {
        AppStateService.OnChanged += HandleStateChanged;
        Navigation.LocationChanged += OnLocationChanged;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    protected void ToggleSidebar()
    {
        AppStateService.IsSidebarOpen = !AppStateService.IsSidebarOpen;
    }

    public void Dispose()
    {
        AppStateService.OnChanged -= HandleStateChanged;
        Navigation.LocationChanged -= OnLocationChanged;
    }
}
