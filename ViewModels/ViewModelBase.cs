using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FinanceFlow.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        // Основное событие MVVM - уведомляет UI об изменении свойств
        public event PropertyChangedEventHandler? PropertyChanged;

        // МЕХАНИЗМ УВЕДОМЛЕНИЙ ДЛЯ ПОЛЬЗОВАТЕЛЯ

        // Событие для показа сообщений пользователю через MessageBox
        // View подписывается на это событие и показывает уведомления
        public event Action<string, string>? RequestNotification;

        // Стандартный метод уведомления UI об изменении свойства
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Метод для массового обновления нескольких свойств за один раз
        protected void OnMultiplePropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        // Умный сеттер для свойств - автоматически проверяет изменения и уведомляет UI
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            // Проверяем действительно ли значение изменилось
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            // Обновляем поле и уведомляем UI
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ СООБЩЕНИЙ ПОЛЬЗОВАТЕЛЮ

        /// Показать сообщение об ошибке (Заголовок "Ошибка")
        protected void ShowError(string message)
        {
            RequestNotification?.Invoke("Ошибка", message);
        }

        // Добавляем перегрузку с заголовком
        protected void ShowError(string message, string title = "Ошибка")
        {
            RequestNotification?.Invoke(title, message);
        }

        /// Показать информационное сообщение (Заголовок "Внимание")
        protected void ShowInfo(string message)
        {
            RequestNotification?.Invoke("Внимание", message);
        }

        /// Показать сообщение об успехе (Заголовок "Успешно")
        protected void ShowSuccess(string message)
        {
            RequestNotification?.Invoke("Успешно", message);
        }
    }
}