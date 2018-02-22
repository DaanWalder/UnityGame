using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class TextureLayer
    {
        public List<TextureArea> Textures { get; private set; }

        public TextureLayer()
        {
            Textures = new List<TextureArea>();
        }

        public TextureArea Add(TextureSplat ts, IMap mf)
        {
            var ta = new TextureArea(ts, mf);
            Textures.Add(ta);
            return ta;
        }
    }

    [Serializable]
    public class TextureArea : ISerializable
    {
        public TextureSplat Splat { get; set; }
        public IMap MapFunction { get; private set; }

        public bool[,] MaskFunction { get; private set; }   // Mask from function
        public bool[,] MaskArea { get; set; }               // Actual Mask used after subtraction from other layers

        public float[,] Alpha { get; set; }                 // Final alpha

        public TextureArea(TextureSplat ts, IMap mf)
        {
            Splat = ts;
            MapFunction = mf;
            MaskArea = new bool[0,0];
        }

        public void UpdateMask(int resolution, float x, float y)
        {
            var floatMask = MapFunction.GetArea(resolution, x, y);

            MaskFunction = Util.FloatToBoolArray(floatMask);
        }
        public void SetAsBase(int resolution)
        {
            MaskFunction = Util.SetArrayValue(resolution, true);
        }

        public TextureArea(SerializationInfo info, StreamingContext context)
        {
            Splat = (TextureSplat) info.GetValue("Splat", typeof (TextureSplat));
            MapFunction = (IMap) info.GetValue("MapFunction", typeof (IMap));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Splat", Splat);
            info.AddValue("MapFunction", MapFunction);
        }
    }

    [Serializable]
    public class TextureSplat : ISerializable
    {
        public Texture2D @Texture { get; set; }
        // Store as seriablizable vector 2  because normal vector2 x/y isn't directly editable
        public SerializableVector2 TileSize { get; set; }
        public SerializableVector2 TileOffset { get; set; }

        public TextureSplat()
        {
            @Texture = null;
            TileSize = new SerializableVector2(15, 15);
            TileOffset = new SerializableVector2(0, 0);
        }
        public TextureSplat(TextureSplat td)
        {
            @Texture = td.Texture;
            TileSize = td.TileSize;
            TileOffset = td.TileOffset;
        }
        public TextureSplat(Texture2D texture)
        {
            @Texture = texture;
            TileSize = new SerializableVector2(15, 15);
            TileOffset = new SerializableVector2(0, 0);
        }
        public void Set(TextureSplat ts)
        {
            @Texture = ts.Texture;
            TileSize = ts.TileSize;
            TileOffset = ts.TileOffset;
        }
        public SplatPrototype ToSplatPrototype()
        {
            return new SplatPrototype { texture = Texture, tileOffset = TileOffset.ToVector2(), tileSize = TileSize.ToVector2() };
        }

        public TextureSplat(SerializationInfo info, StreamingContext context)
        {
            TileSize = (SerializableVector2) info.GetValue("TileSize", typeof (SerializableVector2));
            TileOffset = (SerializableVector2) info.GetValue("TileOffset", typeof (SerializableVector2));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TileSize", TileSize);
            info.AddValue("TileOffset", TileOffset);
        }
    }
}