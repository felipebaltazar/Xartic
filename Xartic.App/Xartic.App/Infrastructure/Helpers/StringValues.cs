using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Xartic.App.Infrastructure.Helpers
{
    /// <summary>
    /// Represents zero/null, one, or many strings in an efficient way.
    /// </summary>
    public struct StringValues : IList<string>, IReadOnlyList<string>, IEquatable<StringValues>, IEquatable<string>, IEquatable<string[]>
    {
        private static readonly string[] EmptyArray = new string[0];
        public static readonly StringValues Empty = new StringValues(EmptyArray);

        private class EncodedString
        {
            public EncodedString(string value, Encoding encoding)
            {
                _value = value;
                _encoding = encoding;
                _bytes = _encoding.GetBytes(value);
            }

            public readonly string _value;
            public readonly Encoding _encoding;
            public readonly byte[] _bytes;
        }

        private object _value;

        public StringValues(string value)
        {
            _value = value;
        }

        public StringValues(string[] values)
        {
            _value = values;
        }

        private StringValues(string value, Encoding encoding)
        {
            _value = new EncodedString(value, encoding);
        }

        /// <summary>
        /// Creates a new instance of <see cref="StringValues"/> with one string and compute its encoded bytes with the specified Encoding.
        /// </summary>
        /// <param name="value">The string to be encoded.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to be use when computing pre-encoded bytes.</param>
        /// <returns>A <see cref="StringValues"/> which contains the pre-encoded bytes.</returns>
        public static StringValues CreatePreEncoded(string value, Encoding encoding)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return new StringValues(value, encoding);
        }

        /// <summary>
        /// Try to retrieve the pre-encoded bytes of the <see cref="StringValues"/>. The return value indicates whether pre-encoded bytes exist.
        /// </summary>
        /// <param name="bytes">If successful, <paramref name="bytes"/> contains the pre-encoded bytes of the <see cref="StringValues"/>.</param>
        /// <param name="encoding">If successful, <paramref name="encoding"/> contains the <see cref="Encoding"/> used to compute the pre-encoded bytes.</param>
        /// <returns><c>true</c> if <see cref="StringValues"/> contains pre-encoded bytes, otherwise <c>false</c>.</returns>
        public bool TryGetPreEncoded(out byte[] bytes, out Encoding encoding)
        {
            var encoded = _value as EncodedString;

            if (encoded != null)
            {
                bytes = encoded._bytes;
                encoding = encoded._encoding;
                return true;
            }
            else
            {
                bytes = null;
                encoding = null;
                return false;
            }
        }

        public static implicit operator StringValues(string value)
        {
            return new StringValues(value);
        }

        public static implicit operator StringValues(string[] values)
        {
            return new StringValues(values);
        }

        public static implicit operator string(StringValues values)
        {
            return values.GetStringValue();
        }

        public static implicit operator string[](StringValues value)
        {
            return value.GetArrayValue();
        }

        public int Count
        {
            get
            {
                if (_value == null)
                {
                    return 0;
                }
                else if (_value is string || _value is EncodedString)
                {
                    return 1;
                }
                else
                {
                    return ((string[])_value).Length;
                }
            }
        }

        bool ICollection<string>.IsReadOnly
        {
            get { return true; }
        }

        string IList<string>.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        public string this[int index]
        {
            get
            {
                if (_value != null)
                {
                    var valueString = _value as string;
                    if (valueString != null && index == 0)
                    {
                        return valueString;
                    }

                    var encodedValue = _value as EncodedString;
                    if (encodedValue != null && index == 0)
                    {
                        return encodedValue._value;
                    }

                    var stringArray = _value as string[];
                    if (stringArray != null)
                    {
                        return stringArray[index];
                    }
                }

                return EmptyArray[0]; // throws
            }
        }

        public override string ToString()
        {
            return GetStringValue() ?? string.Empty;
        }

        private string GetStringValue()
        {
            if (_value != null)
            {
                var valueString = _value as string;
                if (valueString != null)
                {
                    return valueString;
                }

                var encodedValue = _value as EncodedString;
                if (encodedValue != null)
                {
                    return encodedValue._value;
                }

                var stringArray = _value as string[];
                if (stringArray != null)
                {
                    switch (stringArray.Length)
                    {
                        case 0: return null;
                        case 1: return stringArray[0];
                        default: return string.Join(",", stringArray);
                    }
                }
            }

            return null;
        }

        public string[] ToArray()
        {
            return GetArrayValue() ?? EmptyArray;
        }

        private string[] GetArrayValue()
        {
            if (_value != null)
            {
                var valueString = _value as string;
                if (valueString != null)
                {
                    return new[] { valueString };
                }

                var encodedValue = _value as EncodedString;
                if (encodedValue != null)
                {
                    return new[] { encodedValue._value };
                }

                var stringArray = _value as string[];
                if (stringArray != null)
                {
                    return stringArray;
                }
            }

            return null;
        }

        int IList<string>.IndexOf(string item)
        {
            return IndexOf(item);
        }

        private int IndexOf(string item)
        {
            if (_value != null)
            {
                var valueString = _value as string;
                if (valueString != null)
                {
                    return string.Equals(valueString, item, StringComparison.Ordinal) ? 0 : -1;
                }

                var encodedValue = _value as EncodedString;
                if (encodedValue != null)
                {
                    return string.Equals(encodedValue._value, item, StringComparison.Ordinal) ? 0 : -1;
                }

                var stringArray = _value as string[];
                if (stringArray != null)
                {
                    for (int i = 0; i < stringArray.Length; i++)
                    {
                        if (string.Equals(stringArray[i], item, StringComparison.Ordinal))
                        {
                            return i;
                        }
                    }
                    return -1;
                }
            }

            return -1;
        }

        bool ICollection<string>.Contains(string item)
        {
            return IndexOf(item) >= 0;
        }

        void ICollection<string>.CopyTo(string[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        private void CopyTo(string[] array, int arrayIndex)
        {
            if (_value != null)
            {
                var stringArray = _value as string[];
                if (stringArray != null)
                {
                    Array.Copy(stringArray, 0, array, arrayIndex, stringArray.Length);
                    return;
                }

                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                }
                if (array.Length - arrayIndex < 1)
                {
                    throw new ArgumentException(
                        $"'{nameof(array)}' is not long enough to copy all the items in the collection. Check '{nameof(arrayIndex)}' and '{nameof(array)}' length.");
                }

                var valueString = _value as string;
                if (valueString != null)
                {
                    array[arrayIndex] = valueString;
                    return;
                }

                var encodedValue = _value as EncodedString;
                if (encodedValue != null)
                {
                    array[arrayIndex] = encodedValue._value;
                    return;
                }
            }
        }

        void ICollection<string>.Add(string item)
        {
            throw new NotSupportedException();
        }

        void IList<string>.Insert(int index, string item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<string>.Remove(string item)
        {
            throw new NotSupportedException();
        }

        void IList<string>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void ICollection<string>.Clear()
        {
            throw new NotSupportedException();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static bool IsNullOrEmpty(StringValues value)
        {
            var v = value._value;

            if (v != null)
            {
                var valueString = v as string;
                if (valueString != null)
                {
                    return false;
                }

                var encodedValue = v as EncodedString;
                if (encodedValue != null)
                {
                    return false;
                }

                var stringArray = v as string[];
                if (stringArray != null)
                {
                    switch (stringArray.Length)
                    {
                        case 0: return true;
                        case 1: return string.IsNullOrEmpty(stringArray[0]);
                        default: return false;
                    }
                }
            }

            return true;
        }

        public static StringValues Concat(StringValues values1, StringValues values2)
        {
            var count1 = values1.Count;
            var count2 = values2.Count;

            if (count1 == 0)
            {
                return values2;
            }

            if (count2 == 0)
            {
                return values1;
            }

            var combined = new string[count1 + count2];
            values1.CopyTo(combined, 0);
            values2.CopyTo(combined, count1);
            return new StringValues(combined);
        }

        public static bool Equals(StringValues left, StringValues right)
        {
            var count = left.Count;

            if (count != right.Count)
            {
                return false;
            }

            for (var i = 0; i < count; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(StringValues left, StringValues right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StringValues left, StringValues right)
        {
            return !Equals(left, right);
        }

        public bool Equals(StringValues other)
        {
            return Equals(this, other);
        }

        public static bool Equals(string left, StringValues right)
        {
            return Equals(new StringValues(left), right);
        }

        public static bool Equals(StringValues left, string right)
        {
            return Equals(left, new StringValues(right));
        }

        public bool Equals(string other)
        {
            return Equals(this, new StringValues(other));
        }

        public static bool Equals(string[] left, StringValues right)
        {
            return Equals(new StringValues(left), right);
        }

        public static bool Equals(StringValues left, string[] right)
        {
            return Equals(left, new StringValues(right));
        }

        public bool Equals(string[] other)
        {
            return Equals(this, new StringValues(other));
        }

        public static bool operator ==(StringValues left, string right)
        {
            return Equals(left, new StringValues(right));
        }

        public static bool operator !=(StringValues left, string right)
        {
            return !Equals(left, new StringValues(right));
        }

        public static bool operator ==(string left, StringValues right)
        {
            return Equals(new StringValues(left), right);
        }

        public static bool operator !=(string left, StringValues right)
        {
            return !Equals(new StringValues(left), right);
        }

        public static bool operator ==(StringValues left, string[] right)
        {
            return Equals(left, new StringValues(right));
        }

        public static bool operator !=(StringValues left, string[] right)
        {
            return !Equals(left, new StringValues(right));
        }

        public static bool operator ==(string[] left, StringValues right)
        {
            return Equals(new StringValues(left), right);
        }

        public static bool operator !=(string[] left, StringValues right)
        {
            return !Equals(new StringValues(left), right);
        }

        public static bool operator ==(StringValues left, object right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringValues left, object right)
        {
            return !left.Equals(right);
        }
        public static bool operator ==(object left, StringValues right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(object left, StringValues right)
        {
            return !right.Equals(left);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return Equals(this, StringValues.Empty);
            }

            if (obj is string)
            {
                return Equals(this, (string)obj);
            }

            if (obj is string[])
            {
                return Equals(this, (string[])obj);
            }

            if (obj is StringValues)
            {
                return Equals(this, (StringValues)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (_value != null)
            {
                var valueString = _value as string;
                if (valueString != null)
                {
                    return valueString.GetHashCode();
                }

                var encodedValue = _value as EncodedString;
                if (encodedValue != null)
                {
                    return encodedValue._value.GetHashCode();
                }
            }

            return 0;
        }

        public struct Enumerator : IEnumerator<string>
        {
            private readonly StringValues _values;
            private string _current;
            private int _index;

            public Enumerator(StringValues values)
            {
                _values = values;
                _current = null;
                _index = 0;
            }

            public bool MoveNext()
            {
                var v = _values._value;

                if (v != null)
                {
                    var valueString = v as string;
                    if (valueString != null)
                    {
                        if (valueString != null && _index == 0)
                        {
                            _current = valueString;
                            _index = -1; // sentinel value
                            return true;
                        }
                    }

                    var encodedValue = v as EncodedString;
                    if (encodedValue != null)
                    {
                        if (encodedValue._value != null && _index == 0)
                        {
                            _current = encodedValue._value;
                            _index = -1; // sentinel value
                            return true;
                        }
                    }

                    var stringArray = v as string[];
                    if (stringArray != null)
                    {
                        if (_index < stringArray.Length)
                        {
                            _current = stringArray[_index];
                            _index++;
                            return true;
                        }

                        _current = null;
                        return false;
                    }
                }

                _current = null;
                return false;
            }

            public string Current => _current;

            object IEnumerator.Current => _current;

            void IEnumerator.Reset()
            {
                _current = null;
                _index = 0;
            }

            void IDisposable.Dispose()
            {
            }
        }
    }
}
