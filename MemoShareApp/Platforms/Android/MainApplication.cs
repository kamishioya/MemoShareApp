using Android.App;
using Android.Content;
using Android.Runtime;

namespace MemoShareApp;

[Application(LargeHeap = true)]
public class MainApplication : MauiApplication
{
	private static bool _isTerminating = false;
	public static bool IsTerminating => _isTerminating;

	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public override void OnTerminate()
	{
		_isTerminating = true;
		
		// pthread_mutex_lock エラーを防ぐため、
		// 終了処理を最小限に抑える
		try
		{
			// GCを強制しない - Glideのスレッド競合の原因になる
			// 代わりにOSにクリーンアップを任せる
		}
		catch
		{
			// エラーを無視
		}
		
		base.OnTerminate();
	}

	public override void OnTrimMemory(TrimMemory level)
	{
		// メモリトリム時にGCを呼ばない
		// これがGlide/HWUI競合の主な原因
		if (level == TrimMemory.UiHidden && !_isTerminating)
		{
			// UIが非表示の時のみ、軽いクリーンアップ
			base.OnTrimMemory(level);
		}
		else if (!_isTerminating)
		{
			base.OnTrimMemory(level);
		}
		// 終了中は何もしない
	}
}
