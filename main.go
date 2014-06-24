package main

import (
	"encoding/json"
	"github.com/codegangsta/negroni"
	"github.com/jingweno/negroni-gorelic"
	"net/http"
	"os"
	"regexp"
	"strconv"
	"strings"
	"time"
)

func main() {
	env := ReadEnv()

	mux := http.NewServeMux()
	mux.HandleFunc("/", func(w http.ResponseWriter, req *http.Request) {
		//404
		code := APINotFound
		if req.URL.Path == "/" {
			code = SelectAPI
		}
		NewErrorResponse(code, nil).WriteTo(w)
	})
	mux.HandleFunc("/regex.json", func(w http.ResponseWriter, req *http.Request) {
		NewOKResponse(CallRegex(), false).WriteTo(w)
	})
	mux.HandleFunc("/sizes.json", func(w http.ResponseWriter, req *http.Request) {
		uri := req.URL.Query().Get("uri")
		if uri == "" {
			NewErrorResponse(RequireUriParam, nil).WriteTo(w)
			return
		}

		res, err := CallSizes(uri)
		if err.Code == 0 {
			NewOKResponse(res, true).WriteTo(w)
		} else {
			var msg *string
			if err.Error != nil {
				tmp := err.Error.Error()
				msg = &tmp
			}
			NewErrorResponse(err.Code, msg).WriteTo(w)
		}
	})
	mux.HandleFunc("/redirect.json", func(w http.ResponseWriter, req *http.Request) {
		q := req.URL.Query()
		uri := q.Get("uri")
		if uri == "" {
			NewErrorResponse(RequireUriParam, nil).WriteTo(w)
			return
		}

		size := strings.ToLower(q.Get("size"))
		switch size {
		case "":
			size = "full"
		case "full", "large", "thumb", "video":
		default:
			NewErrorResponse(InvalidSizeParam, nil).WriteTo(w)
			return
		}

		location, err := CallRedirect(uri, size)
		if err.Code == 0 {
			NewRedirectResponse(location, true).WriteTo(w)
		} else {
			var msg *string
			if err.Error != nil {
				tmp := err.Error.Error()
				msg = &tmp
			}
			NewErrorResponse(err.Code, msg).WriteTo(w)
		}
	})

	n := negroni.Classic()
	if env.NewRelicLicenseKey != "" {
		appName := env.NewRelicAppName
		if appName == "" {
			appName = "img.azyobuzi.net v3"
		}
		n.Use(negronigorelic.New(env.NewRelicLicenseKey, appName, true))
	}
	n.UseHandler(mux)
	addr := env.Address
	if addr == "" {
		addr = ":61482"
	}
	n.Run(addr)
}

type Env struct {
	Address, NewRelicLicenseKey, NewRelicAppName string
}

func ReadEnv() Env {
	return Env{
		os.Getenv("IMGAZYOBUZI_ADDR"),
		os.Getenv("IMGAZYOBUZI_NR_LICENSE_KEY"),
		os.Getenv("IMGAZYOBUZI_NR_APP_NAME"),
	}
}

type ErrorModel struct {
	Code      int     `json:"code"`
	Message   string  `json:"message"`
	Exception *string `json:"exception"`
}

type ErrorInfo struct {
	Status  int
	Message string
}

const (
	RequireUriParam  = 4001
	UriNotSupported  = 4002
	InvalidSizeParam = 4003
	SelectAPI        = 4041
	APINotFound      = 4042
	PictureNotFound  = 4043
	IsNotPicture     = 4044
	IsNotVideo       = 4045
	InvalidMethod    = 4051
	UnknownError     = 5000
)

