using System;

namespace MIXUI.Helpers
{
    /// <summary>
    /// Poor man's discriminated union
    /// </summary>
    public abstract class Union<A, B>
    {
        public abstract T Match<T>(Func<A, T> f, Func<B, T> g);
        private Union() { }

        public sealed class Case1 : Union<A, B>
        {
            public readonly A Item;
            public Case1(A item) : base() { this.Item = item; }
            public override T Match<T>(Func<A, T> f, Func<B, T> g) => f(Item);
        }

        public sealed class Case2 : Union<A, B>
        {
            public readonly B Item;
            public Case2(B item) : base() { this.Item = item; }
            public override T Match<T>(Func<A, T> f, Func<B, T> g) => g(Item);
        }
    }
}
