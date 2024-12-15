namespace InformacijosKodavimas
{
    public abstract class AbstractCrypt
    {
        public readonly string Key;

        public AbstractCrypt(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length < 16)
                throw new ArgumentException("Raktas turi būti 16 ar daugiau simbolių ilgio.");
            Key = key[..16];
        }
    }
}
