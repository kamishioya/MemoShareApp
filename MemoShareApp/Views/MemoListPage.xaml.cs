using MemoShareApp.ViewModels;

namespace MemoShareApp.Views;

public partial class MemoListPage : ContentPage
{
    private readonly MemoListViewModel _viewModel;

    public MemoListPage(MemoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadMemosAsync();
    }
}
