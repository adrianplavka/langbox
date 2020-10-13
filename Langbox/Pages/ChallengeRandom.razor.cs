using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

using Langbox.Services;

namespace Langbox.Pages
{
    [Route("/challenge")]
    public partial class ChallengeRandom
    {
        [Inject] private ChallengeService ChallengeService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var challenge = await ChallengeService.GetRandomAsync();

            if (challenge is object)
                NavigationManager.NavigateTo($"/challenge/{challenge.Id}");
        }
    }
}
