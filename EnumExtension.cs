#nullable disable
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;

namespace WebApi.Core;

/// <summary>
/// The extension class for Enum types.
/// </summary>
public static class EnumExtension
{
	private static readonly ConcurrentDictionary<string, string> EnumAliases = new();
	private static readonly ConcurrentDictionary<string, object> AliasesEnumValues = new();
	private static readonly ConcurrentDictionary<string, List<string>> Aliases = new();

	/// <summary>
	/// Get string alias of enum by <see cref="value"/> attribute if exists
	/// else returns string.Empty
	/// </summary>
	/// <param name="value"></param>
	/// <returns><b>string.Empty</b> if not found</returns>
	public static string GetAlias(this Enum value)
	{
		var enumType = value.GetType();
		if (!enumType.IsEnum)
		{
			throw new NotSupportedException($"{enumType.FullName} is not enum");
		}

		var enumValue = value.ToString();

		var key = $"{enumType.FullName}.{enumValue}".ToLowerInvariant();

		return EnumAliases.GetOrAdd(key, e => enumType.GetMember(enumValue)
			.FirstOrDefault()?
			.GetCustomAttribute<EnumMemberAttribute>()?
			.Value ?? string.Empty);
	}

	/// <summary>
	/// Returns Enum type of <see cref="TEnum"/> using alias name by <see cref="EnumMemberAttribute"/> if exists
	/// else return default(<see cref="TEnum"/>)
	/// </summary>
	/// <param name="value">Enum alias name</param>
	/// <typeparam name="TEnum">Enum that has some enum items with <see cref="EnumMemberAttribute"/></typeparam>
	/// <returns><see cref="TEnum"/> or default(<see cref="TEnum"/>)</returns>
	public static TEnum ToEnum<TEnum>(this string value) where TEnum : Enum
	{
		var enumType = typeof(TEnum);
		if (!enumType.IsEnum)
		{
			throw new NotSupportedException($"{enumType.FullName} is not enum");
		}

		var key = $"{enumType.FullName}.{value}".ToLowerInvariant();

		return (TEnum)AliasesEnumValues.GetOrAdd(key, e =>
		{
			foreach (var name in Enum.GetNames(enumType))
			{
				var field = enumType.GetField(name);

				if (field == null)
				{
					continue;
				}

				var attr = field.GetCustomAttribute(typeof(EnumMemberAttribute), true);

				if (attr is not EnumMemberAttribute { Value: not null } memberAttr)
				{
					continue;
				}

				if (memberAttr.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					return (TEnum) Enum.Parse(enumType, name);
				}
			}

			if (Enum.TryParse(enumType, value, true, out var result))
			{
				return (TEnum) result;
			}

			return default(TEnum)!;
		});
	}

	/// <summary>
	/// Get list of aliases from enum
	/// </summary>
	/// <param name="value"><see cref="TEnum"/></param>
	/// <typeparam name="TEnum">Enum that has some enum items with <see cref="EnumMemberAttribute"/></typeparam>
	/// <returns></returns>
	public static List<string> GetEnumAliases<TEnum>(this TEnum value)
		where TEnum : Enum
	{
		var enumType = value.GetType();

		var result = Aliases.GetOrAdd(enumType.FullName ?? string.Empty, (key) =>
		{
			var enumNames = Enum.GetNames(enumType);

			var enums = new List<string>(enumNames.Length);
			foreach (var name in enumNames)
			{
				var field = enumType.GetField(name);
				if (field == null)
				{
					continue;
				}

				var attr = field.GetCustomAttribute(typeof(EnumMemberAttribute), true);
				if (attr is EnumMemberAttribute memberAttr)
				{
					if (string.IsNullOrEmpty(memberAttr.Value))
					{
						continue;
					}

					enums.Add(memberAttr.Value);
				}
				else
				{
					enums.Add(name);
				}
			}

			return enums;
		});

		return result;
	}
}