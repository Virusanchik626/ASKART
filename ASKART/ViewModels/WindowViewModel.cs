namespace Askart.ViewModels;

public abstract class WindowViewModel : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource lifetime = new();
    private bool disposed;
    protected CancellationToken Lifetime { get; }
    protected WindowViewModel() => Lifetime = lifetime.Token;
    public bool IsClosed => disposed;
    private string status = "";
    private bool isError;
    public bool IsError { get => isError; private set => Set(ref isError, value); }
    public string Status
    {
        get => status;
        protected set { Set(ref status, value); IsError = false; }
    }
    protected void Error(Exception ex) { if (!disposed) { Status = ex.Message; IsError = true; } }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        lifetime.Cancel();
        lifetime.Dispose();
    }
}
