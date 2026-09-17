#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyAIAssistantPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly AgencyAIAssistantViewModel? _viewModel;

    public AgencyAIAssistantPage()
    {
        InitializeComponent();
    }

    public AgencyAIAssistantPage(AgencyAIAssistantViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
#endif

