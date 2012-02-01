using System;

namespace Zata.FastReflection.Accessors.Generics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="P"></typeparam>
    public class GenericPropertyAccessor<T, P> : IGenericPropertyAccessor<T, P>
    {
        private delegate P GetPropertyDelegate(T obj);
        private delegate void SetPropertyDelegate(T obj, P value);

        private GetPropertyDelegate getPropertyDelegate;
        private SetPropertyDelegate setPropertyDelegate;

        public GenericPropertyAccessor(string propertyName)
        {
            var type = typeof(T);
            var getMethodInfo = type.GetMethod("get_" + propertyName);
            var setMethodInfo = type.GetMethod("set_" + propertyName);
            if (getMethodInfo == null && setMethodInfo == null)
                throw new ArgumentException();

            if (getMethodInfo != null)
                getPropertyDelegate = (GetPropertyDelegate)Delegate.CreateDelegate(typeof(GetPropertyDelegate), getMethodInfo);

            if (setMethodInfo != null)
                setPropertyDelegate = (SetPropertyDelegate)Delegate.CreateDelegate(typeof(SetPropertyDelegate), setMethodInfo);
        }

        #region IPropertyAccessor<T,PropertyType> Members

        public P GetProperty(T obj)
        {
            return getPropertyDelegate(obj);
        }

        public bool SetProperty(T obj, P value)
        {
            if (setPropertyDelegate != null)
            {
                setPropertyDelegate(obj, value);

                return true;
            }

            return false;
        }

        #endregion

        #region IPropertyAccessor Members

        public object GetProperty(object obj)
        {
            return GetProperty((T)obj);
        }

        public void SetProperty(object obj, object value)
        {
            SetProperty((T)obj, (P)value);
        }

        #endregion
    }
}
