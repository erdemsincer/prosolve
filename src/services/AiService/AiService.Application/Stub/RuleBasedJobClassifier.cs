using System.Text.RegularExpressions;
using AiService.Application.Abstractions;

namespace AiService.Application.Stub;

public sealed class RuleBasedJobClassifier : IJobClassifier
{
    private static readonly (string Pattern, string Tag, int Bump)[] Rules = new[]
    {
        ("musluk|lavabo|tesisat|su kaçır|boru", "Tesisat", 20),
        ("elektrik|priz|sigorta|kısa devre|lamba", "Elektrik", 25),
        ("beyaz ?eşya|buzdolab|çamaşır|bulaşık", "BeyazEşya", 20),
        ("klima|komb[iı]|petek", "IsıtmaSoğutma", 15),
        ("cam|pencere|kapı|marangoz|dolap", "Tamir", 10),
        ("boya|badana|sıva|duvar", "BoyaBadana", 10),
        ("telefon|bilgisayar|pc|laptop", "Elektronik", 10),
    };

    public (string[] Tags, int Urgency) Classify(string title, string description, string city, string district, string[] mediaKeys)
    {
        var txt = $"{title} {description}".ToLowerInvariant();
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int urgency = 10; // taban

        foreach (var (pattern, tag, bump) in Rules)
        {
            if (Regex.IsMatch(txt, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                tags.Add(tag);
                urgency += bump;
            }
        }

        // Medya varsa biraz aciliyet artır
        if (mediaKeys is { Length: >0 }) urgency += 5;

        // Şehir/ilçe valid ise küçük bir artış
        if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(district))
            urgency += 5;

        // 0..100 aralığına sıkıştır
        urgency = Math.Clamp(urgency, 0, 100);

        if (tags.Count == 0) tags.Add("Genel");

        return (tags.ToArray(), urgency);
    }
}
