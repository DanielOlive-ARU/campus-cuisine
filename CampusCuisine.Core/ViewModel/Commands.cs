using System.Windows.Input;

namespace CampusCuisine.ViewModel;

public class RelayCommand : ICommand
{
  private readonly Action<object?> _execute;
  private readonly Func<object?, bool>? _canExecute;

  public RelayCommand(Action execute, Func<bool>? canExecute = null)
  {
    if (execute is null)
      throw new ArgumentNullException(nameof(execute));

    _execute = _ => execute();
    _canExecute = canExecute is null ? null : (_ => canExecute());
  }

  public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
  {
    _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    _canExecute = canExecute;
  }

  public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

  public void Execute(object? parameter)
  {
    if (!CanExecute(parameter))
      return;
    _execute(parameter);
  }

  public event EventHandler? CanExecuteChanged;

  public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class AsyncRelayCommand : ICommand
{
  private readonly Func<object?, Task> _execute;
  private readonly Func<object?, bool>? _canExecute;
  private bool _isExecuting;

  public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
  {
    if (execute is null)
      throw new ArgumentNullException(nameof(execute));

    _execute = _ => execute();
    _canExecute = canExecute is null ? null : (_ => canExecute());
  }

  public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
  {
    _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    _canExecute = canExecute;
  }

  public bool IsExecuting => _isExecuting;

  public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

  public async void Execute(object? parameter)
  {
    if (!CanExecute(parameter))
      return;

    _isExecuting = true;
    RaiseCanExecuteChanged();

    try
    {
      await _execute(parameter);
    }
    finally
    {
      _isExecuting = false;
      RaiseCanExecuteChanged();
    }
  }

  public Task ExecuteAsync(object? parameter)
  {
    if (!CanExecute(parameter))
      return Task.CompletedTask;

    _isExecuting = true;
    RaiseCanExecuteChanged();

    return RunAsync(parameter);
  }

  private async Task RunAsync(object? parameter)
  {
    try
    {
      await _execute(parameter);
    }
    finally
    {
      _isExecuting = false;
      RaiseCanExecuteChanged();
    }
  }

  public event EventHandler? CanExecuteChanged;

  public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
