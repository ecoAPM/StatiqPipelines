using Statiq.Common;
using Statiq.Core;

namespace ecoAPM.StatiqPipelines;

public class CopyFromNPM : Pipeline
{
	private const string NodeModulesDirectory = "node_modules/";
	private Dictionary<string, ReadFiles> _files;

	/// <summary>
	/// Copies specific files from a `node_modules` directory to the output
	/// </summary>
	/// <param name="paths">The file paths (relative to `node_modules`) to copy</param>
	/// <param name="output">The path (relative to the output root) where the files will be copied</param>
	/// <param name="flatten">Flatten all files into the <see cref="output">output</see> directory</param>
	public CopyFromNPM(IEnumerable<string> paths, string output = "lib")
	{
		_files = paths.ToDictionary(p => p, p => new ReadFiles(npmPath(p)));

		Isolated = true;
		InputModules = new ModuleList { new NodeRestore() };
		InputModules.AddRange(_files.Values);

		ProcessModules = new ModuleList { CopyTo(output) };

		OutputModules = new ModuleList { new WriteFiles() };
	}

	private static string npmPath(string path)
		=> IExecutionContext.Current.FileSystem
			.GetRootDirectory(NodeModulesDirectory).GetFile(path)
			.Path.FullPath.Replace("\\", "/");

	private SetDestination CopyTo(string output)
		=> new(SetPath(output));

	private Config<NormalizedPath> SetPath(string output)
		=> Config.FromDocument(d => NewPath(output, d));

	private NormalizedPath NewPath(string output, IDocument d)
		=> new(OutputPath(output, d));

	private string OutputPath(string output, IDocument d)
		=> Path.Combine(output, RelativeOutputPath(d));

	private string RelativeOutputPath(IDocument d)
		=> _files.ContainsKey(RelativePath(d))
			? d.Source.FileName.ToString()
			: RelativePath(d);

	private static string RelativePath(IDocument d)
		=> d.Source.RootRelative.ToString().RemoveStart(NodeModulesDirectory);
}