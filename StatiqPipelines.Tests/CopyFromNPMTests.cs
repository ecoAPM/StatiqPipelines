using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;
using Xunit;

namespace ecoAPM.StatiqPipelines.Tests;

public class CopyFromNPMTests
{
	private static readonly IEnumerable<string> Paths = ["x/1.js", "y/2.js"];

	[Fact]
	public async Task NodeModulesAreTranslated()
	{
		//arrange
		var context = new TestExecutionContext { FileSystem = { RootPath = "/code/app" } };
		context.FileSystem.GetInputFile("/code/app/node_modules/x/1.js").OpenWrite();
		context.FileSystem.GetInputFile("/code/app/node_modules/y/2.js").OpenWrite();

		var pipeline = new CopyFromNPM(Paths, "assets/js");
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
	public async Task CopyToFlattensOutputByDefault()
	{
		//arrange
		var docs = new List<IDocument>
			{
				new TestDocument(new NormalizedPath("/code/app/node_modules/x/1.js")),
				new TestDocument(new NormalizedPath("/code/app/node_modules/y/2.js"))
			};
		var context = new TestExecutionContext();
		context.SetInputs(docs);

		var pipeline = new CopyFromNPM(Paths, "assets/js");
		var copy = pipeline.ProcessModules.First(m => m is SetDestination);

		//act
		var output = await copy.ExecuteAsync(context);

		//assert
		var outputDocs = output.ToArray();
		Assert.Equal("assets/js/1.js", outputDocs[0].Destination);
		Assert.Equal("assets/js/2.js", outputDocs[1].Destination);
	}

	[Fact]
	public async Task CopyToFlattensOutputForEmptyValues()
	{
		//arrange
		var docs = new List<IDocument>
			{
				new TestDocument(new NormalizedPath("/code/app/node_modules/x/1.js")),
				new TestDocument(new NormalizedPath("/code/app/node_modules/y/2.js"))
			};
		var context = new TestExecutionContext();
		context.SetInputs(docs);

		var files = new Dictionary<string, string>
		{
			{ "x/1.js", "" },
			{ "y/2.js", " " }
		};
		var pipeline = new CopyFromNPM(files, "assets/js");
		var copy = pipeline.ProcessModules.First(m => m is SetDestination);

		//act
		var output = await copy.ExecuteAsync(context);

		//assert
		var outputDocs = output.ToArray();
		Assert.Equal("assets/js/1.js", outputDocs[0].Destination);
		Assert.Equal("assets/js/2.js", outputDocs[1].Destination);
	}

	[Fact]
	public async Task CopyToUsesSpecifiedValues()
	{
		//arrange
		var docs = new List<IDocument>
			{
				new TestDocument(new NormalizedPath("/code/app/node_modules/x/y/1.js")),
				new TestDocument(new NormalizedPath("/code/app/node_modules/x/y/z/2.js"))
			};
		var context = new TestExecutionContext();
		context.SetInputs(docs);

		var files = new Dictionary<string, string>
		{
			{ "x/y/1.js", "y/1.js" },
			{ "x/y/z/2.js", "y/z/2.js" }
		};
		var pipeline = new CopyFromNPM(files, "assets/js");
		var copy = pipeline.ProcessModules.First(m => m is SetDestination);

		//act
		var output = await copy.ExecuteAsync(context);

		//assert
		var outputDocs = output.ToArray();
		Assert.Equal("assets/js/y/1.js", outputDocs[0].Destination);
		Assert.Equal("assets/js/y/z/2.js", outputDocs[1].Destination);
	}

	[Fact]
	public async Task CanCopyToOutputUsingWildcardKeys()
	{
		//arrange
		var docs = new List<IDocument>
			{
				new TestDocument(new NormalizedPath("/code/app/node_modules/x/1.js")),
				new TestDocument(new NormalizedPath("/code/app/node_modules/x/2.js"))
			};
		var context = new TestExecutionContext();
		context.SetInputs(docs);

		var files = new Dictionary<string, string>
		{
			{ "x/*", "y" }
		};
		var pipeline = new CopyFromNPM(files, "assets/js");
		var copy = pipeline.ProcessModules.First(m => m is SetDestination);

		//act
		var output = await copy.ExecuteAsync(context);

		//assert
		var outputDocs = output.ToArray();
		Assert.Equal("assets/js/y/1.js", outputDocs[0].Destination);
		Assert.Equal("assets/js/y/2.js", outputDocs[1].Destination);
	}
}