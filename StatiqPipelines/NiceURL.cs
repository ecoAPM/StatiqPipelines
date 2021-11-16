using Statiq.Common;
using Statiq.Core;

namespace ecoAPM.StatiqPipelines;

public class NiceURL : SetDestination
{
	/// <summary>
	/// Converts file paths into "nice" URLs
	/// </summary>
	public NiceURL() : base(ConfigPath)
	{
	}

	private static readonly Config<NormalizedPath> ConfigPath
		= Config.FromDocument(NicePath);

	private static NormalizedPath NicePath(IDocument document)
		=> new(GetPath(document));

	private static string GetPath(IDocument document)
	{
		var output = document.GetString("Output");
		if (string.IsNullOrWhiteSpace(output))
			return Path.Combine(Directory(document.Source), "index.html");

		var path = new NormalizedPath(output);
		return path.HasExtension
			? output
			: Path.Combine(path.ToString(), "index.html");
	}

	private static string Directory(NormalizedPath source)
		=> source.GetRelativeInputPath()
			.ChangeExtension("")
			.ToString()
			.TrimEnd('.')
			.Replace("index", "");
}