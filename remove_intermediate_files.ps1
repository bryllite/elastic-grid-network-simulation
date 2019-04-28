Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.FullName -Force -Recurse }
