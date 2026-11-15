using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace LeagueCooldowns {
    public class Champion {
        public string Name;
        public string Path;
        public override string ToString() { return Name; }
    }
    public class Cooldown {
        public string Label;
        public string Text;
        public string[] Values;
        public string Note;
    }
    public class Ability {
        public string Name;
        public string Slot;
        public List<Cooldown> Cooldowns = new List<Cooldown>();
        public override string ToString() { return Name; }
    }
    public static class WikiParser {
        const RegexOptions Options = RegexOptions.Singleline | RegexOptions.IgnoreCase;
        public static string Plain(string html) {
            return Regex.Replace(WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]+>", "")), @"\s+", " ").Trim();
        }
        public static List<Champion> Roster(string html) {
            var result = new List<Champion>();
            foreach (Match m in Regex.Matches(html, "data-champion=\"[^\"]+\" data-skin=\"Original\" data-game=\"lol\"><span[^>]*><a href=\"(/en-us/[^\"]+)\"[^>]*>([^<]+)<br", Options))
                result.Add(new Champion { Name = Plain(m.Groups[2].Value), Path = WebUtility.HtmlDecode(m.Groups[1].Value) });
            if (result.Count < 100) throw new InvalidOperationException("The wiki champion list could not be read.");
            return result.GroupBy(c => c.Path).Select(g => g.First()).OrderBy(c => c.Name).ToList();
        }
        public static List<Ability> Abilities(string html) {
            html = html.Replace("&#95;", "_");
            int start = html.IndexOf("id=\"Abilities\"", StringComparison.Ordinal);
            if (start < 0) throw new InvalidOperationException("The wiki did not return a champion ability page.");
            int end = html.IndexOf("<div class=\"mw-heading mw-heading2\">", start + 1, StringComparison.Ordinal);
            html = html.Substring(start, (end < 0 ? html.Length : end) - start);
            var slots = Regex.Matches(html, "<div class=\"skill skill_(\\w+)\"", Options);
            var result = new List<Ability>();
            for (int i = 0; i < slots.Count; i++) {
                string slot = slots[i].Groups[1].Value;
                if (slot == "innate") continue;
                if (slot == "aphelios") slot = "q";
                if (!"qwer".Contains(slot.Substring(0, 1))) continue;
                slot = slot.Substring(0, 1).ToUpperInvariant();
                string body = html.Substring(slots[i].Index, (i + 1 < slots.Count ? slots[i + 1].Index : html.Length) - slots[i].Index);
                var wrappers = Regex.Matches(body, "<div class=\"ability-info-stats__wrapper\">", Options);
                for (int j = 0; j < wrappers.Count; j++) {
                    int stop = body.IndexOf("<div class=\"ability-json", wrappers[j].Index, StringComparison.Ordinal);
                    if (stop < 0) stop = body.Length;
                    if (j + 1 < wrappers.Count) stop = Math.Min(stop, wrappers[j + 1].Index);
                    string stats = body.Substring(wrappers[j].Index, stop - wrappers[j].Index);
                    var name = Regex.Match(stats, "class=\"ability-info-stats__ability\">([\\s\\S]*?)</div>", Options);
                    if (!name.Success) continue;
                    var ability = new Ability { Name = Plain(name.Groups[1].Value), Slot = slot };
                    foreach (Match m in Regex.Matches(stats, "class=\"ability-info-stats__stat-label\">([\\s\\S]*?)</div>\\s*<div class=\"ability-info-stats__stat-value\">([\\s\\S]*?)</div>", Options)) {
                        string label = Plain(m.Groups[1].Value).TrimEnd(':');
                        if (!Regex.IsMatch(label, "cooldown|recharge", RegexOptions.IgnoreCase)) continue;
                        string value = Plain(m.Groups[2].Value);
                        string note = String.Join(" · ", Regex.Matches(m.Groups[2].Value, "title=\"([^\"]+)\"", Options).Cast<Match>().Select(t => Plain(t.Groups[1].Value)).Distinct());
                        string[] values = null;
                        if (Regex.IsMatch(value, @"^\d+(\.\d+)?(\s*/\s*\d+(\.\d+)?)*$"))
                            values = Regex.Split(value, @"\s*/\s*");
                        ability.Cooldowns.Add(new Cooldown { Label = label, Text = value, Values = values, Note = note });
                    }
                    result.Add(ability);
                }
            }
            if (!new[] { "Q", "W", "E", "R" }.All(slot => result.Any(a => a.Slot == slot)))
                throw new InvalidOperationException("The wiki layout has changed. Some ability slots could not be read.");
            return result;
        }
    }
}
