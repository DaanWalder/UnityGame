namespace ProWorldSDK
{
    public interface IMap
    {
        float[,] GetArea(int resolution, float offsetX = 0, float offsetY = 0);
    }

    public interface ITexture
    {
        float[, ,] GetAlphas(WorldData data, int resolution);
    }

    public interface IEntity
    {
        Entity[] GetEntities(WorldData data);
    }
}
