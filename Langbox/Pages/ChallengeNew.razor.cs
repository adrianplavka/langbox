using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

using AntDesign;
using OneOf;

using Langbox.Data;
using Langbox.Models;
using Langbox.Services;

namespace Langbox.Pages
{
    [Route("/challenge/new")]
    public partial class ChallengeNew
    {
        [Inject] private MonacoEditor MainEditor { get; set; } = default!;
        [Inject] private MonacoEditor TestEditor { get; set; } = default!;
        [Inject] private ChallengeService ChallengeService { get; set; } = default!;
        [Inject] private MessageService AntdMessageService { get; set; } = default!;
        [Inject] private SandboxEnvironmentService SandboxEnvironmentService { get; set; } = default!;
        
        private ICollection<SandboxEnvironment> sandboxEnvironments = new SandboxEnvironment[] { };
        private SandboxEnvironment? currentSandboxEnvironment = null;
        private string challengeTitle = "";
        private string challengeInstructions = "";

        protected override void OnInitialized()
        {
            sandboxEnvironments = SandboxEnvironmentService.ListAll();
            currentSandboxEnvironment = sandboxEnvironments.ElementAtOrDefault(0);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && currentSandboxEnvironment is object)
            {
                await MainEditor.InitializeAsync(
                    "main-editor",
                    currentSandboxEnvironment.Language,
                    currentSandboxEnvironment.MainBoilerplate);

                await TestEditor.InitializeAsync(
                    "test-editor",
                    currentSandboxEnvironment.Language,
                    currentSandboxEnvironment.TestBoilerplate);
            }
        }

        private async void OnTemplateChange(
            OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value,
            OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            if (value.IsT0)
            {
                currentSandboxEnvironment = await SandboxEnvironmentService.GetByTemplateNameAsync(value.AsT0);

                if (currentSandboxEnvironment is object)
                {
                    await MainEditor.SetLanguageAsync(currentSandboxEnvironment.Language);
                    await TestEditor.SetLanguageAsync(currentSandboxEnvironment.Language);

                    await MainEditor.SetValueAsync(currentSandboxEnvironment.MainBoilerplate);
                    await TestEditor.SetValueAsync(currentSandboxEnvironment.TestBoilerplate);
                }
            }
        }

        private async Task OnSubmit()
        {
            if (currentSandboxEnvironment is object)
            {
                var challenge = new Challenge
                {
                    Title = challengeTitle,
                    Instructions = challengeInstructions,
                    MainContent = await MainEditor.GetValueAsync(),
                    TestContent = await TestEditor.GetValueAsync(),
                    SandboxEnvironmentId = currentSandboxEnvironment.TemplateName,
                };

                await ChallengeService.CreateAsync(challenge);
                
                _ = AntdMessageService.Success("Challenge successfully created!");
                _ = ResetForm();
            }
        }

        private async Task ResetForm()
        {
            challengeTitle = "";
            challengeInstructions = "";

            if (currentSandboxEnvironment is object)
            {
                await MainEditor.SetValueAsync(currentSandboxEnvironment.MainBoilerplate);
                await TestEditor.SetValueAsync(currentSandboxEnvironment.TestBoilerplate);
            }
        }
    }
}
