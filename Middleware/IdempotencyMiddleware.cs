using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.Entities;

namespace TaskFlowApi.Middleware;

public class IdempotencyMiddleware
{
    public const string HeaderName = "X-Idempotency-Key";

    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (!IsTargetRequest(context.Request))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var headerValues)
            || !Guid.TryParse(headerValues.ToString(), out var keyGuid))
        {
            await Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Idempotency key required",
                detail: $"Заголовок {HeaderName} (GUID) обязателен для этого запроса")
                .ExecuteAsync(context);
            return;
        }

        var key = keyGuid.ToString();

        context.Request.EnableBuffering();
        string requestBody;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            requestBody = await reader.ReadToEndAsync();
        }
        context.Request.Body.Position = 0;

        var requestHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(requestBody)));

        var existing = await db.IdempotencyRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Key == key);
        if (existing != null)
        {
            if (existing.RequestBodyHash != requestHash)
            {
                await Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Idempotency key conflict",
                    detail: "Idempotency key reused with different body")
                    .ExecuteAsync(context);
                return;
            }

            context.Response.StatusCode = existing.StatusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Headers["Idempotency-Replayed"] = "true";
            await context.Response.WriteAsync(existing.ResponseBody);
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        buffer.Position = 0;
        var responseBody = await new StreamReader(buffer, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();

        if (context.Response.StatusCode is >= 200 and < 300)
        {
            db.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = key,
                RequestBodyHash = requestHash,
                ResponseBody = responseBody,
                StatusCode = context.Response.StatusCode,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
            }
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(originalBody);
    }

    private static bool IsTargetRequest(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method))
        {
            return false;
        }

        var path = (request.Path.Value ?? string.Empty).TrimEnd('/');
        return path.Equals("/api/v2/tasks", StringComparison.OrdinalIgnoreCase)
               || path.Equals("/api/v2.0/tasks", StringComparison.OrdinalIgnoreCase);
    }
}
