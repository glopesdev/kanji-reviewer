using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KanjiReviewer
{
    static class ControlObservable
    {
        public static IObservable<EventArgs> FromClick(AppBarButton button)
        {
            return Observable.FromEvent<RoutedEventHandler, EventArgs>(
                handler => button.Click += handler,
                handler => button.Click -= handler);
        }

        public static IObservable<TResult> FromClick<TResult>(AppBarButton button, TResult result)
        {
            return FromClick(button).Select(evt => result);
        }
    }
}
