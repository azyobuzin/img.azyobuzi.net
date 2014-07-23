package imgazyobuzi

import (
	"github.com/puerkitobio/goquery"
	"regexp"
	"strings"
)

type cameran resolverStruct

func (self *cameran) ServiceName() (string, string) {
	return "cameran", "cameran"
}

func (self *cameran) Regex() *regexp.Regexp {
	if self.re == nil {
		self.re = regexp.MustCompile(`^http://cameran\.in/(?:posts/get|p)/v1/(\w+)/?(?:\?.*)?(?:#.*)?$`)
	}
	return self.re
}

func (self *cameran) Id(groups []string) string {
	return groups[1]
}

func (self *cameran) Sizes(ctx *Context, groups []string) ([]ImageInfo, ResolvingErr) {
	id := groups[1]
	key := "cameran-" + id
	r, err := ctx.HGet(key, "image")
	if err != nil {
		return []ImageInfo{}, ResolvingErr{UnknownError, err}
	}

	if r == "" {
		doc, err := goquery.NewDocument(groups[0])
		if err != nil {
			return []ImageInfo{}, ResolvingErr{UnknownError, err}
		}

		og := doc.Find("meta[property='twitter:image:src']")
		if og.Length() == 0 {
			return []ImageInfo{}, ResolvingErr{PictureNotFound, nil}
		}
		r, _ = og.Attr("content")
		title := strings.TrimSpace(doc.Find(".main-comment").Text())

		ctx.HmSet(key, "image", r, "title", title)
	}

	return []ImageInfo{ImageInfo{r, r, r, ""}}, ResolvingErr{}
}

var CameranInstance = new(cameran)
