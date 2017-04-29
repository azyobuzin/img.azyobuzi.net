# img.azyobuzi.net v3
ASP.NET Core になって新登場。

昔のコードは v2 ブランチをご覧ください。

# ビルド
## 普通にビルド
```
dotnet restore
dotnet build
```

## Docker
```
cd ImgAzyobuziNet
dotnet msbuild /t:DockerBuild /p:Configuration=Release
```

tar にするなら `/t:DockerSave` で。
