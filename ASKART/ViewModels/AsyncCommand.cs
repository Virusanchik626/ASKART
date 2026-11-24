using System.Windows.Input;

namespace Askart.ViewModels;

public sealed class AsyncCommand(Func<object?, Task> execute, Action<Exception> error,
    Predicate<object?>? canExecute = null, bool allowConcurrent = false) : ICommand
{
    private int executions;
    public bool CanExecute(object? parameter) => (allowConcurrent || executions == 0) && (canExecute?.Invoke(parameter) ?? true);
    public event EventHandler? CanExecuteChanged;
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    public void Execute(object? parameter) => _ = ExecuteAsync(parameter);

    public async Task ExecuteAsync(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        executions++;
        Refresh();
        try { await execute(parameter); }
        catch (OperationCanceledException) { }
        catch (Exception ex) { error(ex); }
        finally { executions--; Refresh(); }
    }
}
