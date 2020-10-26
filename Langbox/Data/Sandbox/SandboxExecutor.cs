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
    public static class SandboxExecutor
    {
        private class Execution : IAsyncDisposable
        {
            private Challenge Challenge { get; }

            private string MainContent { get; }

            private CancellationToken CancellationToken { get; }

            private string SandboxDirectory { get; }

            private string MainFilePath { get; }

            private string TestFilePath { get; }

            private readonly string _id = Guid.NewGuid().ToString();

            private readonly DockerClient _dockerClient = new DockerClientConfiguration().CreateClient();

            private CreateContainerResponse? _container;

            public Execution(Challenge challenge, string mainContent, CancellationToken cancellationToken)
            {
                Challenge = challenge;
                MainContent = mainContent;
                CancellationToken = cancellationToken;
                SandboxDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}/Sandbox/{_id}";
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
                    await DisposeAsync();
                }
            }

            private async Task CreateVolumesAsync()
            {
                Directory.CreateDirectory(SandboxDirectory);

                ThrowIfCancellationRequested();

                await using (var sw = File.CreateText(MainFilePath))
                    await sw.WriteLineAsync(new StringBuilder(MainContent), CancellationToken);

                ThrowIfCancellationRequested();

                await using (var sw = File.CreateText(TestFilePath))
                    await sw.WriteLineAsync(new StringBuilder(Challenge.TestContent), CancellationToken);

                ThrowIfCancellationRequested();
            }

            private async Task<string> RunInContainerAsync()
            {
                _container = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
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

                await _dockerClient.Containers.StartContainerAsync(_container.ID, null, CancellationToken);
                await _dockerClient.Containers.WaitContainerAsync(_container.ID, CancellationToken);

                var stream = await _dockerClient.Containers.GetContainerLogsAsync(_container.ID, true, new ContainerLogsParameters
                {
                    ShowStdout = true
                }, CancellationToken);

                var (stdout, _) = await stream.ReadOutputToEndAsync(CancellationToken);
                return stdout;
            }

            private static ExecutionResult RetrieveExecutionResult(string value)
            {
                var rawJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

                return rawJson["Type"] switch
                {
                    "test-succeeded" => JsonConvert.DeserializeObject<ExecutionResult.TestSucceeded>(value),
                    "test-failed" => JsonConvert.DeserializeObject<ExecutionResult.TestFailed>(value),
                    "build-failed" => JsonConvert.DeserializeObject<ExecutionResult.BuildFailed>(value),
                    "timeout" => JsonConvert.DeserializeObject<ExecutionResult.Timeout>(value),
                    _ => new ExecutionResult.UnknownFailure()
                };
            }

            private void ThrowIfCancellationRequested()
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();
            }

            public async ValueTask DisposeAsync()
            {
                if (_container is { })
                    // ReSharper disable once MethodSupportsCancellation
                    await _dockerClient.Containers.RemoveContainerAsync(_container.ID, new ContainerRemoveParameters
                    {
                        Force = true,
                        RemoveVolumes = true
                    });
                
                Directory.Delete($"{AppDomain.CurrentDomain.BaseDirectory}/Sandbox/{_id}", true);

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
