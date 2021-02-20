using System;

namespace ThePBone.BlueZNet.Utils
{
    public static class ReflectionUtils
    {
        public static bool HasMethod(this object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }

        public static object SimpleInvoke(this object obj, string methodName, object parameter)
        {
            if(!HasMethod(obj, methodName))
                throw new ArgumentException($"Object does not provide {methodName}");

            return obj.GetType().GetMethod(methodName)?.Invoke(obj, new []{parameter});
        }
        
        public static object GenericInvoke<T>(this object obj, string methodName, object parameter)
        {
            if(!HasMethod(obj, methodName))
                throw new ArgumentException($"Object does not provide {methodName}");

            return obj.GetType().GetMethod(methodName)?
                .MakeGenericMethod(new []{typeof(T)}).Invoke(obj, new []{parameter});
        }
    }
}