using System.Collections.ObjectModel;
using System.Windows.Input;
using MemoShareApp.Models;
using MemoShareApp.Services;

namespace MemoShareApp.ViewModels;

public class SharedMemoListViewModel : BaseViewModel
{
    private readonly IMemoService _memoService;

    public ObservableCollection<Memo> SharedMemos { get; } = new();

    public ICommand RefreshCommand { get; }
    public ICommand SelectMemoCommand { get; }

    public SharedMemoListViewModel(IMemoService memoService)
    {
        _memoService = memoService;
        Title = "共有メモ";

        RefreshCommand = new Command(async () => await LoadSharedMemosAsync());
        SelectMemoCommand = new Command<Memo>(async (memo) => await SelectMemoAsync(memo));
    }

    public async Task LoadSharedMemosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var memos = await _memoService.GetSharedMemosAsync();
            
            SharedMemos.Clear();
            foreach (var memo in memos)
            {
                SharedMemos.Add(memo);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SelectMemoAsync(Memo? memo)
    {
        if (memo == null) return;
        await Shell.Current.GoToAsync($"MemoDetailPage?id={memo.Id}");
    }
}
