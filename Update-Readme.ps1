function Get-TilInfo()
{
    $categoryLookup = @{
        "csharp" = "C#";
        "uwp" = "UWP";
        "caliburn" = "Caliburn.Micro";
        "git" = "Git";
        "js" = "Javascript";
        "linux" = "Linux";
        "markdown" = "Markdown";
        "mongo" = "Mongo";
        "powershell" = "Powershell";
        "python" = "Python";
        "unity" = "Unity";
        "vs" = "Visual Studio";
        "windows" = "Windows";
    }

    $tilFiles = Get-ChildItem . -Recurse -Filter *.md | ? { $_.DirectoryName -ne $PSScriptRoot}
    foreach($tilFile in $tilFiles)
    {
        $path = $tilFile.FullName.Replace("$($PSScriptRoot)\", "").Replace("\", "/")
        $categoryParts = (Split-Path -Parent $path) -split "/" -split "\\"
        $category = $categoryLookup[$categoryParts[0] ?? ""]
        $subCategory = $categoryLookup[$categoryParts[1] ?? ""]
        $title = (Get-Content $tilFile -TotalCount 1).TrimStart("# ")
        @{
            "fullPath" = $tilFile;
            "path" = $path;
            "category" = $category;
            "subCategory" = $subCategory;
            "title" = $title
        }
    }
}

$tilInfos = Get-TilInfo

$categoryIndex = [Text.StringBuilder]::new()
$tilIndex = [Text.StringBuilder]::new()
$currentCategory = $null
$currentSubCategory = $null
$tilInfos | Sort-Object category, title | ForEach-Object {
    if($_.category -ne $currentCategory)
    {
        # Add category index
        $categoryAnchor = ($_.category -replace "#","" -replace " ", "-").ToLower()
        $categoryIndex.AppendLine("* [$($_.category)](#$($categoryAnchor))") | out-null

        # Write category header
        $tilIndex.AppendLine("`n---`n") | out-null
        $tilIndex.AppendLine("### $($_.category -replace "#","\#")`n") | out-null

        $currentCategory = $_.category
        $currentSubCategory = $null
    }

    if($_.subCategory -and ($_.subCategory -ne $currentSubCategory))
    {
        # Add sub category index
        $subCategoryAnchor = ($_.subCategory -replace "#","" -replace " ", "-").ToLower()
        $categoryIndex.AppendLine("  * [$($_.subCategory)](#$($subCategoryAnchor))") | out-null

        # Add sub-category header
        $tilIndex.AppendLine("`n#### $($_.subCategory)`n") | out-null

        $currentSubCategory = $_.subCategory
    }

    $tilIndex.AppendLine("* [$($_.title)]($($_.path))") | out-null
}

$updatedContent = 
    "<!-- index starts -->`n" +
    "## Categories`n`n" +
    "$categoryIndex" +
    "$tilIndex`n" +
    "<!-- index ends -->"

$indexRegex = "(?s)<!\-\- index starts \-\->.*<!\-\- index ends \-\->"
$content = Get-Content (Join-Path $PSScriptRoot "README.md") -Raw
$content = $content -replace $indexRegex,$updatedContent
$content | Set-Content README.md
