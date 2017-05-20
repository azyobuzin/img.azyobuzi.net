FROM microsoft/dotnet:1.1.2-runtime
ENV ASPNETCORE_URLS http://+:80
WORKDIR /app
EXPOSE 80

ARG configuration=Release
COPY bin/$configuration/netcoreapp1.1/publish/ /app/

ENTRYPOINT ["dotnet", "ImgAzyobuziNet.dll"]
