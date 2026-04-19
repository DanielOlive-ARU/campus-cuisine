using CampusCuisine.ViewModel;
using Xunit;

namespace CampusCuisine.Tests.ViewModel;

public class RelayCommandTests
{
  [Fact]
  public void Execute_InvokesAction()
  {
    var count = 0;
    var cmd = new RelayCommand(() => count++);

    cmd.Execute(null);

    Assert.Equal(1, count);
  }

  [Fact]
  public void Execute_PassesParameter()
  {
    object? received = null;
    var cmd = new RelayCommand(p => received = p);

    cmd.Execute(42);

    Assert.Equal(42, received);
  }

  [Fact]
  public void CanExecute_TrueByDefault()
  {
    var cmd = new RelayCommand(() => { });

    Assert.True(cmd.CanExecute(null));
  }

  [Fact]
  public void CanExecute_FalseGuardsExecute()
  {
    var count = 0;
    var cmd = new RelayCommand(() => count++, canExecute: () => false);

    cmd.Execute(null);

    Assert.False(cmd.CanExecute(null));
    Assert.Equal(0, count);
  }

  [Fact]
  public void CanExecute_WithParameter()
  {
    var cmd = new RelayCommand(_ => { }, canExecute: p => p is int i && i > 0);

    Assert.False(cmd.CanExecute(0));
    Assert.False(cmd.CanExecute(-1));
    Assert.True(cmd.CanExecute(5));
  }

  [Fact]
  public void RaiseCanExecuteChanged_FiresEvent()
  {
    var cmd = new RelayCommand(() => { });
    var fired = 0;
    cmd.CanExecuteChanged += (_, _) => fired++;

    cmd.RaiseCanExecuteChanged();

    Assert.Equal(1, fired);
  }

  [Fact]
  public void Ctor_NullAction_Throws()
  {
    Assert.Throws<ArgumentNullException>(() => new RelayCommand((Action)null!));
    Assert.Throws<ArgumentNullException>(() => new RelayCommand((Action<object?>)null!));
  }
}

public class AsyncRelayCommandTests
{
  [Fact]
  public async Task ExecuteAsync_AwaitsTask()
  {
    var count = 0;
    var cmd = new AsyncRelayCommand(async () =>
    {
      await Task.Yield();
      count++;
    });

    await cmd.ExecuteAsync(null);

    Assert.Equal(1, count);
  }

  [Fact]
  public async Task ExecuteAsync_PassesParameter()
  {
    object? received = null;
    var cmd = new AsyncRelayCommand(async p =>
    {
      await Task.Yield();
      received = p;
    });

    await cmd.ExecuteAsync("hello");

    Assert.Equal("hello", received);
  }

  [Fact]
  public async Task IsExecuting_TrueWhileRunning_FalseAfter()
  {
    var gate = new TaskCompletionSource();
    var cmd = new AsyncRelayCommand(async () => await gate.Task);

    var run = cmd.ExecuteAsync(null);

    Assert.True(cmd.IsExecuting);
    Assert.False(cmd.CanExecute(null));

    gate.SetResult();
    await run;

    Assert.False(cmd.IsExecuting);
    Assert.True(cmd.CanExecute(null));
  }

  [Fact]
  public async Task ExecuteAsync_PreventsReentry()
  {
    var count = 0;
    var gate = new TaskCompletionSource();
    var cmd = new AsyncRelayCommand(async () =>
    {
      count++;
      await gate.Task;
    });

    var first = cmd.ExecuteAsync(null);
    var second = cmd.ExecuteAsync(null);

    gate.SetResult();
    await Task.WhenAll(first, second);

    Assert.Equal(1, count);
  }

  [Fact]
  public async Task CanExecute_FalseGuardsExecuteAsync()
  {
    var count = 0;
    var cmd = new AsyncRelayCommand(async () => { await Task.Yield(); count++; }, canExecute: () => false);

    await cmd.ExecuteAsync(null);

    Assert.Equal(0, count);
  }

  [Fact]
  public async Task ExecuteAsync_RaisesCanExecuteChangedTwice()
  {
    var fired = 0;
    var gate = new TaskCompletionSource();
    var cmd = new AsyncRelayCommand(async () => await gate.Task);
    cmd.CanExecuteChanged += (_, _) => fired++;

    var run = cmd.ExecuteAsync(null);
    Assert.Equal(1, fired);

    gate.SetResult();
    await run;

    Assert.Equal(2, fired);
  }

  [Fact]
  public async Task ExecuteAsync_ResetsIsExecutingOnException()
  {
    var cmd = new AsyncRelayCommand(async () =>
    {
      await Task.Yield();
      throw new InvalidOperationException("boom");
    });

    await Assert.ThrowsAsync<InvalidOperationException>(() => cmd.ExecuteAsync(null));

    Assert.False(cmd.IsExecuting);
    Assert.True(cmd.CanExecute(null));
  }

  [Fact]
  public void Ctor_NullAction_Throws()
  {
    Assert.Throws<ArgumentNullException>(() => new AsyncRelayCommand((Func<Task>)null!));
    Assert.Throws<ArgumentNullException>(() => new AsyncRelayCommand((Func<object?, Task>)null!));
  }
}
