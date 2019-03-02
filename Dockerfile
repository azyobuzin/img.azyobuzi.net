FROM microsoft/dotnet:2.2-sdk AS builder
COPY . /source/
WORKDIR /source/ImgAzyobuziNet
RUN dotnet publish --output /app/ --configuration Release

FROM node:0.12.7 AS apidoc
RUN npm install -g aglio@latest
COPY docs/api.apib /
RUN aglio -i /api.apib -o /developers.html

FROM microsoft/dotnet:2.2-aspnetcore-runtime
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80
WORKDIR /app

COPY --from=builder /app .
COPY --from=apidoc /developers.html /app/wwwroot/

ENTRYPOINT ["dotnet", "ImgAzyobuziNet.dll"]