var Errors = map[int]ErrorInfo{
	4000:             ErrorInfo{400, "Bad request."},
	RequireUriParam:  ErrorInfo{400, "\"uri\" parameter is required."},
	UriNotSupported:  ErrorInfo{400, "\"uri\" parameter you requested is not supported."},
	InvalidSizeParam: ErrorInfo{400, "\"size\" parameter is invalid."},
	4040:             ErrorInfo{404, "Not Found."},
	SelectAPI:        ErrorInfo{404, "Select API."},
	APINotFound:      ErrorInfo{404, "API you requested is not found."},
	PictureNotFound:  ErrorInfo{404, "The picture you requested is not found."},
	IsNotPicture:     ErrorInfo{404, "Your request is not a picture."},
	IsNotVideo:       ErrorInfo{404, "Your request is not a video."},
	4050:             ErrorInfo{405, "The method is not allowed."},
	InvalidMethod:    ErrorInfo{405, "Call with GET or HEAD method."},
	UnknownError:     ErrorInfo{500, "Raised unknown exception on server."},
}

type Response struct {
	Status      int
	Location    string
	Body        interface{}
	HasBody     bool
	HasLocation bool
	Cacheable   bool
}

func NewResponse(status int, body interface{}, cacheable bool) Response {
	return Response{
		Status:    status,
		Body:      body,
		HasBody:   true,
		Cacheable: cacheable,
	}
}

func NewOKResponse(body interface{}, cacheable bool) Response {
	return NewResponse(http.StatusOK, body, cacheable)
}

func NewRedirectResponse(location string, cacheable bool) Response {
	return Response{
		Status:      http.StatusFound,
		Location:    location,
		HasLocation: true,
		Cacheable:   cacheable,
	}
}

func NewErrorResponse(code int, exception *string) Response {
	info := Errors[code]
	return NewResponse(info.Status, ErrorModel{code, info.Message, exception}, false)
}

func (self Response) WriteTo(w http.ResponseWriter) {
	var body []byte
	var err error
	if self.HasBody {
		body, err = json.Marshal(self.Body)
		if err != nil {
			errStr := err.Error()
			NewErrorResponse(UnknownError, &errStr).WriteTo(w)
			return
		}
	}

	header := w.Header()
	if self.HasBody {
		header.Set("Content-Type", "application/json")
		header.Set("Content-Length", strconv.Itoa(len(body)))
	}
	if self.HasLocation {
		header.Set("Location", self.Location)
	}
	if self.Cacheable {
		header.Set("Expires", time.Now().UTC().AddDate(0, 0, 10).Format(time.RFC1123))
	}
	w.WriteHeader(self.Status)

	if self.HasBody {
		w.Write(body)
	}
}

type ImageInfo struct {
	Full  string `json:"full"`
	Large string `json:"large"`
	Thumb string `json:"thumb"`
}

type ResolvingErr struct {
	Code  int
	Error error
}

type Resolver interface {
	ServiceName() string
	Regex() *regexp.Regexp
	Sizes(groups []string) ([]ImageInfo, ResolvingErr)
	Video(groups []string) (*string, ResolvingErr)
}

type ResolverStruct struct {
	re *regexp.Regexp
}

type RegexModel struct {
	Name  string `json:"name"`
	Regex string `json:"regex"`
}

func CallRegex() []RegexModel {
	a := make([]RegexModel, len(Resolvers))
	for i, r := range Resolvers {
		a[i] = RegexModel{r.ServiceName(), r.Regex().String()}
	}
	return a
}

type SizesModel struct {
	Pictures []ImageInfo `json:"pictures"`
	Video    *string     `json:"video"`
}

func CallSizes(uri string) (SizesModel, ResolvingErr) {
	for _, r := range Resolvers {
		g := r.Regex().FindStringSubmatch(uri)
		if len(g) > 0 {
			pictures, err := r.Sizes(g)
			if err.Code != 0 {
				return SizesModel{}, err
			}

			video, err := r.Video(g)
			if err.Code != 0 {
				return SizesModel{}, err
			}

			return SizesModel{pictures, video}, ResolvingErr{}
		}
	}

	return SizesModel{}, ResolvingErr{UriNotSupported, nil}
}

func CallRedirect(uri, size string) (string, ResolvingErr) {
	for _, r := range Resolvers {
		g := r.Regex().FindStringSubmatch(uri)
		if len(g) > 0 {
			if size == "video" {
				video, err := r.Video(g)

				if err.Code != 0 {
					return "", err
				}
				if video == nil {
					return "", ResolvingErr{IsNotVideo, nil}
				}

				return *video, ResolvingErr{}
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
