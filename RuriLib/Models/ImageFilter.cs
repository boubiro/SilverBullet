namespace RuriLib.Models
{
    public class ImageFilter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Value))
                return Name + ":" + Value;
            else return Name;
        }
    }
}
