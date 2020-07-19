namespace OpenBullet.Models
{
    public class ControlText<T>
    {
        public ControlText(T cType, string text)
        {
            Control = cType;
            Text = text;
        }
        public T Control { get; private set; }
        public string Text { get; private set; }
    }
}
