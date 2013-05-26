# -*- coding: utf-8 -*-

from resolvers import *

class Instagram(Resolver):
    @property
    def service_name(self):
        return "Instagram"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?instagr(?:\.am|am\.com)/p/([\w\-]+)(?:/(?:media/?)?)?(?:\?.*)?$"

    def get_full(self, match):
        return "http://instagr.am/p/%s/media/?size=l" % match.group(1)

    def get_full_https(self, match):
        return "https://instagr.am/p/%s/media/?size=l" % match.group(1)

    def get_large(self, match):
        return "http://instagr.am/p/%s/media/?size=m" % match.group(1)

    def get_large_https(self, match):
        return "https://instagr.am/p/%s/media/?size=m" % match.group(1)

    def get_thumb(self, match):
        return "http://instagr.am/p/%s/media/?size=t" % match.group(1)

    def get_thumb_https(self, match):
        return "https://instagr.am/p/%s/media/?size=t" % match.group(1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
