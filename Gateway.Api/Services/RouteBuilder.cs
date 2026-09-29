namespace Gateway.Api.Services
{
    public sealed class RouteBuilder : IRouteBuilder
    {

       ///<inheritdoc/>
        public string Build(string endpoint, string apiVersion = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Adres nie może być pusty", nameof(endpoint));
            }

            string clean = endpoint.TrimStart('/');
            return $"{clean}";
        }

        ///<inheritdoc/>
        //public string Resolve(RestRequest request, string relativeEndpoint) =>
        //    Build(relativeEndpoint, request?.ApiVersion);
    }
}
