using MemoShareApp.ViewModels;

namespace MemoShareApp.Views;

public partial class SharedMemoListPage : ContentPage
{
    private readonly SharedMemoListViewModel _viewModel;

    public SharedMemoListPage(SharedMemoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSharedMemosAsync();
    }
}
