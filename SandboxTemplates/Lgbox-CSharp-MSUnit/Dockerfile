# Base Dockerfile for Lgbox-CSharp-MSUnit template.
#
# Ensures, that the project files will be present in the "/app" folder
# w/ dependencies installed.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster
WORKDIR /app

COPY . .
RUN dotnet restore

CMD python main.py