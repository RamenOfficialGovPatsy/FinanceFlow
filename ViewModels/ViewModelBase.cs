using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FinanceFlow.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        // Событие изменения свойств (стандарт MVVM)
        public event PropertyChangedEventHandler? PropertyChanged;

        // --- МЕХАНИЗМ УВЕДОМЛЕНИЙ ---
        // Событие, на которое подпишется Окно (View), чтобы показать MessageBox.
        // Параметры: (Заголовок, Текст сообщения)
        public event Action<string, string>? RequestNotification;

        // Стандартный метод уведомления UI об изменении свойства
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Метод для массового обновления (Исправление Layout Cycle / Рекурсии)
        protected void OnMultiplePropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        // Удобный сеттер для свойств
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // --- ХЕЛПЕРЫ ДЛЯ ВЫЗОВА СООБЩЕНИЙ ---
        // Эти методы будут доступны во всех ViewModel автоматически

        /// <summary>
        /// Показать сообщение об ошибке (Заголовок "Ошибка")
        /// </summary>
        protected void ShowError(string message)
        {
            RequestNotification?.Invoke("Ошибка", message);
        }

        // Добавляем перегрузку с заголовком
        protected void ShowError(string message, string title = "Ошибка")
        {
            RequestNotification?.Invoke(title, message);
        }

        /// <summary>
        /// Показать информационное сообщение (Заголовок "Внимание")
        /// </summary>
        protected void ShowInfo(string message)
        {
            RequestNotification?.Invoke("Внимание", message);
        }

        /// <summary>
        /// Показать сообщение об успехе (Заголовок "Успешно")
        /// </summary>
        protected void ShowSuccess(string message)
        {
            RequestNotification?.Invoke("Успешно", message);
        }
    }
}