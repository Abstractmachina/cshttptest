namespace MyApp
{
    public class TextPrompt
    {
        public string Text { get; set; }
        public float Weight { get; set; }

        public TextPrompt(string text, float weight)
        {
            Text = text;
            Weight = weight;
        }
    }
}
