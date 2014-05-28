using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Flurl.Test
{
	public static class ReflectionHelper
	{
		public static MethodInfo[] GetAllExtensionMethods<T>(Assembly asm) {
			// http://stackoverflow.com/a/299526/62600
			return (
				from type in asm.GetTypes()
				where type.IsSealed && !type.IsGenericType && !type.IsNested
				from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				where method.IsDefined(typeof(ExtensionAttribute), false)
				where method.GetParameters()[0].ParameterType == typeof(T)
				select method).ToArray();
		}

		public static bool AreSameMethodSignatures(MethodInfo method1, MethodInfo method2) {
			if (method1.Name != method2.Name)
				return false;

			if (!AreSameType(method1.ReturnType, method2.ReturnType))
				return false;

			var genArgs1 = method1.GetGenericArguments();
			var genArgs2 = method2.GetGenericArguments();

			if (genArgs1.Length != genArgs2.Length)
				return false;

			for (int i = 0; i < genArgs1.Length; i++) {
				if (!AreSameType(genArgs1[i], genArgs2[i]))
					return false;
			}

			var args1 = method1.GetParameters().Skip(IsExtensionMethod(method1) ? 1 : 0).ToArray();
			var args2 = method2.GetParameters().Skip(IsExtensionMethod(method2) ? 1 : 0).ToArray();

			if (args1.Length != args2.Length)
				return false;

			for (int i = 0; i < args1.Length; i++) {
				if (args1[i].Name != args2[i].Name) return false;
				if (!AreSameType(args1[i].ParameterType, args2[i].ParameterType)) return false;
				if (args1[i].IsOptional != args2[i].IsOptional) return false;
				if (!AreSameValue(args1[i].DefaultValue, args2[i].DefaultValue)) return false;
				if (args1[i].IsIn != args2[i].IsIn) return false;
			}
			return true;
		}

		public static bool IsExtensionMethod(MethodInfo method) {
			var type = method.DeclaringType;
			return
				type.IsSealed &&
				!type.IsGenericType &&
				!type.IsNested &&
				method.IsStatic &&
				method.IsDefined(typeof(ExtensionAttribute), false);
		}

		public static bool AreSameValue(object a, object b) {
			if (a == null && b == null)
				return true;
			if (a == null ^ b == null)
				return false;
			// ok, neither is null
			return a.Equals(b);
		}

		public static bool AreSameType(Type a, Type b) {
			if (a.IsGenericParameter && b.IsGenericParameter) {
				var constraintsA = a.GetGenericParameterConstraints();
				var constraintsB = b.GetGenericParameterConstraints();

				if (constraintsA.Length != constraintsB.Length)
					return false;

				for (int i = 0; i < constraintsA.Length; i++) {
					if (!AreSameType(constraintsA[i], constraintsB[i]))
						return false;
				}
				return true;
			}

			if (a.IsGenericType && b.IsGenericType) {
				if (a.GetGenericTypeDefinition() != b.GetGenericTypeDefinition())
					return false;

				var genArgsA = a.GetGenericArguments();
				var genArgsB = b.GetGenericArguments();

				if (genArgsA.Length != genArgsB.Length)
					return false;

				for (int i = 0; i < genArgsA.Length; i++) {
					if (!AreSameType(genArgsA[i], genArgsB[i]))
						return false;
				}

				return true;
			}

			return a == b;
		}
	}
}
