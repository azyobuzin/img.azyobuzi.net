package main

import "regexp"

type Twitpic ResolverStruct

func (self *Twitpic) ServiceName() string {
	return "Twitpic"
}

func (self *Twitpic) Regex() *regexp.Regexp {
	if self.re == nil {
		self.re = regexp.MustCompile(`^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:\?.*)?$`)
	}
	return self.re
}

func (self *Twitpic) Sizes(groups []string) ([]ImageInfo, ResolvingErr) {
	return []ImageInfo{
		ImageInfo{
			"https://twitpic.com/show/large/" + groups[1],
			"https://twitpic.com/show/large/" + groups[1],
			"https://twitpic.com/show/thumb/" + groups[1],
		},
	}, ResolvingErr{}
}

func (self *Twitpic) Video(groups []string) (*string, ResolvingErr) {
	return nil, ResolvingErr{}
}
