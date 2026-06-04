using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWindowsService(options => options.ServiceName = "DHCP Relay API");
builder.Services.AddMemoryCache();

var app = builder.Build();

var apiKey = builder.Configuration["ApiKey"] ?? "";

app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    if (!string.IsNullOrEmpty(apiKey))
    {
        if (!ctx.Request.Headers.TryGetValue("X-Api-Key", out var incomingKey) || incomingKey != apiKey)
        {
            Console.WriteLine($"[DHCP.Relay] Unauthorized request to {ctx.Request.Path} from {ctx.Connection.RemoteIpAddress}");
            ctx.Response.StatusCode = 401;
            await ctx.Response.WriteAsync("Unauthorized");
            return;
        }
    }
    await next();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.Now }));

app.MapGet("/leases", async (IMemoryCache cache) =>
{
    var json = await cache.GetOrCreateAsync("leases", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
        return await RunPowerShellAsync(@"
$result = @()
$scopes = Get-DhcpServerv4Scope
foreach ($scope in $scopes) {
    $leases = Get-DhcpServerv4Lease -ScopeId $scope.ScopeId
    foreach ($lease in $leases) {
        $result += [PSCustomObject]@{
            IpAddress   = $lease.IPAddress.ToString()
            HostName    = [string]$lease.HostName
            MacAddress  = [string]$lease.ClientId
            LeaseExpires = if ($lease.LeaseExpiryTime) { $lease.LeaseExpiryTime.ToString('dd-MM-yyyy HH:mm') } else { '' }
            ScopeId     = $scope.ScopeId.ToString()
        }
    }
}
if ($result.Count -eq 0) { '[]' } else { $result | ConvertTo-Json -Compress -Depth 2 }
");
    });

    return Results.Content(json ?? "[]", "application/json");
});

app.MapGet("/scopes", async (IMemoryCache cache) =>
{
    var json = await cache.GetOrCreateAsync("scopes", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
        return await RunPowerShellAsync(@"
$result = foreach ($scope in Get-DhcpServerv4Scope) {
    $count = (Get-DhcpServerv4Lease -ScopeId $scope.ScopeId).Count
    [PSCustomObject]@{
        ScopeId     = $scope.ScopeId.ToString()
        Name        = [string]$scope.Name
        SubnetMask  = $scope.SubnetMask.ToString()
        StartRange  = $scope.StartRange.ToString()
        EndRange    = $scope.EndRange.ToString()
        State       = $scope.State.ToString()
        ActiveLeases = $count
    }
}
if ($null -eq $result) { '[]' } else { @($result) | ConvertTo-Json -Compress -Depth 2 }
");
    });

    return Results.Content(json ?? "[]", "application/json");
});

app.Run();

static async Task<string> RunPowerShellAsync(string script)
{
    var tempFile = Path.Combine(Path.GetTempPath(), $"dhcp_relay_{Guid.NewGuid():N}.ps1");
    try
    {
        await File.WriteAllTextAsync(tempFile, script, Encoding.UTF8);

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NonInteractive -NoProfile -ExecutionPolicy Bypass -File \"{tempFile}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var err = await process.StandardError.ReadToEndAsync();
            Console.WriteLine($"[DHCP.Relay] PowerShell error: {err}");
            return "[]";
        }

        return string.IsNullOrWhiteSpace(output) ? "[]" : output.Trim();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DHCP.Relay] Exception: {ex.Message}");
        return "[]";
    }
    finally
    {
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }
}
