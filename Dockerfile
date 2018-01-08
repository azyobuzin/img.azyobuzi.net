FROM microsoft/aspnetcore-build:2 AS builder

COPY . /source/
WORKDIR /source/ImgAzyobuziNet
RUN dotnet publish --output /app/ --configuration Release

# Microsoft.AspNetCore.All を使っていないので
# microsoft/aspnetcore のほうがイメージが大きくなってしまう
FROM microsoft/dotnet:2-runtime
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80
WORKDIR /app

COPY --from=builder /app .

ENTRYPOINT ["dotnet", "ImgAzyobuziNet.dll"]
