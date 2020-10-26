using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

using Langbox.Services;

namespace Langbox.Pages
{
    [Route("/challenge")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class ChallengeRandom
    {
        [Inject] private ChallengeService ChallengeService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var challenge = await ChallengeService.GetRandomAsync();

            if (challenge is { })
                NavigationManager.NavigateTo($"/challenge/{challenge.Id}");
        }
    }
}
