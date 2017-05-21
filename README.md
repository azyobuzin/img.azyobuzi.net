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

# Pull Request 募集中
v2 からの移行に手が回らない状況です。
v2 ブランチのソースからいい感じに書き直していただけるとよろこびます。
いい感じでなくても手直しするので適当によろしくお願いします。
Issue を作成して、作者のケツ叩きをしてくれてもOKです。
