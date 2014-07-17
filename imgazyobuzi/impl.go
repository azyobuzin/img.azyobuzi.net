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
		_, name := r.ServiceName()
		a[i] = RegexModel{name, r.Regex().String()}
	}
	return a
}

type SizesModel struct {
	Service   string      `json:"service"`
	Sizes     []ImageInfo `json:"sizes"`
	ServiceId string      `json:"-"`
	Id        string      `json:"-"`
}

func (self *Context) GetSizes(uri string) (SizesModel, ResolvingErr) {
	for _, r := range Resolvers {
		g := r.Regex().FindStringSubmatch(uri)
		if len(g) > 0 {
			sizes, err := r.Sizes(self, g)
			if err.Code != 0 {
				return SizesModel{}, err
			}

			svcid, name := r.ServiceName()
			return SizesModel{name, sizes, svcid, r.Id(g)}, ResolvingErr{}
		}
	}

	return SizesModel{}, ResolvingErr{UriNotSupported, nil}
}

type RedirectResult struct {
	Location, ServiceId, Id string
}

func (self *Context) Redirect(uri, size string) (RedirectResult, ResolvingErr) {
	for _, r := range Resolvers {
		g := r.Regex().FindStringSubmatch(uri)
		if len(g) > 0 {
			sizes, err := r.Sizes(self, g)
			if err.Code != 0 {
				return RedirectResult{}, err
			}
			svcid, _ := r.ServiceName()
			id := r.Id(g)

			if size == "video" {
				for _, s := range sizes {
					if s.Video != "" {
						return RedirectResult{s.Video, svcid, id}, ResolvingErr{}
					}
				}
				return RedirectResult{}, ResolvingErr{IsNotVideo, nil}
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

			return RedirectResult{s, svcid, id}, ResolvingErr{}
		}
	}

	return RedirectResult{}, ResolvingErr{UriNotSupported, nil}
}
