using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GanaTester
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<Character> Hiragana;
        List<Character> Katagana;
        public MainPage()
        {
            this.InitializeComponent();
            initGUI();
            
        }
        public async void initGUI()
        {
            Hiragana = new List<Character>();
            Katagana = new List<Character>();
            if (!(await FileExists("kata.json") && await FileExists("hira.json")))
            {
                await SetupGana();
            }
            else
            {
                await LoadGana();
            }
            int CharactersPerLine = (int)(Window.Current.Bounds.Width / 100);

            int i = 0;
            int lines = -1;
            int columscreated = 0;
            foreach (Character gana in Hiragana)
            {
                if (i == CharactersPerLine || i == 0)
                {
                    RowDefinition newrow = new RowDefinition();
                    newrow.Height = new GridLength(1, GridUnitType.Star);
                    HiraganaList.RowDefinitions.Add(newrow);
                    lines++;
                    i = 0;
                   
                }
                if (columscreated != CharactersPerLine)
                {
                    ColumnDefinition newcol = new ColumnDefinition();
                    newcol.Width = new GridLength(1, GridUnitType.Star);
                    HiraganaList.ColumnDefinitions.Add(newcol);
                    columscreated++;
                }
                CheckBox checkbox = new CheckBox();
                checkbox.Content = gana.Gana;
                checkbox.HorizontalAlignment = HorizontalAlignment.Left;
                checkbox.VerticalAlignment = VerticalAlignment.Center;
                HiraganaList.Children.Add(checkbox);
                Grid.SetRow(checkbox, lines);
                Grid.SetColumn(checkbox, i);
                i++;
            }

            i = 0;
            lines = -1;
            columscreated = 0;
            foreach (Character gana in Katagana)
            {
                if (i == CharactersPerLine || i == 0)
                {
                    RowDefinition newrow = new RowDefinition();
                    newrow.Height = new GridLength(1, GridUnitType.Star);
                    KataganaList.RowDefinitions.Add(newrow);
                    lines++;
                    i = 0;

                }
                if (columscreated != CharactersPerLine)
                {
                    ColumnDefinition newcol = new ColumnDefinition();
                    newcol.Width = new GridLength(1, GridUnitType.Star);
                    KataganaList.ColumnDefinitions.Add(newcol);
                    columscreated++;
                }
                CheckBox checkbox = new CheckBox();
                checkbox.Content = gana.Gana;
                checkbox.HorizontalAlignment = HorizontalAlignment.Left;
                checkbox.VerticalAlignment = VerticalAlignment.Center;
                KataganaList.Children.Add(checkbox);
                Grid.SetRow(checkbox, lines);
                Grid.SetColumn(checkbox, i);
                i++;
            }
        }
        public static async Task<bool> FileExists(string _filename)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(_filename);
                return true;
            }
            catch (FileNotFoundException ex)
            {
                return false;
            }
        }
        private async Task<bool> LoadGana()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile katafile = await storageFolder.GetFileAsync("kata.json");
            Windows.Storage.StorageFile hirafile = await storageFolder.GetFileAsync("hira.json");
            string kata = await Windows.Storage.FileIO.ReadTextAsync(katafile);
            string hira = await Windows.Storage.FileIO.ReadTextAsync(hirafile);
            Katagana = JsonConvert.DeserializeObject<List<Character>>(kata);
            Hiragana = JsonConvert.DeserializeObject<List<Character>>(hira);
            return true;
        }
        private async Task<bool> SetupGana()
        {
            // Simple CSV Data with Hira-/Katagana and Romaji
            #region romaji
            string sRomajiCSV = "a,i,u,e,o,ka,ki,ku,ke,ko,sa,si,su,se,so,ta,ti,tu,te,to,na,ni,nu,ne,no,ha,hi,hu,he,ho,ma,mi,mu,me,mo,ya,yu,yo,ra,ri,ru,re,ro,wa,wo,n";
            string[] aRomaji = sRomajiCSV.Split(',');
            #endregion
            #region Hiragana
            string sHiraganaCSV = "あ,い,う,え,お,か,き,く,け,こ,さ,し,す,せ,そ,た,ち,つ,て,と,な,に,ぬ,ね,の,は,ひ,ふ,へ,ほ,ま,み,む,め,も,や,ゆ,よ,ら,り,る,れ,ろ,わ,を,ん";
            string[] aHiragana = sHiraganaCSV.Split(',');
            if (aRomaji.Count() == aHiragana.Count())
            {
                for (int i = 0; i < sRomajiCSV.Split(',').Count(); i++)
                {
                    Character new_character = new Character(aHiragana[i], aRomaji[i]);
                    Hiragana.Add(new_character);
                }
            }

            string jsonhiragana = JsonConvert.SerializeObject(Hiragana);
            #endregion
            #region Katagana
            string sKataganaCSV = "ア,イ,ウ,エ,オ,カ,キ,ク,ケ,コ,サ,シ,ス,セ,ソ,タ,チ,ツ,テ,ト,ナ,ニ,ヌ,ネ,ノ,ハ,ヒ,フ,ヘ,ホ,マ,ミ,ム,メ,モ,ヤ,ユ,ヨ,ラ,リ,ル,レ,ロ,ワ,ヲ,ン";
            string[] aKatagana = sKataganaCSV.Split(',');
            if (aRomaji.Count() == aKatagana.Count())
            {
                for (int i = 0; i < sRomajiCSV.Split(',').Count(); i++)
                {
                    Character new_character = new Character(aKatagana[i], aRomaji[i]);
                    Katagana.Add(new_character);
                }
            }
            string jsonkatagana = JsonConvert.SerializeObject(Katagana);
            #endregion
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile katafile = await storageFolder.CreateFileAsync("kata.json");
            Windows.Storage.StorageFile hirafile = await storageFolder.CreateFileAsync("hira.json");
            await Windows.Storage.FileIO.WriteTextAsync(katafile, jsonhiragana);
            await Windows.Storage.FileIO.WriteTextAsync(hirafile, jsonhiragana);
            return true;
        }
    }
}
