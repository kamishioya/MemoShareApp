using System.Collections.ObjectModel;
using System.Windows.Input;
using MemoShareApp.Models;
using MemoShareApp.Services;
using MemoShareApp.Views;

namespace MemoShareApp.ViewModels;

public class MemoListViewModel : BaseViewModel
{
    private readonly IMemoService _memoService;
    private readonly IAuthService _authService;

    public ObservableCollection<Memo> Memos { get; } = new();

    public ICommand RefreshCommand { get; }
    public ICommand AddMemoCommand { get; }
    public ICommand SelectMemoCommand { get; }
    public ICommand DeleteMemoCommand { get; }
    public ICommand LogoutCommand { get; }

    public MemoListViewModel(IMemoService memoService, IAuthService authService)
    {
        _memoService = memoService;
        _authService = authService;
        Title = "マイメモ";

        RefreshCommand = new Command(async () => await LoadMemosAsync());
        AddMemoCommand = new Command(async () => await AddMemoAsync());
        SelectMemoCommand = new Command<Memo>(async (memo) => await SelectMemoAsync(memo));
        DeleteMemoCommand = new Command<Memo>(async (memo) => await DeleteMemoAsync(memo));
        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    public async Task LoadMemosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var memos = await _memoService.GetMyMemosAsync();
            
            Memos.Clear();
            foreach (var memo in memos)
            {
                Memos.Add(memo);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddMemoAsync()
    {
        await Shell.Current.GoToAsync(nameof(MemoDetailPage));
    }

    private async Task SelectMemoAsync(Memo? memo)
    {
        if (memo == null) return;
        await Shell.Current.GoToAsync($"{nameof(MemoDetailPage)}?id={memo.Id}");
    }

    private async Task DeleteMemoAsync(Memo? memo)
    {
        if (memo == null) return;
        
        var currentPage = Application.Current?.Windows.Count > 0 
            ? Application.Current.Windows[0].Page 
            : null;
        if (currentPage == null) return;
        
        bool confirm = await currentPage.DisplayAlertAsync(
            "確認", 
            "このメモを削除しますか？", 
            "削除", 
            "キャンセル");
        
        if (confirm)
        {
            await _memoService.DeleteMemoAsync(memo.Id);
            Memos.Remove(memo);
        }
    }

    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new NavigationPage(
                App.Current!.Handler!.MauiContext!.Services.GetRequiredService<LoginPage>());
        }
    }
}
