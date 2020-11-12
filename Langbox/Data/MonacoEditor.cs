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
        private readonly IJSRuntime _js;

        private string? _moduleUri;

        public MonacoEditor(IJSRuntime js)
        {
            _js = js;
        }

        /**
         * Initializes Monaco editor at the specified `domId` with `language` support.
         * 
         * If used in Razor views, it should be called right in `OnAfterRenderAsync`
         * callback.
         */
        public async Task InitializeAsync(string domId, string language)
        {
            _moduleUri = await _js.InvokeAsync<string>("Monaco.initialize", domId, language);
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
            if (_moduleUri is null)
                return "";
            
            return await _js.InvokeAsync<string>("Monaco.getValueOfModel", _moduleUri);
        }

        /**
         * Sets value of the editor.
         * 
         * If the editor has not been initialized, no action will be performed.
         */
        public async Task SetValueAsync(string value)
        {
            if (_moduleUri is { })
                await _js.InvokeVoidAsync("Monaco.setValueOfModel", _moduleUri, value);
        }

        /**
        * Sets language of the editor.
        * 
        * If the editor has not been initialized, no action will be performed.
        */
        public async Task SetLanguageAsync(string language)
        {
            if (_moduleUri is { })
                await _js.InvokeVoidAsync("Monaco.setLangOfModel", _moduleUri, language);
        }
    }
}
