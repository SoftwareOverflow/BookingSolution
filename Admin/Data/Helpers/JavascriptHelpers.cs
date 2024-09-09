using Microsoft.JSInterop;

namespace Admin.Data.Helpers
{
    public static class JavascriptHelpers
    {
        public static async Task<DateTime?> GetLocatDateAsync(IJSRuntime js)
        {
            try
            {
                var dateStr = await js.InvokeAsync<string>("getLocalDate");

                if (DateTime.TryParse(dateStr, out var localDate))
                {
                    return localDate;
                }
                else
                {
                    // TODO logging.
                }
            }
            catch (Exception)
            {
                // TODO logging.
            }

            return null;
        }

        public static async Task<bool> CopyToClipboard(IJSRuntime js, string textToCopy)
        {
            try
            {
                return await js.InvokeAsync<bool>("clipboardCopy.copyText", textToCopy);
            }
            catch (Exception)
            {
                // TODO logging
            }

            return false;
        }
    }
}
