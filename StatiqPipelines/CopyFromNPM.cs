using Statiq.Common;
using Statiq.Core;

namespace ecoAPM.StatiqPipelines;

public class CopyFromNPM : Pipeline
{
	private const string NodeModulesDirectory = "node_modules/";

	private readonly IDictionary<string, string> _paths;

	/// <summary>
	/// Copies specific files from a `node_modules` directory to the output
	/// </summary>
	/// <param name="paths">The file paths (relative to `node_modules`) to copy</param>
	/// <param name="output">The path (relative to the output root) where the files will be copied</param>
	public CopyFromNPM(IEnumerable<string> paths, string output = "lib")
		: this(Flatten(paths), output)
	{
	}

	/// <summary>
	/// Copies specific files from a `node_modules` directory to the output
	/// </summary>
	/// <param name="paths">The file paths (relative to `node_modules`) to copy</param>
	/// <param name="output">The path (relative to the output root) where the files will be copied</param>
	public CopyFromNPM(Dictionary<string, string> paths, string output = "lib")
	{
		_paths = paths;

		Isolated = true;
		InputModules = new ModuleList
		{
			new NodeRestore(),
			new ReadFiles(_paths.Keys.Select(npmPath))
		};

		ProcessModules = new ModuleList
		{
			CopyTo(output)
		};

		OutputModules = new ModuleList
		{
			new WriteFiles()
		};
	}

	private static Dictionary<string, string> Flatten(IEnumerable<string> paths)
		=> paths.ToDictionary(p => p, path => new NormalizedPath(path).FileName.ToString());

	private static string npmPath(string path)
		=> IExecutionContext.Current.FileSystem
			.GetRootDirectory(NodeModulesDirectory).GetFile(path)
			.Path.FullPath.Replace("\\", "/");

	private SetDestination CopyTo(string output)
		=> new(SetPath(output));

	private Config<NormalizedPath> SetPath(string output)
		=> Config.FromDocument(d => NewPath(output, d));

	private NormalizedPath NewPath(string output, IDocument doc)
		=> new(OutputPath(output, doc));

	private string OutputPath(string output, IDocument doc)
		=> Path.Combine(output, RelativeOutputPath(doc));

	private string RelativeOutputPath(IDocument doc)
		=> _paths.TryGetValue(RelativePath(doc.Source), out var path)
			? GetPath(path) ?? doc.Source.FileName.ToString()
			: HandleWildcard(doc);

	private static string? GetPath(string path)
		=> !path.IsNullOrWhiteSpace() ? path : null;

	private string HandleWildcard(IDocument doc)
	{
		var paths = _paths.ToDictionary(p => p.Key.Split("*")[0], p => p.Value);
		var match = paths.FirstOrDefault(p => doc.Source.FullPath.Contains(p.Key));
		var value = !match.Value.IsNullOrWhiteSpace()
			? Path.Combine(match.Value, doc.Source.FileName.ToString()).Replace("\\", "/")
			: doc.Source.FileName.ToString();
		return value;
	}

	private static string RelativePath(NormalizedPath p)
		=> p.RootRelative.ToString().Split(NodeModulesDirectory)[1];
}