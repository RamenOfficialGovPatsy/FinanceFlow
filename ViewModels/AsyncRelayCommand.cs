using System.Windows.Input;

namespace FinanceFlow.ViewModels
{
    public class AsyncRelayCommand : ICommand
    {
        // Основная функция для выполнения асинхронной операции
        private readonly Func<Task> _execute;

        // Функция проверки возможности выполнения команды (опционально)
        private readonly Func<bool>? _canExecute;

        // Событие для уведомления об изменении возможности выполнения
        public event EventHandler? CanExecuteChanged;

        // Конструктор принимает функцию выполнения и опциональную функцию проверки
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Проверяет можно ли выполнить команду в данный момент
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        // Основной метод выполнения команды с обработкой ошибок
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

        // Метод для принудительного обновления состояния команды
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // Обобщенная версия асинхронной команды с поддержкой параметров типа T
    // Используется когда команде нужно передать параметр определенного типа
    public class AsyncRelayCommand<T> : ICommand
    {
        // Функция выполнения с параметром типа T
        private readonly Func<T?, Task> _execute;

        // Функция проверки с параметром типа T (опционально)
        private readonly Func<T?, bool>? _canExecute;

        // Событие для уведомления об изменении возможности выполнения
        public event EventHandler? CanExecuteChanged;

        // Конструктор для обобщенной команды
        public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Проверяет возможность выполнения команды с учетом типа параметра
        public bool CanExecute(object? parameter)
        {
            // Обрабатываем случай когда параметр null для nullable типов
            if (parameter == null && default(T) == null)
                return _canExecute?.Invoke(default) ?? true;

            // Если параметр правильного типа - проверяем через canExecute
            if (parameter is T t)
                return _canExecute?.Invoke(t) ?? true;

            // Для несовместимых типов возвращаем false
            return false;
        }

        // Выполняет команду с параметром и обрабатывает ошибки
        public async void Execute(object? parameter)
        {
            try
            {
                // Выполняем команду с параметром если тип совпадает
                if (parameter is T t)
                {
                    await _execute(t);
                }
                else if (parameter == null && default(T) == null) // Обрабатываем случай null для ссылочных типов
                {
                    await _execute(default);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выполнения команды<{typeof(T).Name}>: {ex.Message}");
            }
        }

        // Уведомляет об изменении состояния возможности выполнения
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}