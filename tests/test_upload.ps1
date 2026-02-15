# PowerShell script to test CSV upload
$filePath = "test_russian_dates.csv"
$url = "http://localhost:5021/Contacts/Upload"

# Read file content
$fileBytes = [System.IO.File]::ReadAllBytes($filePath)
$fileContent = [System.Text.Encoding]::UTF8.GetString($fileBytes)

Write-Host "Uploading file content:"
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
    Write-Host "Upload successful!"
    Write-Host "Response: $($response.Content)"
} catch {
    Write-Host "Upload failed!"
    Write-Host "Error: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)"
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error Content: $errorContent"
    }
}