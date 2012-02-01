using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zata.FastReflection.Accessors.NonGenerics
{
    public interface IPropertyAccessor
    {
        object GetProperty(object obj);

        void SetProperty(object obj, object value);
    }
}
