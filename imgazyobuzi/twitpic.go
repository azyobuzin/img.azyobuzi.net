package imgazyobuzi

import "regexp"

type twitpic resolverStruct

func (self *twitpic) ServiceName() string {
	return "Twitpic"
}

func (self *twitpic) Regex() *regexp.Regexp {
	if self.re == nil {
		self.re = regexp.MustCompile(`^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:\?.*)?(?:#.*)?$`)
	}
	return self.re
}

func (self *twitpic) Sizes(ctx *Context, groups []string) ([]ImageInfo, ResolvingErr) {
	return []ImageInfo{
		ImageInfo{
			"https://twitpic.com/show/large/" + groups[1],
			"https://twitpic.com/show/large/" + groups[1],
			"https://twitpic.com/show/thumb/" + groups[1],
			"",
		},
	}, ResolvingErr{}
}

var TwitpicInstance = new(twitpic)
