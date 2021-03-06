﻿using ILEmu.Structures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ILEmu.Values
{
    public class Int4 : IStructure
    {
        private const int size = 4;
        public override int Size => size;

        public int? Value { get; private set; }
        private readonly byte?[] cache = new byte?[size];

        public Int4()
        {
            Value = null;
        }
        public Int4(int value)
        {
            Value = value;
            UpdateCache();
        }

        public Int4(IEnumerable<byte> data, int start)
        {
            if (data.Count() - start < size)
                throw new ArgumentException("There is not enough bytes in data");
            var newCache = data.Skip(start).Take(size).ToArray();
            if (BitConverter.IsLittleEndian)
                newCache = newCache.Reverse().ToArray();
            for(int i = 0; i< size;i++)
            {
                int offset = i;
                TransformOffset(ref offset);
                cache[offset] = newCache[i];
            }
            UpdateValue();
        }
        public Int4(IEnumerable<byte?> data, int start)
        {
            if (data.Count() - start < size)
                throw new ArgumentException("There is not enough bytes in data");
            var newCache = data.Skip(start).Take(size).ToArray();
            if (BitConverter.IsLittleEndian)
                newCache = newCache.Reverse().ToArray();
            for (int i = 0; i < size; i++)
            {
                int offset = i;
                TransformOffset(ref offset);
                cache[offset] = newCache[i];
            }
            UpdateValue();
        }

        public void UpdateCache()
        {
            if (!Value.HasValue)
            {
                Array.Clear(cache, 0, Size);
                return;
            }
            var newCache = BitConverter.GetBytes(Value.Value);
            for (int i = 0; i < size; i++)
            {
                int offset = i;
                TransformOffset(ref offset);
                cache[offset] = newCache[i];
            }
        }
        public void UpdateValue()
        {
            if (cache.Any(c => !c.HasValue))
            {
                Value = null;
                return;
            }
            var toConvert = cache.Cast<byte>().ToArray();
            if (BitConverter.IsLittleEndian)
                toConvert = toConvert.Reverse().ToArray();
            Value = BitConverter.ToInt32(toConvert, 0);
        }

        public void Truncate(int size)
        {
            for (int i = size; i < Size; i++)
            {
                int offset = i;
                TransformOffset(ref offset);
                cache[offset] = 0;
            }
            UpdateValue();
        }

        public override byte? GetByte(int offset)
        {
            if (!InRange(offset))
                throw new ArgumentException("Offset is not in range");
            TransformOffset(ref offset);
            return cache[offset];
        }

        public override void SetByte(int offset, byte? data)
        {
            if (!InRange(offset))
                throw new ArgumentException("Offset is not in range");
            TransformOffset(ref offset);
            cache[offset] = data;
            UpdateValue();
        }
    }
}
