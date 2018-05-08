param([string] $URL, [string] $TargetFolder)

& "youtube-dl" --ffmpeg-location C:\ffmpeg-20171111-3af2bf0-win64-static\bin --extract-audio --audio-format mp3 -o "$TargetFolder\%(title)s.%(ext)s" -f "bestaudio[height <=? 720]/best[height <=? 720]/best" $URL 