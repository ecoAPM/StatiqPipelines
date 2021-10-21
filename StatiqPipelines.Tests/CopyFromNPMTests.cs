using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;
using Xunit;

namespace ecoAPM.StatiqPipelines.Tests
{
	public class CopyFromNPMTests
	{
		[Fact]
		public async Task NodeModulesAreTranslated()
		{
			//arrange
			var context = new TestExecutionContext();
			context.FileSystem.GetInputFile("/node_modules/x/1.js").OpenWrite();
			context.FileSystem.GetInputFile("/node_modules/y/2.js").OpenWrite();

			var pipeline = new CopyFromNPM(new[] { "x/1.js", "y/2.js" }, "assets/js");
			var input = pipeline.InputModules.First(m => m is ReadFiles);

			//act
			var output = await input.ExecuteAsync(context);

			//assert
			var files = output.Select(d => d.Source.FileName.ToString()).ToArray();
			Assert.Equal(2, files.Length);
			Assert.Contains("1.js", files);
			Assert.Contains("2.js", files);
		}

		[Fact]
		public async Task CopyToFlattensOutputPath()
		{
			//arrange
			var docs = new List<IDocument>
			{
				new TestDocument(new NormalizedPath("x/1.js")),
				new TestDocument(new NormalizedPath("y/2.js"))
			};
			var context = new TestExecutionContext();
			context.SetInputs(docs);

			var pipeline = new CopyFromNPM(new[] { "x/1.js", "y/2.js" }, "assets/js");
			var copy = pipeline.ProcessModules.First(m => m is SetDestination);

			//act
			var output = await copy.ExecuteAsync(context);

			//assert
			var outputDocs = output.ToArray();
			Assert.Equal("assets/js/1.js", outputDocs[0].Destination);
			Assert.Equal("assets/js/2.js", outputDocs[1].Destination);
		}
	}
}