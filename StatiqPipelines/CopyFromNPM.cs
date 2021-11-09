using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Statiq.Common;
using Statiq.Core;

namespace ecoAPM.StatiqPipelines
{
	public class CopyFromNPM : Pipeline
	{
		/// <summary>
		/// Copies specific files from a `node_modules` directory to the output
		/// </summary>
		/// <param name="paths">The file paths (relative to `node_modules`) to copy</param>
		/// <param name="output">The path (relative to the output root) where the files will be copied</param>
		public CopyFromNPM(IEnumerable<string> paths, string output = "lib")
		{
			Isolated = true;
			InputModules = new ModuleList
			{
				new NodeRestore(),
				new ReadFiles(npmPath(paths))
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

		private static IReadOnlyList<string> npmPath(IEnumerable<string> paths)
			=> paths.Select(p => IExecutionContext.Current.FileSystem
					.GetRootDirectory("node_modules").GetFile(p)
					.Path.FullPath.Replace("\\", "/"))
				.ToArray();

		private static SetDestination CopyTo(string output)
			=> new(SetPath(output));

		private static Config<NormalizedPath> SetPath(string output)
			=> Config.FromDocument(d => NewPath(output, d));

		private static NormalizedPath NewPath(string output, IDocument d)
			=> new(OutputPath(output, d));

		private static string OutputPath(string output, IDocument d)
			=> Path.Combine(output, d.Source.FileName.ToString());
	}
}