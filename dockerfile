# Use the official .NET SDK as the base image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Install .NET Framework Developer Pack for version 4.8
RUN apt-get update && apt-get install -y \
    wine \
    && wget https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-developer-pack-offline-installer \
    && wine dotnet-framework-4.8-developer-pack-offline-installer.exe /q \
    && apt-get clean && rm -rf /var/lib/apt/lists/* \
    && rm dotnet-framework-4.8-developer-pack-offline-installer.exe


# Copy and restore dependencies for ConsoleAppNetFramework
COPY src/ConsoleAppNetFramework/ConsoleAppNetFramework.csproj ./ConsoleAppNetFramework/
RUN dotnet restore ./ConsoleAppNetFramework/ConsoleAppNetFramework.csproj

# Copy the application source code for ConsoleAppNetFramework
COPY src/ConsoleAppNetFramework ./ConsoleAppNetFramework/

# Copy and restore dependencies for ConsoleApp1
COPY src/ConsoleApp1/ConsoleApp1.csproj ./ConsoleApp1/
RUN dotnet restore ./ConsoleApp1/ConsoleApp1.csproj

# Copy the application source code for ConsoleApp1
COPY src/ConsoleApp1 ./ConsoleApp1/

# Copy and restore dependencies for ClassLibraryNetStandard2
COPY src/ClassLibraryNetStandard2/ClassLibraryNetStandard2.csproj ./ClassLibraryNetStandard2/
RUN dotnet restore ./ClassLibraryNetStandard2/ClassLibraryNetStandard2.csproj

# Copy the application source code for ClassLibraryNetStandard2
COPY src/ClassLibraryNetStandard2 ./ClassLibraryNetStandard2/

# Copy and restore dependencies for Net6.UnitTests
COPY test/Net6.UnitTests/Net6.UnitTests.csproj ./Net6.UnitTests/
RUN dotnet restore ./Net6.UnitTests/Net6.UnitTests.csproj

# Copy the test project source code for Net6.UnitTests
COPY test/Net6.UnitTests ./Net6.UnitTests/

# Build each project
RUN dotnet publish -c Release -o out/ConsoleAppNetFramework ./ConsoleAppNetFramework
RUN dotnet publish -c Release -o out/ConsoleApp1 ./ConsoleApp1
RUN dotnet publish -c Release -o out/ClassLibraryNetStandard2 ./ClassLibraryNetStandard2
RUN dotnet publish -c Release -o out/Net6.UnitTests ./Net6.UnitTests

# Use a smaller runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy the published output of each project into their respective folders in the final image
COPY --from=build /app/out/ConsoleAppNetFramework ./src/ConsoleAppNetFramework/
COPY --from=build /app/out/ConsoleApp1 ./src/ConsoleApp1/
COPY --from=build /app/out/ClassLibraryNetStandard2 ./src/ClassLibraryNetStandard2/
COPY --from=build /app/out/Net6.UnitTests ./test/Net6.UnitTests/

# Set the entry point for your application (adjust as needed)
ENTRYPOINT ["dotnet", "src/ConsoleAppNetFramework/ConsoleAppNetFramework.dll"]
