using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json;

using Langbox.Models;

namespace Langbox.Data.Sandbox
{
    public class SandboxExecutor
    {
        class Execution : IDisposable, IAsyncDisposable
        {
            private Challenge Challenge { get; set; }
            private string MainContent { get; set; }
            private CancellationToken CancellationToken { get; set; }
            private string SandboxDirectory { get; set; }
            private string MainFilePath { get; set; }
            private string TestFilePath { get; set; }
            private string Id = Guid.NewGuid().ToString();
            private DockerClient DockerClient = new DockerClientConfiguration().CreateClient();
            private CreateContainerResponse? Container;

            public Execution(Challenge challenge, string mainContent, CancellationToken cancellationToken)
            {
                Challenge = challenge;
                MainContent = mainContent;
                CancellationToken = cancellationToken;
                SandboxDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}/Sandbox/{Id}";
                MainFilePath = $"{SandboxDirectory}/{Challenge.Environment.MainFileName}";
                TestFilePath = $"{SandboxDirectory}/{Challenge.Environment.TestFileName}";
            }

            public async Task<ExecutionResult> Start()
            {
                try
                {
                    await CreateVolumesAsync();
                    var stdout = await RunInContainerAsync();
                    return RetrieveExecutionResult(stdout);
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException)
                        return new ExecutionResult.Timeout();

                    return new ExecutionResult.UnknownFailure();
                }
                finally
                {
                    Dispose();
                    await DisposeAsync();
                }
            }

            private async Task CreateVolumesAsync()
            {
                Directory.CreateDirectory(SandboxDirectory);

                ThrowIfCancellationRequested();

                using (var sw = File.CreateText(MainFilePath))
                    await sw.WriteLineAsync(new StringBuilder(MainContent), CancellationToken);

                ThrowIfCancellationRequested();

                using (var sw = File.CreateText(TestFilePath))
                    await sw.WriteLineAsync(new StringBuilder(Challenge.TestContent), CancellationToken);

                ThrowIfCancellationRequested();
            }

            private async Task<string> RunInContainerAsync()
            {
                Container = await DockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = Challenge.Environment.DockerImage,
                    Cmd = Challenge.Environment.DockerCommand.Split(" "),
                    AttachStdout = true,
                    Tty = true,
                    NetworkDisabled = true,
                    HostConfig = new HostConfig
                    {
                        Binds = new[] 
                        { 
                            $"{MainFilePath}:{Challenge.Environment.DockerMainPath}", 
                            $"{TestFilePath}:{Challenge.Environment.DockerTestPath}" 
                        }
                    }
                }, CancellationToken);

                await DockerClient.Containers.StartContainerAsync(Container.ID, null, CancellationToken);
                await DockerClient.Containers.WaitContainerAsync(Container.ID, CancellationToken);

                var stream = await DockerClient.Containers.GetContainerLogsAsync(Container.ID, true, new ContainerLogsParameters
                {
                    ShowStdout = true
                }, CancellationToken);

                var (stdout, _) = await stream.ReadOutputToEndAsync(CancellationToken);
                return stdout;
            }

            private ExecutionResult RetrieveExecutionResult(string value)
            {
                var rawJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                var type = rawJson["Type"];

                switch (type)
                {
                    case "test-succeeded":
                        return JsonConvert.DeserializeObject<ExecutionResult.TestSucceeded>(value);
                    case "test-failed":
                        return JsonConvert.DeserializeObject<ExecutionResult.TestFailed>(value);
                    case "build-failed":
                        return JsonConvert.DeserializeObject<ExecutionResult.BuildFailed>(value);
                    case "timeout":
                        return JsonConvert.DeserializeObject<ExecutionResult.Timeout>(value);
                    default:
                        return new ExecutionResult.UnknownFailure();
                }
            }

            private void ThrowIfCancellationRequested()
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();
            }

            public void Dispose()
            {
                Directory.Delete($"{AppDomain.CurrentDomain.BaseDirectory}/Sandbox/{Id}", true);
                GC.SuppressFinalize(this);
            }

            public async ValueTask DisposeAsync()
            {
                if (Container is object)
                    await DockerClient.Containers.RemoveContainerAsync(Container.ID, new ContainerRemoveParameters
                    {
                        Force = true,
                        RemoveVolumes = true
                    });

                GC.SuppressFinalize(this);
            }
        }

        public static async Task<ExecutionResult> ExecuteChallengeAsync(
            Challenge challenge, 
            string mainContent, 
            CancellationToken cancellationToken)
        {
            var execution = new Execution(challenge, mainContent, cancellationToken);
            return await Task.Run(execution.Start, cancellationToken);
        }
    }
}
