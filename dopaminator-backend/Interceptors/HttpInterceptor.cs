public class HttpInterceptor : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Request: {request.Method} {request.RequestUri}");
        
        if (request.Content != null)
        {
            var body = await request.Content.ReadAsStringAsync();
            Console.WriteLine($"Body: {body}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}