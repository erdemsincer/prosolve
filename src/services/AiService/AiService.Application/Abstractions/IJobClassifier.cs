namespace AiService.Application.Abstractions;

public interface IJobClassifier
{
    // Basit kural tabanlı: başlık + açıklama + şehir/ilçe ve medya anahtarlarından çıkarım
    (string[] Tags, int Urgency) Classify(string title, string description, string city, string district, string[] mediaKeys);
}
