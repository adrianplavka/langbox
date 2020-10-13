using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Langbox.Data
{
    /**
     * Provides an injectable class of Monaco editor over JavaScript 
     * interoperability.
     */
    public class MonacoEditor
    {
        private readonly IJSRuntime JS;

        public string? ModelUri { get; private set; } = null;

        public MonacoEditor(IJSRuntime js)
        {
            JS = js;
        }

        /**
         * Initializes Monaco editor at the specified `domId` with `language` support.
         * 
         * If used in Razor views, it should be called right in `OnAfterRenderAsync`
         * callback.
         */
        public async Task InitializeAsync(string domId, string language)
        {
            ModelUri = await JS.InvokeAsync<string>("Monaco.initialize", domId, language);
        }

        /**
         * Initializes Monaco editor at the specified `domId` with `language` support
         * & initial `value`.
         * 
         * If used in Razor views, it should be called right in `OnAfterRenderAsync`
         * callback.
         */
        public async Task InitializeAsync(string domId, string language, string value)
        {
            await InitializeAsync(domId, language);
            await SetValueAsync(value);
        }

        /**
         * Retrieves current value of the editor.
         * 
         * If the editor has not been initialized, returns an empty string.
         */
        public async Task<string> GetValueAsync()
        {
            if (!(ModelUri is null))
            {
                return await JS.InvokeAsync<string>("Monaco.getValueOfModel", ModelUri);
            } else
            {
                return "";
            }
        }

        /**
         * Sets value of the editor.
         * 
         * If the editor has not been initialized, no action will be performed.
         */
        public async Task SetValueAsync(string value)
        {
            if (!(ModelUri is null))
            {
                await JS.InvokeVoidAsync("Monaco.setValueOfModel", ModelUri, value);
            }
        }

        /**
        * Sets language of the editor.
        * 
        * If the editor has not been initialized, no action will be performed.
        */
        public async Task SetLanguageAsync(string language)
        {
            if (!(ModelUri is null))
            {
                await JS.InvokeVoidAsync("Monaco.setLangOfModel", ModelUri, language);
            }
        }
    }
}
