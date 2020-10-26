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
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class ChallengeNew
    {
        [Inject] private MonacoEditor MainEditor { get; set; } = default!;
        [Inject] private MonacoEditor TestEditor { get; set; } = default!;
        [Inject] private ChallengeService ChallengeService { get; set; } = default!;
        [Inject] private MessageService AntdMessageService { get; set; } = default!;
        [Inject] private SandboxEnvironmentService SandboxEnvironmentService { get; set; } = default!;
        
        private ICollection<SandboxEnvironment> SandboxEnvironments { get; set; } 
            = new SandboxEnvironment[] { };
        private SandboxEnvironment? CurrentSandboxEnvironment { get; set; } = null;
        private string ChallengeTitle { get; set; } = "";
        private string ChallengeInstructions { get; set; } = "";

        protected override void OnInitialized()
        {
            SandboxEnvironments = SandboxEnvironmentService.ListAll();
            CurrentSandboxEnvironment = SandboxEnvironments.ElementAtOrDefault(0);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && CurrentSandboxEnvironment is { })
            {
                await MainEditor.InitializeAsync(
                    "main-editor",
                    CurrentSandboxEnvironment.Language,
                    CurrentSandboxEnvironment.MainBoilerplate);

                await TestEditor.InitializeAsync(
                    "test-editor",
                    CurrentSandboxEnvironment.Language,
                    CurrentSandboxEnvironment.TestBoilerplate);
            }
        }

        private async void OnTemplateChange(
            OneOf<string, IEnumerable<string>, LabeledValue, IEnumerable<LabeledValue>> value,
            OneOf<SelectOption, IEnumerable<SelectOption>> option)
        {
            if (!value.IsT0) return;
            
            CurrentSandboxEnvironment = await SandboxEnvironmentService.GetByTemplateNameAsync(value.AsT0);

            if (CurrentSandboxEnvironment is null) return;
            
            await MainEditor.SetLanguageAsync(CurrentSandboxEnvironment.Language);
            await TestEditor.SetLanguageAsync(CurrentSandboxEnvironment.Language);

            await MainEditor.SetValueAsync(CurrentSandboxEnvironment.MainBoilerplate);
            await TestEditor.SetValueAsync(CurrentSandboxEnvironment.TestBoilerplate);
        }

        private async Task OnSubmit()
        {
            if (CurrentSandboxEnvironment is { })
            {
                var challenge = new Challenge
                {
                    Title = ChallengeTitle,
                    Instructions = ChallengeInstructions,
                    MainContent = await MainEditor.GetValueAsync(),
                    TestContent = await TestEditor.GetValueAsync(),
                    SandboxEnvironmentId = CurrentSandboxEnvironment.TemplateName,
                };

                await ChallengeService.CreateAsync(challenge);
                
                _ = AntdMessageService.Success("Challenge successfully created!");
                _ = ResetForm();
            }
        }

        private async Task ResetForm()
        {
            ChallengeTitle = "";
            ChallengeInstructions = "";

            if (CurrentSandboxEnvironment is { })
            {
                await MainEditor.SetValueAsync(CurrentSandboxEnvironment.MainBoilerplate);
                await TestEditor.SetValueAsync(CurrentSandboxEnvironment.TestBoilerplate);
            }
        }
    }
}
