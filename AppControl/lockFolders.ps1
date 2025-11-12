# See http://10.246.231.240:4000/q/peblocker/issues/78#issuecomment-1143
# Locks typical user writable root folders & adds Deny ACEs to prevent permission changes by non-owners

$root = "C:\Users"
$userFolders = Get-ChildItem -Path $root -Directory | Where-Object { $_.Name -ne "Public" }

foreach ($folder in $userFolders) {
    Write-Host "`nProcessing $($folder.FullName) for user $($folder.Name)"

    $acl = Get-Acl $folder.FullName
    $owner = $acl.Owner

    # 1. Disable inheritance and copy inherited ACEs
    $acl.SetAccessRuleProtection($true, $true)

    # 2. Find non-owner principals with Full Control and add Deny ACE for ChangePermissions
    foreach ($ace in $acl.Access) {
        $principal = $ace.IdentityReference
        $isOwner = ($principal.Value -eq $owner)
        $hasFull = ($ace.FileSystemRights -band [System.Security.AccessControl.FileSystemRights]::FullControl) -eq [System.Security.AccessControl.FileSystemRights]::FullControl

        if (-not $isOwner -and $hasFull) {
            Write-Host "Non-owner principal $($principal.Value) has Full Control; adding Deny ChangePermissions ACE."
            $denyChange = [System.Security.AccessControl.FileSystemRights]::ChangePermissions
            $inherit = [System.Security.AccessControl.InheritanceFlags]::ContainerInherit -bor [System.Security.AccessControl.InheritanceFlags]::ObjectInherit
            $propagate = [System.Security.AccessControl.PropagationFlags]::None
            $denyRuleChange = New-Object System.Security.AccessControl.FileSystemAccessRule($principal, $denyChange, $inherit, $propagate, "Deny")
            $acl.AddAccessRule($denyRuleChange)
        }
    }

    # 3. Add Deny for CREATOR_OWNER: Traverse/Execute
    $denyRights = [System.Security.AccessControl.FileSystemRights]::Traverse -bor [System.Security.AccessControl.FileSystemRights]::ExecuteFile
    $inheritCO = [System.Security.AccessControl.InheritanceFlags]::ContainerInherit -bor [System.Security.AccessControl.InheritanceFlags]::ObjectInherit
    $propagateCO = [System.Security.AccessControl.PropagationFlags]::None
    $denyRuleCO = New-Object System.Security.AccessControl.FileSystemAccessRule("CREATOR OWNER", $denyRights, $inheritCO, $propagateCO, "Deny")
    $acl.AddAccessRule($denyRuleCO)

    # 4. Apply new ACL
    Set-Acl -Path $folder.FullName -AclObject $acl

    Write-Host "ACL set for $($folder.FullName)"
}

# Deny CREATOR_OWNER Traverse/Execute for standard paths
$targetPaths = @("C:\Users\Public", "C:\Windows\Temp", "C:\ProgramData")
foreach ($path in $targetPaths) {
    if (Test-Path $path) {
        Write-Host "`nApplying CREATOR_OWNER deny traverse/execute ACE to $path"
        $acl = Get-Acl $path
        $inherit = [System.Security.AccessControl.InheritanceFlags]::ContainerInherit -bor [System.Security.AccessControl.InheritanceFlags]::ObjectInherit
        $propagate = [System.Security.AccessControl.PropagationFlags]::None
        $denyRights = [System.Security.AccessControl.FileSystemRights]::Traverse -bor [System.Security.AccessControl.FileSystemRights]::ExecuteFile
        $denyRule = New-Object System.Security.AccessControl.FileSystemAccessRule("CREATOR OWNER", $denyRights, $inherit, $propagate, "Deny")
        $acl.AddAccessRule($denyRule)
        Set-Acl -Path $path -AclObject $acl
        Write-Host "Deny ACE applied to $path"
    }
}