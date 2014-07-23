package imgazyobuzi

import (
	"encoding/json"
	"net/http"
	"regexp"
)

type cloudApp resolverStruct

func (self *cloudApp) ServiceName() (string, string) {
	return "clly", "CloudApp"
}

func (self *cloudApp) Regex() *regexp.Regexp {
	if self.re == nil {
		self.re = regexp.MustCompile(`^https?://(?:www\.)?cl\.ly/(?:image/)?(\w+)(?:/|/o/?)?(?:\?.*)?(?:#.*)?$`)
	}
	return self.re
}

func (self *cloudApp) Id(groups []string) string {
	return groups[1]
}

func (self *cloudApp) Sizes(ctx *Context, groups []string) ([]ImageInfo, ResolvingErr) {
	id := groups[1]
	key := "clly-" + id
	r, err := ctx.HmGet(key, "remote", "thumb")
	if err != nil {
		return []ImageInfo{}, ResolvingErr{UnknownError, err}
	}

	remote := r[0]
	thumb := r[1]

	if remote == "" {
		req, _ := http.NewRequest("GET", "http://cl.ly/"+id, nil)
		req.Header.Add("Accept", "application/json")
		resp, err := http.DefaultClient.Do(req)
		if err != nil {
			return []ImageInfo{}, ResolvingErr{UnknownError, err}
		}
		defer resp.Body.Close()
		if resp.StatusCode == http.StatusNotFound {
			return []ImageInfo{}, ResolvingErr{PictureNotFound, nil}
		}

		type cllyResponse struct{ Name, Item_type, Remote_url, Thumbnail_url string }
		j := new(cllyResponse)
		json.NewDecoder(resp.Body).Decode(j)

		if j.Item_type != "image" {
			return []ImageInfo{}, ResolvingErr{IsNotPicture, nil}
		}

		remote = j.Remote_url
		thumb = j.Thumbnail_url

		ctx.HmSet(key, "remote", remote, "thumb", thumb, "title", j.Name)
	}

	return []ImageInfo{ImageInfo{remote, remote, thumb, ""}}, ResolvingErr{}
}

var CloudAppInstance = new(cloudApp)
