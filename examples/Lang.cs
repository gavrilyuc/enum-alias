using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using WebApi.Core;

namespace YourNamespace;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Lang
{
	[EnumMember(Value = "")]
	None = 0,

	[EnumMember(Value = "uk")]
	Ukraine,

	[EnumMember(Value = "en")]
	English,
}


internal static class Program
{
	internal static void Main()
	{
		var yourStringLang = "uk";
		Lang lang = yourLang.ToEnum<Lang>();
		if (lang == Lang.Ukraine)
		{
			Console.WriteLine("lang = uk");
		}

		Console.WriteLine("reset lang");

		var notCorrectLang = "asd";
		lang = notCorrectLang.ToEnum<Lang>();
		if (lang == Lang.None)
		{
			Console.WriteLine("not Correct lang");
		}

		Console.WriteLine("show list");
		foreach (var item in lang.GetEnumAliases())
		{
			Console.WriteLine("\t - {0}", item);
		}
		Console.WriteLine();

		lang = Lang.English;
		var alias = lang.GetAlias();
		Console.WriteLine("Your new lang = {0}", alias);
	}
}