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
        List<Character> Gana;
        List<CheckBox> CheckBoxes;
        private int adCount = 0;
        int TimeLimit = 0;
        public MainPage()
        {
            this.InitializeComponent();
            initGUI();
            
        }
        public async void initGUI()
        {
            Hiragana = new List<Character>();
            Katagana = new List<Character>();
            CheckBoxes = new List<CheckBox>();

            await SetupGana();
            if (!(await FileExists("gana.json")))
            {
                Gana = Hiragana.Union(Katagana).ToList();
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile ganafile = await storageFolder.CreateFileAsync("gana.json");
                string jsongana = JsonConvert.SerializeObject(Gana);
                await Windows.Storage.FileIO.WriteTextAsync(ganafile, jsongana);
            }
            if (!(await RoamingFileExists("gana.json")))
            {
                if ((await FileExists("gana.json")))
                {
                    await LoadGana();
                    await UpdateGana();
                }
                else
                {
                    Gana = Hiragana.Union(Katagana).ToList();
                }
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
                System.Diagnostics.Debug.WriteLine(Windows.Storage.ApplicationData.Current.RoamingFolder.Path);
                Windows.Storage.StorageFile ganafile = await storageFolder.CreateFileAsync("gana.json");
                string jsongana = JsonConvert.SerializeObject(Gana);
                await Windows.Storage.FileIO.WriteTextAsync(ganafile, jsongana);
            }
            else
            {
                await LoadRoamingGana();
                await UpdateGana();
            }
            int CharactersPerLine = (int)(Window.Current.Bounds.Width / 100);

            int i = 0;
            int lines = -1;
            int columscreated = 0;
            int[] KanaGroups = new int[] { 5,5,5,5,5,5,5,3,5,3};
            int GroupCounter = 0;
            int ItemsInGroup = 0;
            List<StackPanel> HiraganaStackPanels = new List<StackPanel>();
            foreach (Character gana in Hiragana)
            {
                if(ItemsInGroup == 0)
                {
                    StackPanel Panel = new StackPanel();
                    HiraganaStackPanels.Add(Panel);
                    System.Diagnostics.Debug.WriteLine(GroupCounter);
                    ToggleSwitch tsSwitch = new ToggleSwitch();
                    tsSwitch.Toggled += new RoutedEventHandler(GroupToggled);
                    GroupTag tag = new GanaTester.GroupTag(GroupCounter, 'h');
                    tsSwitch.Tag = tag;
                    HiraganaStackPanels[GroupCounter].Children.Add(tsSwitch);
                }
                gana.GroupID = GroupCounter;
                CheckBox checkbox = new CheckBox();
                checkbox.Content = gana.Gana+"/"+gana.Romaji;
                checkbox.HorizontalAlignment = HorizontalAlignment.Left;
                checkbox.VerticalAlignment = VerticalAlignment.Center;
                IEnumerable<Character> data = Gana.Where(x => x.Gana == gana.Gana);
                if(data.Count() > 0)
                {
                    checkbox.IsChecked = data.First().isActive;
                }
                else
                {
                    checkbox.IsChecked = false;
                }
                checkbox.Checked += new RoutedEventHandler(GanaChecked);
                checkbox.Unchecked += new RoutedEventHandler(GanaChecked);
                CheckBoxes.Add(checkbox);
                HiraganaStackPanels[GroupCounter].Children.Add(checkbox);
                if(KanaGroups[GroupCounter] == ItemsInGroup+1)
                {
                    ItemsInGroup = 0;
                    GroupCounter++;
                }
                else
                {
                    ItemsInGroup++;
                }
            }
            
            for(int stackcounter = 0; stackcounter < HiraganaStackPanels.Count(); stackcounter++)
            {
                try
                {
                    ColumnDefinition newcol = new ColumnDefinition();
                    newcol.Width = new GridLength(1, GridUnitType.Star);
                    HiraganaList.ColumnDefinitions.Add(newcol);
                    HiraganaList.Children.Add(HiraganaStackPanels[stackcounter]);
                    Grid.SetColumn(HiraganaStackPanels[stackcounter], stackcounter);
                }catch(Exception)
                {
                }
                
            }


            i = 0;
            lines = -1;
            columscreated = 0;
            GroupCounter = 0;
            ItemsInGroup = 0;
            List<StackPanel> KatakanaStackPanels = new List<StackPanel>();
            foreach (Character gana in Katagana)
            {
                if (ItemsInGroup == 0)
                {
                    StackPanel Panel = new StackPanel();
                    KatakanaStackPanels.Add(Panel);
                    System.Diagnostics.Debug.WriteLine(GroupCounter);
                    ToggleSwitch tsSwitch = new ToggleSwitch();
                    tsSwitch.Toggled += new RoutedEventHandler(GroupToggled);
                    GroupTag tag = new GanaTester.GroupTag(GroupCounter, 'k');
                    tsSwitch.Tag = tag;
                    KatakanaStackPanels[GroupCounter].Children.Add(tsSwitch);
                }
                gana.GroupID = GroupCounter;
                CheckBox checkbox = new CheckBox();
                checkbox.Content = gana.Gana + "/" + gana.Romaji;
                checkbox.HorizontalAlignment = HorizontalAlignment.Left;
                checkbox.VerticalAlignment = VerticalAlignment.Center;
                IEnumerable<Character> data = Gana.Where(x => x.Gana == gana.Gana);
                if (data.Count() > 0)
                {
                    checkbox.IsChecked = data.First().isActive;
                }
                else
                {
                    checkbox.IsChecked = false;
                }
                checkbox.Checked += new RoutedEventHandler(GanaChecked);
                checkbox.Unchecked += new RoutedEventHandler(GanaChecked);
                CheckBoxes.Add(checkbox);
                KatakanaStackPanels[GroupCounter].Children.Add(checkbox);
                if (KanaGroups[GroupCounter] == ItemsInGroup+1)
                {
                    ItemsInGroup = 0;
                    GroupCounter++;
                }
                else
                {
                    ItemsInGroup++;
                }
            }

            for (int stackcounter = 0; stackcounter < KatakanaStackPanels.Count(); stackcounter++)
            {
                try
                {
                    ColumnDefinition newcol = new ColumnDefinition();
                    newcol.Width = new GridLength(1, GridUnitType.Star);
                    KataganaList.ColumnDefinitions.Add(newcol);
                    KataganaList.Children.Add(KatakanaStackPanels[stackcounter]);
                    Grid.SetColumn(KatakanaStackPanels[stackcounter], stackcounter);
                }
                catch (Exception)
                {
                }

            }
        }

        private void GroupToggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch tsSwitch = (ToggleSwitch)sender;
            if(((GroupTag)tsSwitch.Tag).cGroup == 'h')
            {
                IEnumerable<Character> filter = Hiragana.Where(x => x.isHiragana == true && x.GroupID == ((GroupTag)tsSwitch.Tag).iGroupID);
                foreach(Character cChar in filter)
                {
                    IEnumerable<CheckBox> filter2 = CheckBoxes.Where(x => x.Content.ToString().Contains(cChar.Gana));
                    filter2.First().IsChecked = tsSwitch.IsOn;
                }
            }
            if (((GroupTag)tsSwitch.Tag).cGroup == 'k')
            {
                IEnumerable<Character> filter = Katagana.Where(x => x.isHiragana == false && x.GroupID == ((GroupTag)tsSwitch.Tag).iGroupID);
                foreach (Character cChar in filter)
                {
                    IEnumerable<CheckBox> filter2 = CheckBoxes.Where(x => x.Content.ToString().Contains(cChar.Gana));
                    filter2.First().IsChecked = tsSwitch.IsOn;
                }
            }
        }

        private async void GanaChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cbGana = (CheckBox)sender;
            IEnumerable<Character> filter = Gana.Where(x => cbGana.Content.ToString().Contains(x.Gana));
            foreach(Character item in filter)
            {
                item.bToBeTested = cbGana.IsChecked.Value;
                item.isActive = cbGana.IsChecked.Value;
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
        public static async Task<bool> RoamingFileExists(string _filename)
        {
            try
            {
                var file = await ApplicationData.Current.RoamingFolder.GetFileAsync(_filename);
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
            Windows.Storage.StorageFile ganafile = null;
            ganafile = await storageFolder.GetFileAsync("gana.json");
            
            string gana = await Windows.Storage.FileIO.ReadTextAsync(ganafile);
            Gana = JsonConvert.DeserializeObject<List<Character>>(gana);
            return true;
        }
        private async Task<bool> LoadRoamingGana()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
            Windows.Storage.StorageFile ganafile = null;
            ganafile = await storageFolder.GetFileAsync("gana.json");

            string gana = await Windows.Storage.FileIO.ReadTextAsync(ganafile);
            Gana = JsonConvert.DeserializeObject<List<Character>>(gana);
            return true;
        }
        private async Task<bool> SetupGana()
        {
            // Simple CSV Data with Hira-/Katagana and Romaji
            #region romaji
            string sRomajiCSV = "a,i,u,e,o,ka,ki,ku,ke,ko,sa,shi,su,se,so,ta,chi,tsu,te,to,na,ni,nu,ne,no,ha,hi,fu,he,ho,ma,mi,mu,me,mo,ya,yu,yo,ra,ri,ru,re,ro,wa,wo,n";
            string[] aRomaji = sRomajiCSV.Split(',');
            #endregion
            #region Hiragana
            string sHiraganaCSV = "あ,い,う,え,お,か,き,く,け,こ,さ,し,す,せ,そ,た,ち,つ,て,と,な,に,ぬ,ね,の,は,ひ,ふ,へ,ほ,ま,み,む,め,も,や,ゆ,よ,ら,り,る,れ,ろ,わ,を,ん";
            string[] aHiragana = sHiraganaCSV.Split(',');
            string sHiraganaStrokeCountCSV = "3,2,2,2,3,3,4,1,3,2,3,1,2,3,1,4,2,1,1,2,4,3,2,2,1,3,1,4,1,4,3,2,3,2,3,3,2,2,2,2,1,2,1,2,3,1";
            List<string> lHiraganaStrokeCount = sHiraganaStrokeCountCSV.Split(',').ToList();
            if (aRomaji.Count() == aHiragana.Count())
            {
                for (int i = 0; i < sRomajiCSV.Split(',').Count(); i++)
                {
                    Character new_character = new Character(aHiragana[i], aRomaji[i],true);
                    new_character.strokeCount = Int32.Parse(lHiraganaStrokeCount[i]);
                    Hiragana.Add(new_character);
                }
            }

            string jsonhiragana = JsonConvert.SerializeObject(Hiragana);
            #endregion
            #region Katagana
            string sKataganaCSV = "ア,イ,ウ,エ,オ,カ,キ,ク,ケ,コ,サ,シ,ス,セ,ソ,タ,チ,ツ,テ,ト,ナ,ニ,ヌ,ネ,ノ,ハ,ヒ,フ,ヘ,ホ,マ,ミ,ム,メ,モ,ヤ,ユ,ヨ,ラ,リ,ル,レ,ロ,ワ,ヲ,ン";
            string[] aKatagana = sKataganaCSV.Split(',');
            string sKataganaStrokeCountCSV = "2,2,3,3,3,2,3,2,3,2,3,3,2,2,2,3,3,3,3,2,2,2,2,4,1,2,2,1,1,4,2,3,2,2,3,2,2,3,2,2,2,1,3,2,3,2";
            List<string> lKataganaStrokeCount = sKataganaStrokeCountCSV.Split(',').ToList();
            if (aRomaji.Count() == aKatagana.Count())
            {
                for (int i = 0; i < sRomajiCSV.Split(',').Count(); i++)
                {
                    Character new_character = new Character(aKatagana[i], aRomaji[i],false);
                    new_character.strokeCount = Int32.Parse(lKataganaStrokeCount[i]);
                    Katagana.Add(new_character);
                }
            }
            #endregion
            return true;
        }
        private async Task<bool> UpdateGana()
        {
            bool updated = false;
            if (Gana.Where(x => x.strokeCount == 0).Count() > 0)
            {
                // use this if you need to add anything to existing Data
                string sKataganaCSV = "ア,イ,ウ,エ,オ,カ,キ,ク,ケ,コ,サ,シ,ス,セ,ソ,タ,チ,ツ,テ,ト,ナ,ニ,ヌ,ネ,ノ,ハ,ヒ,フ,ヘ,ホ,マ,ミ,ム,メ,モ,ヤ,ユ,ヨ,ラ,リ,ル,レ,ロ,ワ,ヲ,ン";
                string sKataganaStrokeCountCSV = "2,2,3,3,3,2,3,2,3,2,3,3,2,2,2,3,3,3,3,2,2,2,2,4,1,2,2,1,1,4,2,3,2,2,3,2,2,3,2,2,2,1,3,2,3,2";
                List<string> lKatagana = sKataganaCSV.Split(',').ToList();
                List<string> lKataganaStrokeCount = sKataganaStrokeCountCSV.Split(',').ToList();

                string sHiraganaCSV = "あ,い,う,え,お,か,き,く,け,こ,さ,し,す,せ,そ,た,ち,つ,て,と,な,に,ぬ,ね,の,は,ひ,ふ,へ,ほ,ま,み,む,め,も,や,ゆ,よ,ら,り,る,れ,ろ,わ,を,ん";
                string sHiraganaStrokeCountCSV = "3,2,2,2,3,3,4,1,3,2,3,1,2,3,1,4,2,1,1,2,4,3,2,2,1,3,1,4,1,4,3,2,3,2,3,3,2,2,2,2,1,2,1,2,3,1";
                List<string> lHiragana = sHiraganaCSV.Split(',').ToList();
                List<string> lHiraganaStrokeCount = sHiraganaStrokeCountCSV.Split(',').ToList();
                
                for (int i = 0; i < lKatagana.Count(); i++)
                {
                    try
                    {
                        Character updateh = Gana.Where(x => x.Gana == lHiragana[i]).First();
                        if (updateh.strokeCount == 0)
                        {
                            updated = true;
                            updateh.strokeCount = Int32.Parse(lHiraganaStrokeCount[i]);
                        }
                        Character updatek = Gana.Where(x => x.Gana == lKatagana[i]).First();
                        if (updatek.strokeCount == 0)
                        {
                            updated = true;
                            updatek.strokeCount = Int32.Parse(lKataganaStrokeCount[i]);
                        }
                    }
                    catch (Exception)
                    {
                        // will never be the case anyway just to make sure no crash happens
                    }

                }
                
            }
            if(Gana.Where(x => x.Romaji.ToLower() == "si").Count() > 0)
            {
                foreach(Character change in Gana.Where(x => x.Romaji.ToLower() == "si"))
                {
                    change.Romaji = "shi";
                }
                updated = true;
            }
            if (Gana.Where(x => x.Romaji.ToLower() == "ti").Count() > 0)
            {
                foreach (Character change in Gana.Where(x => x.Romaji.ToLower() == "ti"))
                {
                    change.Romaji = "chi";
                }
                updated = true;
            }
            if (Gana.Where(x => x.Romaji.ToLower() == "hu").Count() > 0)
            {
                foreach (Character change in Gana.Where(x => x.Romaji.ToLower() == "hu"))
                {
                    change.Romaji = "fu";
                }
                updated = true;
            }
            if (updated)
            {
                await SaveData();
            }
            return true;
        }

        private async void NavigateToTesting_Click(object sender, RoutedEventArgs e)
        {
            await SaveData();
            Tuple<List<Character>, int, bool> Transfer = new Tuple<List<Character>, int, bool>(Gana, TimeLimit, ModeSwitcher.IsOn);
            this.Frame.Navigate(typeof(StudyPage), Transfer);
        }
        private async void NavigateToPractice_Click(object sender, RoutedEventArgs e)
        {
            await SaveData();
            Tuple<List<Character>, int,bool> Transfer = new Tuple<List<Character>, int,bool>(Gana, TimeLimit, ModeSwitcher.IsOn);
            this.Frame.Navigate(typeof(PracticePage), Transfer);
        }

        private async void ResetData_Click(object sender, RoutedEventArgs e)
        {
            Gana = Hiragana.Union(Katagana).ToList();
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile ganafile = await storageFolder.GetFileAsync("gana.json");
            string jsongana = JsonConvert.SerializeObject(Gana);
            await Windows.Storage.FileIO.WriteTextAsync(ganafile, jsongana);
        }
        public bool GanaFileLock = false;
        private async Task<bool> SaveData()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            
            Windows.Storage.StorageFile ganafile = await storageFolder.GetFileAsync("gana.json");
            string jsongana = JsonConvert.SerializeObject(Gana);
            await Windows.Storage.FileIO.WriteTextAsync(ganafile, jsongana);
            return true;
        }

        private void Duration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox durationselector = (ComboBox)sender;
            TimeLimit = durationselector.SelectedIndex;
        }

        private void ToggleAllHira_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch tsSwitch = (ToggleSwitch)sender;
            foreach (Character cChar in Hiragana)
            {
                IEnumerable<CheckBox> filter2 = CheckBoxes.Where(x => x.Content.ToString().Contains(cChar.Gana));
                filter2.First().IsChecked = tsSwitch.IsOn;
            }
        }

        private void ToggleAllKata_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch tsSwitch = (ToggleSwitch)sender;
            foreach (Character cChar in Katagana)
            {
                IEnumerable<CheckBox> filter2 = CheckBoxes.Where(x => x.Content.ToString().Contains(cChar.Gana));
                filter2.First().IsChecked = tsSwitch.IsOn;
            }
        }
    }
}
