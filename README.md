# img.azyobuzi.net
img.azyobuzi.net は web 上に数ある画像共有サービスから画像の URI を取得する API を提供します。

## 簡単な使い方
### 1. 正規表現を取得する
```
https://img.azyobuzi.net/api/v3/services
```

対応している画像共有サービスの URI にマッチする正規表現を取得します。

この正規表現は基本的な構文のみを使用しているため、多くの正規表現エンジンで実行することができます。ホスト部は小文字で記述されているため、大文字・小文字を区別しないオプション(i フラグ)を使用することをおすすめします。

### 2. 画像の URI を取得する
```
https://img.azyobuzi.net/api/v3/resolve?uri=http://f.hatena.ne.jp/azyobuzin/20130909162630
```

```json
{
    "service_id": "HatenaFotolife",
    "service_name": "はてなフォトライフ",
    "images": [
        {
            "full": "http://cdn-ak.f.st-hatena.com/images/fotolife/a/azyobuzin/20130909/20130909162630_original.jpg",
            "large": "http://cdn-ak.f.st-hatena.com/images/fotolife/a/azyobuzin/20130909/20130909162630.jpg",
            "thumb": "http://cdn-ak.f.st-hatena.com/images/fotolife/a/azyobuzin/20130909/20130909162630_120.jpg",
            "video_full": null,
            "video_large": null,
            "video_mobile": null
        }
    ]
}
```

full, large, thumb の 3 種類のサイズを取得することができます。

### 3. リダイレクト
```
https://img.azyobuzi.net/api/v3/redirect?uri=http://f.hatena.ne.jp/azyobuzin/20130909162630&size=large
```

```
302
Location: http://cdn-ak.f.st-hatena.com/images/fotolife/a/azyobuzin/20130909/20130909162630.jpg
```

指定したサイズの画像へリダイレクトします。
