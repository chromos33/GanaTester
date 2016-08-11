﻿using Newtonsoft.Json;
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
    public sealed partial class StudyPage : Page
    {
        private StudyPage rootPage;
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
        public StudyPage()
        {
            this.InitializeComponent();
            random = new Random();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            Gana.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            Gana.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            drawingAttributes.Color = Windows.UI.Colors.Black;
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            Gana.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Cross, 1);
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
                    CheckCharacter(str);
                }
            }

        }
        private async Task<bool> CheckCharacter(string entry, bool answer = false)
        {
            
            if(entry == currentChar.Gana)
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
            Tuple<List<Character>, int> Transfer = e.Parameter as Tuple<List<Character>, int>;
            GanaList = Transfer.Item1;
            TimeLimit = Transfer.Item2;
            if(TimeLimit > 0)
            {
                dtTimeLeftInSeconds = new TimeSpan(0,0,TimeLimit * 5 * 60);
                ClockTimer = new DispatcherTimer();
                ClockTimer.Tick += updateClock;
                ClockTimer.Interval = new TimeSpan(0, 0, 1);
                ClockTimer.Start();
            }
            
            NextCharacter();
        }

        private void updateClock(object sender, object e)
        {
            if(dtTimeLeftInSeconds.TotalSeconds > 0)
            {
                dtTimeLeftInSeconds = dtTimeLeftInSeconds.Subtract(new TimeSpan(0, 0, 1));
                System.Diagnostics.Debug.WriteLine(dtTimeLeftInSeconds.TotalSeconds);
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
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                else
                {
                    Frame.Navigate(typeof(MainPage));
                }
            }
            
        }

        private void NextCharacter()
        {
            try
            {
                Answer.Content = "Show";
                SaveData();
                Gana.InkPresenter.StrokeContainer.Clear();
                Convert.ToInt32(true);
                int min = Int32.MaxValue;
                foreach(var gana in GanaList)
                {
                    if(gana.correct == 0)
                    {
                        min = 0;
                    }
                    else
                    {
                        if (gana.correct < min)
                        {
                            min = gana.correct;
                        }
                    }
                }
                var query = from gana in GanaList where gana.bToBeTested == true && gana.isActive && gana != currentChar && gana.correct == min select gana;
                int randomvalue = random.Next(0, query.Count());
                currentChar = query.ToList()[randomvalue];
                if(currentChar.isHiragana)
                {
                    Romaji.Text = currentChar.Romaji.ToUpper() + " (Hiragana)";
                }
                else
                {
                    Romaji.Text = currentChar.Romaji.ToUpper() + " (Katagana)";
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private async void Answer_Click(object sender, RoutedEventArgs e)
        {
            if (Answer.Content.ToString() == "Show")
            {
                Gana.InkPresenter.StrokeContainer.Clear();
                Answer.Content = "Next";
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
                        await CheckCharacter(str,true);
                    }
                }
                Romaji.Text = currentChar.Gana;
            }
            else
            {
                Answer.Content = "Show";
                NextCharacter();
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
        private async void SaveData()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile ganafile = await storageFolder.GetFileAsync("gana.json");
            string jsongana = JsonConvert.SerializeObject(GanaList);
            await Windows.Storage.FileIO.WriteTextAsync(ganafile, jsongana);
        }
    }
}
