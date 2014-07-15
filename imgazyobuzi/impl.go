package imgazyobuzi

type ImageInfo struct {
	Full  string `json:"full"`
	Large string `json:"large"`
	Thumb string `json:"thumb"`
	Video string `json:"video"`
}

type RegexModel struct {
	Name  string `json:"name"`
	Regex string `json:"regex"`
}

func (self *Context) GetRegex() []RegexModel {
	a := make([]RegexModel, len(Resolvers))
	for i, r := range Resolvers {
		a[i] = RegexModel{r.ServiceName(), r.Regex().String()}
	}
	return a
}

type SizesModel struct {
	Service string      `json:"service"`
	Sizes   []ImageInfo `json:"sizes"`
}

func (self *Context) GetSizes(uri string) (SizesModel, ResolvingErr) {
	for _, r := range Resolvers {
		g := r.Regex().FindStringSubmatch(uri)
		if len(g) > 0 {
			sizes, err := r.Sizes(self, g)
			if err.Code != 0 {
				return SizesModel{}, err
			}

			return SizesModel{r.ServiceName(), sizes}, ResolvingErr{}
		}
	}

	return SizesModel{}, ResolvingErr{UriNotSupported, nil}
}

func (self *Context) Redirect(uri, size string) (string, ResolvingErr) {
	for _, r := range Resolvers {
		g := r.Regex().FindStringSubmatch(uri)
		if len(g) > 0 {
			sizes, err := r.Sizes(self, g)
			if err.Code != 0 {
				return "", err
			}

			if size == "video" {
				for _, s := range sizes {
					if s.Video != "" {
						return s.Video, ResolvingErr{}
					}
				}
				return "", ResolvingErr{IsNotVideo, nil}
			}

			p := sizes[0]
			var s string
			switch size {
			case "full":
				s = p.Full
			case "large":
				s = p.Large
			case "thumb":
				s = p.Thumb
			}
			return s, ResolvingErr{}
		}
	}

	return "", ResolvingErr{UriNotSupported, nil}
}
