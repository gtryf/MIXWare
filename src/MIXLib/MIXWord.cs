using System;
using System.Collections.Generic;
using System.Linq;

namespace MIXLib
{
    [Serializable]
    public enum Sign
    {
        Positive,
        Negative
    }

    /*
     * Structure of a MIX Word:
     * +-----------------------+
     * | 0 | 1 | 2 | 3 | 4 | 5 |
     * +-----------------------+
     * | - | D | D | D | D | D |
     * +-----------------------+
     * 
     * Locations 1-5 are one byte (6 bits) each and contain numerical data.
     * Location 0 is a single bit and contains the sign of the word.
     * 
     * We choose (by convention) that 1 stands for negative while 0 stands for positive.
     */
    [Serializable]
    public class MIXWord
    {
        #region Constants

        private const int WORD_SIZE = 5;
        private const int BYTE_SIZE = 6;

        #endregion

        #region Constants (as properties)

        static MIXWord()
        {
            // BYTE_ZERO
            BYTE_ZERO = "";
            for (int i = 0; i < BYTE_SIZE; i++)
                BYTE_ZERO += "0";

            // BYTE_ONE
            BYTE_ONE = "";
            for (int i = 0; i < BYTE_SIZE; i++)
                BYTE_ONE += "1";

            // ALL_ZERO
            ALL_ZERO = "";
            for (int i = 0; i < WORD_SIZE; i++)
                ALL_ZERO += BYTE_ZERO;

            // BYTE_ONE
            ALL_ONE = "";
            for (int i = 0; i < WORD_SIZE; i++)
                ALL_ONE += BYTE_ONE;

            SIGN_POS = MINUS_MASK = Convert.ToInt32("1" + ALL_ZERO, 2);
            PLUS_MASK = Convert.ToInt32("0" + ALL_ONE, 2);
        }

        private static string BYTE_ZERO;
        private static string BYTE_ONE;

        private static string ALL_ZERO;
        private static string ALL_ONE;

        private static int SIGN_POS;
        private static int PLUS_MASK;
        private static int MINUS_MASK;

        public static int MaxValue
        {
            get
            {
                string one = "111111";
                return Convert.ToInt32(one + one + one + one + one, 2);
            }
        }

        #endregion

        #region Data handling

        private int data;

        #region Sign
        public Sign Sign
        {
            get
            {
                if ((data & SIGN_POS) == SIGN_POS)
                    return Sign.Negative;
                else
                    return Sign.Positive;
            }
            set
            {
                if (value == Sign.Positive)
                    data &= PLUS_MASK;
                else
                    data |= MINUS_MASK;
            }
        }
        #endregion

        #region Managing the whole of the value
        public int UnsignedValue
        {
			get => data & Convert.ToInt32(ALL_ONE, 2);
            set
            {
                if (value > Convert.ToInt32(ALL_ONE, 2))
                    throw new Exception("Value too large to fit in word.");

                data &= Convert.ToInt32("1" + ALL_ZERO, 2);
                data |= value;
            }
        }

        public int Value
        {
			get => (data & SIGN_POS) == SIGN_POS ? -UnsignedValue : UnsignedValue;
            set
            {
                this.UnsignedValue = Math.Abs(value);

                Sign = Sign.Positive;
                if (value < 0)
                    Sign = Sign.Negative;
            }
        }
        #endregion

        #region Fields

