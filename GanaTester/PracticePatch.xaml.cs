using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Text.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GanaTester
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PracticePage : Page
    {
        private PracticePage rootPage;
        InkRecognizerContainer inkRecognizerContainer = null;
        private IReadOnlyList<InkRecognizer> recoView = null;
        private Language previousInputLanguage = null;
        private CoreTextServicesManager textServiceManager = null;
        private ToolTip recoTooltip;
        private InkRecognizer japrecog;
        List<Character> GanaList = null;
        Character currentChar = null;
        Random random;
        int TimeLimit = 0;
        TimeSpan dtTimeLeftInSeconds;
        Windows.UI.Xaml.DispatcherTimer ClockTimer;
        public Boolean finish = false;
        public PracticePage()
        {
            this.InitializeComponent();
            random = new Random();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            Gana.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Size = new Size(10,10);
            Gana.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            drawingAttributes.Color = Windows.UI.Colors.Black;
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            Gana.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            inkRecognizerContainer = new InkRecognizerContainer();
            recoView = inkRecognizerContainer.GetRecognizers();
            if (recoView.Count() > 0)
            {
                foreach (InkRecognizer recognizer in recoView)
                {
                    if (recognizer.Name == "Microsoft 日本語手書き認識エンジン")
                    {
                        inkRecognizerContainer.SetDefaultRecognizer(recognizer);

                    }
                }
            }
            else
            {
                ShowMessage("Please Install Japanese Handwriting (www.pinyinjoe.com/windows-10/windows-10-chinese-handwriting-speech-display-language-packs.htm)");
            }
        }
        private async void ShowMessage(string message)
        {
            var dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }
        private async void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            IReadOnlyList<InkStroke> currentStrokes = Gana.InkPresenter.StrokeContainer.GetStrokes();
            if (currentStrokes.Count > 0)
            {

                var recognitionResults = await inkRecognizerContainer.RecognizeAsync(Gana.InkPresenter.StrokeContainer, InkRecognitionTarget.All);
                if (recognitionResults.Count > 0)
                {
                    // Display recognition result
                    string str = "";
                    foreach (var r in recognitionResults)
                    {
                        str += r.GetTextCandidates()[0];
                    }
                    CheckCharacter(str,false, currentStrokes.Count);
                }
            }

        }
        private async Task<bool> CheckCharacter(string entry, bool answer = false,int strokecount = 0)
        {
            if (entry == currentChar.Gana && currentChar.strokeCount == strokecount)
            {
                // Correct
                CorrectState.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 255, 0));
                await Task.Delay(TimeSpan.FromMilliseconds(250));
                CorrectState.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 255, 0));
                currentChar.correct++;
                NextCharacter();
            }
            if (entry != currentChar.Gana && answer)
            {
                // Incorrect
                CorrectState.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
                await Task.Delay(TimeSpan.FromMilliseconds(250));
                CorrectState.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 0, 0));
                currentChar.correct = 0;
            }
            return true;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            Tuple<List<Character>, int,bool> Transfer = e.Parameter as Tuple<List<Character>, int,bool>;
            GanaList = Transfer.Item1;
            TimeLimit = Transfer.Item2;
            if(ClockTimer != null)
            {
                ClockTimer.Stop();
            }
            if(TimeLimit > 2)
            {
                dtTimeLeftInSeconds = new TimeSpan(0,0,TimeLimit * 5 * 60);
                ClockTimer = new DispatcherTimer();
                ClockTimer.Tick += updateClock;
                ClockTimer.Interval = new TimeSpan(0, 0, 1);
                ClockTimer.Start();
            }
            else
            {
                if(TimeLimit > 0)
                {
                    dtTimeLeftInSeconds = new TimeSpan(0, 0, TimeLimit * 1 * 60);
                    ClockTimer = new DispatcherTimer();
                    ClockTimer.Tick += updateClock;
                    ClockTimer.Interval = new TimeSpan(0, 0, 1);
                    ClockTimer.Start();
                }
            }
            
            NextCharacter();
        }

        private void updateClock(object sender, object e)
        {
            if(dtTimeLeftInSeconds.TotalSeconds > 0)
            {
                dtTimeLeftInSeconds = dtTimeLeftInSeconds.Subtract(new TimeSpan(0, 0, 1));
                if(dtTimeLeftInSeconds.Seconds < 10)
                {
                    TimeLimitView.Text = dtTimeLeftInSeconds.Minutes + ":0" + dtTimeLeftInSeconds.Seconds;
                }
                else
                {
                    TimeLimitView.Text = dtTimeLeftInSeconds.Minutes + ":" + dtTimeLeftInSeconds.Seconds;
                }
                
            }
            else
            {
                finish = true;
            }
        }

        private void NextCharacter()
        {
            if(TimeLimit > 0)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                else
                {
                    Frame.Navigate(typeof(MainPage));
                }
            }
            try
            {
                Gana.InkPresenter.StrokeContainer.Clear();
                Convert.ToInt32(true);
                var query = from gana in GanaList where gana.bToBeTested == true && gana.isActive && gana != currentChar select gana;
                if (query.Count() > 0)
                {
                    int randomvalue = random.Next(0, query.Count() - 1);
                    currentChar = query.ToList()[randomvalue];
                }
                Preview.Text = currentChar.Gana;
                if (currentChar.isHiragana)
                {
                    Romaji.Text = currentChar.Romaji.ToUpper() + " (Hiragana)";
                }
                else
                {
                    Romaji.Text = currentChar.Romaji.ToUpper() + " (Katakana)";
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame.CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Gana.InkPresenter.StrokeContainer.Clear();
        }
    }
}
