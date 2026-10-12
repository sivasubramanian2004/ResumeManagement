namespace ResumeManagement.Api.Helpers
{
    public class UrlService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;

            if (request == null)
            {
                throw new InvalidOperationException(
                    "HTTP context is not available.");
            }

            return $"{request.Scheme}://{request.Host}";
        }
    }
}
