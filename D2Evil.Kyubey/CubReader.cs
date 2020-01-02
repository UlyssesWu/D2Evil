using System;
using System.Collections.Generic;
using System.IO;
using D2Evil.Kyubey.Models;

namespace D2Evil.Kyubey
{
    public class CubReader : L2BinaryReader
    {
        //use IL2Object or not? string / null

        public static int SupportVersion = 11;

        public List<object> Objects { get; } = new List<object>();

        internal CubReader(Stream s) : base(s, true)
        {
        }

        public string ReadIdString(L2ObjType type = L2ObjType.Unknown)
        {
            var objType = ReadNumber();
            if (type != L2ObjType.Unknown && objType != (int) type)
            {
                throw new ArgumentException($"Type mismatch: Expect {type}({(int) type}) , Actual {objType}");
            }

            if (objType == (int) L2ObjType.ObjectRef)
            {
                var id = ReadInt32(); //WTF bad design
                return Objects[id]?.ToString(); //can be null, will be the first null in the list
            }

            string obj = null;
            switch (objType)
            {
                case (int) L2ObjType.Null:
                    break;
                case (int) L2ObjType.DrawDataID:
                    obj = ReadUTF8String();
                    break;
                case (int) L2ObjType.BaseDataID:
                    obj = ReadUTF8String();
                    break;
                case (int) L2ObjType.PartsDataID:
                    obj = ReadUTF8String();
                    break;
                case (int) L2ObjType.ParamID:
                    obj = ReadUTF8String();
                    break;

                case (int) L2ObjType.String:
                    obj = ReadUTF8String();
                    break;

                default:
                    throw new ArgumentOutOfRangeException("", $"Type is not a ID: {objType}");
            }

            Objects.Add(obj);
            return obj;
        }

        public object ReadObject(int objType = -1)
        {
            if (objType < 0) //Read Type first
            {
                objType = ReadNumber();
            }

            if (objType == (int) L2ObjType.ObjectRef)
            {
                var id = ReadInt32();
                return Objects[id];
            }

            object obj = null;
            switch (objType)
            {
                case (int) L2ObjType.Null:
                    obj = null;
                    break;
                case (int) L2ObjType.DrawDataID:
                    obj = ReadUTF8String();
                    break;
                case (int) L2ObjType.BaseDataID:
                    obj = ReadUTF8String();
                    break;
                case (int) L2ObjType.PartsDataID:
                    obj = ReadUTF8String();
                    break;
                case (int) L2ObjType.ParamID:
                    obj = ReadUTF8String();
                    break;

                case (int) L2ObjType.String:
                    obj = ReadUTF8String();
                    break;
                case (int) L2ObjType.Color:
                    obj = new L2Color(ReadInt32(), true);
                    break;
                case (int) L2ObjType.RectF:
                    obj = new L2RectF(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
                    break;
                case (int) L2ObjType.RectD:
                    obj = new L2RectF(ReadDouble(), ReadDouble(), ReadDouble(), ReadDouble());
                    break;
                case (int) L2ObjType.PointF:
                    obj = new L2PointF(ReadSingle(), ReadSingle());
                    break;
                case (int) L2ObjType.PointD:
                    obj = new L2PointF(ReadDouble(), ReadDouble());
                    break;
                case (int) L2ObjType.ObjectArray:
                    obj = ReadObjects();
                    break;
                case (int) L2ObjType.IntArray:
                case (int) L2ObjType.IntArray2:
                    obj = ReadIntArray();
                    break;
                case (int) L2ObjType.Matrix2x3:
                    obj = new L2Matrix2x3F(ReadDouble(), ReadDouble(), ReadDouble(), ReadDouble(), ReadDouble(),
                        ReadDouble());
                    break;
                case (int) L2ObjType.Rect:
                    obj = new L2Rect(ReadInt32(), ReadInt32(), ReadInt32(), ReadInt32());
                    break;
                case (int) L2ObjType.Point:
                    obj = new L2Point(ReadInt32(), ReadInt32());
                    break;
                case (int) L2ObjType.DoubleArray:
                    obj = ReadDoubleArray();
                    break;
                case (int) L2ObjType.FloatArray:
                    obj = ReadFloatArray();
                    break;
                case (int) L2ObjType.Array:
                    throw new NotImplementedException($"L2ObjType {L2ObjType.Array} is not implemented.");
                default:
                    if (objType >= L2Consts.SERIALIZABLE_START)
                    {
                        var t = (L2ObjType) objType;
                        obj = ReadKnownTypeObject(objType);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("", $"L2ObjType {objType} is not implemented.");
                    }

                    break;
            }

            Objects.Add(obj);
            return obj;
        }

        private ICubSerializable ReadKnownTypeObject(int objType)
        {
            switch (objType)
            {
                case (int) L2ObjType.ParamDefF:
                    return new Param(this);
                case (int) L2ObjType.ParamDefList:
                    return new ParamList(this);
                case (int) L2ObjType.ParamPivots:
                    return new Pivot(this);
                case (int) L2ObjType.PivotManager:
                    return new PivotList(this);
                case (int) L2ObjType.PartsData:
                    return new Part(this);
                case (int) L2ObjType.DDTexture:
                    return new Mesh(this);
                case (int) L2ObjType.CurvedSurfaceDeformer:
                    return new CurvedSurfaceDeformer(this);
                case (int) L2ObjType.RotationDeformer:
                    return new RotationDeformer(this);
                case (int) L2ObjType.AvatarPartsItem:
                    return new AvatarPart(this);
                case (int) L2ObjType.Affine:
                    return new Affine(this);
                case (int) L2ObjType.ModelImpl:
                    return new ModelData(this);
            }
#if DEBUG
            throw new NotImplementedException($"Type {objType} is unknown");
#endif
            return null; //TODO:
        }

        public T ReadKnownObject<T>() where T : ICubSerializable, new()
        {
            var t = ReadNumber();
            if (t == (int) L2ObjType.ObjectRef)
            {
                var id = ReadInt32();
                return (T) Objects[id];
            }

            if (!Enum.IsDefined(typeof(L2ObjType), t))
            {
                throw new ArgumentOutOfRangeException("", $"Unknown Type: {t}");
            }

            T obj = new T();
            obj.Read(this);
            Objects.Add(obj);
            return obj;
        }

        public object ReadObject(L2ObjType objType)
        {
            if (objType == L2ObjType.ObjectRef)
            {
                var id = ReadInt32();
                return Objects[id];
            }

            return ReadObject((int) objType);
        }

        public List<object> ReadObjects()
        {
            var len = ReadNumber();
            List<object> list = new List<object>(len);
            for (int i = 0; i < len; i++)
            {
                list.Add(ReadObject());
            }

            return list;
        }

        public string Read8Bit()
        {
            return Convert.ToString(ReadByte(), 2).PadLeft(8, '0');
        }

        //public CubReader(Stream s, ModelContextData ctx = null) : base(s)
        //{
        //    Context = ctx ?? new ModelContextData();
        //}

        //public CubReader(Stream s, Encoding e, bool leaveOpen, ModelContextData ctx = null) : base(s, e, leaveOpen)
        //{
        //    Context = ctx ?? new ModelContextData();
        //}
    }
}