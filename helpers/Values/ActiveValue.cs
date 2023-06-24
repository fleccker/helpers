namespace helpers.Values
{
    public class ActiveValue<TValue>
    {
        public bool IsActive { get; set; }

        public TValue Value { get; set; }
    }
}