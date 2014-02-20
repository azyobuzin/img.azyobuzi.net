# -*- coding: utf-8 -*-

from resolvers import *

class MstrIn(Resolver):
    @property
    def service_name(self):
        return u"飯テロ.in"

    @property
    def regex_str(self):
        return r"^https?://mstr\.in/photos/(\w+)(?:\?.*)?$"

    def get_full(self, match):
        return self.get_large(match)

    def get_full_https(self, match):
        return self.get_large_https(match)

    def get_large(self, match):
        return "http://pic.mstr.in/images/" + match.group(1) + ".jpg"

    def get_large_https(self, match):
        return "https://pic.mstr.in/images/" + match.group(1) + ".jpg"

    def get_thumb(self, match):
        return "http://pic.mstr.in/thumbnails/" + match.group(1) + ".jpg"

    def get_thumb_https(self, match):
        return "https://pic.mstr.in/thumbnails/" + match.group(1) + ".jpg"

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
