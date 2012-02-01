using System;
namespace Zata.FastReflection.Accessors.NonGenerics
{
    public class ExpressionPropertyAccessor : IPropertyAccessor
    {
        private GetPropertyExecutor getExecutor;
        private SetPropertyExecutor setExecutor;

        public ExpressionPropertyAccessor(Type type, string propertyName)
        {
            var getMethod = type.GetMethod("get_" + propertyName);
            var setMethod = type.GetMethod("set_" + propertyName);
            if (getMethod == null && setMethod == null)
                throw new InvalidProgramException();

            if (getMethod != null)
                getExecutor = new GetPropertyExecutor(getMethod);
            if (setMethod != null)
                setExecutor = new SetPropertyExecutor(setMethod);
        }

        #region IPropertyAccessor Members

        public object GetProperty(object obj)
        {
            if (getExecutor != null)
                return getExecutor.Execute(obj);
            else
                throw new InvalidOperationException();
        }

        public void SetProperty(object obj, object value)
        {
            if (setExecutor != null)
                setExecutor.Execute(obj, new object[] { value });
        }

        #endregion
    }
}
