using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace LeagueCooldowns {
    public class AppWindow : Window {
        readonly string cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LeagueCooldowns");
        readonly HttpClient client = new HttpClient();
        List<Champion> champions = new List<Champion>();
        List<Ability> abilities = new List<Ability>();
        Champion current;
        int request;
        TextBox search;
        ListBox suggestions;
        TextBlock heading, status;
        Grid table;
        Button refresh, source;
        readonly Brush muted = new SolidColorBrush(Color.FromRgb(151, 166, 178));
        public AppWindow() {
            Title = "League Cooldowns";
            Width = 660; Height = 550; MinWidth = 560; MinHeight = 400;
            Background = new SolidColorBrush(Color.FromRgb(17, 23, 30));
            Foreground = Brushes.WhiteSmoke; FontFamily = new FontFamily("Segoe UI");
            FontSize = 13; WindowStartupLocation = WindowStartupLocation.CenterScreen;
            string xaml = @"<Grid xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Margin='18'>
<Grid.Resources>
<Style TargetType='Button'><Setter Property='Background' Value='#26333F'/><Setter Property='Foreground' Value='#E6EDF3'/><Setter Property='BorderThickness' Value='0'/><Setter Property='Padding' Value='12,7'/><Setter Property='Cursor' Value='Hand'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Button'><Border Background='{TemplateBinding Background}' CornerRadius='4' Padding='{TemplateBinding Padding}'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border><ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'><Setter Property='Background' Value='#354957'/></Trigger><Trigger Property='IsEnabled' Value='False'><Setter Property='Opacity' Value='0.45'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
<Style TargetType='ComboBox'><Setter Property='Foreground' Value='#E6EDF3'/><Setter Property='Background' Value='#202C36'/><Setter Property='BorderBrush' Value='#3B4B59'/><Setter Property='Padding' Value='5'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ComboBox'><Grid><ToggleButton Focusable='False' Background='{TemplateBinding Background}' Foreground='{TemplateBinding Foreground}' BorderBrush='{TemplateBinding BorderBrush}' IsChecked='{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'><ToggleButton.Template><ControlTemplate TargetType='ToggleButton'><Border Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='1' CornerRadius='3'><ContentPresenter/></Border></ControlTemplate></ToggleButton.Template><Grid Margin='5'><Grid.ColumnDefinitions><ColumnDefinition/><ColumnDefinition Width='16'/></Grid.ColumnDefinitions><TextBlock Text='{Binding SelectedItem, RelativeSource={RelativeSource AncestorType=ComboBox}}' TextTrimming='CharacterEllipsis' VerticalAlignment='Center'/><TextBlock Grid.Column='1' Text='⌄'/></Grid></ToggleButton><Popup IsOpen='{TemplateBinding IsDropDownOpen}' Placement='Bottom' AllowsTransparency='True' Focusable='False'><Border Background='#202C36' BorderBrush='#3B4B59' BorderThickness='1' MinWidth='{Binding ActualWidth, RelativeSource={RelativeSource TemplatedParent}}'><ScrollViewer MaxHeight='280'><ItemsPresenter/></ScrollViewer></Border></Popup></Grid></ControlTemplate></Setter.Value></Setter></Style>
<Style TargetType='ComboBoxItem'><Setter Property='Foreground' Value='#E6EDF3'/><Setter Property='Padding' Value='7'/></Style>
</Grid.Resources>
<Grid.RowDefinitions><RowDefinition Height='Auto'/><RowDefinition Height='Auto'/><RowDefinition Height='Auto'/><RowDefinition Height='*'/><RowDefinition Height='Auto'/></Grid.RowDefinitions>
<DockPanel><TextBlock Text='League Cooldowns' FontSize='21' FontWeight='SemiBold'/><CheckBox Name='pin' Content='Keep on top' Foreground='#97A6B2' HorizontalAlignment='Right' VerticalAlignment='Center'/></DockPanel>
<Grid Grid.Row='1' Margin='0,18,0,0'><Grid.RowDefinitions><RowDefinition Height='Auto'/><RowDefinition Height='Auto'/></Grid.RowDefinitions><TextBlock Text='Find a champion' Foreground='#97A6B2' Margin='0,0,0,6'/><TextBox Name='search' Grid.Row='1' Background='#1E2933' Foreground='#F0F5F7' CaretBrush='#57D5C3' BorderBrush='#3B4B59' Padding='11,8' FontSize='15' AutomationProperties.Name='Search champion'/></Grid>
<StackPanel Grid.Row='2' Margin='0,18,0,14'><TextBlock Name='heading' Text='Choose a champion' FontSize='18' FontWeight='SemiBold'/><TextBlock Text='Base cooldowns · seconds · before ability haste' Foreground='#97A6B2' Margin='0,5,0,0'/></StackPanel>
<ScrollViewer Grid.Row='3' VerticalScrollBarVisibility='Auto' HorizontalScrollBarVisibility='Disabled'><Grid Name='table'/></ScrollViewer>
<DockPanel Grid.Row='4' Margin='0,16,0,0'><StackPanel DockPanel.Dock='Right' Orientation='Horizontal'><Button Name='refresh' Content='Refresh' IsEnabled='False'/><Button Name='source' Content='Wiki ↗' Margin='8,0,0,0' IsEnabled='False'/></StackPanel><TextBlock Name='status' Text='Loading champion list…' Foreground='#97A6B2' TextWrapping='Wrap' VerticalAlignment='Center' Margin='0,0,10,0'/></DockPanel>
<Border Name='suggestionBorder' Grid.Row='2' Grid.RowSpan='2' Panel.ZIndex='10' VerticalAlignment='Top' Background='#202C36' BorderBrush='#3B4B59' BorderThickness='1' Visibility='Collapsed'><ListBox Name='suggestions' Background='#202C36' Foreground='#E6EDF3' BorderThickness='0' MaxHeight='220' ScrollViewer.VerticalScrollBarVisibility='Auto'/></Border>
</Grid>";
            var root = (Grid)XamlReader.Parse(xaml);
            Content = root;
            search = (TextBox)root.FindName("search");
            suggestions = (ListBox)root.FindName("suggestions");
            var suggestionBorder = (Border)root.FindName("suggestionBorder");
            heading = (TextBlock)root.FindName("heading"); status = (TextBlock)root.FindName("status");
            table = (Grid)root.FindName("table");
            refresh = (Button)root.FindName("refresh"); source = (Button)root.FindName("source");
            var pin = (CheckBox)root.FindName("pin");
            pin.Checked += delegate { Topmost = true; }; pin.Unchecked += delegate { Topmost = false; };
            search.TextChanged += delegate {
                var query = Normalize(search.Text);
                suggestions.ItemsSource = champions.Where(c => Normalize(c.Name).Contains(query)).OrderByDescending(c => Normalize(c.Name).StartsWith(query)).Take(12).ToList();
                suggestionBorder.Visibility = search.IsKeyboardFocusWithin && query.Length > 0 && suggestions.Items.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            };
            Action choose = () => {
                var champion = suggestions.SelectedItem as Champion;
                if (champion == null && suggestions.Items.Count > 0) champion = (Champion)suggestions.Items[0];
                if (champion == null) return;
                search.Text = champion.Name; suggestionBorder.Visibility = Visibility.Collapsed;
                LoadChampion(champion);
            };
            search.PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Down) { suggestions.SelectedIndex = Math.Min(suggestions.SelectedIndex + 1, suggestions.Items.Count - 1); e.Handled = true; }
                if (e.Key == Key.Up) { suggestions.SelectedIndex = Math.Max(0, suggestions.SelectedIndex - 1); e.Handled = true; }
                if (e.Key == Key.Enter) { choose(); e.Handled = true; }
                if (e.Key == Key.Escape) { suggestionBorder.Visibility = Visibility.Collapsed; e.Handled = true; }
            };
            suggestions.PreviewMouseLeftButtonUp += delegate { choose(); };
            suggestions.KeyDown += (s,e) => { if(e.Key == Key.Enter) choose(); };
            refresh.Click += delegate { if (current != null) LoadChampion(current); };
            source.Click += delegate { if(current != null) System.Diagnostics.Process.Start("https://wiki.leagueoflegends.com" + current.Path); };
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("LeagueCooldowns/1.0");
            Loaded += async delegate { await LoadRoster(); search.Focus(); };
            Closed += delegate { request++; client.Dispose(); };
        }
        static string Normalize(string value) { return new string(value.ToLowerInvariant().Where(Char.IsLetterOrDigit).ToArray()); }
        void Save(string path, string value) {
            try { Directory.CreateDirectory(cache); File.WriteAllText(path, value); } catch (IOException) {} catch (UnauthorizedAccessException) {}
        }
        async Task LoadRoster() {
            string path = Path.Combine(cache, "champions.tsv");
            try {
                string local = File.Exists(path) ? path : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "champions.tsv");
                champions = File.ReadAllLines(local).Where(l => l.Contains("\t")).Select(l => l.Split('\t')).Select(p => new Champion { Name=p[0], Path=p[1] }).ToList();
                status.Text = "Type a champion name to begin.";
                string html = await client.GetStringAsync("https://wiki.leagueoflegends.com/en-us/List_of_champions");
                champions = WikiParser.Roster(html);
                Save(path, String.Join(Environment.NewLine, champions.Select(c => c.Name + "\t" + c.Path)));
            } catch (Exception) {
                if(current == null) status.Text = champions.Count > 0 ? "Using saved champion list. Select a champion to fetch cooldowns." : "Champion list unavailable. Restart to retry.";
            }
        }
        async void LoadChampion(Champion champion) {
            int version = ++request;
            current = champion; heading.Text = champion.Name;
            table.Children.Clear(); status.Text = "Fetching cooldowns from League Wiki…";
            refresh.IsEnabled = false; source.IsEnabled = true;
            string path = Path.Combine(cache, Uri.EscapeDataString(champion.Path) + ".html");
            try {
                string html = await client.GetStringAsync("https://wiki.leagueoflegends.com" + champion.Path);
                var parsed = WikiParser.Abilities(html);
                if (version != request) return;
                abilities = parsed; Save(path, html); Render();
                status.Text = "Fetched " + DateTime.Now.ToString("HH:mm") + " · League Wiki";
            } catch (Exception ex) {
                if (version != request) return;
                try {
                    abilities = WikiParser.Abilities(File.ReadAllText(path)); Render();
                    status.Text = "Offline copy · " + File.GetLastWriteTime(path).ToString("dd MMM HH:mm") + " · Refresh to retry";
                } catch {
                    status.Text = ex is InvalidOperationException ? ex.Message : "Could not fetch the wiki. Check your connection and try Refresh.";
                }
            } finally { if(version == request) refresh.IsEnabled = true; }
        }
        TextBlock Text(string value, double size, Brush color) {
            return new TextBlock { Text=value, FontSize=size, Foreground=color, TextWrapping=TextWrapping.Wrap, Margin=new Thickness(8,6,8,6) };
        }
        void Render() {
            table.Children.Clear(); table.ColumnDefinitions.Clear();
            table.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(48) });
            var ranks = new StackPanel();
            ranks.Children.Add(new Border { Height=81 });
            int maxRanks = current.Name == "Jayce" || current.Name == "Udyr" ? 6 : 5;
            for(int i=1; i<=maxRanks; i++) ranks.Children.Add(new TextBlock { Text=i.ToString(), Height=26, Foreground=muted, VerticalAlignment=VerticalAlignment.Center, Margin=new Thickness(12,0,0,0) });
            table.Children.Add(ranks);
            int column=1;
            foreach(string slot in new[] {"Q","W","E","R"}) {
                table.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(1,GridUnitType.Star) });
                var variants=abilities.Where(a=>a.Slot==slot).ToList();
                var panel=new StackPanel();
                var key=Text(slot,20,new SolidColorBrush(Color.FromRgb(87,213,195))); key.FontWeight=FontWeights.SemiBold; key.Height=30;
                panel.Children.Add(key);
                var select=new ComboBox { ItemsSource=variants, Margin=new Thickness(5,2,5,5), Height=32, FontSize=11, ToolTip="Select ability or form" };
                var details=new StackPanel();
                panel.Children.Add(select); panel.Children.Add(details);
                Action update=()=>{
                    var ability=(Ability)select.SelectedItem;
                    if(ability==null)return;
                    select.ToolTip=ability.Name;
                    details.Children.Clear();
                    int count=slot=="R" ? 3 : maxRanks;
                    if(slot=="R" && new[]{"Karma","Elise","Nidalee"}.Contains(current.Name))count=4;
                    if(slot=="R" && current.Name=="Udyr")count=6;
                    if(slot=="R" && current.Name=="Jayce")count=1;
                    var primary=ability.Cooldowns.FirstOrDefault();
                    if(primary!=null && primary.Values!=null && primary.Values.Length>1)count=primary.Values.Length;
                    for(int rank=0;rank<maxRanks;rank++){
                        string value="—";
                        if(primary!=null && primary.Values!=null && rank<count) value=primary.Values.Length==1 ? primary.Values[0] : (rank<primary.Values.Length ? primary.Values[rank] : "—");
                        details.Children.Add(new TextBlock { Text=value, Height=26, FontSize=16, TextAlignment=TextAlignment.Center, Foreground=Brushes.WhiteSmoke });
                    }
                    if(primary==null) details.Children.Add(Text("No listed cooldown",11,muted));
                    foreach(var cooldown in ability.Cooldowns) {
                        if(cooldown!=primary || cooldown.Values==null) details.Children.Add(Text(cooldown.Label+" · "+cooldown.Text,11,muted));
                        if(!String.IsNullOrWhiteSpace(cooldown.Note)) details.ToolTip = cooldown.Note;
                    }
                };
                select.SelectionChanged += delegate { update(); };
                select.SelectedItem=variants.FirstOrDefault(a=>a.Cooldowns.Count>0) ?? variants.First();
                var border=new Border { Background=new SolidColorBrush(Color.FromRgb(25,35,44)), CornerRadius=new CornerRadius(6), Margin=new Thickness(3,0,3,0), Child=panel, Padding=new Thickness(0,0,0,8) };
                Grid.SetColumn(border,column++); table.Children.Add(border);
            }
        }
        [STAThread]
        public static void Main() {
            ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
            new Application().Run(new AppWindow());
        }
    }
}
