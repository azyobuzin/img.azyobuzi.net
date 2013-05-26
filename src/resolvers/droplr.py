# -*- coding: utf-8 -*-

from resolvers import *

class Droplr(Resolver):
    @property
    def service_name(self):
        return "Droplr"

    @property
    def regex_str(self):
        return r"^https?://d\.pr/(?:i/)?(\w+)\+?/?(?:\?.*)?$"

    def get_full(self, match):
        return "http://d.pr/i/" + match.group(1) + "+"

    def get_full_https(self, match):
        return "https://d.pr/i/" + match.group(1) + "+"

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return "http://d.pr/i/" + match.group(1) + "/medium"

    def get_thumb_https(self, match):
        return "https://d.pr/i/" + match.group(1) + "/medium"

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
