package imgazyobuzi

import (
	"encoding/json"
	"net/http"
	"regexp"
)

type fiveHundredPx resolverStruct

func (self *fiveHundredPx) ServiceName() (string, string) {
	return "500px", "500px"
}

func (self *fiveHundredPx) Regex() *regexp.Regexp {
	if self.re == nil {
		self.re = regexp.MustCompile(`^https?://(?:www\.)?500px\.com/photo/(\d+)(?:/|/[\w\-]+/?)?(?:\?.*)?(?:#.*)?$`)
	}
	return self.re
}

func (self *fiveHundredPx) Id(groups []string) string {
	return groups[1]
}

func (self *fiveHundredPx) Sizes(ctx *Context, groups []string) ([]ImageInfo, ResolvingErr) {
	id := groups[1]
	key := "500px-" + id
	r, err := ctx.HGet(key, "image")
	if err != nil {
		return []ImageInfo{}, ResolvingErr{UnknownError, err}
	}

	if r == "" {
		resp, err := http.Get("https://api.500px.com/v1/photos/" +
			id + "?image_size=5&consumer_key=jDMkZjOXcidZZex6lhloa95YRnZDVUQhrxX0IHKv")
		if err != nil {
			return []ImageInfo{}, ResolvingErr{UnknownError, err}
		}
		defer resp.Body.Close()
		if resp.StatusCode == http.StatusNotFound {
			return []ImageInfo{}, ResolvingErr{PictureNotFound, nil}
		}

		type photo struct{ Name, Image_url string }
		type apiResponse struct{ Photo photo }
		j := new(apiResponse)
		json.NewDecoder(resp.Body).Decode(j)

		r = j.Photo.Image_url
		ctx.HmSet(key, "image", r, "title", j.Photo.Name)
	}

	base := r[:len(r)-5]
	return []ImageInfo{
		ImageInfo{
			r,
			base + "4.jpg",
			base + "2.jpg",
			"",
		},
	}, ResolvingErr{}
}

var FiveHundredPxInstance = new(fiveHundredPx)
