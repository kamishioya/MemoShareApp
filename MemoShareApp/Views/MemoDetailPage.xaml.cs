using MemoShareApp.ViewModels;

namespace MemoShareApp.Views;

public partial class MemoDetailPage : ContentPage
{
    public MemoDetailPage(MemoDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
