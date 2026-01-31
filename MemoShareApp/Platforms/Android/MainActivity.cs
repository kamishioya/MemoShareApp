using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Runtime;

namespace MemoShareApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | 
    ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    HardwareAccelerated = true)]
public class MainActivity : MauiAppCompatActivity
{
    private bool _isFinishing = false;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // ステータスバーとナビゲーションバーの色を設定
        if (Window != null)
        {
#pragma warning disable CA1422
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#512BD4"));
            Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#512BD4"));
#pragma warning restore CA1422
        }

        // Glide例外をグローバルにハンドル
        AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;
    }

    private void OnUnhandledException(object? sender, RaiseThrowableEventArgs e)
    {
        // Glide関連のエラーを無視（アプリ終了時の問題）
        if (e.Exception?.Message?.Contains("Glide") == true ||
            e.Exception?.Message?.Contains("pthread_mutex") == true ||
            e.Exception?.StackTrace?.Contains("Glide") == true)
        {
            e.Handled = true;
        }
    }

    protected override void OnStop()
    {
        // アクティビティが停止する前にGCを強制しない
        // これによりGlideのスレッド競合を防ぐ
        base.OnStop();
    }

    protected override void OnDestroy()
    {
        _isFinishing = true;
        
        // イベントハンドラを解除
        AndroidEnvironment.UnhandledExceptionRaiser -= OnUnhandledException;

        // Glide/HWUIのmutex問題を回避するため、
        // UIスレッドで保留中の操作を完了させる
        try
        {
            // 保留中のUI操作をフラッシュ
            Window?.DecorView?.Handler?.RemoveCallbacksAndMessages(null);
            
            // ContentViewをクリアして画像参照を解放
            if (Window?.DecorView is ViewGroup viewGroup)
            {
                ClearImageViews(viewGroup);
            }
        }
        catch
        {
            // 例外を無視（アプリ終了時なので問題なし）
        }
        
        base.OnDestroy();
    }

    private void ClearImageViews(ViewGroup viewGroup)
    {
        try
        {
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                var child = viewGroup.GetChildAt(i);
                if (child is Android.Widget.ImageView imageView)
                {
                    imageView.SetImageDrawable(null);
                }
                else if (child is ViewGroup childGroup)
                {
                    ClearImageViews(childGroup);
                }
            }
        }
        catch
        {
            // 無視
        }
    }

    public override void OnLowMemory()
    {
        // OnLowMemoryでGC.Collectを呼ばない
        // Glideのスレッド競合を防ぐため
        base.OnLowMemory();
    }

    public override void Finish()
    {
        _isFinishing = true;
        base.Finish();
    }
}
