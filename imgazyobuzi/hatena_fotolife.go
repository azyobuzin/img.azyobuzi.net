package imgazyobuzi

import (
	"fmt"
	"github.com/puerkitobio/goquery"
	"regexp"
	"strings"
)

type hatenaFotolife resolverStruct

func (self *hatenaFotolife) ServiceName() (string, string) {
	return "hfoto", "はてなフォトライフ"
}

func (self *hatenaFotolife) Regex() *regexp.Regexp {
	if self.re == nil {
		self.re = regexp.MustCompile(`^http://f\.hatena\.ne\.jp/(\w+)/(\d+)(?:\?.*)?(?:#.*)?$`)
	}
	return self.re
}

func (self *hatenaFotolife) Id(groups []string) string {
	return groups[1] + "/" + groups[2]
}

func (self *hatenaFotolife) Sizes(ctx *Context, groups []string) ([]ImageInfo, ResolvingErr) {
	key := "hfoto-" + self.Id(groups)
	r, err := ctx.HmGet(key, "ext", "orig", "video")
	if err != nil {
		return []ImageInfo{}, ResolvingErr{UnknownError, err}
	}

	username := groups[1]
	id := groups[2]

	ext := r[0]
	hasOriginal := r[1] != "0"
	isVideo := r[2] != "0"

	if ext == "" {
		doc, err := goquery.NewDocument(fmt.Sprintf("http://f.hatena.ne.jp/%s/%s", username, id))
		if err != nil {
			return []ImageInfo{}, ResolvingErr{UnknownError, err}
		}

		tag := doc.Find("#foto-for-html-tag>img")
		if tag.Length() == 0 {
			return []ImageInfo{}, ResolvingErr{PictureNotFound, nil}
		}
		src, _ := tag.Attr("src")
		ext = src[strings.LastIndex(src, ".")+1:]

		hasOriginal = doc.Find("a:contains('オリジナルサイズを表示')").Length() != 0
		isVideo = doc.Find("#flvplayer").Length() != 0

		ctx.HmSet(key, "ext", ext, "orig", hasOriginal, "video", isVideo)
	}

	video := ""
	if isVideo {
		video = fmt.Sprintf("http://cdn-ak.f.st-hatena.com/images/fotolife/%c/%s/%s/%s.flv",
			username[0], username, id[:8], id)
	}
	large := fmt.Sprintf("http://cdn-ak.f.st-hatena.com/images/fotolife/%c/%s/%s/%s.%s",
		username[0], username, id[:8], id, ext)
	full := large
	if hasOriginal {
		full = fmt.Sprintf("http://cdn-ak.f.st-hatena.com/images/fotolife/%c/%s/%s/%s_original.%s",
			username[0], username, id[:8], id, ext)
	}
	return []ImageInfo{
		ImageInfo{
			full, large,
			fmt.Sprintf("http://cdn-ak.f.st-hatena.com/images/fotolife/%c/%s/%s/%s_120.jpg",
				username[0], username, id[:8], id),
			video,
		},
	}, ResolvingErr{}
}

var HatenaFotolifeInstance = new(hatenaFotolife)
