using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LeagueCooldowns;
class Checks {
 [STAThread] static void Main() {
  var ahri=WikiParser.Abilities(File.ReadAllText("tests/fixtures/Ahri.html"));
  string[] expected={"7","9 / 8 / 7 / 6 / 5","12","140 / 120 / 100"};
  for(int i=0;i<4;i++) if(ahri[i].Cooldowns[0].Text!=expected[i]) throw new Exception("Ahri mismatch");
  foreach(string path in Directory.GetFiles("tests/fixtures","sample-*.tmp")) {
   var a=WikiParser.Abilities(File.ReadAllText(path));
   Console.WriteLine(Path.GetFileName(path)+" "+a.Count+" abilities, "+a.Sum(x=>x.Cooldowns.Count)+" cooldowns");
  }
  var window=new AppWindow();
  var flags=BindingFlags.Instance|BindingFlags.NonPublic;
  typeof(AppWindow).GetField("abilities",flags).SetValue(window,ahri);
  typeof(AppWindow).GetField("current",flags).SetValue(window,new Champion{Name="Ahri",Path="/en-us/Ahri"});
  typeof(AppWindow).GetMethod("Render",flags,null,Type.EmptyTypes,null).Invoke(window,null);
  window.Measure(new Size(660,520)); window.Arrange(new Rect(0,0,660,480)); window.UpdateLayout();
  var content=(FrameworkElement)window.Content;
  content.Measure(new Size(624,448));content.Arrange(new Rect(0,0,616,436));content.UpdateLayout();
  var bitmap=new RenderTargetBitmap(660,480,96,96,PixelFormats.Pbgra32); bitmap.Render(content);
  var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));
  using(var stream=File.Create("build/preview.png"))encoder.Save(stream);
  Console.WriteLine("Ahri assertions and WPF rendering passed.");
 }
}


