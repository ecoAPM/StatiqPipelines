using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace ecoAPM.StatiqPipelines
{
	public class NodeRestore : Module
	{
		private readonly Func<ProcessStartInfo, Process> _newProcess;
		private readonly Func<bool> _isWindows;

		public NodeRestore(Func<ProcessStartInfo, Process> newProcess = null, Func<bool> isWindows = null)
		{
			_newProcess = newProcess ?? (info => new Process { StartInfo = info });
			_isWindows = isWindows ?? OperatingSystem.IsWindows;
		}

		protected override void BeforeExecution(IExecutionContext context)
		{
			var node_modules = context.FileSystem.GetRootDirectory("node_modules");
			if (node_modules.Exists)
			{
				context.Log(LogLevel.Debug, "Skipping restore: node_modules already exists");
				return;
			}

			context.Log(LogLevel.Debug, "Restoring node_modules...");

			var yarnLock = context.FileSystem.GetRootFile("yarn.lock");
			context.Log(LogLevel.Debug, $"Project uses {(yarnLock.Exists ? "yarn" : "npm")}");
			var process = _isWindows()
				? GetWindowsProcess(context, yarnLock.Exists)
				: GetPosixProcess(context, yarnLock.Exists);

			process?.WaitForExit();
		}

		private Process GetWindowsProcess(IExecutionContext context, bool usesYarn)
		{
			var info = ProcessStartInfo(context, "cmd");
			var process = _newProcess(info);
			process?.Start();

			var cmd = usesYarn ? "yarn" : "npm install";
			process?.StandardInput.WriteLine(cmd + " & exit");
			context.Log(LogLevel.Debug, $"Starting {cmd} in cmd...");
			return process;
		}

		private static ProcessStartInfo ProcessStartInfo(IExecutionContext context, string command, string args = null) =>
			new(command, args ?? string.Empty)
			{
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				WorkingDirectory = context.FileSystem.RootPath.ToString()
			};

		private Process GetPosixProcess(IExecutionContext context, bool usesYarn)
		{
			var info = usesYarn
				? ProcessStartInfo(context, "yarn")
				: ProcessStartInfo(context, "npm", "install");

			context.Log(LogLevel.Debug, $"Starting {info}...");
			var process = _newProcess(info);
			process?.Start();
			return process;
		}
	}
}