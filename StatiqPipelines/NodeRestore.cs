using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace ecoAPM.StatiqPipelines;

public class NodeRestore : Module
{
	private readonly Func<ProcessStartInfo, Process?> _newProcess;
	private readonly Func<bool> _isWindows;

	public NodeRestore(Func<ProcessStartInfo, Process?>? newProcess = null, Func<bool>? isWindows = null)
	{
		_newProcess = newProcess ?? (info => new Process { StartInfo = info });
		_isWindows = isWindows ?? OperatingSystem.IsWindows;
	}

	protected override async Task BeforeExecutionAsync(IExecutionContext context)
	{
		var nodeModules = context.FileSystem.GetRootDirectory("node_modules");
		if (nodeModules.Exists)
		{
			if (context.IsEnabled(LogLevel.Debug))
				context.Log(LogLevel.Debug, "Skipping restore: node_modules already exists");
			return;
		}

		context.Log(LogLevel.Debug, "Restoring node_modules...");

		var yarnLock = context.FileSystem.GetRootFile("yarn.lock");

		if (context.IsEnabled(LogLevel.Debug))
			context.Log(LogLevel.Debug, "Project uses {Program}", yarnLock.Exists ? "yarn" : "npm");

		var process = _isWindows()
			? GetWindowsProcess(context, yarnLock.Exists)
			: GetPosixProcess(context, yarnLock.Exists);

		await (process?.WaitForExitAsync() ?? Task.CompletedTask);
	}

	private Process? GetWindowsProcess(IExecutionContext context, bool usesYarn)
	{
		var info = ProcessStartInfo(context, "cmd");
		var process = _newProcess(info);
		process?.Start();

		var cmd = usesYarn ? "yarn" : "npm install";
		process?.StandardInput.WriteLine(cmd + " & exit");

		if (context.IsEnabled(LogLevel.Debug))
			context.Log(LogLevel.Debug, "Starting {Command} in cmd...", cmd);

		return process;
	}

	private static ProcessStartInfo ProcessStartInfo(IExecutionState state, string command, string? args = null) =>
		new(command, args ?? string.Empty)
		{
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			WorkingDirectory = state.FileSystem.RootPath.ToString()
		};

	private Process? GetPosixProcess(IExecutionContext context, bool usesYarn)
	{
		var info = usesYarn
			? ProcessStartInfo(context, "yarn")
			: ProcessStartInfo(context, "npm", "install");

		if (context.IsEnabled(LogLevel.Debug))
			context.Log(LogLevel.Debug, "Starting {Info}...", info);

		var process = _newProcess(info);
		process?.Start();

		return process;
	}
}