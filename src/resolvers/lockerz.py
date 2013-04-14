# -*- coding: utf-8 -*-

import urllib

from resolvers import *

class Lockerz(Resolver):
    @property
    def service_name(self):
        return "Lockerz"

    @property
    def regex_str(self):
        return r"^https?://(?:(?:www\.|pics\.)?lockerz\.com/(?:s|u/\d+/photos)|(?:www\.)?plixi\.com/p)/(\d+)(?:\?.*)?$"

    def get_full(self, match):
        return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=big&url=" + urllib.quote("http://lockerz.com/s/" + match.group(1))

    def get_full_https(self, match):
        return "https://api.plixi.com/api/tpapi.svc/imagefromurl?size=big&url=" + urllib.quote("http://lockerz.com/s/" + match.group(1))

    def get_large(self, match):
        return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=medium&url=" + urllib.quote("http://lockerz.com/s/" + match.group(1))

    def get_large_https(self, match):
        return "https://api.plixi.com/api/tpapi.svc/imagefromurl?size=medium&url=" + urllib.quote("http://lockerz.com/s/" + match.group(1))

    def get_thumb(self, match):
        return "http://api.plixi.com/api/tpapi.svc/imagefromurl?size=small&url=" + urllib.quote("http://lockerz.com/s/" + match.group(1))

    def get_thumb_https(self, match):
        return "https://api.plixi.com/api/tpapi.svc/imagefromurl?size=small&url=" + urllib.quote("http://lockerz.com/s/" + match.group(1))

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
