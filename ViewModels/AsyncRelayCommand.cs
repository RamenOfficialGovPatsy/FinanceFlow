using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    // 1. Обычная версия (без параметров)
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter)
        {
            try
            {
                await _execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выполнения команды: {ex.Message}");
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // 2. Обобщенная версия (с параметром T)
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            // Разрешаем выполнение, если параметр соответствует типу T (или null для ссылочных типов)
            if (parameter == null && default(T) == null)
                return _canExecute?.Invoke(default) ?? true;

            if (parameter is T t)
                return _canExecute?.Invoke(t) ?? true;

            return false;
        }

        public async void Execute(object? parameter)
        {
            try
            {
                if (parameter is T t)
                {
                    await _execute(t);
                }
                else if (parameter == null && default(T) == null)
                {
                    await _execute(default);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выполнения команды<{typeof(T).Name}>: {ex.Message}");
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}