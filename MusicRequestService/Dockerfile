FROM microsoft/aspnetcore-build AS builder

# copy projects and restore
WORKDIR /source
COPY *.csproj .
RUN dotnet restore

# copy full source and build/publish
COPY . .
RUN dotnet publish --output /app/ --configuration Release

# new stage for the aspnetcore runtime image
FROM microsoft/aspnetcore
WORKDIR /app
COPY --from=builder /app .

# install powershell, python, pip, ffmpeg and youtube-dl
RUN apt-get update &&\
    apt-get install apt-utils curl gnupg apt-transport-https -y &&\
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - &&\
    sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-debian-stretch-prod stretch main" > /etc/apt/sources.list.d/microsoft.list' &&\
    apt-get update &&\
    apt-get install powershell python ffmpeg -y &&\
    curl https://bootstrap.pypa.io/get-pip.py -o get-pip.py &&\
    python get-pip.py &&\
    pip install youtube-dl

ENTRYPOINT ["dotnet", "MusicRequestService.dll"]