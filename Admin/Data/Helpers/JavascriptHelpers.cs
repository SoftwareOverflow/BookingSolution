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
    }
}
