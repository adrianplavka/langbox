/*
 * Helper functions for Monaco Editor under unified namespace.
 */

const Monaco = {
    /**
    * Initializes Monaco Editor at the specified DOM id.
    * 
    * @param {string} id
    * @param {string} lang
    * @returns string
    */
    initialize: (id, lang) =>
        monaco.editor.create(document.getElementById(id), {
            language: lang,
            theme: 'vs-dark'
        }).getModel().uri.toString(),

    /**
     * Retrieves current value of editor model under `uri` path.
     * 
     * @param {string} uri
     * @returns string
     */
    getValueOfModel: (uri) =>
        monaco.editor.getModel(uri).getValue(),

    /**
     * Sets `value` of editor model under `uri` path.
     * 
     * @param {string} uri
     * @param {string} value
     */
    setValueOfModel: (uri, value) =>
        monaco.editor.getModel(uri).setValue(value),

    /**
     * Sets `lang` of editor model under `uri` path.
     * 
     * @param {string} uri
     * @param {string} lang
     */
    setLangOfModel: (uri, lang) =>
        monaco.editor.getModel(uri).updateOptions({
            language: lang,
            theme: 'vs-dark'
        }),
};

window.Monaco = Monaco;