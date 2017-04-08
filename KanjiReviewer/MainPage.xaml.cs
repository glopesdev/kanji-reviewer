using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        private void flipButton_Click(object sender, RoutedEventArgs e)
        {
            FlipAnimation.To = ActiveSide == CardSide.Front ? 180 : 360;
            FlipStoryboard.Begin();
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
