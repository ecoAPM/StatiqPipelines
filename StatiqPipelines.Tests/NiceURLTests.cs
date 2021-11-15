using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Testing;
using Xunit;

namespace ecoAPM.StatiqPipelines.Tests;

public class NiceURLTests
{
	[Fact]
	public async Task ConvertsPathToNiceURL()
	{
		//arrange
		var doc = new TestDocument(new NormalizedPath("/input/content/file.md"));
		var context = new TestExecutionContext();
		context.SetInputs(doc);

		var niceURL = new NiceURL();

		//act
		var docs = await niceURL.ExecuteAsync(context);

		//assert
		Assert.Equal("content/file/index.html", docs.First().Destination);
	}

	[Fact]
	public async Task CanOverrideWithOutputField()
	{
		//arrange
		var context = new TestExecutionContext();

		var doc = new TestDocument(new NormalizedPath("/input/content/file.md"),
			new Dictionary<string, object>
			{
					{ "Output", "content/override/index.html" }
			});

		context.SetInputs(doc);

		var niceURL = new NiceURL();

		//act
		var docs = await niceURL.ExecuteAsync(context);

		//assert
		Assert.Equal("content/override/index.html", docs.First().Destination);
	}

	[Fact]
	public async Task NicelyFormattedOverrideConvertsURL()
	{
		//arrange
		var context = new TestExecutionContext();

		var doc = new TestDocument(new NormalizedPath("/input/content/file.md"),
			new Dictionary<string, object>
			{
					{ "Output", "content/override" }
			});

		context.SetInputs(doc);

		var niceURL = new NiceURL();

		//act
		var docs = await niceURL.ExecuteAsync(context);

		//assert
		Assert.Equal("content/override/index.html", docs.First().Destination);
	}
}
