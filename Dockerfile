ARG tag=2.1.700
FROM mcr.microsoft.com/dotnet/core/sdk:${tag}-bionic

RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y software-properties-common && \
    add-apt-repository universe && \
    apt-get install -y powershell

