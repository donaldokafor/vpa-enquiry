FROM microsoft/dotnet:2.0.0-sdk
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out


EXPOSE 32166/tcp

ENV ASPNETCORE_URLS http://*:32166

ENTRYPOINT ["dotnet", "out/VPA.dll"]
