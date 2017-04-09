using System;
using System.Reactive;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KanjiReviewer
{
    static class ControlObservable
    {
        public static IObservable<EventPattern<RoutedEventArgs>> FromClick(AppBarButton button)
        {
            return Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                handler => button.Click += handler,
                handler => button.Click -= handler);
        }

        public static IObservable<TResult> FromClick<TResult>(AppBarButton button, TResult result)
        {
            return FromClick(button).Select(evt => result);
        }
    }
}
