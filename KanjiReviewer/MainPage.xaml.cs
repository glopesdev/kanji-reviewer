using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KanjiReviewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Settings settings;
        KanjiEntry currentEntry;

        public MainPage()
        {
            this.InitializeComponent();
            LayoutProjection.RegisterPropertyChangedCallback(PlaneProjection.RotationYProperty, (sender, dp) =>
            {
                UpdatePageLayout();
            });

            var drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Size = new Size(8, 8);
            drawingAttributes.Color = Colors.Black;
            drawingAttributes.IgnorePressure = true;
            drawingAttributes.FitToCurve = false;
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            inkCanvas.InkPresenter.InputDeviceTypes =
                CoreInputDeviceTypes.Mouse |
                CoreInputDeviceTypes.Pen |
                CoreInputDeviceTypes.Touch;

            InitializeSettings();
        }

        private void ApplicationData_DataChanged(ApplicationData sender, object args)
        {
            settings.Read(sender.RoamingFolder);
        }

        enum CardSide { Front, Back }

        CardSide ActiveSide
        {
            get
            {
                return LayoutProjection.RotationY > 90 && LayoutProjection.RotationY < 270 ?
                    CardSide.Back :
                    CardSide.Front;
            }
        }

        void ClearCanvas()
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            kanjiCanvas.Invalidate();
        }

        void UpdatePageLayout()
        {
            kanjiCanvas.Invalidate();
            if (ActiveSide == CardSide.Back)
            {
                // Show backside
                LayoutTransform.ScaleX = -1;
                kanjiBox.Visibility = Visibility.Visible;
                inkCanvas.Visibility = Visibility.Collapsed;
                if (kanjiBox.Child != null)
                {
                    noButton.IsEnabled = true;
                    yesButton.IsEnabled = true;
                    easyButton.IsEnabled = true;
                }
            }
            else
            {
                // Show frontside
                LayoutTransform.ScaleX = 1;
                kanjiBox.Visibility = Visibility.Collapsed;
                inkCanvas.Visibility = Visibility.Visible;
            }
        }

        async void InitializeSettings()
        {
            settings = await Settings.Create();
            ApplicationData.Current.DataChanged += ApplicationData_DataChanged;
            ApplicationData.Current.SignalDataChanged();

            var noClick = ControlObservable.FromClick(noButton, ReviewResult.No);
            var yesClick = ControlObservable.FromClick(yesButton, ReviewResult.Yes);
            var easyClick = ControlObservable.FromClick(easyButton, ReviewResult.Easy);

            var now = DateTime.UtcNow;
            var start = Observable.Return(ReviewResult.No);
            var review = noClick.Amb(yesClick).Amb(easyClick)
                                .Do(result => currentEntry.Review(result))
                                .Do(result => settings.Write(ApplicationData.Current.RoamingFolder))
                                .Take(1).Repeat();
            review = start.Concat(review);

            var expiredKanji = from entry in settings.Database.Take(settings.FrameNumber)
                               where now > entry.NextReview
                               orderby KanjiController.Random.Next()
                               select entry;
            var kanjiReview = expiredKanji.Concat(Enumerable.Repeat<KanjiEntry>(null, 1));
            var transition = review.Zip(kanjiReview, (result, entry) => entry);
            transition.Subscribe(entry => LoadKanji(entry));
        }

        async void LoadKanji(KanjiEntry entry)
        {
            if (entry == null)
            {
                PageTitle.Text = " ";
                kanjiBox.Child = null;
            }
            else
            {
                PageTitle.Text = entry.Meaning;
                var path = System.IO.Path.Combine("Assets\\Kanji", entry.StrokeSource);
                var strokeFile = await Package.Current.InstalledLocation.GetFileAsync(path);
                var xaml = await FileIO.ReadTextAsync(strokeFile);
                var content = (UIElement)XamlReader.Load(xaml);
                kanjiBox.Child = content;
                currentEntry = entry;
            }

            noButton.IsEnabled = false;
            yesButton.IsEnabled = false;
            easyButton.IsEnabled = false;
            LayoutProjection.RotationY = 0;
            UpdatePageLayout();
            ClearCanvas();
        }

        private void flipButton_Click(object sender, RoutedEventArgs e)
        {
            FlipAnimation.To = ActiveSide == CardSide.Front ? 180 : 360;
            FlipStoryboard.Begin();
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCanvas();
        }

        private void FlipStoryboard_Completed(object sender, object e)
        {
            if (LayoutProjection.RotationY == 360)
            {
                LayoutProjection.RotationY = 0;
            }
        }

        private void kanjiCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (ActiveSide == CardSide.Front)
            {
                args.DrawingSession.DrawInk(inkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            }
        }

        private void kanjiBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            kanjiBorder.Width = kanjiBorder.ActualHeight;
        }
    }
}
