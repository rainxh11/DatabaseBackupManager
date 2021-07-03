$Application = (Get-ChildItem -Path . -Filter MegaDatabaseBackup.exe -Recurse -ErrorAction SilentlyContinue -Force |Select-Object -first 1).FullName
$Version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($Application).FileVersion
$GitAccount = git config user.email

$CommitTime = Get-Date
$CommitDescription = "${CommitTime} | Version:${Version} | ${GitAccount}" 

Echo $Application $Version $CommitTime  
Echo $CommitDescription


git add .
git commit -m $CommitDescription
git push -f 