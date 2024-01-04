namespace PrismKit.Commands;

public class AdaptiveCommand : DelegateCommand
{
    private const double DefaultInvocationDelay = 0.7;

    public double InvocationDelayInSeconds { get; set; }

    private DateTime lastInvokeTime = DateTime.MinValue;
    private readonly Func<bool>? canExecute;
    private readonly Func<Task>? task;

    public AdaptiveCommand(
        Action executeAction,
        double invocationDelayInSeconds = DefaultInvocationDelay)
        : base(executeAction)
    {
        InvocationDelayInSeconds = invocationDelayInSeconds;
    }

    public AdaptiveCommand(
        Func<Task> task,
        double invocationDelayInSeconds = DefaultInvocationDelay)
        : base(() => { })
    {
        this.task = task;
        InvocationDelayInSeconds = invocationDelayInSeconds;
    }

    public AdaptiveCommand(
        Action executeAction,
        Func<bool> canExecute,
        double invocationDelayInSeconds = DefaultInvocationDelay)
        : base(executeAction, canExecute)
    {
        this.canExecute = canExecute;
        InvocationDelayInSeconds = invocationDelayInSeconds;
    }

    protected override async void Execute(object? parameter)
    {
        if (DateTime.Now - lastInvokeTime < TimeSpan.FromSeconds(InvocationDelayInSeconds))
            return;

        if (IsActive || (canExecute != null && !canExecute()))
            return;

        IsActive = true;
        if (task != null)
        {
            await task();
            IsActive = false;
        }
        else
        {
            base.Execute(parameter);
            IsActive = false;
        }

        lastInvokeTime = DateTime.Now;
    }

    protected override void OnIsActiveChanged()
    {
        base.OnIsActiveChanged();
        RaiseCanExecuteChanged();
    }

    protected override bool CanExecute(object? parameter)
    {
        return !IsActive;
    }
}

public class AdaptiveCommand<T> : DelegateCommand<T>
{
    private const double DefaultInvocationDelay = 0.7;

    public double InvocationDelayInSeconds { get; set; }

    private DateTime lastInvokeTime = DateTime.MinValue;
    private readonly Func<T, bool>? canExecute;
    private readonly Func<T, Task>? task;

    public AdaptiveCommand(
        Action<T> executeAction,
        double invocationDelayInSeconds = DefaultInvocationDelay)
        : base(executeAction)
    {
        InvocationDelayInSeconds = invocationDelayInSeconds;
    }

    public AdaptiveCommand(
        Func<T, Task> task,
        double invocationDelayInSeconds = DefaultInvocationDelay)
        : base(_ => { })
    {
        this.task = task;
        InvocationDelayInSeconds = invocationDelayInSeconds;
    }

    public AdaptiveCommand(
        Action<T> executeAction,
        Func<T?, bool> canExecute,
        double invocationDelayInSeconds = DefaultInvocationDelay)
        : base(executeAction, canExecute)
    {
        this.canExecute = canExecute;
        InvocationDelayInSeconds = invocationDelayInSeconds;
    }

    protected override void Execute(object? parameter)
    {
        if (DateTime.Now - lastInvokeTime < TimeSpan.FromSeconds(InvocationDelayInSeconds))
            return;

        var param = (T)parameter!;

        if (IsActive || (canExecute != null && !canExecute(param)))
            return;

        IsActive = true;
        if (task != null)
        {
            _ = task(param).ContinueWith(_ => IsActive = false);
        }
        else
        {
            base.Execute(parameter);
            IsActive = false;
        }

        lastInvokeTime = DateTime.Now;
    }

    protected override void OnIsActiveChanged()
    {
        base.OnIsActiveChanged();
        RaiseCanExecuteChanged();
    }

    protected override bool CanExecute(object? parameter)
    {
        return !IsActive;
    }
}
