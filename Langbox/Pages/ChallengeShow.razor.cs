using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

using Langbox.Data;
using Langbox.Data.Sandbox;
using Langbox.Models;
using Langbox.Services;

namespace Langbox.Pages
{
    [Route("/challenge/{Id:int}")]
    public partial class ChallengeShow : IDisposable
    {
        [Parameter] public int Id { get; set; }
        [Inject] private MonacoEditor MainEditor { get; set; } = default!;
        [Inject] private ChallengeService ChallengeService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        
        private Challenge currentChallenge { get; set; } = new Challenge();
        private string currentTab = "instructions";
        private ExecutionResult? lastExecutionResult;

        private bool isTesting = false;
        private CancellationTokenSource sandboxExecutionCancellationSource = new CancellationTokenSource();

        protected override async Task OnInitializedAsync()
        {
            var challenge = await ChallengeService.GetByIdWithEnvironmentAsync(Id);

            if (challenge is object)
                currentChallenge = challenge;
            else
                NavigationManager.NavigateTo("/");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await MainEditor.InitializeAsync(
                    "main-editor",
                    currentChallenge.Environment.Language,
                    currentChallenge.MainContent);
        }

        private async Task OnTest()
        {
            if (isTesting)
                return;

            currentTab = "output";
            isTesting = true;

            lastExecutionResult = await SandboxExecutor.ExecuteChallengeAsync(
                currentChallenge,
                await MainEditor.GetValueAsync(),
                sandboxExecutionCancellationSource.Token
            );

            currentTab = "output";
            isTesting = false;
        }

        private void OnCancelTest()
        {
            sandboxExecutionCancellationSource.Cancel();
            sandboxExecutionCancellationSource.Dispose();
            sandboxExecutionCancellationSource = new CancellationTokenSource();
            isTesting = false;
        }

        public void Dispose()
        {
            sandboxExecutionCancellationSource.Cancel();
            sandboxExecutionCancellationSource.Dispose();
        }
    }
}
