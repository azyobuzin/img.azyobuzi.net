FROM microsoft/aspnetcore-build:2 AS builder

COPY . /source/
WORKDIR /source/ImgAzyobuziNet
RUN dotnet publish --output /app/ --configuration Release

FROM microsoft/aspnetcore:2
WORKDIR /app
EXPOSE 80

COPY --from=builder /app .

ENTRYPOINT ["dotnet", "ImgAzyobuziNet.dll"]
