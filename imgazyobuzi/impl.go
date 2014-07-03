package imgazyobuzi

type ImageInfo struct {
	Full  string `json:"full"`
	Large string `json:"large"`
	Thumb string `json:"thumb"`
}

type RegexModel struct {
	Name  string `json:"name"`
	Regex string `json:"regex"`
}

func GetRegex() []RegexModel {
	a := make([]RegexModel, len(Resolvers))
	for i, r := range Resolvers {
		a[i] = RegexModel{r.ServiceName(), r.Regex().String()}
	}
	return a
}

type SizesModel struct {
	Pictures []ImageInfo `json:"pictures"`
	Videos   []string    `json:"videos"`
}

func GetSizes(uri string) (SizesModel, ResolvingErr) {
	for _, r := range Resolvers {
		g := r.Regex().FindStringSubmatch(uri)
		if len(g) > 0 {
			pictures, err := r.Sizes(g)
			if err.Code != 0 {
				return SizesModel{}, err
			}

			videos, err := r.Videos(g)
			if err.Code != 0 {
				return SizesModel{}, err
			}

			return SizesModel{pictures, videos}, ResolvingErr{}
		}
	}

	return SizesModel{}, ResolvingErr{UriNotSupported, nil}
}

func Redirect(uri, size string) (string, ResolvingErr) {
	for _, r := range Resolvers {
		g := r.Regex().FindStringSubmatch(uri)
		if len(g) > 0 {
			if size == "video" {
				videos, err := r.Videos(g)

				if err.Code != 0 {
					return "", err
				}
				if len(videos) == 0 {
					return "", ResolvingErr{IsNotVideo, nil}
				}

				return videos[0], ResolvingErr{}
			}

			pictures, err := r.Sizes(g)
			if err.Code != 0 {
				return "", err
			}

			p := pictures[0]
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