        public int this[byte index]
        {
            get
            {
                if (index < 0 || index > WORD_SIZE)
                    throw new Exception("Invalid word index.");

                if (index == 0)
                    return Sign == Sign.Positive ? 0 : 1;

                string strBitmask = BYTE_ONE + Enumerable.Repeat(BYTE_ZERO, (WORD_SIZE - index)).Aggregate("", (x, y) => x + y);
                int bitmask = Convert.ToInt32(strBitmask, 2);
                return (data & bitmask) >> ((WORD_SIZE - index) * BYTE_SIZE);
            }
            set
            {
                if (index < 0 || index > WORD_SIZE)
                    throw new Exception("Invalid word index.");

                if (index == 0)
                {
                    if (value == 1)
                        Sign = Sign.Negative;
                    else if (value == 0)
                        Sign = Sign.Positive;
                    else
                        throw new Exception("Invalid sign value.");
                }
                else
                {
                    byte leftCount = (byte)(index - 1);
                    byte rightCount = (byte)(WORD_SIZE - index);

                    int bitmaskClear = Convert.ToInt32(
                        "1" + // Sign
                        Enumerable.Repeat(BYTE_ONE, leftCount).Aggregate("", (x, y) => x + y) +
                        BYTE_ZERO +
                        Enumerable.Repeat(BYTE_ONE, rightCount).Aggregate("", (x, y) => x + y),
                        2);

                    string valString = Convert.ToString(Math.Abs(value), 2);
                    valString = Enumerable.Repeat("0", BYTE_SIZE - valString.Length).Aggregate("", (x, y) => x + y) + valString;

                    int bitmaskSet = Convert.ToInt32(
                        "0" +
                        Enumerable.Repeat(BYTE_ZERO, leftCount).Aggregate("", (x, y) => x + y) +
                        valString +
                        Enumerable.Repeat(BYTE_ZERO, rightCount).Aggregate("", (x, y) => x + y),
                        2);

                    data &= bitmaskClear;
                    data |= bitmaskSet;
                }
            }
        }

