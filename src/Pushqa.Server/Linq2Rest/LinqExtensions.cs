// (c) Copyright Reimers.dk.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.
// Based on code from http://stackoverflow.com/questions/606104/how-to-create-linq-expression-tree-with-anonymous-type-in-it

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Linq2Rest
{
    internal static class LinqExtensions
	{
		private static readonly AssemblyName AssemblyName = new AssemblyName { Name = "DynamicLinqTypes" };
		private static readonly ModuleBuilder ModuleBuilder;
		private static readonly Dictionary<string, Type> BuiltTypes = new Dictionary<string, Type>();

		static LinqExtensions()
		{
			ModuleBuilder = Thread.GetDomain()
				.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run)
				.DefineDynamicModule(AssemblyName.Name);
		}

		public static bool IsAnonymousType(this Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
				&& type.IsGenericType
				&& type.Name.Contains("AnonymousType") && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
				&& (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
		}

		public static Type GetDynamicType(this IEnumerable<PropertyInfo> fields)
		{
			Contract.Requires<ArgumentNullException>(fields != null);

			if (!fields.Any())
			{
				throw new ArgumentOutOfRangeException("fields", "fields must have at least 1 field definition");
			}

			var dictionary = fields.ToDictionary(f => f.Name, f => f.PropertyType);

			Monitor.Enter(BuiltTypes);
			try
			{
				var className = GetTypeKey(dictionary);

				if (BuiltTypes.ContainsKey(className))
				{
					return BuiltTypes[className];
				}

				var typeBuilder = ModuleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);

				foreach (var field in dictionary)
				{
					typeBuilder.DefineField(field.Key, field.Value, FieldAttributes.Public);
				}

				BuiltTypes[className] = typeBuilder.CreateType();

				return BuiltTypes[className];
			}
			catch
			{
				return null;
			}
			finally
			{
				Monitor.Exit(BuiltTypes);
			}
		}

		private static string GetTypeKey(Dictionary<string, Type> fields)
		{
			return fields.Aggregate(string.Empty, (current, field) => current + (field.Key + ";" + field.Value.Name + ";"));
		}
	}
}