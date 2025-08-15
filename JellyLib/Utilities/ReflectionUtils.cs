using System;
using System.Reflection;

namespace JellyLib.Utilities
{
    public class ReflectionUtils
    {
        public static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Type type = target.GetType();
            FieldInfo fieldInfo = null;

            // Traverse inheritance chain if needed
            while (type != null)
            {
                fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                    break;

                type = type.BaseType;
            }

            if (fieldInfo == null)
                throw new MissingFieldException($"Field '{fieldName}' not found in type hierarchy.");

            fieldInfo.SetValue(target, value);
        }
        
        public static T GetPrivateField<T>(object target, string fieldName)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Type type = target.GetType();
            FieldInfo fieldInfo = null;

            while (type != null)
            {
                fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                    break;

                type = type.BaseType;
            }

            if (fieldInfo == null)
                throw new MissingFieldException($"Field '{fieldName}' not found in type hierarchy.");

            return (T)fieldInfo.GetValue(target);
        }

        public static T CallPrivateMethod<T>(object target, string methodName, params object[] args)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Type type = target.GetType();
            MethodInfo methodInfo = null;

            // Traverse inheritance chain if needed
            while (type != null)
            {
                methodInfo = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (methodInfo != null)
                    break;

                type = type.BaseType;
            }

            if (methodInfo == null)
                throw new MissingFieldException($"Field '{methodName}' not found in type hierarchy.");
            

            object result = methodInfo.Invoke(target, args);
            
            if (typeof(T) == typeof(void) || methodInfo.ReturnType == typeof(void))
                return default;

            return (T)result;
        }
        
        public static void CallPrivateMethod(object target, string methodName, params object[] args)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Type type = target.GetType();
            MethodInfo methodInfo = null;

            // Traverse inheritance chain if needed
            while (type != null)
            {
                methodInfo = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (methodInfo != null)
                    break;

                type = type.BaseType;
            }

            if (methodInfo == null)
                throw new MissingFieldException($"Field '{methodName}' not found in type hierarchy.");
            

            methodInfo.Invoke(target, args);
        }
    }
}

