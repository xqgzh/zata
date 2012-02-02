using System;
using System.Reflection.Emit;

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

        public GenericPropertyAccessor(string memberName)
        {
            var type = typeof(T);
            var propertyInfo = type.GetProperty(memberName);
            var fieldInfo = type.GetField(memberName);
            if (propertyInfo != null)
            {
                var getMethodInfo = type.GetMethod("get_" + memberName);
                var setMethodInfo = type.GetMethod("set_" + memberName);

                if (getMethodInfo != null)
                    getPropertyDelegate = (GetPropertyDelegate)Delegate.CreateDelegate(typeof(GetPropertyDelegate), getMethodInfo);

                if (setMethodInfo != null)
                    setPropertyDelegate = (SetPropertyDelegate)Delegate.CreateDelegate(typeof(SetPropertyDelegate), setMethodInfo);
            }
            else if (fieldInfo != null)
            {
                var dynamicGet = new DynamicMethod("get_" + memberName, typeof(P), new[] { type }, type);
                var ilEmitor = dynamicGet.GetILGenerator();
                ilEmitor.Emit(OpCodes.Ldarg_0);
                ilEmitor.Emit(OpCodes.Ldfld, fieldInfo);
                ilEmitor.Emit(OpCodes.Ret);

                getPropertyDelegate = (GetPropertyDelegate)dynamicGet.CreateDelegate(typeof(GetPropertyDelegate));

                var dynamicSet = new DynamicMethod("set_" + memberName, typeof(void), new[] { type, typeof(P) }, type);
                ilEmitor = dynamicSet.GetILGenerator();
                ilEmitor.Emit(OpCodes.Ldarg_0);
                ilEmitor.Emit(OpCodes.Ldarg_1);
                ilEmitor.Emit(OpCodes.Stfld, fieldInfo);
                ilEmitor.Emit(OpCodes.Ret);

                setPropertyDelegate = (SetPropertyDelegate)dynamicSet.CreateDelegate(typeof(SetPropertyDelegate));
            }
            else
            {
                throw new ArgumentException(String.Format("Member: '{0}' is not a Public Property or Field of Type: '{1}'", memberName, type.Name));
            }
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
