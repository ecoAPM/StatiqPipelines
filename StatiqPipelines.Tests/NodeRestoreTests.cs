using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Testing;
using Xunit;

namespace ecoAPM.StatiqPipelines.Tests;

public class NodeRestoreTests
{
	[Fact]
	public async Task RunsYarnWhenExists()
	{
		//arrange
		var context = new TestExecutionContext();
		context.FileSystem.GetRootFile("yarn.lock").OpenWrite();

		var process = new ProcessStartInfo();

		Process NewProcess(ProcessStartInfo i)
		{
			process = i;
			return null;
		}

		var module = new NodeRestore(NewProcess, () => false);

		//act
		await module.ExecuteAsync(context);

		//assert
		Assert.Equal("yarn", process.FileName);
	}

	[Fact]
	public async Task RunsNPMWhenYarnDoesNotExist()
	{
		//arrange
		var context = new TestExecutionContext();
		context.FileSystem.GetRootFile("package-lock.json").OpenWrite();

		var process = new ProcessStartInfo();

		Process NewProcess(ProcessStartInfo i)
		{
			process = i;
			return null;
		}

		var module = new NodeRestore(NewProcess, () => false);

		//act
		await module.ExecuteAsync(context);

		//assert
		Assert.Equal("npm", process.FileName);
	}

	[Fact]
	public async Task RunsYarnOnWindows()
	{
		//arrange
		var context = new TestExecutionContext();
		context.FileSystem.GetRootFile("yarn.lock").OpenWrite();

		var process = new ProcessStartInfo();

		Process NewProcess(ProcessStartInfo i)
		{
			process = i;
			return null;
		}

		var module = new NodeRestore(NewProcess, () => true);

		//act
		await module.ExecuteAsync(context);

		//assert
		Assert.Equal("cmd", process.FileName);
		Assert.Contains(context.LogMessages, l => l.FormattedMessage.Contains("yarn"));
		Assert.DoesNotContain(context.LogMessages, l => l.FormattedMessage.Contains("npm"));
	}

	[Fact]
	public async Task RunsNPMOnWindows()
	{
		//arrange
		var context = new TestExecutionContext();
		context.FileSystem.GetRootFile("package-lock.json").OpenWrite();

		var process = new ProcessStartInfo();

		Process NewProcess(ProcessStartInfo i)
		{
			process = i;
			return null;
		}

		var module = new NodeRestore(NewProcess, () => true);

		//act
		await module.ExecuteAsync(context);

		//assert
		Assert.Equal("cmd", process.FileName);
		Assert.Contains(context.LogMessages, l => l.FormattedMessage.Contains("npm"));
		Assert.DoesNotContain(context.LogMessages, l => l.FormattedMessage.Contains("yarn"));
	}

	[Fact]
	public async Task SkipsWhenNodeModulesExists()
	{
		//arrange
		var context = new TestExecutionContext();
		context.FileSystem.GetRootDirectory("node_modules").Create();

		var module = new NodeRestore(_ => throw new Exception(), () => false);

		//act
		await module.ExecuteAsync(context);

		//assert
		Assert.Contains(context.LogMessages, l => l.FormattedMessage.Contains("Skipping restore"));
	}
}
