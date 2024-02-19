namespace uDataBinder.Utils
{
    public readonly struct Nullable<T> where T : class
    {
        public Nullable(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public override readonly string ToString() => Value?.ToString();

        public override bool Equals(object obj)
        {
            if (obj is Nullable<T> nullable)
            {
                var n = nullable.Value;
                if (ReferenceEquals(Value, n))
                {
                    return true;
                }
                return Value.Equals(n);
            }
            return false;
        }

        public override readonly int GetHashCode() => Value is null ? 0 : Value.GetHashCode();

        public static bool operator ==(Nullable<T> x, Nullable<T> y) => x.Equals(y);
        public static bool operator !=(Nullable<T> x, Nullable<T> y) => !x.Equals(y);
        public static implicit operator T(Nullable<T> source) => source.Value;
        public static implicit operator Nullable<T>(T source) => new(source);
    }
}