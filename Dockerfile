FROM microsoft/dotnet:2.2-sdk AS builder

COPY . /source/
WORKDIR /source/ImgAzyobuziNet
RUN dotnet publish --output /app/ --configuration Release

FROM microsoft/dotnet:2.2-aspnetcore-runtime
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80
WORKDIR /app

COPY --from=builder /app .

ENTRYPOINT ["dotnet", "ImgAzyobuziNet.dll"]