        public int this[byte l, byte r]
        {
            get
            {
                // If the fieldspecs are given in the wrong order
                if (l > r)
                {
                    byte t = r;
                    r = l;
                    l = t;
                }

                if (l < 0 || l > WORD_SIZE)
                    throw new Exception("Invalid word index (left).");

                if (r < 0 || r > WORD_SIZE)
                    throw new Exception("Invalid word index (right).");

                if (r == 0)
                    return this[0];

                bool useSign = false;
                if (l == 0)
                {
                    l++;
                    useSign = true;
                }

                byte leftCount = (byte)(l - 1);
                byte midCount = (byte)(1 + (r - l));
                byte rightCount = (byte)(WORD_SIZE - r);

                int bitmask = Convert.ToInt32(
                    Enumerable.Repeat(BYTE_ZERO, leftCount).Aggregate("", (x, y) => x + y) +
                    Enumerable.Repeat(BYTE_ONE, midCount).Aggregate("", (x, y) => x + y) +
                    Enumerable.Repeat(BYTE_ZERO, rightCount).Aggregate("", (x, y) => x + y),
                    2);

                int result = (data & bitmask) >> (rightCount * BYTE_SIZE);

                if (useSign && Sign == Sign.Negative)
                    return -result;

                return result;
            }
            set
            {
                // If the fieldspecs are given in the wrong order
                if (l > r)
                {
                    byte t = r;
                    r = l;
                    l = t;
                }

                if (l < 0 || l > WORD_SIZE)
                    throw new Exception("Invalid word index (left).");

                if (r < 0 || r > WORD_SIZE)
                    throw new Exception("Invalid word index (right).");

                if (r == 0)
                {
                    this[0] = value;
                    return;
                }

                bool useSign = false;
                if (l == 0)
                {
                    l++;
                    useSign = true;
                }

                byte leftCount = (byte)(l - 1);
                byte midCount = (byte)(1 + (r - l));
                byte rightCount = (byte)(WORD_SIZE - r);

                int bitmaskClear = Convert.ToInt32(
                    "1" + // Sign
                    Enumerable.Repeat(BYTE_ONE, leftCount).Aggregate("", (x, y) => x + y) +
                    Enumerable.Repeat(BYTE_ZERO, midCount).Aggregate("", (x, y) => x + y) +
                    Enumerable.Repeat(BYTE_ONE, rightCount).Aggregate("", (x, y) => x + y),
                    2);

                string valString = Convert.ToString(Math.Abs(value), 2);
                valString = Enumerable.Repeat("0", BYTE_SIZE * midCount - valString.Length).Aggregate("", (x, y) => x + y) + valString;

                int bitmaskSet = Convert.ToInt32(
                    "0" + // Sign
                    Enumerable.Repeat(BYTE_ZERO, leftCount).Aggregate("", (x, y) => x + y) +
                    valString +
                    Enumerable.Repeat(BYTE_ZERO, rightCount).Aggregate("", (x, y) => x + y),
                    2);

                data &= bitmaskClear;
                data |= bitmaskSet;

                if (useSign)
                {
                    // Clear sign
                    data &= PLUS_MASK;
                    Sign = value < 0 ? Sign.Negative : Sign.Positive;
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MIXWord() { Value = 0; }
        public MIXWord(byte v)
        {
            Value = v;
        }

        public MIXWord(int v)
        {
            Value = v;
        }

        public MIXWord(MIXWord w)
        {
            Value = w.Value;
        }

        #endregion

        #region Operators

        #region Conversions

		public static implicit operator int(MIXWord w) => w.Value;

		public static explicit operator byte(MIXWord w) => (byte)w.Value;

		public static implicit operator MIXWord(byte b) => new MIXWord(b);

		public static explicit operator MIXWord(int i) => new MIXWord((byte)i);

        #endregion

        #region Unary Operators

		public static MIXWord operator +(MIXWord w) => new MIXWord(w);

		public static MIXWord operator -(MIXWord w) => new MIXWord(-w.Value);

		public static MIXWord operator ++(MIXWord w) => new MIXWord(w + 1);

		public static MIXWord operator --(MIXWord w) => new MIXWord(w - 1);

        #endregion

        #region Binary Operators

        public static MIXWord operator +(MIXWord w1, MIXWord w2)
		    => new MIXWord(w1.Value + w2.Value);

        public static MIXWord operator -(MIXWord w1, MIXWord w2)
		    => new MIXWord(w1.Value - w2.Value);

        #endregion

        #region Comparison Operatod

        public static bool operator ==(MIXWord w1, MIXWord w2)
        {
            if ((object)w1 == null)
                return ((object)w2 == null);
            else if ((object)w2 == null)
                return ((object)w1 == null);
            else
                return w1.Equals(w2);
        }

        public static bool operator !=(MIXWord w1, MIXWord w2)
        {
            if ((object)w1 == null)
                return ((object)w2 != null);
            else if ((object)w2 == null)
                return ((object)w1 != null);
            else
                return !w1.Equals(w2);
        }

        public static bool operator <(MIXWord w1, MIXWord w2)
		    => w1.Value < w2.Value;

        public static bool operator >(MIXWord w1, MIXWord w2)
		    => w1.Value > w2.Value;

        public static bool operator <=(MIXWord w1, MIXWord w2)
		    => w1.Value <= w2.Value;

        public static bool operator >=(MIXWord w1, MIXWord w2)
		    => w1.Value >= w2.Value;

        #endregion

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (!(obj is MIXWord))
                throw new ApplicationException("Equals: parameter is not a MIXWord.");
            MIXWord other = obj as MIXWord;
            return this.Value == other.Value;
        }

		public override int GetHashCode() => Value.GetHashCode();

        public override string ToString()
        {
            return "[" + (Sign == Sign.Positive ? "+|" : "-|") +
                string.Format("{0:D2}", this[1]) + "|" +
                string.Format("{0:D2}", this[2]) + "|" +
                string.Format("{0:D2}", this[3]) + "|" +
                string.Format("{0:D2}", this[4]) + "|" +
                string.Format("{0:D2}", this[5]) + "|" + 
                "]";
        }

        #endregion

        #region Helpers for Construction

        public static MIXWord FromByteArray(byte[] buffer)
        {
            MIXWord result = new MIXWord();

            for (byte i = 0; i < 6; i++)
                result[i] = buffer[i];

            return result;
        }

        public byte[] ToByteArray()
        {
            byte[] result = new byte[6];

            for (byte i = 0; i < 6; i++)
                result[i] = (byte)this[i];

            return result;
        }

        #endregion

        public string ToInstructionString()
        {
            return "[" + (Sign == Sign.Positive ? "+|" : "-|") +
                string.Format("{0:D4}", this[1, 2]) + "|" +
                string.Format("{0:D2}", this[3]) + "|" +
                string.Format("{0:D2}", this[4]) + "|" +
                string.Format("{0:D2}", this[5]) + "|" +
                "]";
        }
    }
}