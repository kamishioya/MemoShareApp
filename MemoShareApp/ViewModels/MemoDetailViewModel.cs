using System.Collections.ObjectModel;
using System.Windows.Input;
using MemoShareApp.Models;
using MemoShareApp.Services;

namespace MemoShareApp.ViewModels;

[QueryProperty(nameof(MemoId), "id")]
public class MemoDetailViewModel : BaseViewModel
{
    private readonly IMemoService _memoService;
    private readonly IAuthService _authService;
    
    private string _memoId = string.Empty;
    private string _memoTitle = string.Empty;
    private string _content = string.Empty;
    private bool _isShared;
    private bool _isNewMemo = true;
    private Memo? _currentMemo;

    public string MemoId
    {
        get => _memoId;
        set
        {
            SetProperty(ref _memoId, value);
            if (!string.IsNullOrEmpty(value))
            {
                LoadMemoAsync(value).ConfigureAwait(false);
            }
        }
    }

    public string MemoTitle
    {
        get => _memoTitle;
        set => SetProperty(ref _memoTitle, value);
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public bool IsShared
    {
        get => _isShared;
        set => SetProperty(ref _isShared, value);
    }

    public bool IsNewMemo
    {
        get => _isNewMemo;
        set => SetProperty(ref _isNewMemo, value);
    }

    public ObservableCollection<User> AvailableUsers { get; } = new();

    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ShareWithUserCommand { get; }

    public MemoDetailViewModel(IMemoService memoService, IAuthService authService)
    {
        _memoService = memoService;
        _authService = authService;
        Title = "メモ詳細";

        SaveCommand = new Command(async () => await SaveMemoAsync());
        DeleteCommand = new Command(async () => await DeleteMemoAsync());
        ShareWithUserCommand = new Command<User>(async (user) => await ShareWithUserAsync(user));
    }

    private async Task LoadMemoAsync(string id)
    {
        try
        {
            IsBusy = true;
            _currentMemo = await _memoService.GetMemoByIdAsync(id);
            
            if (_currentMemo != null)
            {
                IsNewMemo = false;
                MemoTitle = _currentMemo.Title;
                Content = _currentMemo.Content;
                IsShared = _currentMemo.IsShared;
                Title = "メモを編集";
            }

            await LoadAvailableUsersAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadAvailableUsersAsync()
    {
        var users = await _authService.GetAllUsersAsync();
        AvailableUsers.Clear();
        foreach (var user in users)
        {
            AvailableUsers.Add(user);
        }
    }

    private async Task SaveMemoAsync()
    {
        if (string.IsNullOrWhiteSpace(MemoTitle))
        {
            await Application.Current!.MainPage!.DisplayAlert("エラー", "タイトルを入力してください", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            if (IsNewMemo)
            {
                var newMemo = new Memo
                {
                    Title = MemoTitle,
                    Content = Content,
                    IsShared = IsShared
                };
                await _memoService.CreateMemoAsync(newMemo);
            }
            else if (_currentMemo != null)
            {
                _currentMemo.Title = MemoTitle;
                _currentMemo.Content = Content;
                _currentMemo.IsShared = IsShared;
                await _memoService.UpdateMemoAsync(_currentMemo);
            }

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteMemoAsync()
    {
        if (_currentMemo == null) return;

        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "確認",
            "このメモを削除しますか？",
            "削除",
            "キャンセル");

        if (confirm)
        {
            await _memoService.DeleteMemoAsync(_currentMemo.Id);
            await Shell.Current.GoToAsync("..");
        }
    }

    private async Task ShareWithUserAsync(User? user)
    {
        if (user == null || _currentMemo == null) return;

        await _memoService.ShareMemoAsync(_currentMemo.Id, user.Id);
        await Application.Current!.MainPage!.DisplayAlert(
            "共有完了",
            $"{user.DisplayName}さんとメモを共有しました",
            "OK");
    }
}
