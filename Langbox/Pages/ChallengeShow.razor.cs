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
        
        private Challenge CurrentChallenge { get; set; } = new Challenge();
        private string CurrentTab { get; set; } = "instructions";
        private bool IsTesting { get; set; } = false;
        private ExecutionResult? LastExecutionResult { get; set; }
        private CancellationTokenSource SandboxExecutionCancellationSource { get; set; } 
            = new CancellationTokenSource();

        protected override async Task OnInitializedAsync()
        {
            var challenge = await ChallengeService.GetByIdWithEnvironmentAsync(Id);

            if (challenge is { })
                CurrentChallenge = challenge;
            else
                NavigationManager.NavigateTo("/");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await MainEditor.InitializeAsync(
                    "main-editor",
                    CurrentChallenge.Environment.Language,
                    CurrentChallenge.MainContent);
        }

        private async Task OnNextChallenge()
        {
            var nextChallenge = await ChallengeService.GetRandomWithoutIdAsync(CurrentChallenge.Id);

            if (nextChallenge is { })
            {
                OnCancelTest();

                CurrentChallenge = nextChallenge;
                NavigationManager.NavigateTo($"/challenge/{CurrentChallenge.Id}");
                await MainEditor.SetValueAsync(CurrentChallenge.MainContent);
            }
        }

        private async Task OnTest()
        {
            if (IsTesting)
                return;

            CurrentTab = "output";
            IsTesting = true;

            LastExecutionResult = await SandboxExecutor.ExecuteChallengeAsync(
                CurrentChallenge,
                await MainEditor.GetValueAsync(),
                SandboxExecutionCancellationSource.Token
            );

            CurrentTab = "output";
            IsTesting = false;
        }

        private void OnCancelTest()
        {
            SandboxExecutionCancellationSource.Cancel();
            SandboxExecutionCancellationSource.Dispose();
            SandboxExecutionCancellationSource = new CancellationTokenSource();
            IsTesting = false;
        }

        public void Dispose()
        {
            SandboxExecutionCancellationSource.Cancel();
            SandboxExecutionCancellationSource.Dispose();
        }
    }
}
