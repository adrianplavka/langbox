using Microsoft.AspNetCore.Components;

using Markdig;

namespace Langbox.Shared.Components
{
    public partial class Markdown
    {
        private string _content = "";

        [Parameter]
        public string Content
        {
            get 
            { 
                return _content; 
            }
            
            set
            {
                _content = Markdig.Markdown.ToHtml(
                    markdown: value,
                    pipeline: new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()
                );
            }
        }
    }
}
