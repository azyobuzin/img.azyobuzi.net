# -*- coding: utf-8 -*-

from resolvers import *

class Pckles(Resolver):
    @property
    def service_name(self):
        return "Pckles"

    @property
    def regex_str(self):
        return r"^https?://pckles\.com/(\w+)/(\w+)(?:/|\.png|\.(?:resize\.)?jpg)?(?:\?.*)?$"

    def get_full(self, match):
        return "http://pckles.com/%s/%s.png" % (match.group(1), match.group(2))

    def get_full_https(self, match):
        return "https://pckles.com/%s/%s.png" % (match.group(1), match.group(2))

    def get_large(self, match):
        return "http://pckles.com/%s/%s.jpg" % (match.group(1), match.group(2))

    def get_large_https(self, match):
        return "https://pckles.com/%s/%s.jpg" % (match.group(1), match.group(2))

    def get_thumb(self, match):
        return "http://pckles.com/%s/%s.resize.jpg" % (match.group(1), match.group(2))

    def get_thumb_https(self, match):
        return "https://pckles.com/%s/%s.resize.jpg" % (match.group(1), match.group(2))

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
