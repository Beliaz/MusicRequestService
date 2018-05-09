param([string] $URL, [string] $TargetFolder)

$outputTemplate = [System.IO.Path]::Combine($TargetFolder, "%(title)s.%(ext)s")

& "youtube-dl" --extract-audio --audio-format mp3 -o $outputTemplate -f "bestaudio[height <=? 720]/best[height <=? 720]/best" $URL 
