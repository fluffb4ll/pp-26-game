namespace Structs
{
    public struct ShortenedValue
    {
        public float value;
        public string formatTemplate;

        public ShortenedValue(float value, string formatTemplate)
        {
            this.value = value;
            this.formatTemplate = formatTemplate;
        }
    }
}