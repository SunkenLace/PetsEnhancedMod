using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.Text.Json;

namespace Pets_Enhanced_Mod.Utilities
{
    public static class SunkenLaceUtilities
    {
        /// <summary>Compares two types and determines whether they both are the same instance.</summary>
        /// <returns>True if value is null.</returns>
        /// <param name="result">Same value as entry.</param>
        public static bool IsNull<T>(this T entry, out T result) => (result = entry) is null;

        public static Vector2 OffsetByScale(int width, int height, float scale, bool anchorToOrigin = false)
        {
            float half_W = width / 2;
            float half_H = height / 2;
            return anchorToOrigin ? new Vector2((half_W * scale) - half_W, (half_H * scale) - half_H) : new Vector2(half_W * scale, half_H * scale);
        }
        public static float GetDecimal(this float _float) => _float - (int)_float;
        public static double GetDecimal(this double _double) => _double - (int)_double;
        public class ExceptionRequiredTypeNotPresent : Exception
        {
            public string RequiredType { get; set; }
            public string ReceivedType { get; set; }
            public int LineNumber { get; set; }
            public int LinePosition { get; set; }
            public string Path { get; set; }

            public ExceptionRequiredTypeNotPresent(string _required_type, string _received_type, int _line, int _position, string _path)
            {
                this.RequiredType = _required_type;
                this.ReceivedType = _received_type;
                this.LineNumber = _line;
                this.LinePosition = _position;
                this.Path = _path;
            }

        }
        public class SimpleException : Exception
        {
            public new string Message { get; set; }
            public SimpleException(string _message)
            {
                Message = _message;
            }
        }

        /// <summary>Returns whether this vector2 is within a square area.</summary>
        public static bool Intercepts(this Vector2 _vec, float _xMin, float _yMin, float _xMax, float _yMax)
        {
            return _vec.X >= _xMin && _vec.X <= _xMax && _vec.Y >= _yMin && _vec.Y <= _yMax;
        }




        /// <summary>Compares two types and determines whether they both are the same instance.</summary>
        /// <returns>True if they're the same instance. False if they're not, or atleast one of them is null.</returns>
        public static bool CompareAndNullCheck<T>(T compareA, T compareB) => compareA is not null && compareB is not null && compareA.Equals(compareB);




        /// <summary>Checks if an array contains an element which value equals the one given in a more efficient way.</summary>
        /// <returns>True or false.</returns>

        public static bool BetterContains<T>(this T[] _array, Predicate<T> _predicate, out T result)
        {
            result = default;
            bool found = false;
            if (_predicate is not null && _array is not null && _array.Length > 0)
            {
                var index = 0;
                while (index < _array.Length)
                {
                    if (!IsNull(_array[index], out var element) && _predicate(element))
                    {
                        found = true;
                        result = element;
                        break;
                    }
                    index++;
                }
            }

            return found;
        }
        public static bool BetterContainsReturnIndex<T>(this T[] _list, Predicate<T> _predicate, out int _index)
        {
            _index = 0;
            bool found = false;
            if (_predicate is not null && _list is not null && _list.Length > 0)
            {
                var index = -1;
                while (++index < _list.Length)
                {
                    if (!IsNull(_list[index], out var element) && _predicate(element))
                    {
                        found = true;
                        _index = index;
                        break;
                    }
                }
            }

            return found;
        }

    }
}
