[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()

$keys = docker exec valkey valkey-cli --raw --scan --pattern "sample_chat:*"

foreach ($key in $keys) {
    Write-Host "`n=== $key ==="

    docker exec valkey valkey-cli --raw LRANGE $key 0 -1 |
    ForEach-Object {
        $message = $_ | ConvertFrom-Json

        [PSCustomObject]@{
            Role = $message.role
            Text = ($message.contents | ForEach-Object { $_.text }) -join "`n"
        }
    } |
    Format-Table -Wrap
}