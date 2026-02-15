# PowerShell script to test Russian format CSV upload
$filePath = "test_russian_format.csv"
$url = "http://localhost:5021/Contacts/Upload"

Write-Host "Testing Russian date format CSV upload..."

# Read file content
$fileBytes = [System.IO.File]::ReadAllBytes($filePath)
$fileContent = [System.Text.Encoding]::UTF8.GetString($fileBytes)

Write-Host "File content:"
Write-Host $fileContent

# Create multipart form data
$boundary = [System.Guid]::NewGuid().ToString()
$LF = "`r`n"

$bodyLines = (
    "--$boundary",
    "Content-Disposition: form-data; name=`"file`"; filename=`"$([System.IO.Path]::GetFileName($filePath))`"",
    "Content-Type: text/csv",
    "",
    $fileContent,
    "--$boundary--"
) -join $LF

# Send request
try {
    $response = Invoke-WebRequest -Uri $url -Method POST -ContentType "multipart/form-data; boundary=$boundary" -Body $bodyLines
    Write-Host "Russian format upload successful!"
    Write-Host "Response: $($response.Content)"
} catch {
    Write-Host "Russian format upload failed!"
    Write-Host "Error: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)"
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error Content: $errorContent"
    }
}