# -*- coding: utf-8 -*-

from resolvers import *

class Molome(Resolver):
    @property
    def service_name(self):
        return "MOLOME"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?molo\.me/p/(\w+)/?(?:\?.*)?$"

    def get_full(self, match):
        return "http://p.molo.me/" + match.group(1)

    def get_full_https(self, match):
        return "https://p.molo.me/" + match.group(1)

    def get_large(self, match):
        return "http://p480x480.molo.me/%s_480x480" % match.group(1)

    def get_large_https(self, match):
        return "https://p480x480.molo.me/%s_480x480" % match.group(1)

    def get_thumb(self, match):
        return "http://p135x135.molo.me/%s_135x135" % match.group(1)

    def get_thumb_https(self, match):
        return "https://p135x135.molo.me/%s_135x135" % match.group(1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
