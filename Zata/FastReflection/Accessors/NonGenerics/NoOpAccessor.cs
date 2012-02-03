
namespace Zata.FastReflection.Accessors.NonGenerics
{
    /// <summary>
    /// 
    /// </summary>
    public class NoOpAccessor : IPropertyAccessor
    {
        private static NoOpAccessor instance = new NoOpAccessor();

        private NoOpAccessor() { }

        public static NoOpAccessor Instance { get { return instance; } }

        #region IPropertyAccessor Members

        public object GetProperty(object obj)
        {
            return null;
        }

        public void SetProperty(object obj, object value)
        {
            // Do nothing.
        }

        #endregion
    }
}
