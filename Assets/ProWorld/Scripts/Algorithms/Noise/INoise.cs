namespace ProWorldSDK
{
    public enum OutputMorph
    {
        Shift,
        Clamp,
        Abs
    }

    public interface INoise
    {
        float Noise(float x, float y);
    }
}
