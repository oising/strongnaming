
if (-not $dte) {
    write-warning "Not running in NuGet PM console. Package related functions are disabled."
} else {
    $SCRIPT:packagesRoot = join-path (split-path $dte.solution.filename) packages
    $SCRIPT:packageCache = @{}
}

function Update-PackageReferenceCache {
	if (-not $dte) { return }

	if (-not (test-path $packagesRoot)) {
		write-warning "No package repository found."
		return
	}

	dir -rec $packagesRoot -include *.nuspec | % {
		$m = [xml](gc $_.fullname)
		$SCRIPT:packageCache[$m.package.metadata.id] = $m.package.metadata.references.reference | `
			select -expand file
	}
}

function Get-ProjectReference {
 
    param(
        [Parameter(
            Position = 0,
            Mandatory= $true,
            ValueFromPipelineByPropertyName = $true
        )]
        [ValidateNotNullOrEmpty()]
        [string]$ProjectName,

        [Parameter()]
        [switch]$IncludeSigned
    )
    
    if (-not $dte) { return }
    
    $project = get-project -name $ProjectName
        
	$project.object.references | where-object {
		(-not $_.strongname) -or $IncludeSigned.IsPresent
	} | select -expand path | % {

		$path = $_

		#new-object io.fileinfo $_ | add-member 
	}
}