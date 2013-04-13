# -*- coding: utf-8 -*-

from resolvers import *

class Gyazo(Resolver):
    @property
    def service_name(self):
        return "Gyazo"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?gyazo\.com/(\w+)(?:\.png)?(?:\?.*)?$"

    def get_full(self, match):
        return "http://gyazo.com/" + match.group(1) + ".png"

    def get_full_https(self, match):
        return "https://gyazo.com/" + match.group(1) + ".png"

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return self.get_full(match)

    def get_thumb_https(self, match):
        return self.get_full_https(match)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
